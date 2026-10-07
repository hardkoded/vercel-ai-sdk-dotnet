// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini language model. Posts to <c>models/{id}:generateContent</c>.</summary>
public sealed class GoogleLanguageModel : ILanguageModel
{
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
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            Url(":generateContent"),
            GoogleJson.Write(prepared.Body),
            await HeadersAsync(options, prepared, cancellationToken).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var result = GoogleResponse.Parse(document.RootElement, Context(prepared));
        return new LanguageModelGenerateResult(
            result.Content,
            result.FinishReason,
            result.Usage,
            result.RawFinishReason,
            result.Warnings,
            result.ResponseId,
            result.ProviderMetadata,
            response.Body,
            null,
            null,
            response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var prepared = Prepare(options, true);
        yield return new StreamStartStreamPart(Warnings(prepared));
        string? responseId = null;
        var emittedMetadata = false;
        var textId = (string?)null;
        var reasoningId = (string?)null;
        var block = 0;
        FinishReason finish = FinishReason.Other;
        string? rawFinish = null;
        var usage = LanguageModelUsage.Empty;
        JsonElement? metadata = null;
        var clientTool = false;
        string? confirmedBlock = null;
        var seenSources = new HashSet<string>(StringComparer.Ordinal);
        var callContext = Context(prepared);
        var activeCalls = new List<StreamingCall>();

        var headers = await HeadersAsync(options, prepared, cancellationToken).ConfigureAwait(false);
        await foreach (var data in _provider.Http.SendSseAsync(Url(":streamGenerateContent?alt=sse"), GoogleJson.Write(prepared.Body), headers, cancellationToken).ConfigureAwait(false))
        {
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
                if (!emittedMetadata && root.TryGetProperty("responseId", out var id) && id.ValueKind == JsonValueKind.String)
                {
                    responseId = id.GetString();
                    emittedMetadata = true;
                    yield return new ResponseMetadataStreamPart(responseId, ModelId, _provider.Clock());
                }

                if (root.TryGetProperty("usageMetadata", out var usageElement))
                {
                    usage = GoogleUsage.Convert(usageElement)?.ToLanguageModelUsage() ?? usage;
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
                    metadata = GoogleResponse.Parse(root, Context(prepared)).ProviderMetadata ?? metadata;
                    continue;
                }

                var parsed = GoogleResponse.Parse(root, Context(prepared));
                metadata = parsed.ProviderMetadata ?? metadata;
                if (!string.IsNullOrEmpty(parsed.RawFinishReason))
                {
                    rawFinish = parsed.RawFinishReason;
                    finish = parsed.FinishReason;
                }

                if (candidates[0].TryGetProperty("groundingMetadata", out var grounding))
                {
                    foreach (var source in GoogleResponse.Sources(grounding, Context(prepared)))
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
                            block++;
                            reasoningId = "reasoning-" + block;
                            yield return new ReasoningStartStreamPart(reasoningId);
                        }

                        yield return new ReasoningDeltaStreamPart(reasoningId, reasoning.Text);
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
                            block++;
                            textId = "text-" + block;
                            yield return new TextStartStreamPart(textId);
                        }

                        yield return new TextDeltaStreamPart(textId, text.Text);
                    }
                    else if (generated is GoogleToolCall tool)
                    {
                        // Provider-executed calls. Client function calls stream below, after the rest of the chunk.
                        yield return new ToolCallStreamPart(tool.ToolCallId, tool.ToolName, tool.ArgumentsJson, tool.ProviderMetadata);
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

        yield return new FinishStreamPart(finish, usage, rawFinish, metadata);
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
