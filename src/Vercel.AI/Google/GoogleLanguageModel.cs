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

/// <summary>Gemini language model. Posts to <c>models/{id}:generateContent</c>.</summary>
public sealed class GoogleLanguageModel : ILanguageModel
{
    /// <summary>The most requests that follow a CONTINUATION response. A model that never stops continuing would loop forever.</summary>
    internal const int MaxContinuationHops = 64;

    private static readonly string ContinuationLimitMessage = "The Gemini model kept answering with CONTINUATION after " + MaxContinuationHops + " continuation requests. Giving up.";

    private readonly GoogleProvider _provider;

    /// <summary>Creates a model.</summary>
    public GoogleLanguageModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "V4"; }
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _provider.ModelProvider; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Builds the generateContent body without sending it.</summary>
    public GooglePreparedRequest Prepare(LanguageModelCallOptions options, bool streaming)
    {
        return GoogleRequest.Prepare(ModelId, Provider, options, streaming);
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(options ?? new LanguageModelCallOptions(), false);
        var headers = await HeadersAsync(options, prepared, cancellationToken).ConfigureAwait(false);
        var context = Context(prepared);
        var response = await PostAsync(prepared.Body, headers, cancellationToken).ConfigureAwait(false);

        // Gemini 4 long decode: a request that reaches the per-request cap while maxOutputTokens budget remains finishes
        // with CONTINUATION. Resend the same request with the continuation token and combine the content of all requests.
        // Usage is summed across requests. The first request's prompt token count is kept for context window checks.
        var content = new List<GeneratedContent>();
        JsonElement? usage = null;
        int? firstPromptTokenCount = null;
        var isContinued = false;
        var hops = 0;
        string? candidateToken;
        JsonNode? last;
        while (true)
        {
            last = JsonNode.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
            using var document = JsonDocument.Parse(last?.ToJsonString() ?? "{}");
            var root = document.RootElement;
            var candidate = FirstCandidate(root);
            candidateToken = candidate is { } found ? GoogleJson.String(found, "continuationToken") : null;
            var hopUsage = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("usageMetadata", out var usageValue) ? usageValue.Clone() : (JsonElement?)null;
            if (!isContinued && hopUsage is { ValueKind: JsonValueKind.Object } first && first.TryGetProperty("promptTokenCount", out var promptCount) && promptCount.TryGetInt32(out var promptTokens))
            {
                firstPromptTokenCount = promptTokens;
            }

            usage = GoogleUsage.Add(usage, hopUsage);
            var next = candidate is { } hop && GoogleJson.String(hop, "finishReason") == "CONTINUATION" ? candidateToken : null;
            if (next == null)
            {
                break;
            }

            if (hops == MaxContinuationHops)
            {
                throw new AiSdkException(ContinuationLimitMessage);
            }

            hops++;
            content.AddRange(GoogleResponse.Parse(root, context).Content);
            isContinued = true;
            var body = (JsonObject)prepared.Body.DeepClone();
            body["continuationToken"] = next;
            response = await PostAsync(body, headers, cancellationToken).ConfigureAwait(false);
        }

        if (isContinued && usage is { } summed && last is JsonObject lastObject)
        {
            lastObject["usageMetadata"] = JsonNode.Parse(summed.GetRawText());
        }

        using var lastDocument = JsonDocument.Parse(last?.ToJsonString() ?? "{}");
        var result = GoogleResponse.Parse(lastDocument.RootElement, context);
        content.AddRange(result.Content);
        var metadata = GoogleResponse.WithLongDecode(result.ProviderMetadata, candidateToken, isContinued ? firstPromptTokenCount : null, null);
        return new LanguageModelGenerateResult(
            content,
            result.FinishReason,
            result.Usage,
            result.RawFinishReason,
            result.Warnings,
            result.ResponseId,
            metadata,
            response.Body,
            null,
            null,
            response.Headers);
    }

    private async Task<ProviderTextResponse> PostAsync(JsonObject body, Dictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        return await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            Url(":generateContent"),
            GoogleJson.Write(body),
            headers,
            cancellationToken).ConfigureAwait(false);
    }

    private static JsonElement? FirstCandidate(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("candidates", out var candidates)
            && candidates.ValueKind == JsonValueKind.Array
            && candidates.GetArrayLength() > 0
            ? candidates[0]
            : null;
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var prepared = Prepare(options, true);
        var headers = await HeadersAsync(options, prepared, cancellationToken).ConfigureAwait(false);
        var json = GoogleJson.Write(prepared.Body);
        var sse = await _provider.Http.OpenSseResponseAsync(Url(":streamGenerateContent?alt=sse"), json, headers, cancellationToken).ConfigureAwait(false);
        var stream = StreamAsync(options, prepared, headers, sse.Events, cancellationToken);
        if (prepared.JsonResponseToolName != null)
        {
            stream = JsonResponseTools.ConvertJsonResponseToolStream(stream, prepared.JsonResponseToolName, cancellationToken);
        }

        return sse.ToStreamResult(stream, json);
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> StreamAsync(
        LanguageModelCallOptions options,
        GooglePreparedRequest prepared,
        Dictionary<string, string?> headers,
        IAsyncEnumerable<string> firstEvents,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new StreamStartStreamPart(Warnings(prepared));
        string? responseId = null;
        var emittedMetadata = false;
        var textId = (string?)null;
        var reasoningId = (string?)null;
        var block = 0;
        FinishReason finish = FinishReason.Other;
        string? rawFinish = null;
        JsonElement? usageRaw = null;
        // Usage of the completed requests of an automatically continued response.
        JsonElement? continuedUsage = null;
        // Prompt token count of the first request, for context window checks.
        int? initialPromptTokenCount = null;
        string? pendingToken = null;
        string? lastToken = null;
        JsonElement? metadata = null;
        var clientTool = false;
        string? confirmedBlock = null;
        var seenSources = new HashSet<string>(StringComparer.Ordinal);
        var callContext = StreamContext(prepared);
        var activeCalls = new List<StreamingCall>();

        var hop = 0;
        while (true)
        {
            var requestBody = prepared.Body;
            if (pendingToken != null)
            {
                // Resend the same request with the continuation token. The new stream carries only newly generated content.
                requestBody = (JsonObject)prepared.Body.DeepClone();
                requestBody["continuationToken"] = pendingToken;
                pendingToken = null;
            }

            Exception? failure = null;
            // The first request is already open. A continuation sends a new request.
            var enumerator = (hop == 0 ? firstEvents : _provider.Http.SendSseAsync(Url(":streamGenerateContent?alt=sse"), GoogleJson.Write(requestBody), headers, cancellationToken))
                .GetAsyncEnumerator(cancellationToken);
            try
            {
                while (true)
                {
                    bool moved;
                    try
                    {
                        moved = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    }
                    catch (Exception error) when (hop > 0 && error is not OperationCanceledException)
                    {
                        failure = error;
                        break;
                    }

                    if (!moved)
                    {
                        break;
                    }

                    var data = enumerator.Current;
                    if (options.IncludeRawChunks)
                    {
                        yield return new RawStreamPart(data);
                    }

                    JsonDocument? document = null;
                    var invalidJson = false;
                    try
                    {
                        document = JsonDocument.Parse(data);
                    }
                    catch (JsonException)
                    {
                        invalidJson = true;
                    }

                    if (invalidJson || document == null)
                    {
                        yield return new ErrorStreamPart("The Gemini stream chunk was not valid JSON.");
                        continue;
                    }

                    using (document)
                    {
                        var root = document.RootElement;
                        if (root.TryGetProperty("error", out var streamError) && streamError.ValueKind == JsonValueKind.Object)
                        {
                            finish = FinishReason.Error;
                            rawFinish = null;
                            yield return new ErrorStreamPart(CreateStreamError(streamError));
                            continue;
                        }

                        if (!emittedMetadata && root.TryGetProperty("responseId", out var id) && id.ValueKind == JsonValueKind.String)
                        {
                            responseId = id.GetString();
                            emittedMetadata = true;
                            yield return new ResponseMetadataStreamPart(responseId, ModelId, _provider.Clock());
                        }

                        if (root.TryGetProperty("usageMetadata", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
                        {
                            usageRaw = usageElement.Clone();
                        }

                        if (root.TryGetProperty("promptFeedback", out var feedback) && confirmedBlock == null)
                        {
                            var reason = GoogleJson.String(feedback, "blockReason");
                            if (GoogleFinishReason.IsConfirmedPromptBlock(reason))
                            {
                                confirmedBlock = reason;
                                finish = FinishReason.ContentFilter;
                                rawFinish = reason;
                            }
                        }

                        if (confirmedBlock != null || !root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                        {
                            metadata = GoogleResponse.Parse(root, StreamContext(prepared)).ProviderMetadata ?? metadata;
                            continue;
                        }

                        var parsed = GoogleResponse.Parse(root, StreamContext(prepared));
                        metadata = parsed.ProviderMetadata ?? metadata;
                        if (!string.IsNullOrEmpty(parsed.RawFinishReason))
                        {
                            rawFinish = parsed.RawFinishReason;
                            finish = parsed.FinishReason;
                        }

                        var chunkToken = GoogleJson.String(candidates[0], "continuationToken");
                        if (chunkToken != null)
                        {
                            lastToken = chunkToken;
                        }

                        if (GoogleJson.String(candidates[0], "finishReason") == "CONTINUATION")
                        {
                            // Usage metadata is per request. The next request reports its own usage, so bank the usage of this one.
                            if (continuedUsage == null && usageRaw is { ValueKind: JsonValueKind.Object } firstUsage && firstUsage.TryGetProperty("promptTokenCount", out var promptCount) && promptCount.TryGetInt32(out var promptTokens))
                            {
                                initialPromptTokenCount = promptTokens;
                            }

                            continuedUsage = GoogleUsage.Add(continuedUsage, usageRaw);
                            usageRaw = null;
                            pendingToken = chunkToken;
                        }

                        if (candidates[0].TryGetProperty("groundingMetadata", out var grounding))
                        {
                            foreach (var source in GoogleResponse.Sources(grounding, StreamContext(prepared)))
                            {
                                if (seenSources.Add(source.Url))
                                {
                                    yield return new SourceStreamPart(source.Id, source.Url, source.Title, source.ProviderMetadata);
                                }
                            }
                        }

                        foreach (var generated in parsed.Content)
                        {
                            if (generated is GoogleReasoning reasoning)
                            {
                                if (textId != null)
                                {
                                    yield return new TextEndStreamPart(textId);
                                    textId = null;
                                }

                                if (reasoningId == null)
                                {
                                    reasoningId = (block++).ToString(CultureInfo.InvariantCulture);
                                    yield return new ReasoningStartStreamPart(reasoningId, reasoning.ProviderMetadata);
                                }

                                yield return new ReasoningDeltaStreamPart(reasoningId, reasoning.Text, reasoning.ProviderMetadata);
                            }
                            else if (generated is GeneratedText text)
                            {
                                if (reasoningId != null)
                                {
                                    yield return new ReasoningEndStreamPart(reasoningId);
                                    reasoningId = null;
                                }

                                if (textId == null)
                                {
                                    textId = (block++).ToString(CultureInfo.InvariantCulture);
                                    yield return new TextStartStreamPart(textId, text.ProviderMetadata);
                                }

                                yield return new TextDeltaStreamPart(textId, text.Text, text.ProviderMetadata);
                            }
                            else if (generated is GoogleToolCall tool)
                            {
                                // Provider-executed calls. Client function calls stream below, after the rest of the chunk.
                                yield return new ToolCallStreamPart(tool.ToolCallId, tool.ToolName, tool.ArgumentsJson, tool.ProviderMetadata) { ProviderExecuted = tool.ProviderExecuted };
                            }
                            else if (generated is GoogleGeneratedFile file)
                            {
                                yield return new FileStreamPart(file.Data, file.MediaType);
                            }
                        }

                        if (candidates[0].TryGetProperty("content", out var body) && body.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var toolPart in FunctionCallParts(parts, callContext, activeCalls))
                            {
                                clientTool |= toolPart is ToolCallStreamPart;
                                yield return toolPart;
                            }
                        }
                    }
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            if (failure != null)
            {
                // Surfaced as an error part, followed by the finish part with the last continuation token so the caller can resume.
                yield return new ErrorStreamPart(failure.Message);
                break;
            }

            if (pendingToken == null)
            {
                break;
            }

            if (hop == MaxContinuationHops)
            {
                // Same shape as a failed continuation: an error part, then the finish part with the last token so the caller can resume.
                finish = FinishReason.Error;
                yield return new ErrorStreamPart(ContinuationLimitMessage);
                break;
            }

            hop++;
        }

        if (textId != null)
        {
            yield return new TextEndStreamPart(textId);
        }

        if (reasoningId != null)
        {
            yield return new ReasoningEndStreamPart(reasoningId);
        }

        if (clientTool && finish == FinishReason.Stop)
        {
            finish = FinishReason.ToolCalls;
        }

        var totalUsage = GoogleUsage.Add(continuedUsage, usageRaw);
        var usage = GoogleUsage.Convert(totalUsage)?.ToLanguageModelUsage() ?? LanguageModelUsage.Empty;
        // Only surface the token when the response stopped early (budget used up, or continuing failed), not for the
        // checkpoint tokens of a stream that finished normally.
        var exposeToken = lastToken != null && (rawFinish == "MAX_TOKENS" || rawFinish == "CONTINUATION");
        metadata = GoogleResponse.WithLongDecode(metadata, exposeToken ? lastToken : null, initialPromptTokenCount, continuedUsage != null ? totalUsage : null);
        yield return new FinishStreamPart(finish, usage, rawFinish, metadata);
    }

    private static ProviderStreamError CreateStreamError(JsonElement error)
    {
        int? code = error.TryGetProperty("code", out var codeElement) && codeElement.ValueKind == JsonValueKind.Number && codeElement.TryGetInt32(out var number)
            ? number
            : null;
        var retryable = code is 408 or 409 or 429 or >= 500;
        return new ProviderStreamError(
            GoogleJson.String(error, "message") ?? string.Empty,
            error.Clone(),
            GoogleJson.String(error, "status"),
            code,
            code,
            retryable);
    }

    private Uri Url(string method)
    {
        var baseUrl = _provider.Options.BaseUrl ?? string.Empty;
        if (GoogleVertexEndpoints.IsEndpointModel(ModelId)
            && baseUrl.EndsWith("/publishers/google", StringComparison.Ordinal))
        {
            baseUrl = baseUrl.Substring(0, baseUrl.Length - "/publishers/google".Length);
        }

        return ApiKeys.Combine(baseUrl, GoogleModelPath.Get(ModelId) + method);
    }

    private async Task<Dictionary<string, string?>> HeadersAsync(LanguageModelCallOptions? options, GooglePreparedRequest prepared, CancellationToken cancellationToken)
    {
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        if (options?.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        foreach (var pair in prepared.ExtraHeaders)
        {
            headers[pair.Key] = pair.Value;
        }

        return headers;
    }

    private GoogleParseContext Context(GooglePreparedRequest prepared)
    {
        return new GoogleParseContext(Provider, _provider.Options.GenerateId, prepared.Tools, prepared.Warnings, prepared.JsonResponseToolName);
    }

    // The stream rewrites the response tool once, from its tool input parts. Parsing chunks with the tool name would also
    // turn the call into text parts here and close an open reasoning block early.
    private GoogleParseContext StreamContext(GooglePreparedRequest prepared)
    {
        return new GoogleParseContext(Provider, _provider.Options.GenerateId, prepared.Tools, prepared.Warnings);
    }

    private static IReadOnlyList<CallWarning> Warnings(GooglePreparedRequest prepared)
    {
        var warnings = new List<CallWarning>(prepared.Warnings.Count);
        foreach (var warning in prepared.Warnings)
        {
            warnings.Add(warning.ToCallWarning());
        }

        return warnings;
    }

    // Gemini sends a function call whole, without arguments, or streamed: a named
    // chunk with willContinue, then partialArgs chunks, then an empty terminal chunk.
    private static IEnumerable<LanguageModelStreamPart> FunctionCallParts(JsonElement parts, GoogleParseContext context, List<StreamingCall> active)
    {
        foreach (var part in parts.EnumerateArray())
        {
            if (!part.TryGetProperty("functionCall", out var call) || call.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var metadata = GoogleResponse.SignatureMetadata(part, context);
            var name = GoogleJson.String(call, "name");
            var hasArgs = call.TryGetProperty("args", out var args) && args.ValueKind != JsonValueKind.Null;
            var hasPartial = call.TryGetProperty("partialArgs", out var partial) && partial.ValueKind == JsonValueKind.Array;
            bool? willContinue = call.TryGetProperty("willContinue", out var cont) && (cont.ValueKind == JsonValueKind.True || cont.ValueKind == JsonValueKind.False)
                ? cont.GetBoolean()
                : null;

            if (hasPartial || (name != null && willContinue == true))
            {
                StreamingCall? current = null;
                if (name != null)
                {
                    current = new StreamingCall(CallId(call, context), name, metadata);
                    active.Add(current);
                    yield return new ToolInputStartStreamPart(current.Id, name, metadata);
                }
                else if (active.Count > 0)
                {
                    current = active[active.Count - 1];
                }

                if (hasPartial && current != null)
                {
                    var updates = new List<GooglePartialArgument>();
                    foreach (var item in partial.EnumerateArray())
                    {
                        updates.Add(ReadPartial(item));
                    }

                    var update = current.Accumulator.Process(updates);
                    if (update.TextDelta.Length > 0)
                    {
                        yield return new ToolInputDeltaStreamPart(current.Id, update.TextDelta, metadata);
                    }

                    if (willContinue != true && updates.All(argument => argument.WillContinue != true))
                    {
                        foreach (var finished in FinishCall(active))
                        {
                            yield return finished;
                        }
                    }
                }
            }
            else if (name == null && !hasArgs && willContinue == null && active.Count > 0)
            {
                foreach (var finished in FinishCall(active))
                {
                    yield return finished;
                }
            }
            else if (name != null && hasArgs)
            {
                var id = CallId(call, context);
                var input = args.ValueKind == JsonValueKind.String ? args.GetString() ?? string.Empty : args.GetRawText();
                yield return new ToolInputStartStreamPart(id, name, metadata);
                yield return new ToolInputDeltaStreamPart(id, input, metadata);
                yield return new ToolInputEndStreamPart(id, metadata);
                yield return new ToolCallStreamPart(id, name, input, metadata);
            }
            else if (name != null && willContinue != true)
            {
                var id = CallId(call, context);
                yield return new ToolInputStartStreamPart(id, name, metadata);
                yield return new ToolInputEndStreamPart(id, metadata);
                yield return new ToolCallStreamPart(id, name, "{}", metadata);
            }
        }
    }

    private static IEnumerable<LanguageModelStreamPart> FinishCall(List<StreamingCall> active)
    {
        var call = active[active.Count - 1];
        active.RemoveAt(active.Count - 1);
        var final = call.Accumulator.Finalize();
        if (final.ClosingDelta.Length > 0)
        {
            yield return new ToolInputDeltaStreamPart(call.Id, final.ClosingDelta, call.Metadata);
        }

        yield return new ToolInputEndStreamPart(call.Id, call.Metadata);
        yield return new ToolCallStreamPart(call.Id, call.Name, final.FinalJson, call.Metadata);
    }

    private static string CallId(JsonElement call, GoogleParseContext context)
    {
        var id = GoogleJson.String(call, "id");
        return string.IsNullOrEmpty(id) ? context.NextId() : id!;
    }

    private static GooglePartialArgument ReadPartial(JsonElement item)
    {
        var argument = new GooglePartialArgument { JsonPath = GoogleJson.String(item, "jsonPath") ?? string.Empty };
        if (item.TryGetProperty("stringValue", out var text) && text.ValueKind == JsonValueKind.String)
        {
            argument.HasString = true;
            argument.StringValue = text.GetString();
        }

        if (item.TryGetProperty("numberValue", out var number) && number.TryGetDouble(out var value))
        {
            argument.NumberValue = value;
        }

        if (item.TryGetProperty("boolValue", out var flag) && (flag.ValueKind == JsonValueKind.True || flag.ValueKind == JsonValueKind.False))
        {
            argument.BoolValue = flag.GetBoolean();
        }

        if (item.TryGetProperty("nullValue", out _))
        {
            argument.HasNull = true;
        }

        if (item.TryGetProperty("willContinue", out var cont) && (cont.ValueKind == JsonValueKind.True || cont.ValueKind == JsonValueKind.False))
        {
            argument.WillContinue = cont.GetBoolean();
        }

        return argument;
    }

    private sealed class StreamingCall
    {
        public StreamingCall(string id, string name, JsonElement? metadata)
        {
            Id = id;
            Name = name;
            Metadata = metadata;
        }

        public string Id { get; }

        public string Name { get; }

        public JsonElement? Metadata { get; }

        public GoogleJsonAccumulator Accumulator { get; } = new();
    }
}

/// <summary>A streamed file part. The shared stream model has no file delta, so this carries the bytes.</summary>
public sealed class FileStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a file part.</summary>
    public FileStreamPart(byte[] data, string mediaType)
        : base("file")
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = mediaType ?? "application/octet-stream";
    }

    /// <summary>File bytes.</summary>
    public byte[] Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }
}
