// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock settings.</summary>
public sealed class AmazonBedrockOptions
{
    /// <summary>AWS region.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Agent Runtime origin for rerank. When empty, the host is
    /// <c>https://bedrock-agent-runtime.{region}.{suffix}</c>.
    /// </summary>
    public string? AgentRuntimeBaseUrl { get; set; }

    /// <summary>Access key. Falls back to <c>AWS_ACCESS_KEY_ID</c>.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Secret key. Falls back to <c>AWS_SECRET_ACCESS_KEY</c>.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Session token. Falls back to <c>AWS_SESSION_TOKEN</c>.</summary>
    public string? SessionToken { get; set; }

    /// <summary>Clock used for signing. Tests can pin this.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }

    /// <summary>
    /// Headers sent with every request. Call headers override them.
    /// </summary>
    public Dictionary<string, string?> Headers { get; set; } = new();
}

/// <summary>Amazon Bedrock Converse provider.</summary>
public sealed class AmazonBedrockProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "amazon-bedrock";

    /// <summary>Creates a provider.</summary>
    public AmazonBedrockProvider(HttpClient httpClient, AmazonBedrockOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new AmazonBedrockOptions();
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Http = new ProviderHttp(HttpClient);
    }

    /// <summary>Options.</summary>
    public AmazonBedrockOptions Options { get; }

    /// <summary>HTTP client that receives the signed request.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>
    /// Adds the provider headers, then the call headers (which win; a null call value drops the header), then the
    /// <c>ai-sdk-amazon-bedrock</c> user agent. Call before signing.
    /// </summary>
    public void AddHeaders(HttpRequestMessage request, IEnumerable<KeyValuePair<string, string?>>? callHeaders)
    {
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in new IEnumerable<KeyValuePair<string, string?>>?[] { Options.Headers, callHeaders })
        {
            if (source == null)
            {
                continue;
            }

            foreach (var header in source)
            {
                if (header.Value != null)
                {
                    merged[header.Key] = header.Value;
                }
                else
                {
                    merged.Remove(header.Key);
                }
            }
        }

        merged["user-agent"] = merged.TryGetValue("user-agent", out var existing)
            ? existing + " " + AmazonBedrockFetch.UserAgent
            : AmazonBedrockFetch.UserAgent;
        foreach (var header in merged)
        {
            request.Headers.Remove(header.Key);
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    /// <summary>Creates a provider.</summary>
    public static AmazonBedrockProvider Create(AmazonBedrockOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AmazonBedrockProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId) => new AmazonBedrockLanguageModel(this, modelId);

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId) => new AmazonBedrockRerankingModel(this, modelId);

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId) => new AmazonBedrockEmbeddingModel(this, modelId);

    /// <summary>Creates an embedding model with <paramref name="settings"/>.</summary>
    public AmazonBedrockEmbeddingModel EmbeddingModel(string modelId, AmazonBedrockEmbeddingModelSettings? settings) => new AmazonBedrockEmbeddingModel(this, modelId, settings);

    internal Uri ConverseUri(string modelId, string operation = "converse")
    {
        if (!HostnameParts.IsValidHostnamePart(Options.Region))
        {
            throw new ArgumentException("An AWS region must be a single DNS label.", nameof(AmazonBedrockOptions.Region));
        }

        return new Uri("https://bedrock-runtime." + Options.Region + ".amazonaws.com/model/" + Uri.EscapeDataString(modelId) + "/" + operation);
    }
}

/// <summary>Bedrock Converse language model.</summary>
public sealed class AmazonBedrockLanguageModel : ILanguageModel
{
    private const string JsonToolName = "json";

    private readonly AmazonBedrockProvider _provider;

    /// <summary>Creates a model.</summary>
    public AmazonBedrockLanguageModel(AmazonBedrockProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AmazonBedrockProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var bodyObject = Build(options, out var warnings, out var usesJsonTool, out var usesJsonInstruction);
        var request = CreateRequest(_provider.ConverseUri(ModelId), bodyObject, options);
        var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw AmazonBedrockErrors.Create((int)response.StatusCode, text);
        }

