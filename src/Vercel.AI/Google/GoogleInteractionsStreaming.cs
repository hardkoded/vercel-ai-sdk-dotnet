// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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

                // The limit counts failures in a row, so an event that arrives clears the count.
                receivedAny = true;
                attempt = 0;
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
/// Turns Interactions events into language model stream parts: response metadata, text, reasoning, function calls,
/// built-in tool calls and results, sources, custom processing parts, errors, and the finish part.
/// Image and video outputs are not yet surfaced.
/// </summary>
internal sealed class GoogleInteractionsStreamTransform
{
    private static readonly HashSet<string> BuiltinToolCallTypes = new(StringComparer.Ordinal)
    {
        "google_search_call",
        "code_execution_call",
        "url_context_call",
        "file_search_call",
        "google_maps_call",
        "mcp_server_tool_call",
    };

    private static readonly HashSet<string> BuiltinToolResultTypes = new(StringComparer.Ordinal)
    {
        "google_search_result",
        "code_execution_result",
        "url_context_result",
        "file_search_result",
        "google_maps_result",
        "mcp_server_tool_result",
    };

    private readonly IReadOnlyList<CallWarning> _warnings;
    private readonly bool _includeRawChunks;
    private readonly Func<string> _generateId;
    private readonly Dictionary<int, OpenBlock> _openBlocks = new();

    // Citations often reappear across annotation deltas as the text grows, so each source is surfaced once per stream.
    private readonly HashSet<string> _emittedSourceKeys = new(StringComparer.Ordinal);
    private string? _interactionId;
    private JsonObject? _usage;
    private string? _serviceTier;
    private string? _finishStatus;
    private bool _hasFunctionCall;

    /// <param name="warnings">Warnings for the stream-start part.</param>
    /// <param name="includeRawChunks">Whether to emit a raw part for every event.</param>
    /// <param name="generateId">Makes the id of a source. Defaults to a random id.</param>
    /// <param name="serviceTier">The tier from the <c>x-gemini-service-tier</c> response header. The tier in the <c>interaction.completed</c> body wins over it.</param>
    internal GoogleInteractionsStreamTransform(IReadOnlyList<CallWarning> warnings, bool includeRawChunks, Func<string>? generateId = null, string? serviceTier = null)
    {
        _warnings = warnings;
        _includeRawChunks = includeRawChunks;
        _generateId = generateId ?? (() => Guid.NewGuid().ToString("N").Substring(0, 16));
        _serviceTier = serviceTier;
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
        _serviceTier = GoogleInteractionsStreaming.String(response, "service_tier") ?? _serviceTier;
        DateTimeOffset? timestamp = DateTimeOffset.TryParse(GoogleInteractionsStreaming.String(response, "created"), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ? parsed : null;
        yield return new ResponseMetadataStreamPart(_interactionId, GoogleInteractionsStreaming.String(response, "model"), timestamp);
        if (_includeRawChunks)
        {
            yield return new RawStreamPart(response.ToJsonString());
        }

        var (content, hasFunctionCall) = GoogleInteractionsOutput.Parse(response);
        _hasFunctionCall = hasFunctionCall;
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

                yield return new TextEndStreamPart(blockId, null, GoogleMetadata());
            }
            else if (item is GeneratedToolCall call)
            {
                yield return new ToolInputStartStreamPart(call.ToolCallId, call.ToolName);
                yield return new ToolInputDeltaStreamPart(call.ToolCallId, call.ArgumentsJson);
                yield return new ToolInputEndStreamPart(call.ToolCallId);
                yield return new ToolCallStreamPart(call.ToolCallId, call.ToolName, call.ArgumentsJson);
            }
        }

        yield return Finish();
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
        // A feed that ends or fails mid-block leaves text blocks open. Close them so consumers see matching start and end parts.
        foreach (var open in _openBlocks.OrderBy(pair => pair.Key).Select(pair => pair.Value).Where(block => block.Kind == OpenBlockKind.Text))
        {
            yield return new TextEndStreamPart(open.Id, null, GoogleMetadata());
        }

