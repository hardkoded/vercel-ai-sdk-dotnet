// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>One server-sent event of an Interactions stream. <see cref="Value"/> is null when the data was not a JSON object.</summary>
internal sealed class GoogleInteractionsEvent
{
    internal GoogleInteractionsEvent(string raw)
    {
        Raw = raw;
        try
        {
            Value = JsonNode.Parse(raw) as JsonObject;
        }
        catch (JsonException)
        {
            Value = null;
        }
    }

    internal string Raw { get; }

    internal JsonObject? Value { get; }

    internal string? EventType
    {
        get { return GoogleInteractionsStreaming.String(Value, "event_type"); }
    }
}

/// <summary>Reads <c>GET /interactions/{id}?stream=true</c> and turns its events into stream parts.</summary>
internal static class GoogleInteractionsStreaming
{
    internal const int DefaultMaxRetries = 3;

    internal static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(500);

    internal static string? String(JsonObject? node, string name)
    {
        return node?[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
    }

    /// <summary>
    /// Connects to the event feed of a background interaction and yields its events. The connection can drop while a long
    /// run idles between events, so the stream remembers the last <c>event_id</c> and reconnects with <c>last_event_id</c>.
    /// After <paramref name="maxRetries"/> failed attempts in a row it throws. The stream ends cleanly after an
    /// <c>interaction.completed</c> or <c>error</c> event. When the caller cancels, or stops reading before the run ends,
    /// it sends a best-effort <c>POST /interactions/{id}/cancel</c> so the run stops billing on Google's side.
    /// </summary>
    internal static IAsyncEnumerable<GoogleInteractionsEvent> StreamEvents(
        ProviderHttp http,
        string baseUrl,
        string? interactionId,
        IReadOnlyDictionary<string, string?> headers,
        int maxRetries,
        TimeSpan retryDelay,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(interactionId))
        {
            throw new InvalidOperationException("google.interactions: cannot stream a background interaction without an id.");
        }

        return StreamEventsCore(http, baseUrl, interactionId!, headers, maxRetries, retryDelay, cancellationToken);
    }

    private static async IAsyncEnumerable<GoogleInteractionsEvent> StreamEventsCore(
        ProviderHttp http,
        string baseUrl,
        string id,
        IReadOnlyDictionary<string, string?> headers,
        int maxRetries,
        TimeSpan retryDelay,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var eventHeaders = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in headers)
        {
            eventHeaders[pair.Key] = pair.Value;
        }

        eventHeaders["Accept"] = "text/event-stream";

