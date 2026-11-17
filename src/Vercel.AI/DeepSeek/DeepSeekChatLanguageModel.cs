// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.DeepSeek;

/// <summary>
/// The configuration of a <see cref="DeepSeekChatLanguageModel"/>. Maps to <c>DeepSeekChatConfig</c>.
/// <see cref="Transport"/> is .NET-only. It replaces upstream's <c>headers</c> and <c>fetch</c>.
/// </summary>
public sealed class DeepSeekChatConfig
{
    /// <summary>Creates a configuration.</summary>
    /// <param name="provider">Provider id, such as <c>deepseek.chat</c>. The segment before the first dot is the provider-options key.</param>
    /// <param name="transport">The provider whose HTTP client and headers send the requests.</param>
    /// <param name="url">Builds the request URL from the model id and the path, such as <c>/chat/completions</c>. Maps to <c>url</c>.</param>
    public DeepSeekChatConfig(
        string provider,
        OpenAICompatibleProvider transport,
        Func<string, string, Uri> url
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        Url = url ?? throw new ArgumentNullException(nameof(url));
    }

    /// <summary>Provider id, such as <c>deepseek.chat</c>.</summary>
    public string Provider { get; }

    /// <summary>The provider whose HTTP client and headers send the requests.</summary>
    public OpenAICompatibleProvider Transport { get; }

    /// <summary>Builds the request URL from the model id and the path. Maps to <c>url</c>.</summary>
    public Func<string, string, Uri> Url { get; }

    /// <summary>Whether the endpoint supports assistant prefix completion. Maps to <c>supportsAssistantPrefixCompletion</c>.</summary>
    public bool? SupportsAssistantPrefixCompletion { get; init; }

    /// <summary>Whether the endpoint supports strict tool calls. Maps to <c>supportsStrictToolCalls</c>.</summary>
    public bool? SupportsStrictToolCalls { get; init; }

    /// <summary>Whether the endpoint supports penalty sampling. Maps to <c>supportsPenaltySampling</c>.</summary>
    public bool? SupportsPenaltySampling { get; init; }

    /// <summary>Whether the endpoint supports thinking. Maps to <c>supportsThinking</c>.</summary>
    public bool? SupportsThinking { get; init; }

    /// <summary>Whether the endpoint supports JSON schema response formats. Maps to <c>supportsStructuredOutputs</c>.</summary>
    public bool? SupportsStructuredOutputs { get; init; }
}

/// <summary>DeepSeek Chat Completions language model. Maps to <c>DeepSeekChatLanguageModel</c>.</summary>
public sealed class DeepSeekChatLanguageModel : ILanguageModel
{
    private static readonly Dictionary<string, string> EffortMap = new()
    {
        ["low"] = "low",
        ["high"] = "high",
        ["max"] = "max",
    };

    private readonly DeepSeekChatConfig _config;