        _openBlocks.Clear();
        yield return Finish();
    }

    private FinishStreamPart Finish()
    {
        return new FinishStreamPart(
            GoogleInteractionsFinish.Map(_finishStatus, _hasFunctionCall),
            GoogleInteractionsModel.Usage(_usage),
            _finishStatus,
            GoogleInteractionsModel.ProviderMetadata(_interactionId, _serviceTier, GoogleInteractionsModel.OutputTokensByModality(_usage)));
    }

    // The provider metadata of a part: the interaction id and, where the part has one, its signature.
    private JsonElement? GoogleMetadata(string? signature = null)
    {
        var google = new JsonObject();
        if (signature != null)
        {
            google["signature"] = signature;
        }

        if (_interactionId != null)
        {
            google["interactionId"] = _interactionId;
        }

        return google.Count == 0 ? null : JsonSerializer.SerializeToElement(new JsonObject { ["google"] = google });
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
        var stepType = GoogleInteractionsStreaming.String(step, "type");
        switch (stepType)
        {
            case "model_output":
                // The step often starts bare. The first delta then reveals whether the block is text.
                var first = (step?["content"] as JsonArray)?.FirstOrDefault() as JsonObject;
                if (GoogleInteractionsStreaming.String(first, "type") == "text")
                {
                    _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.Text);
                    parts.Add(new TextStartStreamPart(blockId));
                    AddSources(GoogleInteractionsSources.AnnotationsToSources(first!["annotations"] as JsonArray, _generateId), parts);
                }
                else
                {
                    _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.PendingModelOutput);
                }

                break;
            case "thought":
                _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.Reasoning) { Signature = GoogleInteractionsStreaming.String(step, "signature") };
                parts.Add(new ReasoningStartStreamPart(blockId));

                // The summary of a thought can already hold text when the step starts.
                foreach (var item in (step?["summary"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                {
                    if (GoogleInteractionsStreaming.String(item, "type") == "text" && GoogleInteractionsStreaming.String(item, "text") is { } text)
                    {
                        parts.Add(new ReasoningDeltaStreamPart(blockId, text));
                    }
                }

                break;
            case "processing_call":
            case "processing_result":
                var google = new JsonObject();
                if (GoogleInteractionsStreaming.String(step, "signature") is { } signature)
                {
                    google["signature"] = signature;
                }

                if (_interactionId != null)
                {
                    google["interactionId"] = _interactionId;
                }

                if (stepType == "processing_call")
                {
                    google["processingId"] = NonEmpty(GoogleInteractionsStreaming.String(step, "id")) ?? blockId;
                }
                else
                {
                    google["processingCallId"] = NonEmpty(GoogleInteractionsStreaming.String(step, "call_id")) ?? blockId;
                }

                _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.Custom) { CustomKind = "google." + stepType, Google = google };
                break;
            case "function_call":
                var toolCallId = NonEmpty(GoogleInteractionsStreaming.String(step, "id")) ?? blockId;
                var toolName = GoogleInteractionsStreaming.String(step, "name") ?? "unknown";
                _hasFunctionCall = true;
                _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.FunctionCall)
                {
                    ToolCallId = toolCallId,
                    ToolName = toolName,
                    Signature = GoogleInteractionsStreaming.String(step, "signature"),
                };
                parts.Add(new ToolInputStartStreamPart(toolCallId, toolName));
                break;
            case { } type when BuiltinToolCallTypes.Contains(type):
                _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.BuiltinToolCall)
                {
                    BlockType = type,
                    ToolCallId = NonEmpty(GoogleInteractionsStreaming.String(step, "id")) ?? blockId,
                    ToolName = type == "mcp_server_tool_call" ? GoogleInteractionsStreaming.String(step, "name") ?? "mcp_server_tool" : Strip(type, "_call"),
                    Arguments = step!["arguments"] as JsonObject ?? new JsonObject(),
                };
                break;
            case { } type when BuiltinToolResultTypes.Contains(type):
                _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.BuiltinToolResult)
                {
                    BlockType = type,
                    ToolCallId = NonEmpty(GoogleInteractionsStreaming.String(step, "call_id")) ?? blockId,
                    ToolName = type == "mcp_server_tool_result" ? GoogleInteractionsStreaming.String(step, "name") ?? "mcp_server_tool" : Strip(type, "_result"),
                    Result = step!["result"]?.DeepClone(),
                };
                break;
            default:
                _openBlocks[index] = new OpenBlock(blockId, OpenBlockKind.Unknown);
                break;
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

        // The first text-shaped delta promotes a bare model output step to a text block.
        if (open.Kind == OpenBlockKind.PendingModelOutput && (type == "text" || type == "text_annotation" || type == "text_annotation_delta"))
        {
            open.Kind = OpenBlockKind.Text;
            parts.Add(new TextStartStreamPart(open.Id));
        }

        // Image and video deltas are not yet surfaced. FileStreamPart carries bytes and cannot hold a uri.
        if (type == "image" || type == "video")
        {
            return;
        }

        switch (open.Kind)
        {
            case OpenBlockKind.Custom when type == "processing_call" || type == "processing_result":
                if (GoogleInteractionsStreaming.String(delta, "signature") is { } signature)
                {
                    open.Google!["signature"] = signature;
                }

                if (type == "processing_call" && NonEmpty(GoogleInteractionsStreaming.String(delta, "id")) is { } processingId)
                {
                    open.Google!["processingId"] = processingId;
                }

                if (type == "processing_result" && NonEmpty(GoogleInteractionsStreaming.String(delta, "call_id")) is { } processingCallId)
                {
                    open.Google!["processingCallId"] = processingCallId;
                }

                break;
            case OpenBlockKind.Text when type == "text":
                var text = GoogleInteractionsStreaming.String(delta, "text") ?? string.Empty;
                if (text.Length > 0)
                {
                    parts.Add(new TextDeltaStreamPart(open.Id, text));
                }

                break;
            case OpenBlockKind.Text when type == "text_annotation" || type == "text_annotation_delta":
                AddSources(GoogleInteractionsSources.AnnotationsToSources(delta!["annotations"] as JsonArray, _generateId), parts);
                break;
            case OpenBlockKind.Reasoning when type == "thought_summary":
                var item = delta!["content"] as JsonObject;
                if (GoogleInteractionsStreaming.String(item, "type") == "text" && GoogleInteractionsStreaming.String(item, "text") is { } summary)
                {
                    parts.Add(new ReasoningDeltaStreamPart(open.Id, summary));
                }

                break;
            case OpenBlockKind.Reasoning when type == "thought_signature":
                open.Signature = GoogleInteractionsStreaming.String(delta, "signature") ?? open.Signature;
                break;
            case OpenBlockKind.FunctionCall when type == "arguments_delta":
                // The arguments arrive as slices of one JSON string. The full object is emitted at step.stop.
                var slice = delta!["arguments"] is JsonValue argument && argument.TryGetValue<string>(out var sliceText) ? sliceText : string.Empty;
                if (slice.Length > 0)
                {
                    open.ArgumentsAccum.Append(slice);
                    parts.Add(new ToolInputDeltaStreamPart(open.ToolCallId!, slice));
                }

                open.ToolCallId = NonEmpty(GoogleInteractionsStreaming.String(delta, "id")) ?? open.ToolCallId;
                open.Signature = GoogleInteractionsStreaming.String(delta, "signature") ?? open.Signature;
                _hasFunctionCall = true;
                break;
            case OpenBlockKind.BuiltinToolCall when type == open.BlockType:
                open.ToolCallId = NonEmpty(GoogleInteractionsStreaming.String(delta, "id")) ?? open.ToolCallId;
                open.Arguments = delta!["arguments"] as JsonObject ?? open.Arguments;
                if (open.BlockType == "mcp_server_tool_call")
                {
                    open.ToolName = GoogleInteractionsStreaming.String(delta, "name") ?? open.ToolName;
                }

                break;
            case OpenBlockKind.BuiltinToolResult when type == open.BlockType:
                open.ToolCallId = NonEmpty(GoogleInteractionsStreaming.String(delta, "call_id")) ?? open.ToolCallId;
                if (delta!.ContainsKey("result"))
                {
                    open.Result = delta["result"]?.DeepClone();
                }

                if (open.BlockType == "mcp_server_tool_result")
                {
                    open.ToolName = GoogleInteractionsStreaming.String(delta, "name") ?? open.ToolName;
                }

                break;
        }
    }

    private void StepStop(JsonObject value, List<LanguageModelStreamPart> parts)
    {
        if (Index(value) is not { } index || !_openBlocks.TryGetValue(index, out var open))
        {
            return;
        }

        switch (open.Kind)
        {
            case OpenBlockKind.Text:
                parts.Add(new TextEndStreamPart(open.Id, null, GoogleMetadata()));
                break;
            case OpenBlockKind.Reasoning:
                parts.Add(new ReasoningEndStreamPart(open.Id, GoogleMetadata(open.Signature)));
                break;
            case OpenBlockKind.FunctionCall:
                parts.Add(new ToolInputEndStreamPart(open.ToolCallId!));
                var arguments = open.ArgumentsAccum.Length > 0 ? open.ArgumentsAccum.ToString() : "{}";
                parts.Add(new ToolCallStreamPart(open.ToolCallId!, open.ToolName!, arguments, GoogleMetadata(open.Signature)));
                break;
            case OpenBlockKind.Custom:
                parts.Add(new CustomStreamPart(open.CustomKind!, JsonSerializer.SerializeToElement(new JsonObject { ["google"] = open.Google!.DeepClone() })));
                break;
            case OpenBlockKind.BuiltinToolCall:
                parts.Add(new ToolCallStreamPart(open.ToolCallId!, open.ToolName!, (open.Arguments ?? new JsonObject()).ToJsonString()) { ProviderExecuted = true });
                break;
            case OpenBlockKind.BuiltinToolResult:
                parts.Add(new ToolResultStreamPart(open.ToolCallId!, open.ToolName!, JsonSerializer.SerializeToElement(open.Result)));
                var block = new JsonObject { ["type"] = open.BlockType, ["call_id"] = open.ToolCallId, ["result"] = open.Result?.DeepClone() };
                AddSources(GoogleInteractionsSources.BuiltinToolResultToSources(block, _generateId), parts);
                break;
        }

        _openBlocks.Remove(index);
    }

    private void AddSources(IReadOnlyList<LanguageModelStreamPart> sources, List<LanguageModelStreamPart> parts)
    {
        foreach (var source in sources)
        {
            if (_emittedSourceKeys.Add(GoogleInteractionsSources.Key(source)))
            {
                parts.Add(source);
            }
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

    // The API sends an empty id for a step it did not name. Upstream falls back to the block id for an empty id too.
    private static string? NonEmpty(string? value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static string Strip(string value, string suffix)
    {
        return value.EndsWith(suffix, StringComparison.Ordinal) ? value.Substring(0, value.Length - suffix.Length) : value;
    }

    private enum OpenBlockKind
    {
        Unknown,
        PendingModelOutput,
        Text,
        Reasoning,
        FunctionCall,
        BuiltinToolCall,
        BuiltinToolResult,
        Custom,
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

        internal string? Signature { get; set; }

        internal string? ToolCallId { get; set; }

        internal string? ToolName { get; set; }

        internal StringBuilder ArgumentsAccum { get; } = new();

        internal string? BlockType { get; init; }

        internal JsonObject? Arguments { get; set; }

        internal JsonNode? Result { get; set; }

        internal string? CustomKind { get; init; }

        internal JsonObject? Google { get; init; }
    }
}