        using var document = JsonDocument.Parse(text);
        return Parse(document.RootElement, warnings, CopyHeaders(response), usesJsonTool, usesJsonInstruction);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var bodyObject = Build(options, out var warnings, out var usesJsonTool, out var usesJsonInstruction);
        var request = CreateRequest(_provider.ConverseUri(ModelId, "converse-stream"), bodyObject, options);
        using var response = await _provider.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw AmazonBedrockErrors.Create((int)response.StatusCode, errorText);
        }

        var (headers, chunks) = await AmazonBedrockEventStreamResponseHandler
            .HandleAsync(response, AmazonBedrockStreamSchema.Validate, cancellationToken)
            .ConfigureAwait(false);

        var isMistral = AmazonBedrockToolIds.IsMistralModel(ModelId);
        var finishReason = FinishReason.Other;
        string? rawFinishReason = null;
        JsonElement? usage = null;
        JsonObject? metadataPayload = null;
        var isJsonResponseFromTool = false;
        string? stopSequence = null;
        var extractor = usesJsonInstruction ? new JsonObjectTextExtractor() : null;
        var blocks = new Dictionary<int, StreamBlock>();

        yield return new StreamStartStreamPart(warnings);
        DateTimeOffset? timestamp = null;
        if (headers.TryGetValue("date", out var date)
            && DateTimeOffset.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedDate))
        {
            timestamp = parsedDate;
        }

        headers.TryGetValue("x-amzn-requestid", out var requestId);
        yield return new ResponseMetadataStreamPart(requestId, ModelId, timestamp);

        await foreach (var chunk in chunks.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (options.IncludeRawChunks && chunk.Success)
            {
                yield return new RawStreamPart(chunk.RawValue.GetRawText());
            }

            if (!chunk.Success)
            {
                finishReason = FinishReason.Error;
                rawFinishReason = null;
                yield return new ErrorStreamPart(chunk.Error ?? "Amazon Bedrock stream chunk could not be parsed.");
                continue;
            }

            var value = chunk.Value;
            var errorType = StreamErrorTypes.FirstOrDefault(type => Present(value, type));
            if (errorType is not null)
            {
                finishReason = FinishReason.Error;
                rawFinishReason = null;
                yield return new ErrorStreamPart(CreateStreamError(errorType, value.GetProperty(errorType), chunk.RawValue));
                continue;
            }

            if (Present(value, "messageStop"))
            {
                var stop = value.GetProperty("messageStop");
                rawFinishReason = ReadString(stop, "stopReason");
                finishReason = MapFinishReason(rawFinishReason, isJsonResponseFromTool);
                stopSequence = null;
                if (stop.TryGetProperty("additionalModelResponseFields", out var extra)
                    && extra.ValueKind == JsonValueKind.Object
                    && extra.TryGetProperty("delta", out var delta)
                    && delta.ValueKind == JsonValueKind.Object)
                {
                    stopSequence = ReadString(delta, "stop_sequence");
                }
            }

            if (Present(value, "metadata"))
            {
                var metadata = value.GetProperty("metadata");
                if (Present(metadata, "usage"))
                {
                    usage = metadata.GetProperty("usage");
                }

                var payload = new JsonObject();
                if (Present(metadata, "usage"))
                {
                    var usageObject = new JsonObject();
                    var usageElement = metadata.GetProperty("usage");
                    if (Present(usageElement, "cacheWriteInputTokens"))
                    {
                        usageObject["cacheWriteInputTokens"] = JsonNode.Parse(usageElement.GetProperty("cacheWriteInputTokens").GetRawText());
                    }

                    if (Present(usageElement, "cacheDetails"))
                    {
                        usageObject["cacheDetails"] = JsonNode.Parse(usageElement.GetProperty("cacheDetails").GetRawText());
                    }

                    if (usageObject.Count > 0)
                    {
                        payload["usage"] = usageObject;
                    }
                }

                foreach (var name in new[] { "trace", "performanceConfig", "serviceTier" })
                {
                    if (Present(metadata, name))
                    {
                        payload[name] = JsonNode.Parse(metadata.GetProperty(name).GetRawText());
                    }
                }

                if (payload.Count > 0)
                {
                    metadataPayload = payload;
                }
            }

            if (Present(value, "contentBlockStart"))
            {
                var start = value.GetProperty("contentBlockStart");
                var hasToolUse = start.TryGetProperty("start", out var startBody)
                    && startBody.ValueKind == JsonValueKind.Object
                    && Present(startBody, "toolUse");
                if (!hasToolUse)
                {
                    var index = start.GetProperty("contentBlockIndex").GetInt32();
                    blocks[index] = new StreamBlock(StreamBlockKind.Text);
                    yield return new TextStartStreamPart(index.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            JsonElement? deltaBody = null;
            var deltaIndex = 0;
            if (Present(value, "contentBlockDelta"))
            {
                var blockDelta = value.GetProperty("contentBlockDelta");
                deltaIndex = blockDelta.GetProperty("contentBlockIndex").GetInt32();
                if (Present(blockDelta, "delta"))
                {
                    deltaBody = blockDelta.GetProperty("delta");
                }
            }

            var deltaId = deltaIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (deltaBody is { } textBody && textBody.TryGetProperty("text", out var textValue) && textValue.ValueKind == JsonValueKind.String && textValue.GetString()!.Length > 0)
            {
                if (!blocks.ContainsKey(deltaIndex))
                {
                    blocks[deltaIndex] = new StreamBlock(StreamBlockKind.Text);
                    yield return new TextStartStreamPart(deltaId);
                }

                var incoming = textValue.GetString()!;
                var textDelta = extractor is null ? incoming : extractor.Process(incoming);
                if (textDelta.Length > 0)
                {
                    yield return new TextDeltaStreamPart(deltaId, textDelta);
                }
            }

            if (Present(value, "contentBlockStop"))
            {
                var index = value.GetProperty("contentBlockStop").GetProperty("contentBlockIndex").GetInt32();
                var id = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (blocks.TryGetValue(index, out var block))
                {
                    if (block.Kind == StreamBlockKind.Reasoning)
                    {
                        yield return new ReasoningEndStreamPart(id, block.RedactedContent is null ? null : ReasoningMetadata("redactedContent", block.RedactedContent));
                    }
                    else if (block.Kind == StreamBlockKind.Text)
                    {
                        yield return new TextEndStreamPart(id);
                    }
                    else if (block.IsJsonResponseTool)
                    {
                        isJsonResponseFromTool = true;
                        yield return new TextStartStreamPart(id);
                        yield return new TextDeltaStreamPart(id, block.JsonText.ToString());
                        yield return new TextEndStreamPart(id);
                    }
                    else
                    {
                        yield return new ToolInputEndStreamPart(block.ToolCallId!);
                        var input = block.JsonText.Length == 0 ? "{}" : block.JsonText.ToString();
                        yield return new ToolCallStreamPart(block.ToolCallId!, block.ToolName!, input);
                    }

                    blocks.Remove(index);
                }
            }

            if (deltaBody is { } reasoningBody
                && reasoningBody.TryGetProperty("reasoningContent", out var reasoningContent)
                && reasoningContent.ValueKind == JsonValueKind.Object)
            {
                var reasoningText = ReadString(reasoningContent, "text");
                var signature = ReadString(reasoningContent, "signature");
                var data = ReadString(reasoningContent, "data");
                var redacted = ReadString(reasoningContent, "redactedContent");
                if (!string.IsNullOrEmpty(reasoningText))
                {
                    if (!blocks.ContainsKey(deltaIndex))
                    {
                        blocks[deltaIndex] = new StreamBlock(StreamBlockKind.Reasoning);
                        yield return new ReasoningStartStreamPart(deltaId);
                    }

                    yield return new ReasoningDeltaStreamPart(deltaId, reasoningText!);
                }
                else if (!string.IsNullOrEmpty(signature) || !string.IsNullOrEmpty(data))
                {
                    if (!blocks.ContainsKey(deltaIndex))
                    {
                        blocks[deltaIndex] = new StreamBlock(StreamBlockKind.Reasoning);
                        yield return new ReasoningStartStreamPart(deltaId);
                    }

                    yield return string.IsNullOrEmpty(signature)
                        ? new ReasoningDeltaStreamPart(deltaId, string.Empty, ReasoningMetadata("redactedData", data!))
                        : new ReasoningDeltaStreamPart(deltaId, string.Empty, ReasoningMetadata("signature", signature!));
                }
                else if (!string.IsNullOrEmpty(redacted))
                {
                    if (!blocks.TryGetValue(deltaIndex, out var existing))
                    {
                        existing = new StreamBlock(StreamBlockKind.Reasoning);
                        blocks[deltaIndex] = existing;
                        yield return new ReasoningStartStreamPart(deltaId);
                    }

                    // Accumulated and attached once on reasoning-end: the merged provider metadata of a
                    // reasoning part is last-write-wins, so per-delta metadata would drop earlier chunks.
                    if (existing.Kind == StreamBlockKind.Reasoning)
                    {
                        existing.RedactedContent = (existing.RedactedContent ?? string.Empty) + redacted;
                    }
                }
            }

            if (Present(value, "contentBlockStart"))
            {
                var start = value.GetProperty("contentBlockStart");
                if (start.TryGetProperty("start", out var startBody)
                    && startBody.ValueKind == JsonValueKind.Object
                    && Present(startBody, "toolUse"))
                {
                    var toolUse = startBody.GetProperty("toolUse");
                    var index = start.GetProperty("contentBlockIndex").GetInt32();
                    var name = ReadString(toolUse, "name")!;
                    var isJsonResponseTool = usesJsonTool && string.Equals(name, JsonToolName, StringComparison.Ordinal);
                    var toolCallId = AmazonBedrockToolIds.Normalize(ReadString(toolUse, "toolUseId")!, isMistral);
                    blocks[index] = new StreamBlock(StreamBlockKind.ToolCall)
                    {
                        ToolCallId = toolCallId,
                        ToolName = name,
                        IsJsonResponseTool = isJsonResponseTool,
                    };

                    // The json response tool is emitted as text, so it has no tool events.
                    if (!isJsonResponseTool)
                    {
                        yield return new ToolInputStartStreamPart(toolCallId, name);
                    }
                }
            }

            if (deltaBody is { } toolBody
                && toolBody.TryGetProperty("toolUse", out var toolDelta)
                && toolDelta.ValueKind == JsonValueKind.Object
                && blocks.TryGetValue(deltaIndex, out var toolBlock)
                && toolBlock.Kind == StreamBlockKind.ToolCall)
            {
                var input = ReadString(toolDelta, "input") ?? string.Empty;
                if (!toolBlock.IsJsonResponseTool)
                {
                    yield return new ToolInputDeltaStreamPart(toolBlock.ToolCallId!, input);
                }

                toolBlock.JsonText.Append(input);
            }
        }

        if (isJsonResponseFromTool || stopSequence is not null)
        {
            metadataPayload ??= new JsonObject();
            if (isJsonResponseFromTool)
            {
                metadataPayload["isJsonResponseFromTool"] = true;
            }

            metadataPayload["stopSequence"] = stopSequence;
        }

        JsonElement? providerMetadata = null;
        if (metadataPayload is not null)
        {
            var wrapper = new JsonObject { ["amazonBedrock"] = metadataPayload, ["bedrock"] = metadataPayload.DeepClone() };
            using var metadataDocument = JsonDocument.Parse(wrapper.ToJsonString());
            providerMetadata = metadataDocument.RootElement.Clone();
        }

        yield return new FinishStreamPart(finishReason, AmazonBedrockUsage.Convert(usage), rawFinishReason, providerMetadata);
    }

    private static readonly string[] StreamErrorTypes =
    {
        "internalServerException",
        "modelStreamErrorException",
        "serviceUnavailableException",
        "throttlingException",
        "validationException",
    };

    private enum StreamBlockKind
    {
        Text,
        Reasoning,
        ToolCall,
    }

    private sealed class StreamBlock
    {
        public StreamBlock(StreamBlockKind kind)
        {
            Kind = kind;
        }

        public StreamBlockKind Kind { get; }

        public string? ToolCallId { get; init; }

        public string? ToolName { get; init; }

        public bool IsJsonResponseTool { get; init; }

        public StringBuilder JsonText { get; } = new StringBuilder();

        public string? RedactedContent { get; set; }
    }

    private static bool Present(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            && value.ValueKind != JsonValueKind.Undefined;
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }

    private static JsonElement ReasoningMetadata(string key, string value)
    {
        var payload = new JsonObject { [key] = value };
        var wrapper = new JsonObject { ["amazonBedrock"] = payload, ["bedrock"] = payload.DeepClone() };
        using var document = JsonDocument.Parse(wrapper.ToJsonString());
        return document.RootElement.Clone();
    }

    private static Vercel.AI.Prompt.ProviderStreamError CreateStreamError(string type, JsonElement error, JsonElement data)
    {
        var message = ReadString(error, "message") ?? "Amazon Bedrock stream failed with " + type;
        var (status, retryable) = AmazonBedrockStreamError.Metadata(type);
        return new Vercel.AI.Prompt.ProviderStreamError(message, data, type, null, status, retryable);
    }

    private static FinishReason MapFinishReason(string? raw, bool isJsonResponseFromTool)
    {
        if (isJsonResponseFromTool && string.Equals(raw, "tool_use", StringComparison.Ordinal))
        {
            return FinishReason.Stop;
        }

        return string.Equals(raw, "guardrail_intervened", StringComparison.Ordinal)
            ? FinishReason.ContentFilter
            : FinishReasons.Parse(raw);
    }

    private HttpRequestMessage CreateRequest(Uri uri, JsonObject bodyObject, LanguageModelCallOptions options)
    {
        var body = bodyObject.ToJsonString();
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        _provider.AddHeaders(request, options.Headers);
        Sign(request, Encoding.UTF8.GetBytes(body));
        return request;
    }

    private void Sign(HttpRequestMessage request, byte[] payload)
    {
        var accessKey = _provider.Options.AccessKeyId ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secret = _provider.Options.SecretAccessKey ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secret))
        {
            throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
        }

        var token = _provider.Options.SessionToken ?? Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
        AwsSigV4.Sign(request, payload, _provider.Options.Region, "bedrock", accessKey!, secret!, token, _provider.Options.UtcNow?.Invoke() ?? DateTimeOffset.UtcNow);
    }

    private JsonObject Build(LanguageModelCallOptions options, out List<CallWarning> warnings, out bool usesJsonTool, out bool usesJsonInstruction)
    {
        warnings = new List<CallWarning>();
        var bedrock = ReadBedrockOptions(options.ProviderOptions);
        var reasoningConfig = BedrockReasoningConfig.Read(bedrock);
        var additional = ReadAdditionalModelRequestFields(bedrock);
        var capabilities = AnthropicModelCapabilities.Get(ModelId);
        var isAnthropicModel = AmazonBedrockModelSupport.IsAnthropicModel(ModelId, reasoningConfig?.BudgetTokens);
        var temperature = options.Temperature;
        var topP = options.TopP;
        var topK = options.TopK;
        var stopSequences = options.StopSequences;

        if (capabilities.RejectsSamplingParameters)
        {
            if (temperature != null)
            {
                temperature = null;
                warnings.Add(new CallWarning("unsupported", "temperature is not supported by " + ModelId + " and will be ignored"));
            }

            if (topK != null)
            {
                topK = null;
                warnings.Add(new CallWarning("unsupported", "topK is not supported by " + ModelId + " and will be ignored"));
            }

            if (topP != null)
            {
                topP = null;
                warnings.Add(new CallWarning("unsupported", "topP is not supported by " + ModelId + " and will be ignored"));
            }
        }

        var openAiMatch = Regex.Match(ModelId, @"^(?:[^.]+\.)?(openai\..+)$", RegexOptions.CultureInvariant);
        var isOpenAiModel = openAiMatch.Success;
        var isOpenAiGptOssModel = isOpenAiModel && openAiMatch.Groups[1].Value.StartsWith("openai.gpt-oss-", StringComparison.Ordinal);

        // OpenAI models other than gpt-oss reject temperature outright, so clamping it would only add a misleading warning.
        if (!isOpenAiModel || isOpenAiGptOssModel)
        {
            if (temperature > 1)
            {
                warnings.Add(new CallWarning("unsupported", FormatNumber(temperature.Value) + " exceeds bedrock maximum of 1.0. clamped to 1.0"));
                temperature = 1;
            }
            else if (temperature < 0)
            {
                warnings.Add(new CallWarning("unsupported", FormatNumber(temperature.Value) + " is below bedrock minimum of 0. clamped to 0"));
                temperature = 0;
            }
        }

        reasoningConfig = ResolveReasoningConfig(options.Reasoning, reasoningConfig, warnings, isAnthropicModel, isOpenAiModel, ModelId, capabilities);
        var isThinkingEnabled = reasoningConfig?.Type == "enabled" || reasoningConfig?.Type == "adaptive";

        var structuredOutputMode = ReadString(bedrock, "structuredOutputMode")
            ?? ReadString(ReadProviderOptions(options.ProviderOptions, "anthropic"), "structuredOutputMode")
            ?? "auto";

        if (structuredOutputMode == "jsonTool"
            && additional?["output_config"] is JsonObject existingOutputConfig)
        {
            existingOutputConfig.Remove("format");
            if (existingOutputConfig.Count == 0)
            {
                additional.Remove("output_config");
            }
        }

        var hasSchema = options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Null && schema.ValueKind != JsonValueKind.Undefined;
        var supportsNative = AmazonBedrockModelSupport.SupportsNativeStructuredOutput(ModelId)
            && (capabilities.SupportsStructuredOutput || isThinkingEnabled);
        var useNativeStructuredOutput = isAnthropicModel
            && hasSchema
            && (structuredOutputMode == "outputFormat" || (structuredOutputMode == "auto" && supportsNative));
        usesJsonInstruction = !useNativeStructuredOutput
            && isAnthropicModel
            && hasSchema
            && (capabilities.RejectsForcedToolUse
                || (structuredOutputMode != "jsonTool"
                    && !AmazonBedrockModelSupport.SupportsStrictTools(ModelId)
                    && options.Tools is { Count: > 0 }));
        usesJsonTool = hasSchema && !useNativeStructuredOutput && !usesJsonInstruction;

        var tools = options.Tools;
        var toolChoice = options.ToolChoice;
        if (usesJsonTool)
        {
            var list = new List<LanguageModelTool>(tools ?? Array.Empty<LanguageModelTool>())
            {
                new LanguageModelTool(JsonToolName, "Respond with a JSON object.", options.JsonSchema!.Value),
            };
            tools = list;
            toolChoice = ToolChoice.Required;
        }

        var toolConfig = AmazonBedrockTools.Prepare(ModelId, tools, toolChoice, out var toolWarnings);
        warnings.AddRange(toolWarnings);

        var thinkingType = reasoningConfig?.Type;
        var thinkingBudget = thinkingType == "enabled" ? reasoningConfig?.BudgetTokens : null;
        var isAnthropicThinkingEnabled = isAnthropicModel && isThinkingEnabled;
        var maxTokens = options.MaxOutputTokens;

        if (isAnthropicThinkingEnabled)
        {
            if (thinkingBudget is { } budget)
            {
                maxTokens = maxTokens is { } current ? current + budget : budget + 4096;
                additional ??= new JsonObject();
                additional["thinking"] = new JsonObject { ["type"] = "enabled", ["budget_tokens"] = budget };
            }
            else if (thinkingType == "adaptive")
            {
                var thinking = new JsonObject { ["type"] = "adaptive" };
                if (reasoningConfig!.Display != null)
                {
                    thinking["display"] = reasoningConfig.Display;
                }

                additional ??= new JsonObject();
                additional["thinking"] = thinking;
            }
        }
        else if (!isAnthropicModel)
        {
            if (reasoningConfig?.BudgetTokens != null)
            {
                warnings.Add(new CallWarning("unsupported", "budgetTokens applies only to Anthropic models on Bedrock and will be ignored for this model."));
            }

            if (thinkingType == "adaptive")
            {
                warnings.Add(new CallWarning("unsupported", "adaptive thinking type applies only to Anthropic models on Bedrock."));
            }
        }

        var maxReasoningEffort = reasoningConfig?.MaxReasoningEffort;
        if (maxReasoningEffort != null)
        {
            additional ??= new JsonObject();
            if (isAnthropicModel)
            {
                var outputConfig = additional["output_config"] as JsonObject ?? new JsonObject();
                outputConfig["effort"] = maxReasoningEffort;
                additional["output_config"] = outputConfig;
            }
            else if (isOpenAiModel)
            {
                // gpt-oss models expect a flat `reasoning_effort`; GPT-5.x models expect a nested `reasoning.effort`.
                if (isOpenAiGptOssModel)
                {
                    additional["reasoning_effort"] = maxReasoningEffort;
                }
                else
                {
                    var reasoning = additional["reasoning"] as JsonObject ?? new JsonObject();
                    reasoning["effort"] = maxReasoningEffort;
                    additional["reasoning"] = reasoning;
                }
            }
            else
            {
                // Other models, such as Nova 2, use the reasoningConfig format.
                var novaConfig = new JsonObject();
                if (thinkingType != null && thinkingType != "adaptive")
                {
                    novaConfig["type"] = thinkingType;
                }

                if (thinkingBudget is { } novaBudget)
                {
                    novaConfig["budgetTokens"] = novaBudget;
                }

                novaConfig["maxReasoningEffort"] = maxReasoningEffort;
                additional["reasoningConfig"] = novaConfig;
            }
        }

        if (useNativeStructuredOutput)
        {
            additional ??= new JsonObject();
            var outputConfig = additional["output_config"] as JsonObject ?? new JsonObject();
            outputConfig["format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["schema"] = AnthropicJsonSchema.Sanitize(options.JsonSchema!.Value),
            };
            additional["output_config"] = outputConfig;
        }

        if (isAnthropicThinkingEnabled)
        {
            if (temperature != null)
            {
                temperature = null;
                warnings.Add(new CallWarning("unsupported", "temperature is not supported when thinking is enabled"));
            }

            if (topP != null)
            {
                topP = null;
                warnings.Add(new CallWarning("unsupported", "topP is not supported when thinking is enabled"));
            }

            if (topK != null)
            {
                topK = null;
                warnings.Add(new CallWarning("unsupported", "topK is not supported when thinking is enabled"));
            }
        }

        // OpenAI models reject stopSequences on the Converse API. Models other than gpt-oss also reject temperature and topP.
        if (isOpenAiModel)
        {
            if (!isOpenAiGptOssModel && temperature != null)
            {
                temperature = null;
                warnings.Add(new CallWarning("unsupported", "temperature is not supported by this OpenAI model on the Converse API"));
            }

            if (!isOpenAiGptOssModel && topP != null)
            {
                topP = null;
                warnings.Add(new CallWarning("unsupported", "topP is not supported by this OpenAI model on the Converse API"));
            }

            if (stopSequences != null)
            {
                stopSequences = null;
                warnings.Add(new CallWarning("unsupported", "stopSequences is not supported by this OpenAI model on the Converse API"));
            }
        }

        var prompt = options.Prompt;
        if (usesJsonInstruction)
        {
            prompt = InjectJsonInstructionIntoMessages(prompt, options.JsonSchema!.Value);
        }

        AmazonBedrockMessages.Convert(ModelId, prompt, out var system, out var messages);
        var body = new JsonObject { ["messages"] = messages };
        if (system != null)
        {
            body["system"] = system;
        }

        if (additional != null)
        {
            body["additionalModelRequestFields"] = additional;
        }

        if (isAnthropicModel)
        {
            body["additionalModelResponseFieldPaths"] = new JsonArray("/delta/stop_sequence");
        }

        var inference = new JsonObject();
        if (maxTokens is { } max)
        {
            inference["maxTokens"] = max;
        }

        if (temperature is { } temperatureValue)
        {
            inference["temperature"] = temperatureValue;
        }

        if (topP is { } topPValue)
        {
            inference["topP"] = topPValue;
        }

        if (topK is { } topKValue)
        {
            inference["topK"] = topKValue;
        }

        if (stopSequences != null)
        {
            var stop = new JsonArray();
            foreach (var sequence in stopSequences)
            {
                stop.Add(sequence);
            }

            inference["stopSequences"] = stop;
        }

        if (inference.Count > 0)
        {
            body["inferenceConfig"] = inference;
        }

        if (ReadServiceTier(bedrock) is { } serviceTier)
        {
            body["serviceTier"] = new JsonObject { ["type"] = serviceTier };
        }

        var requestMetadata = ReadRequestMetadata(options);
        if (requestMetadata != null)
        {
            body["requestMetadata"] = requestMetadata;
        }

        // Every other provider option, such as guardrailConfig, goes to the request as is.
        if (bedrock is { ValueKind: JsonValueKind.Object } bedrockObject)
        {
            foreach (var property in bedrockObject.EnumerateObject())
            {
                if (!NonPassthroughOptions.Contains(property.Name))
                {
                    body[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                }
            }
        }

        if (toolConfig.Count > 0)
        {
            body.Remove("toolConfig");
            body["toolConfig"] = toolConfig;
        }

        return body;
    }

    private static readonly HashSet<string> NonPassthroughOptions = new HashSet<string>(StringComparer.Ordinal)
    {
        "reasoningConfig",
        "additionalModelRequestFields",
        "serviceTier",
        "structuredOutputMode",
        "requestMetadata",
    };

    private static IReadOnlyList<ModelMessage> InjectJsonInstructionIntoMessages(IReadOnlyList<ModelMessage> messages, JsonElement schema)
    {
        var first = messages.Count > 0 ? messages[0] as SystemModelMessage : null;
        var instruction = Vercel.AI.GenerateText.JsonInstructions.InjectJsonInstruction(
            first?.Content,
            schema,
            schemaSuffix: Vercel.AI.GenerateText.JsonInstructionText.From("You MUST answer with only a JSON object that matches the JSON schema above. Do not wrap it in markdown fences or include any other text."));
        var result = new List<ModelMessage> { new SystemModelMessage(instruction, first?.ProviderOptions) };
        for (var i = first != null ? 1 : 0; i < messages.Count; i++)
        {
            result.Add(messages[i]);
        }

        return result;
    }

    private static BedrockReasoningConfig? ResolveReasoningConfig(
        string? reasoning,
        BedrockReasoningConfig? user,
        List<CallWarning> warnings,
        bool isAnthropicModel,
        bool isOpenAIModel,
        string modelId,
        AnthropicModelCapabilities capabilities)
    {
        if (!ReasoningMap.IsCustomReasoning(reasoning))
        {
            return user;
        }

        // Preserve disabled thinking without deriving settings or warnings that will not be sent.
        if (user?.Type == "disabled")
        {
            return new BedrockReasoningConfig { Type = "disabled", Display = user.Display };
        }

        var modelWarnings = new List<ModelWarning>();
        BedrockReasoningConfig? result = user;
        var hasPortableReasoning = reasoning != "none";
        var isNovaReasoningModel = modelId.IndexOf("amazon.nova-2-lite-v1:0", StringComparison.Ordinal) >= 0;
        var supportsPortableReasoning = isOpenAIModel || isNovaReasoningModel || user != null;
        if (isAnthropicModel)
        {
            if (reasoning == "none")
            {
                result = new BedrockReasoningConfig { Type = "disabled" };
            }
            else if (capabilities.SupportsAdaptiveThinking)
            {
                var effort = user?.MaxReasoningEffort
                    ?? ReasoningMap.MapReasoningToProviderEffort(reasoning!, ReasoningEffortMap, modelWarnings);
                result = new BedrockReasoningConfig { Type = "adaptive", MaxReasoningEffort = effort }.MergeUser(user);
            }
            else
            {
                var budget = user?.BudgetTokens
                    ?? ReasoningMap.MapReasoningToProviderBudget(reasoning!, capabilities.MaxOutputTokens, capabilities.MaxOutputTokens, modelWarnings);
                if (budget != null)
                {
                    result = new BedrockReasoningConfig { Type = "enabled", BudgetTokens = budget }.MergeUser(user);
                }
            }
        }
        else if (hasPortableReasoning)
        {
            if (supportsPortableReasoning)
            {
                var effort = user?.MaxReasoningEffort
                    ?? ReasoningMap.MapReasoningToProviderEffort(reasoning!, isNovaReasoningModel ? NovaReasoningEffortMap : ReasoningEffortMap, modelWarnings);
                result = new BedrockReasoningConfig { Type = isNovaReasoningModel ? "enabled" : null, MaxReasoningEffort = effort }.MergeUser(user);
            }
            else
            {
                warnings.Add(new CallWarning(
                    "unsupported",
                    "Portable reasoning is not supported for this model and will be ignored. If the model supports a provider-specific reasoning configuration, use providerOptions.amazonBedrock.reasoningConfig."));
            }
        }

        foreach (var warning in modelWarnings)
        {
            switch (warning)
            {
                case UnsupportedWarning unsupported:
                    warnings.Add(new CallWarning("unsupported", unsupported.Details ?? unsupported.Feature));
                    break;
                case CompatibilityWarning compatibility:
                    warnings.Add(new CallWarning("compatibility", compatibility.Details ?? compatibility.Feature));
                    break;
            }
        }

        return result;
    }

    private static readonly IReadOnlyDictionary<string, string> ReasoningEffortMap = new Dictionary<string, string>
    {
        { "minimal", "low" },
        { "low", "low" },
        { "medium", "medium" },
        { "high", "high" },
        { "xhigh", "max" },
        { "max", "max" },
    };

    private static readonly IReadOnlyDictionary<string, string> NovaReasoningEffortMap = new Dictionary<string, string>
    {
        { "minimal", "low" },
        { "low", "low" },
        { "medium", "medium" },
        { "high", "high" },
        { "xhigh", "high" },
        { "max", "high" },
    };

    /// <summary>Reads provider options from <c>amazonBedrock</c>, then <c>bedrock</c>, then <c>amazon-bedrock</c>.</summary>
    private static JsonElement? ReadBedrockOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        return ReadProviderOptions(providerOptions, "amazonBedrock")
            ?? ReadProviderOptions(providerOptions, "bedrock")
            ?? ReadProviderOptions(providerOptions, "amazon-bedrock");
    }

    private static JsonElement? ReadProviderOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions, string key)
    {
        if (providerOptions != null && providerOptions.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.Object)
        {
            return value;
        }

        return null;
    }

    private static string? ReadString(JsonElement? element, string name)
    {
        if (element is { } value && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static string? ReadServiceTier(JsonElement? bedrock)
    {
        if (bedrock is not { } value || !value.TryGetProperty("serviceTier", out var tier) || tier.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var name = tier.ValueKind == JsonValueKind.String ? tier.GetString() : null;
        if (name is not ("reserved" or "priority" or "default" or "flex"))
        {
            throw new ArgumentException("Bedrock serviceTier must be one of reserved, priority, default, or flex.");
        }

        return name;
    }

    private static string FormatNumber(double value) => value.ToString(CultureInfo.InvariantCulture);

    private static JsonObject? ReadAdditionalModelRequestFields(JsonElement? bedrock)
    {
        if (bedrock is { } value
            && value.TryGetProperty("additionalModelRequestFields", out var fields)
            && fields.ValueKind == JsonValueKind.Object)
        {
            return JsonNode.Parse(fields.GetRawText()) as JsonObject;
        }

        return null;
    }

    private sealed class BedrockReasoningConfig
    {
        public string? Type { get; set; }

        public int? BudgetTokens { get; set; }

        public string? MaxReasoningEffort { get; set; }

        public string? Display { get; set; }

        public static BedrockReasoningConfig? Read(JsonElement? bedrock)
        {
            if (bedrock is not { } value
                || !value.TryGetProperty("reasoningConfig", out var config)
                || config.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var result = new BedrockReasoningConfig { Type = ReadString(config, "type"), MaxReasoningEffort = ReadString(config, "maxReasoningEffort"), Display = ReadString(config, "display") };
            if (config.TryGetProperty("budgetTokens", out var budget) && budget.ValueKind == JsonValueKind.Number && budget.TryGetInt32(out var tokens))
            {
                result.BudgetTokens = tokens;
            }

            return result;
        }

        /// <summary>Returns a copy of this derived config with every field the user set laid over it.</summary>
        public BedrockReasoningConfig MergeUser(BedrockReasoningConfig? user)
        {
            return new BedrockReasoningConfig
            {
                Type = user?.Type ?? Type,
                BudgetTokens = user?.BudgetTokens ?? BudgetTokens,
                MaxReasoningEffort = user?.MaxReasoningEffort ?? MaxReasoningEffort,
                Display = user?.Display ?? Display,
            };
        }
    }

    /// <summary>Reads <c>requestMetadata</c> from <c>amazon-bedrock</c>, then <c>amazonBedrock</c>, then <c>bedrock</c>.</summary>
    private static JsonObject? ReadRequestMetadata(LanguageModelCallOptions options)
    {
        var providerOptions = options.ProviderOptions;
        if (providerOptions == null)
        {
            return null;
        }

        JsonElement provider;
        if (!providerOptions.TryGetValue("amazon-bedrock", out provider)
            && !providerOptions.TryGetValue("amazonBedrock", out provider)
            && !providerOptions.TryGetValue("bedrock", out provider))
        {
            return null;
        }

        if (provider.ValueKind != JsonValueKind.Object || !provider.TryGetProperty("requestMetadata", out var metadata))
        {
            return null;
        }

        if (metadata.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
        }

        var result = new JsonObject();
        foreach (var property in metadata.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
            }

            var value = property.Value.GetString();
            if (value == null)
            {
                throw new ArgumentException("Bedrock requestMetadata must be a JSON object whose values are strings.");
            }

            result[property.Name] = value;
        }

        return result;
    }

    private LanguageModelGenerateResult Parse(JsonElement root, IReadOnlyList<CallWarning> warnings, IReadOnlyDictionary<string, string> headers, bool usesJsonTool, bool usesJsonInstruction)
    {
        var extractor = usesJsonInstruction ? new JsonObjectTextExtractor() : null;
        var isMistral = AmazonBedrockToolIds.IsMistralModel(ModelId);
        var content = new List<GeneratedContent>();
        var jsonTool = false;
        if (root.TryGetProperty("output", out var output) && output.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var parts))
        {
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    content.Add(new GeneratedText(Extract(extractor, text.GetString() ?? string.Empty)));
                }
                else if (part.TryGetProperty("citationsContent", out var citations) && citations.TryGetProperty("content", out var cited) && cited.ValueKind == JsonValueKind.Array)
                {
                    foreach (var citedPart in cited.EnumerateArray())
                    {
                        if (citedPart.TryGetProperty("text", out var citedText) && citedText.ValueKind == JsonValueKind.String)
                        {
                            content.Add(new GeneratedText(Extract(extractor, citedText.GetString() ?? string.Empty)));
                        }
                    }
                }

                if (part.TryGetProperty("reasoningContent", out var reasoning) && reasoning.ValueKind == JsonValueKind.Object)
                {
                    if (reasoning.TryGetProperty("reasoningText", out var reasoningText) && reasoningText.ValueKind == JsonValueKind.Object && reasoningText.TryGetProperty("text", out var reasoningValue))
                    {
                        content.Add(new GeneratedReasoning(reasoningValue.GetString() ?? string.Empty));
                    }
                    else if (reasoning.TryGetProperty("redactedReasoning", out _) || reasoning.TryGetProperty("redactedContent", out _))
                    {
                        content.Add(new GeneratedReasoning(string.Empty));
                    }
                }

                if (part.TryGetProperty("toolUse", out var toolUse) && toolUse.ValueKind == JsonValueKind.Object)
                {
                    var name = toolUse.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "tool" : "tool";
                    if (usesJsonTool && string.Equals(name, JsonToolName, StringComparison.Ordinal))
                    {
                        jsonTool = true;
                        var json = toolUse.TryGetProperty("input", out var jsonInput) ? jsonInput.GetRawText() : "{}";
                        content.Add(new GeneratedText(json));
                        continue;
                    }

                    var id = toolUse.TryGetProperty("toolUseId", out var idElement) ? idElement.GetString() ?? "tool" : "tool";
                    var input = toolUse.TryGetProperty("input", out var inputElement) ? inputElement.GetRawText() : "{}";
                    content.Add(new GeneratedToolCall(AmazonBedrockToolIds.Normalize(id, isMistral), name, input));
                }
            }
        }

        var raw = root.TryGetProperty("stopReason", out var stop) ? stop.GetString() : "end_turn";

        JsonElement? usageElement = root.TryGetProperty("usage", out var usageProperty) ? usageProperty : null;
        var usage = AmazonBedrockUsage.Convert(usageElement);
        string? responseId = null;
        if (headers.TryGetValue("x-amzn-requestid", out var requestId))
        {
            responseId = requestId;
        }

        JsonElement? providerMetadata = null;
        if (jsonTool)
        {
            string? stopSequence = null;
            if (root.TryGetProperty("additionalModelResponseFields", out var extra)
                && extra.ValueKind == JsonValueKind.Object
                && extra.TryGetProperty("delta", out var delta)
                && delta.ValueKind == JsonValueKind.Object
                && delta.TryGetProperty("stop_sequence", out var stopValue)
                && stopValue.ValueKind == JsonValueKind.String)
            {
                stopSequence = stopValue.GetString();
            }

            var payload = new JsonObject { ["isJsonResponseFromTool"] = true, ["stopSequence"] = stopSequence };
            var metadata = new JsonObject { ["amazonBedrock"] = payload, ["bedrock"] = payload.DeepClone() };
            using var document = JsonDocument.Parse(metadata.ToJsonString());
            providerMetadata = document.RootElement.Clone();
        }

        return new LanguageModelGenerateResult(content, MapFinishReason(raw, jsonTool), usage, raw, warnings, responseId, providerMetadata, responseHeaders: headers);
    }

    private static string Extract(JsonObjectTextExtractor? extractor, string text) => extractor?.Process(text) ?? text;

    private static Dictionary<string, string> CopyHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }
}

/// <summary>Registers Bedrock.</summary>
public static class AmazonBedrockServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AmazonBedrockProvider"/>.</summary>
    public static IServiceCollection AddAmazonBedrock(this IServiceCollection services, Action<AmazonBedrockOptions>? configure = null)
    {
        services.AddHttpClient(AmazonBedrockProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new AmazonBedrockOptions();
            configure?.Invoke(options);
            return new AmazonBedrockProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AmazonBedrockProvider.ProviderName), options);
        });
        return services;
    }
}
