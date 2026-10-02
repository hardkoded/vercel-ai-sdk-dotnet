// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>User-agent suffix sent by <see cref="OpenAIChatLanguageModel"/>.</summary>
public static class OpenAISdk
{
    /// <summary>Package version used when no build injects one. Matches the upstream test double.</summary>
    public const string Version = "0.0.0-test";

    /// <summary>Value appended to the <c>User-Agent</c> header.</summary>
    public const string UserAgent = "ai-sdk/openai/" + Version;
}

/// <summary>OpenAI API call error raised when a stream fails before any output.</summary>
public sealed class OpenAIApiCallException : ApiException
{
    /// <summary>Creates an API call error.</summary>
    public OpenAIApiCallException(string message, int statusCode, bool isRetryable, string? responseBody)
        : base(message, null, null, statusCode, null, responseBody, null, isRetryable, null)
    {
    }
}

/// <summary>A streamed tool-input start event.</summary>
public sealed class OpenAIToolInputStartStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input start.</summary>
    public OpenAIToolInputStartStreamPart(string id, string toolName)
        : base("tool-input-start")
    {
        Id = id ?? string.Empty;
        ToolName = toolName ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }
}

/// <summary>A streamed tool-argument fragment.</summary>
public sealed class OpenAIToolInputDeltaStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input delta.</summary>
    public OpenAIToolInputDeltaStreamPart(string id, string delta)
        : base("tool-input-delta")
    {
        Id = id ?? string.Empty;
        Delta = delta ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Argument fragment.</summary>
    public string Delta { get; }
}

/// <summary>Marks the end of streamed tool arguments.</summary>
public sealed class OpenAIToolInputEndStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a tool-input end.</summary>
    public OpenAIToolInputEndStreamPart(string id)
        : base("tool-input-end")
    {
        Id = id ?? string.Empty;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }
}

/// <summary>An error frame emitted after the stream has already produced output.</summary>
public sealed class OpenAIStreamErrorPart : LanguageModelStreamPart
{
    /// <summary>Creates a stream error part.</summary>
    public OpenAIStreamErrorPart(string message, int? statusCode, bool isRetryable, string? errorType)
        : base("error")
    {
        Message = message ?? string.Empty;
        StatusCode = statusCode;
        IsRetryable = isRetryable;
        ErrorType = errorType;
    }

    /// <summary>Error message.</summary>
    public string Message { get; }

    /// <summary>HTTP status inferred from the error frame.</summary>
    public int? StatusCode { get; }

    /// <summary>Whether the error is retryable.</summary>
    public bool IsRetryable { get; }

    /// <summary>Provider error type.</summary>
    public string? ErrorType { get; }
}