    /// <summary>Creates a chat model for <paramref name="modelId"/>.</summary>
    public DeepSeekChatLanguageModel(string modelId, DeepSeekChatConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>URL patterns the model accepts without downloading: <c>image/*</c> over http or https. Maps to <c>supportedUrls</c>.</summary>
    public IReadOnlyDictionary<string, string> SupportedUrls { get; } =
        new Dictionary<string, string> { ["image/*"] = "^https?://.*\\z" };

    private static string ProviderOptionsNameOf(string provider)
    {
        var dot = provider.IndexOf('.');
        return (dot < 0 ? provider : provider.Substring(0, dot)).Trim();
    }

    private string ProviderOptionsName => ProviderOptionsNameOf(_config.Provider);

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options ??= new LanguageModelCallOptions();
        var (args, warnings) = GetArgs(options);
        var uri = _config.Url(ModelId, "/chat/completions");
        var json = args.ToJsonString();
        var http = await _config
            .Transport.Http.SendJsonStringAsync(
                HttpMethod.Post,
                uri,
                json,
                _config.Transport.CreateHeaders(options.Headers),
                cancellationToken
            )
            .ConfigureAwait(false);
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(http.Body) ? "null" : http.Body
            );
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, http.Body);
        }

        using (document)
        {
            var root = document.RootElement;
            if (!DeepSeekChatApiTypes.IsValidResponse(root))
            {
                throw new ApiException(
                    "Invalid JSON response",
                    200,
                    http.Body,
                    new TypeValidationException(
                        "Type validation failed: Value: " + http.Body + ".",
                        root.Clone()
                    )
                );
            }

            return Parse(root, args, json, warnings, http.Body, http.Headers);
        }
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options ??= new LanguageModelCallOptions();
        var (args, warnings) = GetArgs(options);
        args["stream"] = true;
        args["stream_options"] = new JsonObject { ["include_usage"] = true };
        var json = args.ToJsonString();
        var sse = await _config
            .Transport.Http.OpenSseResponseAsync(
                _config.Url(ModelId, "/chat/completions"),
                json,
                _config.Transport.CreateHeaders(options.Headers),
                cancellationToken
            )
            .ConfigureAwait(false);
        return sse.ToStreamResult(
            StreamAsync(options, warnings, sse.Events, cancellationToken),
            json
        );
    }

    private static string? String(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static int? Int(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private static DateTimeOffset? Created(JsonElement element)
    {
        return
            element.TryGetProperty("created", out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt64(out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;
    }

    private static JsonObject? Usage(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? new JsonObject { [name] = JsonNode.Parse(value.GetRawText()) }
            : null;
    }

    private static JsonObject CacheMetadata(JsonElement? usage)
    {
        var inner = new JsonObject();
        if (usage is { ValueKind: JsonValueKind.Object } element)
        {
            if (Usage(element, "prompt_cache_hit_tokens") is { } hit)
            {
                inner["promptCacheHitTokens"] = hit["prompt_cache_hit_tokens"]!.DeepClone();
            }

            if (Usage(element, "prompt_cache_miss_tokens") is { } miss)
            {
                inner["promptCacheMissTokens"] = miss["prompt_cache_miss_tokens"]!.DeepClone();
            }
        }

        return inner;
    }

    private static (int? StatusCode, bool? IsRetryable) GetDeepSeekStreamErrorMetadata(
        string? type,
        object? code
    )
    {
        var codeText = code as string;
        if (codeText == "insufficient_quota" || type == "insufficient_quota")
        {
            return (429, false);
        }

        if (GetHttpStatusCode(code) is { } explicitStatusCode)
        {
            return (
                explicitStatusCode,
                Util.StreamProviderError.IsRetryableStatusCode(explicitStatusCode)
            );
        }

        foreach (var discriminator in new[] { codeText, type })
        {
            switch (discriminator)
            {
                case "rate_limit_exceeded":
                case "rate_limit_error":
                    return (429, true);
                case "server_error":
                case "api_error":
                case "internal_server_error":
                    return (500, true);
                case "overloaded_error":
                case "service_unavailable":
                    return (503, true);
                case "timeout":
                case "timeout_error":
                    return (504, true);
                case "authentication_error":
                case "invalid_api_key":
                    return (401, false);
                case "permission_error":
                    return (403, false);
                case "not_found_error":
                case "model_not_found":
                    return (404, false);
                case "bad_request":
                case "context_length_exceeded":
                case "invalid_request_error":
                    return (400, false);
            }
        }

        return (null, null);
    }

    private static int? GetHttpStatusCode(object? value)
    {
        int? statusCode = value switch
        {
            string text
                when text.Length == 3 && text.All(character => character is >= '0' and <= '9') =>
                int.Parse(text, System.Globalization.CultureInfo.InvariantCulture),
            int number => number,
            long number when number >= 0 && number <= int.MaxValue => (int)number,
            _ => null,
        };
        return statusCode is >= 400 and <= 599 ? statusCode : null;
    }

    private static ProviderStreamError CreateDeepSeekStreamError(JsonElement chunk)
    {
        var error = chunk.GetProperty("error");
        var type = String(error, "type");
        object? code = null;
        if (error.TryGetProperty("code", out var codeValue))
        {
            code = codeValue.ValueKind switch
            {
                JsonValueKind.String => codeValue.GetString(),
                JsonValueKind.Number when codeValue.TryGetInt64(out var number) => number,
                _ => null,
            };
        }

        var (statusCode, isRetryable) = GetDeepSeekStreamErrorMetadata(type, code);
        return new ProviderStreamError(
            error.GetProperty("message").GetString()!,
            chunk.Clone(),
            type,
            code,
            statusCode,
            isRetryable
        );
    }

    private static string MapDeepSeekProviderReasoningEffort(
        string reasoningEffort,
        List<CallWarning> warnings
    )
    {
        var mapped = reasoningEffort switch
        {
            "medium" => "high",
            "xhigh" => "max",
            _ => reasoningEffort,
        };
        if (mapped != reasoningEffort)
        {
            warnings.Add(
                new CallWarning(
                    "compatibility",
                    "reasoningEffort",
                    "reasoningEffort \""
                        + reasoningEffort
                        + "\" is not a canonical DeepSeek value. mapped to \""
                        + mapped
                        + "\"."
                )
            );
        }

        return mapped;
    }

    private (JsonObject Args, List<CallWarning> Warnings) GetArgs(LanguageModelCallOptions options)
    {
        var deepseekOptions = DeepSeekLanguageModelChatOptions.Parse(
            options.ProviderOptions,
            ProviderOptionsName
        );
        var capabilities = DeepSeekModelCatalog.GetDeepSeekLanguageModelEntry(
            ModelId,
            new DeepSeekEndpointOptions
            {
                SupportsAssistantPrefixCompletion = _config.SupportsAssistantPrefixCompletion,
                SupportsStrictToolCalls = _config.SupportsStrictToolCalls,
                SupportsPenaltySampling = _config.SupportsPenaltySampling,
                SupportsThinking = _config.SupportsThinking,
                SupportsStructuredOutputs = _config.SupportsStructuredOutputs,
            }
        );
        var supportsStructuredOutputs = capabilities.StructuredOutputJsonSchema;
        var supportsPenaltySampling = capabilities.SamplingPenalties;

        // A JSON schema alone asks for a JSON response, as upstream's responseFormat of type json.
        var jsonResponse =
            options.JsonResponse
            || (
                options.JsonSchema is { } responseSchema
                && responseSchema.ValueKind != JsonValueKind.Undefined
                && responseSchema.ValueKind != JsonValueKind.Null
            );
        var converted = ConvertToDeepSeekChatMessages.Convert(
            options.Prompt,
            jsonResponse,
            options.JsonSchema,
            ModelId,
            ProviderOptionsName,
            capabilities.EndpointsAssistantPrefixCompletion,
            supportsStructuredOutputs
        );
        var warnings = new List<CallWarning>(converted.Warnings);

        if (options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK", null));
        }

        if (options.Seed != null)
        {
            warnings.Add(new CallWarning("unsupported", "seed", null));
        }

        if (!supportsPenaltySampling && options.FrequencyPenalty != null)
        {
            warnings.Add(
                CallWarning.Deprecated(
                    "frequencyPenalty",
                    "frequencyPenalty is deprecated by DeepSeek and has been omitted. Remove frequencyPenalty from the request."
                )
            );
        }

        if (!supportsPenaltySampling && options.PresencePenalty != null)
        {
            warnings.Add(
                CallWarning.Deprecated(
                    "presencePenalty",
                    "presencePenalty is deprecated by DeepSeek and has been omitted. Remove presencePenalty from the request."
                )
            );
        }

        var tools = DeepSeekPrepareTools.PrepareTools(
            options.Tools,
            options.ToolChoice,
            capabilities.ToolsStrict
        );

        var thinkingType = deepseekOptions.Thinking?.Type;
        if (thinkingType == "adaptive")
        {
            warnings.Add(
                new CallWarning(
                    "compatibility",
                    "thinking.type",
                    "thinking.type \"adaptive\" is not a canonical DeepSeek value. mapped to \"enabled\"."
                )
            );
        }

        string? thinking = null;
        if (capabilities.ReasoningConfigurable)
        {
            if (thinkingType != null)
            {
                thinking = thinkingType == "adaptive" ? "enabled" : thinkingType;
            }
            else if (ReasoningMap.IsCustomReasoning(options.Reasoning))
            {
                thinking = options.Reasoning == "none" ? "disabled" : "enabled";
            }
        }

        var isThinkingEnabled =
            capabilities.ReasoningConfigurable
            && thinking != "disabled"
            && (thinking != null || capabilities.ReasoningEnabledByDefault);

        if (isThinkingEnabled && options.Temperature != null)
        {
            warnings.Add(
                new CallWarning(
                    "unsupported",
                    "temperature",
                    "temperature has no effect when DeepSeek thinking is enabled. Set providerOptions.deepseek.thinking.type to 'disabled' to use temperature."
                )
            );
        }

        if (isThinkingEnabled && options.TopP != null)
        {
            warnings.Add(
                new CallWarning(
                    "unsupported",
                    "topP",
                    "topP has no effect when DeepSeek thinking is enabled. Set providerOptions.deepseek.thinking.type to 'disabled' to use topP."
                )
            );
        }

        string? reasoningEffort = null;
        if (deepseekOptions.ReasoningEffort != null)
        {
            reasoningEffort = MapDeepSeekProviderReasoningEffort(
                deepseekOptions.ReasoningEffort,
                warnings
            );
        }
        else if (ReasoningMap.IsCustomReasoning(options.Reasoning) && options.Reasoning != "none")
        {
            var mappingWarnings = new List<ModelWarning>();
            reasoningEffort = ReasoningMap.MapReasoningToProviderEffort(
                options.Reasoning!,
                EffortMap,
                mappingWarnings
            );
            foreach (var warning in mappingWarnings)
            {
                if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
                {
                    warnings.Add(callWarning);
                }
            }
        }

        var args = new JsonObject { ["model"] = ModelId };
        if (deepseekOptions.Logprobs == true || deepseekOptions.TopLogprobs != null)
        {
            args["logprobs"] = true;
        }

        if (deepseekOptions.TopLogprobs is { } topLogprobs)
        {
            args["top_logprobs"] = topLogprobs;
        }

        Set(args, "max_tokens", options.MaxOutputTokens);
        Set(args, "temperature", isThinkingEnabled ? null : options.Temperature);
        Set(args, "top_p", isThinkingEnabled ? null : options.TopP);
        Set(args, "frequency_penalty", supportsPenaltySampling ? options.FrequencyPenalty : null);
        Set(args, "presence_penalty", supportsPenaltySampling ? options.PresencePenalty : null);
        if (jsonResponse)
        {
            if (
                supportsStructuredOutputs
                && options.JsonSchema is { } schema
                && schema.ValueKind != JsonValueKind.Undefined
                && schema.ValueKind != JsonValueKind.Null
            )
            {
                var jsonSchema = new JsonObject
                {
                    ["schema"] = JsonNode.Parse(schema.GetRawText()),
                    ["strict"] = deepseekOptions.StrictJsonSchema ?? true,
                    ["name"] = options.JsonSchemaName ?? "response",
                };
                if (options.JsonSchemaDescription != null)
                {
                    jsonSchema["description"] = options.JsonSchemaDescription;
                }

                args["response_format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["json_schema"] = jsonSchema,
                };
            }
            else
            {
                args["response_format"] = new JsonObject { ["type"] = "json_object" };
            }
        }

        if (options.StopSequences != null)
        {
            args["stop"] = new JsonArray(
                options.StopSequences.Select(stop => (JsonNode?)JsonValue.Create(stop)).ToArray()
            );
        }

        args["messages"] = converted.Messages;
        if (tools.Tools != null)
        {
            args["tools"] = tools.Tools;
        }

        if (tools.ToolChoice != null)
        {
            args["tool_choice"] = tools.ToolChoice;
        }

        if (thinking != null)
        {
            args["thinking"] = new JsonObject { ["type"] = thinking };
        }

        if (deepseekOptions.UserId != null)
        {
            args["user_id"] = deepseekOptions.UserId;
        }

        if (thinking != "disabled" && reasoningEffort != null)
        {
            args["reasoning_effort"] = reasoningEffort;
        }

        warnings.AddRange(tools.ToolWarnings);
        return (args, warnings);
    }

    private static void Set(JsonObject target, string name, double? value)
    {
        if (value is { } number)
        {
            target[name] = number;
        }
    }

    private static void Set(JsonObject target, string name, int? value)
    {
        if (value is { } number)
        {
            target[name] = number;
        }
    }

    private LanguageModelGenerateResult Parse(
        JsonElement root,
        JsonObject args,
        string requestJson,
        List<CallWarning> warnings,
        string rawResponse,
        IReadOnlyDictionary<string, string> headers
    )
    {
        var choices = root.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
        {
            throw new InvalidResponseDataException(
                root.Clone(),
                "Response did not contain any choices."
            );
        }

        var choice = choices[0];
        var message = choice.GetProperty("message");
        var content = new List<GeneratedContent>();

        // reasoning content (before text)
        if (String(message, "reasoning_content") is { Length: > 0 } reasoning)
        {
            content.Add(new GeneratedReasoning(reasoning));
        }

        // tool calls
        JsonElement? toolCalls =
            message.TryGetProperty("tool_calls", out var calls)
            && calls.ValueKind == JsonValueKind.Array
                ? calls
                : null;
        if (toolCalls is { } toolCallArray)
        {
            foreach (var toolCall in toolCallArray.EnumerateArray())
            {
                var function = toolCall.GetProperty("function");
                content.Add(
                    new GeneratedToolCall(
                        String(toolCall, "id") is { Length: > 0 } id
                            ? id
                            : JsonValues.GenerateId(string.Empty),
                        function.GetProperty("name").GetString()!,
                        function.GetProperty("arguments").GetString()!
                    )
                );
            }
        }

        // text content
        if (String(message, "content") is { Length: > 0 } text)
        {
            content.Add(new GeneratedText(text));
        }

        var rawFinish = String(choice, "finish_reason");
        JsonElement? usage =
            root.TryGetProperty("usage", out var usageValue)
            && usageValue.ValueKind == JsonValueKind.Object
                ? usageValue
                : null;
        var inner = CacheMetadata(usage);
        if (String(root, "object") is { } responseObject)
        {
            inner["responseObject"] = responseObject;
        }

        if (Int(choice, "index") is { } choiceIndex)
        {
            inner["choiceIndex"] = choiceIndex;
        }

        if (String(message, "role") is { } messageRole)
        {
            inner["messageRole"] = messageRole;
        }

        if (toolCalls is { } types)
        {
            inner["toolCallTypes"] = new JsonArray(
                types
                    .EnumerateArray()
                    .Select(toolCall => String(toolCall, "type"))
                    .Where(type => type != null)
                    .Select(type => (JsonNode?)JsonValue.Create(type))
                    .ToArray()
            );
        }

        if (
            choice.TryGetProperty("logprobs", out var logprobs)
            && logprobs.ValueKind == JsonValueKind.Object
        )
        {
            inner["logprobs"] = JsonNode.Parse(logprobs.GetRawText());
        }

        if (String(root, "system_fingerprint") is { } systemFingerprint)
        {
            inner["systemFingerprint"] = systemFingerprint;
        }

        return new LanguageModelGenerateResult(
            content,
            MapDeepSeekFinishReason.Map(rawFinish),
            ConvertToDeepSeekUsage.ConvertDeepSeekUsage(usage),
            rawFinish,
            warnings,
            String(root, "id"),
            JsonSerializer.SerializeToElement(new JsonObject { [ProviderOptionsName] = inner }),
            rawResponse,
            String(root, "model"),
            Created(root),
            headers,
            request: LanguageModelStreamRequest.FromJson(requestJson)
        );
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> StreamAsync(
        LanguageModelCallOptions options,
        IReadOnlyList<CallWarning> warnings,
        IAsyncEnumerable<string> events,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var state = new StreamState(ProviderOptionsName);
        yield return new StreamStartStreamPart(warnings);
        var enumerator = events.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                foreach (var part in state.Accept(enumerator.Current, options.IncludeRawChunks))
                {
                    yield return part;
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        foreach (var part in state.Flush())
        {
            yield return part;
        }
    }

    /// <summary>Turns DeepSeek chat stream chunks into stream parts. Maps to the <c>TransformStream</c> of <c>doStream</c>.</summary>
    private sealed class StreamState(string providerOptionsName)
    {
        private readonly StreamingToolCallTracker _toolCallTracker = new(() =>
            JsonValues.GenerateId(string.Empty)
        );
        private readonly SortedDictionary<int, string> _toolCallTypes = [];
        private readonly JsonArray _contentLogprobs = [];
        private readonly JsonArray _reasoningLogprobs = [];

        // A clean end without the provider's completion signal is interrupted.
        private FinishReason _finishReason = FinishReason.Interrupted;
        private string? _rawFinishReason;
        private JsonElement? _usage;
        private string? _systemFingerprint;
        private bool _isFirstChunk = true;
        private bool _isActiveReasoning;
        private bool _isActiveText;
        private string? _responseObject;
        private int? _choiceIndex;
        private string? _messageRole;

        public List<LanguageModelStreamPart> Accept(string data, bool includeRawChunks)
        {
            var parts = new List<LanguageModelStreamPart>();
            if (data == "[DONE]")
            {
                return parts;
            }

            // Emit the raw chunk if requested, before anything else.
            if (includeRawChunks)
            {
                parts.Add(new RawStreamPart(data));
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(data);
            }
            catch (JsonException)
            {
                _finishReason = FinishReason.Error;
                _rawFinishReason = null;
                parts.Add(new ErrorStreamPart("JSON parsing failed: Text: " + data + "."));
                return parts;
            }

            using (document)
            {
                var value = document.RootElement;

                // handle failed chunk validation
                if (!DeepSeekChatApiTypes.IsValidChunk(value))
                {
                    _finishReason = FinishReason.Error;
                    _rawFinishReason = null;
                    parts.Add(
                        new ErrorStreamPart(
                            new TypeValidationException(
                                "Type validation failed: Value: " + data + ".",
                                value.Clone()
                            )
                        )
                    );
                    return parts;
                }

                // handle error chunks
                if (DeepSeekChatApiTypes.IsError(value))
                {
                    _finishReason = FinishReason.Error;
                    _rawFinishReason = null;
                    parts.Add(new ErrorStreamPart(CreateDeepSeekStreamError(value)));
                    return parts;
                }

                Handle(value, parts);
            }

            return parts;
        }

        public List<LanguageModelStreamPart> Flush()
        {
            var parts = new List<LanguageModelStreamPart>();
            if (_isActiveReasoning)
            {
                parts.Add(new ReasoningEndStreamPart("reasoning-0"));
            }

            if (_isActiveText)
            {
                parts.Add(new TextEndStreamPart("txt-0"));
            }

            foreach (var call in _toolCallTracker.Flush())
            {
                parts.Add(new ToolInputEndStreamPart(call.ToolCallId));
                parts.Add(call);
            }

            var inner = CacheMetadata(_usage);
            if (_responseObject != null)
            {
                inner["responseObject"] = _responseObject;
            }

            if (_choiceIndex is { } choiceIndex)
            {
                inner["choiceIndex"] = choiceIndex;
            }

            if (_messageRole != null)
            {
                inner["messageRole"] = _messageRole;
            }

            if (_toolCallTypes.Count > 0)
            {
                inner["toolCallTypes"] = new JsonArray(
                    _toolCallTypes
                        .Values.Select(type => (JsonNode?)JsonValue.Create(type))
                        .ToArray()
                );
            }

            if (_contentLogprobs.Count > 0 || _reasoningLogprobs.Count > 0)
            {
                var logprobs = new JsonObject();
                if (_contentLogprobs.Count > 0)
                {
                    logprobs["content"] = _contentLogprobs.DeepClone();
                }

                if (_reasoningLogprobs.Count > 0)
                {
                    logprobs["reasoning_content"] = _reasoningLogprobs.DeepClone();
                }

                inner["logprobs"] = logprobs;
            }

            if (_systemFingerprint != null)
            {
                inner["systemFingerprint"] = _systemFingerprint;
            }

            parts.Add(
                new FinishStreamPart(
                    _finishReason,
                    ConvertToDeepSeekUsage.ConvertDeepSeekUsage(_usage),
                    _rawFinishReason,
                    JsonSerializer.SerializeToElement(
                        new JsonObject { [providerOptionsName] = inner }
                    )
                )
            );
            return parts;
        }

        private void Handle(JsonElement value, List<LanguageModelStreamPart> parts)
        {
            if (_isFirstChunk)
            {
                _isFirstChunk = false;
                parts.Add(
                    new ResponseMetadataStreamPart(
                        String(value, "id"),
                        String(value, "model"),
                        Created(value)
                    )
                );
            }

            if (
                value.TryGetProperty("usage", out var usage)
                && usage.ValueKind == JsonValueKind.Object
            )
            {
                _usage = usage.Clone();
            }

            _responseObject = String(value, "object") ?? _responseObject;

            // The fingerprint is repeated on stream chunks. Keep the latest non-null value in case it changes during the response.
            _systemFingerprint = String(value, "system_fingerprint") ?? _systemFingerprint;

            var choices = value.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                return;
            }

            var choice = choices[0];
            if (Int(choice, "index") is { } index)
            {
                _choiceIndex = index;
            }

            if (String(choice, "finish_reason") is { } finish)
            {
                _finishReason = MapDeepSeekFinishReason.Map(finish);
                _rawFinishReason = finish;
            }

            if (
                choice.TryGetProperty("logprobs", out var logprobs)
                && logprobs.ValueKind == JsonValueKind.Object
            )
            {
                AppendLogprobs(logprobs, "content", _contentLogprobs);
                AppendLogprobs(logprobs, "reasoning_content", _reasoningLogprobs);
            }

            if (
                !choice.TryGetProperty("delta", out var delta)
                || delta.ValueKind != JsonValueKind.Object
            )
            {
                return;
            }

            if (String(delta, "role") is { } role)
            {
                _messageRole = role;
            }

            // enqueue reasoning before text deltas
            if (String(delta, "reasoning_content") is { Length: > 0 } reasoningContent)
            {
                if (!_isActiveReasoning)
                {
                    parts.Add(new ReasoningStartStreamPart("reasoning-0"));
                    _isActiveReasoning = true;
                }

                parts.Add(new ReasoningDeltaStreamPart("reasoning-0", reasoningContent));
            }

            if (String(delta, "content") is { Length: > 0 } text)
            {
                if (!_isActiveText)
                {
                    parts.Add(new TextStartStreamPart("txt-0"));
                    _isActiveText = true;
                }

                // end reasoning when text starts
                if (_isActiveReasoning)
                {
                    parts.Add(new ReasoningEndStreamPart("reasoning-0"));
                    _isActiveReasoning = false;
                }

                parts.Add(new TextDeltaStreamPart("txt-0", text));
            }

            if (
                delta.TryGetProperty("tool_calls", out var toolCalls)
                && toolCalls.ValueKind == JsonValueKind.Array
                && toolCalls.GetArrayLength() > 0
            )
            {
                // end reasoning when tool calls start
                if (_isActiveReasoning)
                {
                    parts.Add(new ReasoningEndStreamPart("reasoning-0"));
                    _isActiveReasoning = false;
                }

                foreach (var toolCallDelta in toolCalls.EnumerateArray())
                {
                    var toolIndex = toolCallDelta.GetProperty("index").GetInt32();
                    if (String(toolCallDelta, "type") is { } type)
                    {
                        _toolCallTypes[toolIndex] = type;
                    }

                    var function = toolCallDelta.GetProperty("function");
                    var input = _toolCallTracker.ProcessDelta(
                        new StreamingToolCallDelta(
                            toolIndex,
                            String(toolCallDelta, "id"),
                            String(function, "name"),
                            String(function, "arguments")
                        )
                    );
                    if (input != null)
                    {
                        if (input.Started)
                        {
                            parts.Add(new ToolInputStartStreamPart(input.Id, input.Name));
                        }

                        if (input.ArgumentsDelta != null)
                        {
                            parts.Add(new ToolInputDeltaStreamPart(input.Id, input.ArgumentsDelta));
                        }
                    }
                }
            }
        }

        private static void AppendLogprobs(JsonElement logprobs, string name, JsonArray target)
        {
            if (
                logprobs.TryGetProperty(name, out var items)
                && items.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var item in items.EnumerateArray())
                {
                    target.Add(JsonNode.Parse(item.GetRawText()));
                }
            }
        }
    }
}
