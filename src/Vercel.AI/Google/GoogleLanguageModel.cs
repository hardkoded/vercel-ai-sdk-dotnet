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
            Headers(options, prepared),
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
        var accumulator = (GoogleJsonAccumulator?)null;
        string? streamingCallId = null;
        string? streamingCallName = null;
        JsonElement? streamingMetadata = null;

        await foreach (var data in _provider.Http.SendSseAsync(Url(":streamGenerateContent?alt=sse"), GoogleJson.Write(prepared.Body), Headers(options, prepared), cancellationToken).ConfigureAwait(false))
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

                if (candidates[0].TryGetProperty("content", out var body) && body.TryGetProperty("parts", out var parts))
                {
                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.TryGetProperty("partialArgs", out var partial) && partial.ValueKind == JsonValueKind.Array)
                        {
                            accumulator ??= new GoogleJsonAccumulator();
                            var updates = new List<GooglePartialArgument>();
                            foreach (var item in partial.EnumerateArray())
                            {
                                updates.Add(ReadPartial(item));
                            }

                            var update = accumulator.Process(updates);
                            if (streamingCallId == null)
                            {
                                streamingCallId = contextId(part);
                                streamingCallName = GoogleJson.String(part, "name") ?? "tool";
                            }

                            if (update.TextDelta.Length > 0)
                            {
                                yield return new ToolInputDeltaStreamPart(streamingCallId, update.TextDelta);
                            }

                            continue;
                        }

                        if (accumulator != null && streamingCallId != null)
                        {
                            var final = accumulator.Finalize();
                            if (final.ClosingDelta.Length > 0)
                            {
                                yield return new ToolInputDeltaStreamPart(streamingCallId, final.ClosingDelta);
                            }

                            yield return new ToolCallStreamPart(streamingCallId, streamingCallName ?? "tool", final.FinalJson, streamingMetadata);
                            clientTool = true;
                            accumulator = null;
                            streamingCallId = null;
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
                    else if (generated is GeneratedToolCall tool)
                    {
                        var executed = tool is GoogleToolCall google && google.ProviderExecuted;
                        if (!executed)
                        {
                            clientTool = true;
                        }

                        yield return new ToolCallStreamPart(tool.ToolCallId, tool.ToolName, tool.ArgumentsJson, tool.ProviderMetadata);
                    }
                    else if (generated is GoogleGeneratedFile file)
                    {
                        yield return new FileStreamPart(file.Data, file.MediaType);
                    }
                }
            }
        }

        if (accumulator != null && streamingCallId != null)
        {
            var final = accumulator.Finalize();
            if (final.ClosingDelta.Length > 0)
            {
                yield return new ToolInputDeltaStreamPart(streamingCallId, final.ClosingDelta);
            }

            yield return new ToolCallStreamPart(streamingCallId, streamingCallName ?? "tool", final.FinalJson, streamingMetadata);
            clientTool = true;
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

    private Dictionary<string, string?> Headers(LanguageModelCallOptions? options, GooglePreparedRequest prepared)
    {
        var headers = _provider.Headers();
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

    private string contextId(JsonElement part)
    {
        return GoogleJson.String(part, "id") ?? (_provider.Options.GenerateId?.Invoke() ?? "call");
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

/// <summary>A streamed tool-argument fragment.</summary>
public sealed class ToolInputDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input delta.</summary>
    public ToolInputDeltaStreamPart(string id, string delta)
        : base("tool-input-delta")
    {
        Id = id ?? string.Empty;
        Delta = delta ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>JSON fragment.</summary>
    public string Delta { get; }
}