        string? lastEventId = null;
        var complete = false;
        var failed = false;
        var attempt = 0;
        var receivedAny = false;
        IAsyncEnumerator<string>? reader = null;
        try
        {
            while (!complete)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (reader == null)
                {
                    try
                    {
                        var opened = await http.OpenSseResponseAsync(HttpMethod.Get, Url(baseUrl, id, lastEventId), null, eventHeaders, cancellationToken).ConfigureAwait(false);
                        reader = opened.Events.GetAsyncEnumerator(cancellationToken);
                        receivedAny = false;
                    }
                    catch (Exception error) when (error is not OperationCanceledException)
                    {
                        attempt++;
                        if (attempt >= maxRetries)
                        {
                            failed = true;
                            throw;
                        }

                        await Task.Delay(TimeSpan.FromTicks(retryDelay.Ticks * attempt), cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                }

                bool moved;
                try
                {
                    moved = await reader.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    await DisposeReaderAsync(reader).ConfigureAwait(false);
                    reader = null;
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        failed = true;
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromTicks(retryDelay.Ticks * attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (!moved)
                {
                    // The connection ended. After a terminal event that is the end. Otherwise it is a drop, so reconnect.
                    // A connection that produced no events counts as a failed attempt, so an empty server cannot loop forever.
                    await DisposeReaderAsync(reader).ConfigureAwait(false);
                    reader = null;
                    if (receivedAny)
                    {
                        attempt = 0;
                        continue;
                    }

                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        failed = true;
                        throw new AiSdkException("google.interactions: SSE stream closed without producing any events.");
                    }

                    await Task.Delay(TimeSpan.FromTicks(retryDelay.Ticks * attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                receivedAny = true;
                var streamEvent = new GoogleInteractionsEvent(reader.Current);
                var eventId = String(streamEvent.Value, "event_id");
                if (!string.IsNullOrEmpty(eventId))
                {
                    lastEventId = eventId;
                }

                if (streamEvent.EventType == "interaction.completed" || streamEvent.EventType == "error")
                {
                    complete = true;
                }

                yield return streamEvent;
            }
        }
        finally
        {
            if (reader != null)
            {
                await DisposeReaderAsync(reader).ConfigureAwait(false);
            }

            // The caller cancelled, or stopped reading before the agent finished.
            if (!complete && !failed)
            {
                await GoogleInteractionsPolling.CancelOnServerAsync(http, baseUrl, id, headers).ConfigureAwait(false);
            }
        }
    }

    private static async Task DisposeReaderAsync(IAsyncEnumerator<string> reader)
    {
        try
        {
            await reader.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The connection is already gone.
        }
    }

    private static Uri Url(string baseUrl, string id, string? lastEventId)
    {
        var url = ApiKeys.Combine(baseUrl, "interactions/" + Uri.EscapeDataString(id)).AbsoluteUri + "?stream=true";
        if (lastEventId != null)
        {
            url += "&last_event_id=" + Uri.EscapeDataString(lastEventId);
        }

        return new Uri(url, UriKind.Absolute);
    }
}

/// <summary>
/// Turns Interactions events into language model stream parts: response metadata, text blocks, errors, and the finish part.
/// Other step kinds are not yet surfaced.
/// </summary>
internal sealed class GoogleInteractionsStreamTransform
{
    private readonly IReadOnlyList<CallWarning> _warnings;
    private readonly bool _includeRawChunks;
    private readonly Dictionary<int, OpenBlock> _openBlocks = new();
    private string? _interactionId;
    private JsonObject? _usage;
    private string? _serviceTier;
    private string? _finishStatus;

    internal GoogleInteractionsStreamTransform(IReadOnlyList<CallWarning> warnings, bool includeRawChunks)
    {
        _warnings = warnings;
        _includeRawChunks = includeRawChunks;
    }

    internal IEnumerable<LanguageModelStreamPart> Start()
    {
        yield return new StreamStartStreamPart(_warnings);
    }

    /// <summary>Streams the parts of a live event feed, from stream-start to finish.</summary>
    internal async IAsyncEnumerable<LanguageModelStreamPart> Pipe(IAsyncEnumerable<GoogleInteractionsEvent> events, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var part in Start())
        {
            yield return part;
        }

        await foreach (var streamEvent in events.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            foreach (var part in Transform(streamEvent))
            {
                yield return part;
            }
        }

        foreach (var part in Flush())
        {
            yield return part;
        }
    }

    /// <summary>
    /// Replays a finished interaction as the parts a live feed would have produced. The server has already made every
    /// block by now, so each text block is one delta.
    /// </summary>
    internal IEnumerable<LanguageModelStreamPart> Replay(JsonObject response)
    {
        foreach (var part in Start())
        {
            yield return part;
        }

        var id = GoogleInteractionsStreaming.String(response, "id");
        _interactionId = string.IsNullOrEmpty(id) ? null : id;
        _finishStatus = GoogleInteractionsStreaming.String(response, "status");
        _usage = response["usage"] as JsonObject;
        _serviceTier = GoogleInteractionsStreaming.String(response, "service_tier");
        DateTimeOffset? timestamp = DateTimeOffset.TryParse(GoogleInteractionsStreaming.String(response, "created"), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ? parsed : null;
        yield return new ResponseMetadataStreamPart(_interactionId, GoogleInteractionsStreaming.String(response, "model"), timestamp);
        if (_includeRawChunks)
        {
            yield return new RawStreamPart(response.ToJsonString());
        }

        var (content, hasFunctionCall) = GoogleInteractionsOutput.Parse(response);
        var block = 0;
        foreach (var item in content)
        {
            if (item is GeneratedText text)
            {
                var blockId = (_interactionId ?? "agent") + ":" + (block++).ToString(CultureInfo.InvariantCulture);
                yield return new TextStartStreamPart(blockId);
                if (text.Text.Length > 0)
                {
                    yield return new TextDeltaStreamPart(blockId, text.Text);
                }

                yield return new TextEndStreamPart(blockId, null, TextMetadata());
            }
            else if (item is GeneratedToolCall call)
            {
                yield return new ToolInputStartStreamPart(call.ToolCallId, call.ToolName);
                yield return new ToolInputDeltaStreamPart(call.ToolCallId, call.ArgumentsJson);
                yield return new ToolInputEndStreamPart(call.ToolCallId);
                yield return new ToolCallStreamPart(call.ToolCallId, call.ToolName, call.ArgumentsJson);
            }
        }

        yield return Finish(hasFunctionCall);
    }

    internal IEnumerable<LanguageModelStreamPart> Transform(GoogleInteractionsEvent streamEvent)
    {
        var parts = new List<LanguageModelStreamPart>();
        if (_includeRawChunks)
        {
            parts.Add(new RawStreamPart(streamEvent.Raw));
        }

        if (streamEvent.Value == null)
        {
            _finishStatus = "failed";
            parts.Add(new ErrorStreamPart("google.interactions: the stream sent data that is not a JSON object."));
            return parts;
        }

        var value = streamEvent.Value;
        switch (streamEvent.EventType)
        {
            case "interaction.created":
                Created(value, parts);
                break;
            case "step.start":
                StepStart(value, parts);
                break;
            case "step.delta":
                StepDelta(value, parts);
                break;
            case "step.stop":
                StepStop(value, parts);
                break;
            case "interaction.status_update":
            case "interaction.in_progress":
            case "interaction.requires_action":
                _finishStatus = GoogleInteractionsStreaming.String(value, "status")
                    ?? (streamEvent.EventType == "interaction.requires_action" ? "requires_action" : "in_progress");
                break;
            case "interaction.completed":
                Completed(value);
                break;
            case "error":
                _finishStatus = "failed";
                var error = value["error"] as JsonObject;
                parts.Add(new ErrorStreamPart(new ProviderStreamError(
                    GoogleInteractionsStreaming.String(error, "message") ?? "Unknown interaction error",
                    value,
                    "error",
                    Code(error))));
                break;
        }

        return parts;
    }

    internal IEnumerable<LanguageModelStreamPart> Flush()
    {
        yield return Finish(false);
    }

    private FinishStreamPart Finish(bool hasFunctionCall)
    {
        var google = new JsonObject();
        if (_interactionId != null)
        {
            google["interactionId"] = _interactionId;
        }

        if (_serviceTier != null)
        {
            google["serviceTier"] = _serviceTier;
        }

        var metadata = JsonSerializer.SerializeToElement(new JsonObject { ["google"] = google });
        return new FinishStreamPart(
            GoogleInteractionsFinish.Map(_finishStatus, hasFunctionCall),
            GoogleInteractionsModel.Usage(_usage),
            _finishStatus,
            metadata);
    }

    private JsonElement? TextMetadata()
    {
        return _interactionId == null
            ? null
            : JsonSerializer.SerializeToElement(new JsonObject { ["google"] = new JsonObject { ["interactionId"] = _interactionId } });
    }

    private void Created(JsonObject value, List<LanguageModelStreamPart> parts)
    {
        var interaction = value["interaction"] as JsonObject;
        // The API sends id "" when store is false. Treat that as no id.
        var id = GoogleInteractionsStreaming.String(interaction, "id");
        _interactionId = string.IsNullOrEmpty(id) ? null : id;
        DateTimeOffset? timestamp = DateTimeOffset.TryParse(GoogleInteractionsStreaming.String(interaction, "created"), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ? parsed : null;
        parts.Add(new ResponseMetadataStreamPart(_interactionId, GoogleInteractionsStreaming.String(interaction, "model"), timestamp));
    }

    private void StepStart(JsonObject value, List<LanguageModelStreamPart> parts)
    {
        if (Index(value) is not { } index)
        {
            return;
        }

        var blockId = (_interactionId ?? "interaction") + ":" + index.ToString(CultureInfo.InvariantCulture);
        var step = value["step"] as JsonObject;
        if (GoogleInteractionsStreaming.String(step, "type") != "model_output")
        {
            _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.Other);
            return;
        }

        // The step often starts bare. The first delta then reveals whether the block is text.
        var first = (step?["content"] as JsonArray)?.FirstOrDefault() as JsonObject;
        if (GoogleInteractionsStreaming.String(first, "type") == "text")
        {
            _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.Text);
            parts.Add(new TextStartStreamPart(blockId));
        }
        else
        {
            _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.PendingModelOutput);
        }
    }

    private void StepDelta(JsonObject value, List<LanguageModelStreamPart> parts)
    {
        if (Index(value) is not { } index || !_openBlocks.TryGetValue(index, out var open))
        {
            return;
        }

        var delta = value["delta"] as JsonObject;
        var type = GoogleInteractionsStreaming.String(delta, "type");
        if (open.Kind == OpenBlockKind.PendingModelOutput && type == "text")
        {
            open.Kind = OpenBlockKind.Text;
            parts.Add(new TextStartStreamPart(open.Id));
        }

        if (open.Kind == OpenBlockKind.Text && type == "text")
        {
            var text = GoogleInteractionsStreaming.String(delta, "text") ?? string.Empty;
            if (text.Length > 0)
            {
                parts.Add(new TextDeltaStreamPart(open.Id, text));
            }
        }
    }

    private void StepStop(JsonObject value, List<LanguageModelStreamPart> parts)
    {
        if (Index(value) is not { } index || !_openBlocks.TryGetValue(index, out var open))
        {
            return;
        }

        if (open.Kind == OpenBlockKind.Text)
        {
            parts.Add(new TextEndStreamPart(open.Id, null, TextMetadata()));
        }
    }

    private void Completed(JsonObject value)
    {
        if (value["interaction"] is not JsonObject interaction)
        {
            return;
        }

        var id = GoogleInteractionsStreaming.String(interaction, "id");
        if (!string.IsNullOrEmpty(id))
        {
            _interactionId = id;
        }

        _finishStatus = GoogleInteractionsStreaming.String(interaction, "status") ?? _finishStatus;
        _usage = interaction["usage"] as JsonObject ?? _usage;
        _serviceTier = GoogleInteractionsStreaming.String(interaction, "service_tier") ?? _serviceTier;
    }

    private static object? Code(JsonObject? error)
    {
        return error?["code"] is JsonValue code
            ? code.TryGetValue<string>(out var text) ? text : code.TryGetValue<int>(out var number) ? number : null
            : null;
    }

    private static int? Index(JsonObject value)
    {
        return value["index"] is JsonValue number && number.TryGetValue<int>(out var index) ? index : null;
    }

    private enum OpenBlockKind
    {
        Other,
        PendingModelOutput,
        Text,
    }

    private sealed class OpenBlock
    {
        internal OpenBlock(string id, OpenBlockKind kind)
        {
            Id = id;
            Kind = kind;
        }

        internal string Id { get; }

        internal OpenBlockKind Kind { get; set; }
    }
}