/// <summary>OpenAI Chat Completions language model.</summary>
public sealed class OpenAIChatLanguageModel : ILanguageModel
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a chat model.</summary>
    public OpenAIChatLanguageModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name + ".chat";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Response headers from the most recent <see cref="DoGenerateAsync"/> or <see cref="DoStreamAsync"/> call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var prepared = Prepare(options, stream: false);
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            _provider.ChatUri(ModelId),
            prepared.Body.ToJsonString(),
            BuildHeaders(options),
            cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = response.Headers;
        return ParseGenerate(response.Body, response.Headers, prepared.Warnings);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var prepared = Prepare(options, stream: true);
        var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var events = _provider.Http.SendSseAsync(
            _provider.ChatUri(ModelId),
            prepared.Body.ToJsonString(),
            BuildHeaders(options),
            responseHeaders,
            cancellationToken);
        await using var enumerator = events.GetAsyncEnumerator(cancellationToken);
        var buffered = new List<string>();
        JsonObject? earlyError = null;
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var data = enumerator.Current;
            buffered.Add(data);
            JsonNode? node;
            try
            {
                node = JsonNode.Parse(data);
            }
            catch (JsonException)
            {
                break;
            }

            if (node is not JsonObject root)
            {
                break;
            }

            var error = ErrorFrame(root);
            if (error != null)
            {
                earlyError = error;
                break;
            }

            if (IsOutputChunk(root))
            {
                break;
            }
        }

        LastResponseHeaders = responseHeaders;
        if (earlyError != null)
        {
            var described = DescribeError(earlyError);
            throw new OpenAIApiCallException(described.Message, described.StatusCode, described.IsRetryable, earlyError.ToJsonString());
        }

        yield return new StreamStartStreamPart(prepared.Warnings);
        var state = new StreamState();
        foreach (var data in buffered)
        {
            foreach (var part in Consume(data, options, state))
            {
                yield return part;
            }
        }

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            foreach (var part in Consume(enumerator.Current, options, state))
            {
                yield return part;
            }
        }

        foreach (var part in Finish(state))
        {
            yield return part;
        }
    }

    private PreparedChatRequest Prepare(LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<CallWarning>();
        var settings = OpenAIChatSettings.Read(options.ProviderOptions);
        var capabilities = OpenAILanguageModelCapabilities.Get(ModelId);
        var reasoningEffort = settings.ReasoningEffort;
        if (reasoningEffort == null && IsCustomReasoning(options.Reasoning))
        {
            reasoningEffort = options.Reasoning;
        }

        if (reasoningEffort != null
            && capabilities.SupportedReasoningEfforts != null
            && !Contains(capabilities.SupportedReasoningEfforts, reasoningEffort))
        {
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "reasoningEffort",
                ModelId + " only supports the following reasoning efforts: " + string.Join(", ", capabilities.SupportedReasoningEfforts),
                null));
            reasoningEffort = null;
        }

        if (options.TopK != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "topK", null, null));
        }

        var isReasoning = settings.ForceReasoning ?? capabilities.IsReasoningModel;
        var systemMode = settings.SystemMessageMode ?? (isReasoning ? "developer" : capabilities.SystemMessageMode);
        var converted = OpenAIChatMessages.ConvertToOpenAIChatMessages(ToPrompt(options.Prompt), systemMode);
        warnings.AddRange(converted.Warnings);

        JsonNode? responseFormat = null;
        if (options.JsonSchema != null)
        {
            var normalized = NormalizeOpenAIJsonSchema.Normalize(JsonNode.Parse(options.JsonSchema.Value.GetRawText())!);
            warnings.AddRange(normalized.Warnings);
            var schema = new JsonObject
            {
                ["schema"] = normalized.Schema,
                ["strict"] = settings.StrictJsonSchema ?? true,
                ["name"] = string.IsNullOrEmpty(options.JsonSchemaName) ? "response" : options.JsonSchemaName,
            };
            if (!string.IsNullOrEmpty(settings.SchemaDescription))
            {
                schema["description"] = settings.SchemaDescription;
            }

            responseFormat = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = schema,
            };
        }
        else if (settings.ResponseFormat == "json")
        {
            responseFormat = new JsonObject { ["type"] = "json_object" };
        }

        bool? logprobs = null;
        int? topLogprobs = null;
        if (settings.LogprobsNumber != null)
        {
            logprobs = true;
            topLogprobs = settings.LogprobsNumber;
        }
        else if (settings.LogprobsBool == true)
        {
            logprobs = true;
            topLogprobs = 0;
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["messages"] = converted.Messages,
        };
        Set(body, "logit_bias", settings.LogitBias);
        if (logprobs == true)
        {
            body["logprobs"] = true;
        }

        if (topLogprobs != null)
        {
            body["top_logprobs"] = topLogprobs.Value;
        }

        Set(body, "user", settings.User);
        if (settings.ParallelToolCalls != null)
        {
            body["parallel_tool_calls"] = settings.ParallelToolCalls.Value;
        }

        if (options.MaxOutputTokens != null)
        {
            body["max_tokens"] = options.MaxOutputTokens.Value;
        }

        if (options.Temperature != null)
        {
            body["temperature"] = options.Temperature.Value;
        }

        if (options.TopP != null)
        {
            body["top_p"] = options.TopP.Value;
        }

        if (options.FrequencyPenalty != null)
        {
            body["frequency_penalty"] = options.FrequencyPenalty.Value;
        }

        if (options.PresencePenalty != null)
        {
            body["presence_penalty"] = options.PresencePenalty.Value;
        }

        if (responseFormat != null)
        {
            body["response_format"] = responseFormat;
        }

        if (options.StopSequences != null)
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop"] = stop;
        }

        if (options.Seed != null)
        {
            body["seed"] = options.Seed.Value;
        }

        Set(body, "verbosity", settings.TextVerbosity);
        if (settings.MaxCompletionTokens != null)
        {
            body["max_completion_tokens"] = settings.MaxCompletionTokens.Value;
        }

        if (settings.Store != null)
        {
            body["store"] = settings.Store.Value;
        }

        Set(body, "metadata", settings.Metadata);
        Set(body, "prediction", settings.Prediction);
        Set(body, "reasoning_effort", reasoningEffort);
        Set(body, "service_tier", settings.ServiceTier);
        Set(body, "prompt_cache_key", settings.PromptCacheKey);
        Set(body, "prompt_cache_options", settings.PromptCacheOptions);
        Set(body, "prompt_cache_retention", settings.PromptCacheRetention);
        Set(body, "safety_identifier", settings.SafetyIdentifier);

        if (capabilities.SupportedReasoningEfforts != null && body["prompt_cache_retention"] != null)
        {
            body.Remove("prompt_cache_retention");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "promptCacheRetention",
                "promptCacheRetention is not supported by GPT-6 and later models; use promptCacheOptions instead",
                null));
        }

        if (isReasoning)
        {
            var keepSampling = reasoningEffort == "none" && capabilities.SupportsNonReasoningParameters;
            if (!keepSampling)
            {
                RemoveUnsupported(body, warnings, "temperature", "temperature", "temperature is not supported for reasoning models", "unsupported");
                RemoveUnsupported(body, warnings, "top_p", "topP", "topP is not supported for reasoning models", "unsupported");
                if (body["logprobs"] != null)
                {
                    body.Remove("logprobs");
                    warnings.Add(new OpenAICallWarning("other", null, null, "logprobs is not supported for reasoning models"));
                }
            }

            RemoveUnsupported(body, warnings, "frequency_penalty", "frequencyPenalty", "frequencyPenalty is not supported for reasoning models", "unsupported");
            RemoveUnsupported(body, warnings, "presence_penalty", "presencePenalty", "presencePenalty is not supported for reasoning models", "unsupported");
            if (body["logit_bias"] != null)
            {
                body.Remove("logit_bias");
                warnings.Add(new OpenAICallWarning("other", null, null, "logitBias is not supported for reasoning models"));
            }

            if (body["top_logprobs"] != null)
            {
                body.Remove("top_logprobs");
                warnings.Add(new OpenAICallWarning("other", null, null, "topLogprobs is not supported for reasoning models"));
            }

            if (body["max_tokens"] != null)
            {
                if (body["max_completion_tokens"] == null)
                {
                    body["max_completion_tokens"] = body["max_tokens"]!.DeepClone();
                }

                body.Remove("max_tokens");
            }
        }
        else if (ModelId.StartsWith("gpt-4o-search-preview", StringComparison.Ordinal)
            || ModelId.StartsWith("gpt-4o-mini-search-preview", StringComparison.Ordinal))
        {
            RemoveUnsupported(
                body,
                warnings,
                "temperature",
                "temperature",
                "temperature is not supported for the search preview models and has been removed.",
                "unsupported");
        }

        if (settings.ServiceTier == "flex" && !capabilities.SupportsFlexProcessing)
        {
            body.Remove("service_tier");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "serviceTier",
                "flex processing is only available for o3, o4-mini, and gpt-5 models",
                null));
        }

        if ((settings.ServiceTier == "priority" || settings.ServiceTier == "fast") && !capabilities.SupportsPriorityProcessing)
        {
            body.Remove("service_tier");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "serviceTier",
                "priority processing is only available for supported models (gpt-4, gpt-5, gpt-5-mini, o3, o4-mini) and requires Enterprise access. gpt-5-nano is not supported",
                null));
        }

        var tools = OpenAIChatTools.Prepare(options.Tools, options.ToolChoice);
        warnings.AddRange(tools.Warnings);
        if (tools.Tools != null)
        {
            body["tools"] = tools.Tools;
        }

        if (tools.ToolChoice != null)
        {
            body["tool_choice"] = tools.ToolChoice;
        }

        if (stream)
        {
            body["stream"] = true;
            body["stream_options"] = new JsonObject { ["include_usage"] = true };
        }

        return new PreparedChatRequest(body, warnings);
    }

    private Dictionary<string, string?> BuildHeaders(LanguageModelCallOptions options)
    {
        var headers = _provider.CreateHeaders();
        if (_provider.Options is OpenAIOptions openAI)
        {
            if (!string.IsNullOrEmpty(openAI.Organization))
            {
                headers["OpenAI-Organization"] = openAI.Organization;
            }

            if (!string.IsNullOrEmpty(openAI.Project))
            {
                headers["OpenAI-Project"] = openAI.Project;
            }
        }

        var userAgent = OpenAISdk.UserAgent;
        if (headers.TryGetValue("User-Agent", out var existing) && !string.IsNullOrEmpty(existing))
        {
            userAgent = existing + " " + OpenAISdk.UserAgent;
        }

        headers["User-Agent"] = userAgent;
        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    headers[pair.Key] = pair.Value;
                }
            }
        }

        return headers;
    }

    private static List<OpenAIChatPromptMessage> ToPrompt(IReadOnlyList<ModelMessage>? prompt)
    {
        var messages = new List<OpenAIChatPromptMessage>();
        if (prompt == null)
        {
            return messages;
        }

        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    messages.Add(new OpenAISystemChatMessage(system.Content));
                    break;
                case UserModelMessage user:
                    var parts = new List<OpenAIChatUserPart>();
                    foreach (var part in user.Content)
                    {
                        if (part is TextContentPart text)
                        {
                            parts.Add(new OpenAIChatTextPart(text.Text));
                        }
                        else if (part is FileContentPart file)
                        {
                            OpenAIChatFileData data;
                            if (!string.IsNullOrEmpty(file.Url))
                            {
                                data = OpenAIChatFileData.FromUrl(file.Url!);
                            }
                            else if (file.Data != null)
                            {
                                data = OpenAIChatFileData.FromBytes(file.Data);
                            }
                            else
                            {
                                throw new AiSdkException("file part requires a URL or inline bytes");
                            }

                            parts.Add(new OpenAIChatFilePart(file.MediaType, data, file.FileName));
                        }
                    }

                    messages.Add(new OpenAIUserChatMessage(parts));
                    break;
                case AssistantModelMessage assistant:
                    var assistantParts = new List<OpenAIAssistantChatPart>();
                    if (assistant.Text != null)
                    {
                        assistantParts.Add(new OpenAIAssistantTextPart(assistant.Text));
                    }

                    foreach (var call in assistant.ToolCalls)
                    {
                        assistantParts.Add(new OpenAIAssistantToolCallPart(call.ToolCallId, call.ToolName, ParseToolInput(call.ArgumentsJson)));
                    }

                    messages.Add(new OpenAIAssistantChatMessage(assistantParts));
                    break;
                case ToolModelMessage tool:
                    messages.Add(new OpenAIToolChatMessage(new[] { new OpenAIToolResultChatPart("tool-result", tool.ToolCallId, tool.ToolName, ParseToolOutput(tool), tool.ProviderMetadata) }));
                    break;
                default:
                    throw new AiSdkException("Unsupported message role '" + message.Role + "'.");
            }
        }

        return messages;
    }

    private static JsonNode? ParseToolInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(json);
            return node is JsonObject ? node : JsonValue.Create(json);
        }
        catch (JsonException)
        {
            return JsonValue.Create(json);
        }
    }

    private static OpenAIToolOutput ParseToolOutput(ToolModelMessage tool)
    {
        try
        {
            var node = JsonNode.Parse(tool.OutputJson);
            if (tool.IsError)
            {
                return OpenAIToolOutput.FromErrorJson(node);
            }

            return OpenAIToolOutput.FromJson(node);
        }
        catch (JsonException)
        {
            return tool.IsError
                ? OpenAIToolOutput.FromErrorText(tool.OutputJson)
                : OpenAIToolOutput.FromText(tool.OutputJson);
        }
    }

    private LanguageModelGenerateResult ParseGenerate(string body, IReadOnlyDictionary<string, string> headers, IReadOnlyList<CallWarning> warnings)
    {
        var root = JsonNode.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body) as JsonObject
            ?? throw new AiSdkException("Response did not contain any choices.");
        if (root["choices"] is not JsonArray choices || choices.Count == 0 || choices[0] is not JsonObject choice)
        {
            throw new AiSdkException("Response did not contain any choices.");
        }

        var message = choice["message"] as JsonObject ?? new JsonObject();
        var content = new List<GeneratedContent>();
        var text = StringOrNull(message["content"]);
        if (string.IsNullOrEmpty(text) && message["audio"] is JsonObject audio)
        {
            text = StringOrNull(audio["transcript"]);
        }

        if (!string.IsNullOrEmpty(text))
        {
            content.Add(new GeneratedText(text!));
        }

        if (message["tool_calls"] is JsonArray toolCalls)
        {
            foreach (var item in toolCalls)
            {
                if (item is not JsonObject call || call["function"] is not JsonObject function)
                {
                    continue;
                }

                var id = StringOrNull(call["id"]);
                if (string.IsNullOrEmpty(id))
                {
                    id = JsonValues.GenerateId("call_");
                }

                content.Add(new GeneratedToolCall(
                    id!,
                    StringOrNull(function["name"]) ?? string.Empty,
                    StringOrNull(function["arguments"]) ?? "{}"));
            }
        }

        if (message["annotations"] is JsonArray annotations)
        {
            foreach (var item in annotations)
            {
                if (item is JsonObject annotation && annotation["url_citation"] is JsonObject citation)
                {
                    content.Add(new GeneratedSource(
                        JsonValues.GenerateId("src_"),
                        StringOrNull(citation["url"]) ?? string.Empty,
                        StringOrNull(citation["title"])));
                }
            }
        }

        var metadata = new JsonObject { ["openai"] = ProviderMetadata(root["usage"] as JsonObject, choice) };
        var rawFinish = StringOrNull(choice["finish_reason"]);
        return new LanguageModelGenerateResult(
            content,
            MapFinishReason(rawFinish),
            ConvertUsage(root["usage"] as JsonObject),
            rawFinish,
            warnings,
            StringOrNull(root["id"]),
            JsonDocument.Parse(metadata.ToJsonString()).RootElement.Clone(),
            body,
            StringOrNull(root["model"]),
            Timestamp(root),
            headers);
    }

    private static List<LanguageModelStreamPart> Consume(string data, LanguageModelCallOptions options, StreamState state)
    {
        var parts = new List<LanguageModelStreamPart>();
        if (options.IncludeRawChunks)
        {
            parts.Add(new RawStreamPart(data));
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(data);
        }
        catch (JsonException)
        {
            state.FinishReason = FinishReason.Error;
            state.RawFinish = null;
            parts.Add(new ErrorStreamPart("JSON parsing failed: Text: " + data + "."));
            return parts;
        }

        if (node is not JsonObject root)
        {
            state.FinishReason = FinishReason.Error;
            parts.Add(new ErrorStreamPart("JSON parsing failed: Text: " + data + "."));
            return parts;
        }

        var error = ErrorFrame(root);
        if (error != null)
        {
            var described = DescribeError(error);
            state.FinishReason = FinishReason.Error;
            state.RawFinish = null;
            parts.Add(new OpenAIStreamErrorPart(described.Message, described.StatusCode, described.IsRetryable, described.Type));
            return parts;
        }

        if (!state.MetadataExtracted && TryMetadata(root, out var id, out var modelId, out var timestamp))
        {
            state.MetadataExtracted = true;
            parts.Add(new ResponseMetadataStreamPart(id, modelId, timestamp));
        }

        if (root["usage"] is JsonObject usage)
        {
            state.Usage = usage;
            AddPredictionTokens(usage, state.ProviderMetadata);
        }

        if (root["choices"] is not JsonArray choices || choices.Count == 0 || choices[0] is not JsonObject choice)
        {
            return parts;
        }

        var rawFinish = StringOrNull(choice["finish_reason"]);
        if (rawFinish != null)
        {
            state.RawFinish = rawFinish;
            state.FinishReason = MapFinishReason(rawFinish);
        }

        if (choice["logprobs"] is JsonObject logprobs && logprobs["content"] is JsonNode logprobContent && logprobContent.GetValueKind() != JsonValueKind.Null)
        {
            state.ProviderMetadata["logprobs"] = JsonNode.Parse(logprobContent.ToJsonString());
        }

        if (choice["delta"] is not JsonObject delta)
        {
            return parts;
        }

        if (delta["content"] is JsonValue content && content.TryGetValue<string>(out var text) && text != null)
        {
            if (!state.TextActive)
            {
                parts.Add(new TextStartStreamPart("0"));
                state.TextActive = true;
            }

            parts.Add(new TextDeltaStreamPart("0", text));
        }

        if (delta["tool_calls"] is JsonArray toolDeltas)
        {
            foreach (var item in toolDeltas)
            {
                if (item is JsonObject tool)
                {
                    parts.AddRange(state.Tools.ProcessDelta(tool));
                }
            }
        }

        if (delta["annotations"] is JsonArray annotations)
        {
            foreach (var item in annotations)
            {
                if (item is JsonObject annotation && annotation["url_citation"] is JsonObject citation)
                {
                    parts.Add(new SourceStreamPart(
                        JsonValues.GenerateId("src_"),
                        StringOrNull(citation["url"]) ?? string.Empty,
                        StringOrNull(citation["title"])));
                }
            }
        }

        return parts;
    }

    private static List<LanguageModelStreamPart> Finish(StreamState state)
    {
        var parts = new List<LanguageModelStreamPart>();
        if (state.TextActive)
        {
            parts.Add(new TextEndStreamPart("0"));
        }

        parts.AddRange(state.Tools.Flush());
        var metadata = new JsonObject { ["openai"] = state.ProviderMetadata };
        parts.Add(new FinishStreamPart(
            state.FinishReason,
            ConvertUsage(state.Usage),
            state.RawFinish,
            JsonDocument.Parse(metadata.ToJsonString()).RootElement.Clone()));
        return parts;
    }

    private static bool IsOutputChunk(JsonObject root)
    {
        if (root["error"] != null)
        {
            return false;
        }

        if (root["choices"] is not JsonArray choices)
        {
            return false;
        }

        foreach (var item in choices)
        {
            if (item is not JsonObject choice || choice["delta"] is not JsonObject delta)
            {
                continue;
            }

            if (delta["content"] is JsonValue content && content.TryGetValue<string>(out var text) && text != null && text.Length > 0)
            {
                return true;
            }

            if (delta["tool_calls"] is JsonArray tools && tools.Count > 0)
            {
                return true;
            }

            if (delta["annotations"] is JsonArray notes && notes.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static JsonObject? ErrorFrame(JsonObject root)
    {
        return root["error"] as JsonObject;
    }

    private static ErrorDescription DescribeError(JsonObject error)
    {
        var message = StringOrNull(error["message"]) ?? "OpenAI stream failed before any output was generated";
        var type = StringOrNull(error["type"]);
        var codeText = StringOrNull(error["code"]);
        int? codeNumber = null;
        if (error["code"] is JsonValue codeValue && codeValue.TryGetValue<int>(out var number))
        {
            codeNumber = number;
        }

        var status = StatusCode(codeNumber, codeText, type);
        var retryable = !(codeText == "insufficient_quota" || type == "insufficient_quota") && IsRetryableStatus(status);
        return new ErrorDescription(message, status, retryable, type);
    }

    private static int StatusCode(int? codeNumber, string? codeText, string? type)
    {
        if (codeNumber != null && codeNumber.Value >= 400 && codeNumber.Value <= 599)
        {
            return codeNumber.Value;
        }

        if (codeText != null && codeText.Length == 3 && int.TryParse(codeText, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && parsed >= 400 && parsed <= 599)
        {
            return parsed;
        }

        var discriminator = ((codeText ?? string.Empty) + " " + (type ?? string.Empty)).ToLowerInvariant();
        if (discriminator.IndexOf("insufficient_quota", StringComparison.Ordinal) >= 0 || discriminator.IndexOf("rate_limit", StringComparison.Ordinal) >= 0)
        {
            return 429;
        }

        if (discriminator.IndexOf("authentication", StringComparison.Ordinal) >= 0)
        {
            return 401;
        }

        if (discriminator.IndexOf("permission", StringComparison.Ordinal) >= 0)
        {
            return 403;
        }

        if (discriminator.IndexOf("not_found", StringComparison.Ordinal) >= 0)
        {
            return 404;
        }

        if (discriminator.IndexOf("invalid", StringComparison.Ordinal) >= 0
            || discriminator.IndexOf("bad_request", StringComparison.Ordinal) >= 0
            || discriminator.IndexOf("context_length", StringComparison.Ordinal) >= 0)
        {
            return 400;
        }

        if (discriminator.IndexOf("overload", StringComparison.Ordinal) >= 0)
        {
            return 503;
        }

        if (discriminator.IndexOf("timeout", StringComparison.Ordinal) >= 0)
        {
            return 504;
        }

        return 500;
    }

    private static bool IsRetryableStatus(int statusCode)
    {
        return statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500;
    }

    private static void AddPredictionTokens(JsonObject usage, JsonObject metadata)
    {
        if (usage["completion_tokens_details"] is not JsonObject details)
        {
            return;
        }

        if (details["accepted_prediction_tokens"] is JsonValue accepted && accepted.TryGetValue<int>(out var acceptedTokens))
        {
            metadata["acceptedPredictionTokens"] = acceptedTokens;
        }

        if (details["rejected_prediction_tokens"] is JsonValue rejected && rejected.TryGetValue<int>(out var rejectedTokens))
        {
            metadata["rejectedPredictionTokens"] = rejectedTokens;
        }
    }

    private static JsonObject ProviderMetadata(JsonObject? usage, JsonObject choice)
    {
        var metadata = new JsonObject();
        if (usage != null)
        {
            AddPredictionTokens(usage, metadata);
        }

        if (choice["logprobs"] is JsonObject logprobs && logprobs["content"] is JsonNode content && content.GetValueKind() != JsonValueKind.Null)
        {
            metadata["logprobs"] = JsonNode.Parse(content.ToJsonString());
        }

        return metadata;
    }

    private static LanguageModelUsage ConvertUsage(JsonObject? usage)
    {
        if (usage == null)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var prompt = ReadInt(usage, "prompt_tokens") ?? 0;
        var completion = ReadInt(usage, "completion_tokens") ?? 0;
        var total = ReadInt(usage, "total_tokens");
        var promptDetails = usage["prompt_tokens_details"] as JsonObject;
        var completionDetails = usage["completion_tokens_details"] as JsonObject;
        var cacheRead = promptDetails == null ? 0 : ReadInt(promptDetails, "cached_tokens") ?? 0;
        int? cacheWrite = null;
        if (promptDetails != null && promptDetails.ContainsKey("cache_write_tokens"))
        {
            cacheWrite = ReadInt(promptDetails, "cache_write_tokens");
        }

        var reasoning = completionDetails == null ? 0 : ReadInt(completionDetails, "reasoning_tokens") ?? 0;
        JsonElement? raw = JsonDocument.Parse(usage.ToJsonString()).RootElement.Clone();
        return new LanguageModelUsage(prompt, completion, total, cacheRead, cacheWrite, reasoning, raw);
    }

    private static FinishReason MapFinishReason(string? finishReason)
    {
        switch (finishReason)
        {
            case "stop":
                return FinishReason.Stop;
            case "length":
                return FinishReason.Length;
            case "content_filter":
                return FinishReason.ContentFilter;
            case "function_call":
            case "tool_calls":
                return FinishReason.ToolCalls;
            default:
                return FinishReason.Other;
        }
    }

    private static bool TryMetadata(JsonObject root, out string? id, out string? modelId, out DateTimeOffset? timestamp)
    {
        id = StringOrNull(root["id"]);
        modelId = StringOrNull(root["model"]);
        timestamp = Timestamp(root);
        return !string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(modelId) || timestamp != null;
    }

    private static DateTimeOffset? Timestamp(JsonObject root)
    {
        if (root["created"] is not JsonValue created || !created.TryGetValue<long>(out var seconds) || seconds == 0)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    private static void RemoveUnsupported(JsonObject body, List<CallWarning> warnings, string propertyName, string feature, string details, string type)
    {
        if (body[propertyName] == null)
        {
            return;
        }

        body.Remove(propertyName);
        warnings.Add(new OpenAICallWarning(type, feature, details, null));
    }

    private static void Set(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Set(JsonObject body, string name, JsonNode? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static bool IsCustomReasoning(string? reasoning)
    {
        return reasoning != null && reasoning != "provider-default";
    }

    private static bool Contains(IReadOnlyList<string> values, string value)
    {
        foreach (var item in values)
        {
            if (item == value)
            {
                return true;
            }
        }

        return false;
    }

    private static string? StringOrNull(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private static int? ReadInt(JsonObject obj, string name)
    {
        if (obj[name] is JsonValue value && value.TryGetValue<int>(out var number))
        {
            return number;
        }

        return null;
    }

    private sealed class PreparedChatRequest
    {
        public PreparedChatRequest(JsonObject body, List<CallWarning> warnings)
        {
            Body = body;
            Warnings = warnings;
        }

        public JsonObject Body { get; }

        public List<CallWarning> Warnings { get; }
    }

    private sealed class ErrorDescription
    {
        public ErrorDescription(string message, int statusCode, bool isRetryable, string? type)
        {
            Message = message;
            StatusCode = statusCode;
            IsRetryable = isRetryable;
            Type = type;
        }

        public string Message { get; }

        public int StatusCode { get; }

        public bool IsRetryable { get; }

        public string? Type { get; }
    }

    private sealed class StreamState
    {
        public bool MetadataExtracted { get; set; }

        public bool TextActive { get; set; }

        public FinishReason FinishReason { get; set; } = FinishReason.Other;

        public string? RawFinish { get; set; }

        public JsonObject? Usage { get; set; }

        public JsonObject ProviderMetadata { get; } = new JsonObject();

        public OpenAIChatToolTracker Tools { get; } = new OpenAIChatToolTracker();
    }
}

/// <summary>Prepares Chat Completions function tools.</summary>
public static class OpenAIChatTools
{
    /// <summary>Prepared tools and tool choice.</summary>
    public sealed class Prepared
    {
        internal Prepared(JsonArray? tools, JsonNode? toolChoice, IReadOnlyList<CallWarning> warnings)
        {
            Tools = tools;
            ToolChoice = toolChoice;
            Warnings = warnings;
        }

        /// <summary>Tool array, or null when the request has no tools.</summary>
        public JsonArray? Tools { get; }

        /// <summary>Tool choice value, or null when unset.</summary>
        public JsonNode? ToolChoice { get; }

        /// <summary>Warnings produced while preparing tools.</summary>
        public IReadOnlyList<CallWarning> Warnings { get; }
    }

    /// <summary>Prepares function tools. An empty list is treated as no tools.</summary>
    public static Prepared Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
    {
        if (tools == null || tools.Count == 0)
        {
            return new Prepared(null, null, Array.Empty<CallWarning>());
        }

        var warnings = new List<CallWarning>();
        var prepared = new JsonArray();
        foreach (var tool in tools)
        {
            var normalized = NormalizeOpenAIJsonSchema.Normalize(JsonNode.Parse(tool.InputSchema.GetRawText())!);
            warnings.AddRange(normalized.Warnings);
            var function = new JsonObject
            {
                ["name"] = tool.Name,
                ["parameters"] = normalized.Schema,
            };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            if (tool.Strict != null)
            {
                function["strict"] = tool.Strict.Value;
            }

            prepared.Add(new JsonObject
            {
                ["type"] = "function",
                ["function"] = function,
            });
        }

        JsonNode? choice = null;
        if (toolChoice != null)
        {
            if (toolChoice.Type == "auto" || toolChoice.Type == "none" || toolChoice.Type == "required")
            {
                choice = toolChoice.Type;
            }
            else if (toolChoice is ToolChoice.NamedChoice named)
            {
                choice = new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject { ["name"] = named.ToolName },
                };
            }
            else
            {
                throw new AiSdkException("tool choice type: " + toolChoice.Type);
            }
        }

        return new Prepared(prepared, choice, warnings);
    }
}

internal sealed class OpenAIChatSettings
{
    public JsonNode? LogitBias { get; private set; }

    public bool? LogprobsBool { get; private set; }

    public int? LogprobsNumber { get; private set; }

    public bool? ParallelToolCalls { get; private set; }

    public string? User { get; private set; }

    public string? ReasoningEffort { get; private set; }

    public int? MaxCompletionTokens { get; private set; }

    public bool? Store { get; private set; }

    public JsonNode? Metadata { get; private set; }

    public JsonNode? Prediction { get; private set; }

    public string? ServiceTier { get; private set; }

    public bool? StrictJsonSchema { get; private set; }

    public string? TextVerbosity { get; private set; }

    public string? PromptCacheKey { get; private set; }

    public JsonNode? PromptCacheOptions { get; private set; }

    public string? PromptCacheRetention { get; private set; }

    public string? SafetyIdentifier { get; private set; }

    public string? SystemMessageMode { get; private set; }

    public bool? ForceReasoning { get; private set; }

    public string? SchemaDescription { get; private set; }

    public string? ResponseFormat { get; private set; }

    public static OpenAIChatSettings Read(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var settings = new OpenAIChatSettings();
        if (providerOptions == null || !providerOptions.TryGetValue("openai", out var openai) || openai.ValueKind != JsonValueKind.Object)
        {
            return settings;
        }

        if (openai.TryGetProperty("logitBias", out var logitBias) && logitBias.ValueKind == JsonValueKind.Object)
        {
            settings.LogitBias = JsonNode.Parse(logitBias.GetRawText());
        }

        if (openai.TryGetProperty("logprobs", out var logprobs))
        {
            if (logprobs.ValueKind == JsonValueKind.Number && logprobs.TryGetInt32(out var count))
            {
                settings.LogprobsNumber = count;
            }
            else if (logprobs.ValueKind == JsonValueKind.True || logprobs.ValueKind == JsonValueKind.False)
            {
                settings.LogprobsBool = logprobs.GetBoolean();
            }
        }

        settings.ParallelToolCalls = Bool(openai, "parallelToolCalls");
        settings.User = Text(openai, "user");
        settings.ReasoningEffort = Text(openai, "reasoningEffort");
        settings.MaxCompletionTokens = Int(openai, "maxCompletionTokens");
        settings.Store = Bool(openai, "store");
        settings.Metadata = Object(openai, "metadata");
        settings.Prediction = Object(openai, "prediction");
        settings.ServiceTier = Text(openai, "serviceTier");
        settings.StrictJsonSchema = Bool(openai, "strictJsonSchema");
        settings.TextVerbosity = Text(openai, "textVerbosity");
        settings.PromptCacheKey = Text(openai, "promptCacheKey");
        settings.PromptCacheOptions = Object(openai, "promptCacheOptions");
        settings.PromptCacheRetention = Text(openai, "promptCacheRetention");
        settings.SafetyIdentifier = Text(openai, "safetyIdentifier");
        settings.SystemMessageMode = Text(openai, "systemMessageMode");
        settings.ForceReasoning = Bool(openai, "forceReasoning");
        settings.SchemaDescription = Text(openai, "schemaDescription");
        settings.ResponseFormat = Text(openai, "responseFormat");
        return settings;
    }

    private static string? Text(JsonElement openai, string name)
    {
        return openai.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static bool? Bool(JsonElement openai, string name)
    {
        if (!openai.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        {
            return value.GetBoolean();
        }

        return null;
    }

    private static int? Int(JsonElement openai, string name)
    {
        return openai.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private static JsonNode? Object(JsonElement openai, string name)
    {
        return openai.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object
            ? JsonNode.Parse(value.GetRawText())
            : null;
    }
}

internal sealed class OpenAIChatToolTracker
{
    private readonly List<Tracked> _calls = new List<Tracked>();
    private readonly Dictionary<string, Tracked> _byId = new Dictionary<string, Tracked>(StringComparer.Ordinal);
    private readonly Dictionary<int, Tracked> _byIndex = new Dictionary<int, Tracked>();
    private Tracked? _latest;

    public List<LanguageModelStreamPart> ProcessDelta(JsonObject tool)
    {
        var id = StringOrNull(tool["id"]);
        int? index = null;
        if (tool["index"] is JsonValue indexValue && indexValue.TryGetValue<int>(out var parsedIndex))
        {
            index = parsedIndex;
        }

        Tracked? call = null;
        if (!string.IsNullOrEmpty(id))
        {
            _byId.TryGetValue(id!, out call);
        }
        else if (index != null)
        {
            _byIndex.TryGetValue(index.Value, out call);
        }
        else
        {
            call = _latest;
        }

        var parts = call == null ? New(tool, id) : Existing(call, tool);
        if (index != null && _latest != null)
        {
            _byIndex[index.Value] = _latest;
        }

        return parts;
    }

    public List<LanguageModelStreamPart> Flush()
    {
        var parts = new List<LanguageModelStreamPart>();
        foreach (var call in _calls)
        {
            if (!call.Finished)
            {
                parts.Add(new OpenAIToolInputEndStreamPart(call.Id));
                parts.Add(new ToolCallStreamPart(call.Id, call.Name, call.Arguments));
                call.Finished = true;
            }
        }

        return parts;
    }

    private List<LanguageModelStreamPart> New(JsonObject tool, string? id)
    {
        if (tool["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var type) && type != "function")
        {
            throw new AiSdkException("Expected 'function' type.");
        }

        if (id == null)
        {
            throw new AiSdkException("Expected 'id' to be a string.");
        }

        var function = tool["function"] as JsonObject;
        var name = function == null ? null : StringOrNull(function["name"]);
        if (name == null)
        {
            throw new AiSdkException("Expected 'function.name' to be a string.");
        }

        var arguments = function != null && function["arguments"] is JsonValue argumentValue && argumentValue.TryGetValue<string>(out var argumentText)
            ? argumentText ?? string.Empty
            : string.Empty;
        var call = new Tracked(id, name, arguments);
        _calls.Add(call);
        if (id.Length > 0)
        {
            _byId[id] = call;
        }

        _latest = call;
        var parts = new List<LanguageModelStreamPart> { new OpenAIToolInputStartStreamPart(id, name) };
        if (arguments.Length > 0)
        {
            parts.Add(new OpenAIToolInputDeltaStreamPart(id, arguments));
        }

        return parts;
    }

    private List<LanguageModelStreamPart> Existing(Tracked call, JsonObject tool)
    {
        _latest = call;
        var parts = new List<LanguageModelStreamPart>();
        if (call.Finished)
        {
            return parts;
        }

        if (tool["function"] is JsonObject function
            && function["arguments"] is JsonValue argumentValue
            && argumentValue.TryGetValue<string>(out var argumentText)
            && argumentText != null)
        {
            call.Arguments += argumentText;
            parts.Add(new OpenAIToolInputDeltaStreamPart(call.Id, argumentText));
        }

        return parts;
    }

    private static string? StringOrNull(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private sealed class Tracked
    {
        public Tracked(string id, string name, string arguments)
        {
            Id = id;
            Name = name;
            Arguments = arguments;
        }

        public string Id { get; }

        public string Name { get; }

        public string Arguments { get; set; }

        public bool Finished { get; set; }
    }
}
