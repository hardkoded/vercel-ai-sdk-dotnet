// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Chat Completions request body and the warnings produced while building it.</summary>
public sealed class OpenAIChatPreparedRequest
{
    internal OpenAIChatPreparedRequest(JsonObject body, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Body = body;
        Warnings = warnings;
    }

    /// <summary>JSON body sent to <c>/chat/completions</c>.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>OpenAI Chat Completions language model.</summary>
public sealed class OpenAIChatLanguageModel : ILanguageModel
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a Chat Completions model.</summary>
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

    /// <summary>Builds the Chat Completions body for <paramref name="modelId"/>.</summary>
    public static OpenAIChatPreparedRequest Prepare(string modelId, LanguageModelCallOptions options)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<OpenAICallWarning>();
        var openai = OpenAIJson.Provider(options.ProviderOptions);
        var capabilities = OpenAILanguageModelCapabilities.ForModel(modelId);
        var effort = ResolveEffort(openai, options.Reasoning, capabilities, modelId, warnings);
        var reasoning = OpenAIJson.Bool(openai, "forceReasoning") ?? capabilities.IsReasoningModel;
        if (options.TopK != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "topK", null));
        }

        var systemMode = OpenAIJson.String(openai, "systemMessageMode")
            ?? (reasoning ? "developer" : capabilities.SystemMessageMode);
        var converted = OpenAIChatMessages.Convert(options.Prompt, systemMode);
        warnings.AddRange(converted.Warnings);

        var strictJsonSchema = OpenAIJson.Bool(openai, "strictJsonSchema") ?? true;
        var body = new JsonObject
        {
            ["model"] = modelId,
            ["messages"] = converted.Messages,
        };
        Copy(body, "logit_bias", OpenAIJson.Node(OpenAIJson.Child(openai, "logitBias")));
        var logprobs = ReadLogprobs(openai);
        if (logprobs.Enabled)
        {
            body["logprobs"] = true;
        }

        if (logprobs.Top != null)
        {
            body["top_logprobs"] = logprobs.Top.Value;
        }

        Copy(body, "user", OpenAIJson.String(openai, "user"));
        if (OpenAIJson.Bool(openai, "parallelToolCalls") is { } parallel)
        {
            body["parallel_tool_calls"] = parallel;
        }

        if (options.MaxOutputTokens is { } maxOutput)
        {
            body["max_tokens"] = maxOutput;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequency_penalty"] = frequency;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presence_penalty"] = presence;
        }

        ApplyResponseFormat(body, warnings, options, openai, strictJsonSchema);
        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop"] = stop;
        }

        if (options.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        Copy(body, "verbosity", OpenAIJson.String(openai, "textVerbosity"));
        if (OpenAIJson.Int(openai, "maxCompletionTokens") is { } maxCompletion)
        {
            body["max_completion_tokens"] = maxCompletion;
        }

        if (OpenAIJson.Bool(openai, "store") is { } store)
        {
            body["store"] = store;
        }

        Copy(body, "metadata", OpenAIJson.Node(OpenAIJson.Child(openai, "metadata")));
        Copy(body, "prediction", OpenAIJson.Node(OpenAIJson.Child(openai, "prediction")));
        Copy(body, "reasoning_effort", effort);
        Copy(body, "service_tier", OpenAIJson.String(openai, "serviceTier"));
        Copy(body, "prompt_cache_key", OpenAIJson.String(openai, "promptCacheKey"));
        Copy(body, "prompt_cache_options", OpenAIJson.Node(OpenAIJson.Child(openai, "promptCacheOptions")));
        Copy(body, "prompt_cache_retention", OpenAIJson.String(openai, "promptCacheRetention"));
        Copy(body, "safety_identifier", OpenAIJson.String(openai, "safetyIdentifier"));

        if (capabilities.SupportedReasoningEfforts != null && body["prompt_cache_retention"] != null)
        {
            body.Remove("prompt_cache_retention");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "promptCacheRetention",
                "promptCacheRetention is not supported by GPT-6 and later models; use promptCacheOptions instead"));
        }

        if (reasoning)
        {
            var keepSampling = effort == "none" && capabilities.SupportsNonReasoningParameters;
            if (!keepSampling)
            {
                Remove(body, warnings, "temperature", "unsupported", "temperature", "temperature is not supported for reasoning models");
                Remove(body, warnings, "top_p", "unsupported", "topP", "topP is not supported for reasoning models");
                Remove(body, warnings, "logprobs", "other", null, null, "logprobs is not supported for reasoning models");
            }

            Remove(body, warnings, "frequency_penalty", "unsupported", "frequencyPenalty", "frequencyPenalty is not supported for reasoning models");
            Remove(body, warnings, "presence_penalty", "unsupported", "presencePenalty", "presencePenalty is not supported for reasoning models");
            Remove(body, warnings, "logit_bias", "other", null, null, "logitBias is not supported for reasoning models");
            Remove(body, warnings, "top_logprobs", "other", null, null, "topLogprobs is not supported for reasoning models");
            if (body["max_tokens"] != null)
            {
                if (body["max_completion_tokens"] == null)
                {
                    body["max_completion_tokens"] = body["max_tokens"]!.DeepClone();
                }

                body.Remove("max_tokens");
            }
        }
        else if (modelId.StartsWith("gpt-4o-search-preview", StringComparison.Ordinal)
            || modelId.StartsWith("gpt-4o-mini-search-preview", StringComparison.Ordinal))
        {
            Remove(
                body,
                warnings,
                "temperature",
                "unsupported",
                "temperature",
                "temperature is not supported for the search preview models and has been removed.");
        }

        var serviceTier = OpenAIJson.String(openai, "serviceTier");
        if (serviceTier == "flex" && !capabilities.SupportsFlexProcessing)
        {
            body.Remove("service_tier");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "serviceTier",
                "flex processing is only available for o3, o4-mini, and gpt-5 models"));
        }

        if ((serviceTier == "priority" || serviceTier == "fast") && !capabilities.SupportsPriorityProcessing)
        {
            body.Remove("service_tier");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "serviceTier",
                "priority processing is only available for supported models (gpt-4, gpt-5, gpt-5-mini, o3, o4-mini) and requires Enterprise access. gpt-5-nano is not supported"));
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

        return new OpenAIChatPreparedRequest(body, warnings);
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, options);
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "chat/completions"),
            prepared.Body.ToJsonString(),
            _provider.CreateOpenAIHeaders(options.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        return Parse(document.RootElement, prepared.Warnings, response.Body, response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, options);
        var body = prepared.Body;
        body["stream"] = true;
        body["stream_options"] = new JsonObject { ["include_usage"] = true };
        var enumerator = _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "chat/completions"),
            body.ToJsonString(),
            _provider.CreateOpenAIHeaders(options.Headers),
            cancellationToken).GetAsyncEnumerator(cancellationToken);
        var primed = new List<string>();
        try
        {
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                var data = enumerator.Current;
                if (IsOutputChunk(data))
                {
                    primed.Add(data);
                    break;
                }

                if (TryGetError(data, out var error))
                {
                    throw error;
                }

                primed.Add(data);
                if (!IsJsonObject(data))
                {
                    break;
                }
            }

            yield return new StreamStartStreamPart(ToCallWarnings(prepared.Warnings));
            var state = new StreamState();
            foreach (var data in primed)
            {
                foreach (var part in state.Read(data, options.IncludeRawChunks))
                {
                    yield return part;
                }
            }

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                foreach (var part in state.Read(enumerator.Current, options.IncludeRawChunks))
                {
                    yield return part;
                }
            }

            foreach (var part in state.Finish())
            {
                yield return part;
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    internal static LanguageModelGenerateResult Parse(
        JsonElement root,
        IReadOnlyList<OpenAICallWarning> warnings,
        string raw,
        IReadOnlyDictionary<string, string> headers)
    {
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new AiSdkException("Response did not contain any choices.");
        }

        var choice = choices[0];
        var message = choice.TryGetProperty("message", out var messageElement) ? messageElement : default;
        var content = new List<GeneratedContent>();
        string? text = null;
        if (message.ValueKind == JsonValueKind.Object
            && message.TryGetProperty("content", out var contentElement)
            && contentElement.ValueKind == JsonValueKind.String)
        {
            text = contentElement.GetString();
        }

        if (string.IsNullOrEmpty(text)
            && message.ValueKind == JsonValueKind.Object
            && message.TryGetProperty("audio", out var audio)
            && audio.ValueKind == JsonValueKind.Object
            && audio.TryGetProperty("transcript", out var transcript)
            && transcript.ValueKind == JsonValueKind.String)
        {
            text = transcript.GetString();
        }

        if (!string.IsNullOrEmpty(text))
        {
            content.Add(new GeneratedText(text!));
        }

        if (message.ValueKind == JsonValueKind.Object && message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
        {
            foreach (var call in toolCalls.EnumerateArray())
            {
                var id = call.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String
                    ? idElement.GetString()
                    : null;
                var function = call.TryGetProperty("function", out var functionElement) ? functionElement : default;
                var name = function.ValueKind == JsonValueKind.Object && function.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty;
                var arguments = function.ValueKind == JsonValueKind.Object && function.TryGetProperty("arguments", out var argumentsElement)
                    ? argumentsElement.GetString() ?? "{}"
                    : "{}";
                content.Add(new GeneratedToolCall(string.IsNullOrEmpty(id) ? JsonValues.GenerateId("call_") : id!, name, arguments));
            }
        }

        if (message.ValueKind == JsonValueKind.Object && message.TryGetProperty("annotations", out var annotations) && annotations.ValueKind == JsonValueKind.Array)
        {
            foreach (var annotation in annotations.EnumerateArray())
            {
                if (!annotation.TryGetProperty("url_citation", out var citation) || citation.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var url = citation.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
                var title = citation.TryGetProperty("title", out var titleElement) && titleElement.ValueKind == JsonValueKind.String
                    ? titleElement.GetString()
                    : null;
                content.Add(new GeneratedSource(JsonValues.GenerateId("source_"), url, title));
            }
        }

        var metadata = new JsonObject();
        if (root.TryGetProperty("usage", out var usage)
            && usage.TryGetProperty("completion_tokens_details", out var details)
            && details.ValueKind == JsonValueKind.Object)
        {
            if (details.TryGetProperty("accepted_prediction_tokens", out var accepted) && accepted.ValueKind == JsonValueKind.Number)
            {
                metadata["acceptedPredictionTokens"] = accepted.GetInt32();
            }

            if (details.TryGetProperty("rejected_prediction_tokens", out var rejected) && rejected.ValueKind == JsonValueKind.Number)
            {
                metadata["rejectedPredictionTokens"] = rejected.GetInt32();
            }
        }

        if (choice.TryGetProperty("logprobs", out var logprobs)
            && logprobs.ValueKind == JsonValueKind.Object
            && logprobs.TryGetProperty("content", out var logprobContent)
            && logprobContent.ValueKind != JsonValueKind.Null)
        {
            metadata["logprobs"] = JsonNode.Parse(logprobContent.GetRawText());
        }

        var rawReason = choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String
            ? finish.GetString()
            : null;
        var responseId = StringOrNull(root, "id");
        var responseModel = StringOrNull(root, "model");
        var created = OpenAIJson.Unix(root, "created");
        return new LanguageModelGenerateResult(
            content,
            FinishReasons.Parse(rawReason),
            OpenAIJson.ChatUsage(root.TryGetProperty("usage", out var usageElement) ? usageElement : (JsonElement?)null),
            rawReason,
            ToCallWarnings(warnings),
            responseId,
            OpenAIJson.ProviderMetadata(metadata),
            raw,
            responseModel,
            OpenAIJson.UnixSeconds(created),
            headers);
    }

    private static List<CallWarning> ToCallWarnings(IReadOnlyList<OpenAICallWarning> warnings)
    {
        var result = new List<CallWarning>(warnings.Count);
        foreach (var warning in warnings)
        {
            result.Add(warning.ToCallWarning());
        }

        return result;
    }

    private static void ApplyResponseFormat(
        JsonObject body,
        List<OpenAICallWarning> warnings,
        LanguageModelCallOptions options,
        JsonElement? openai,
        bool strictJsonSchema)
    {
        var format = OpenAIJson.Child(openai, "responseFormat");
        JsonElement? schema = options.JsonSchema;
        string? name = options.JsonSchemaName;
        string? description = null;
        string? type = schema != null ? "json" : null;
        if (format != null)
        {
            type = OpenAIJson.String(format, "type") ?? type;
            var formatSchema = OpenAIJson.Child(format, "schema");
            if (formatSchema != null)
            {
                schema = formatSchema;
                type ??= "json";
            }

            name = OpenAIJson.String(format, "name") ?? name;
            description = OpenAIJson.String(format, "description");
        }

        if (type == null)
        {
            return;
        }

        if (type == "json_object" || (type == "json" && schema == null))
        {
            body["response_format"] = new JsonObject { ["type"] = "json_object" };
            return;
        }

        if (schema == null)
        {
            return;
        }

        var normalized = OpenAIJsonSchema.Normalize(schema.Value);
        warnings.AddRange(normalized.Warnings);
        var jsonSchema = new JsonObject
        {
            ["schema"] = normalized.Schema,
            ["strict"] = strictJsonSchema,
            ["name"] = string.IsNullOrEmpty(name) ? "response" : name,
        };
        if (description != null)
        {
            jsonSchema["description"] = description;
        }

        body["response_format"] = new JsonObject
        {
            ["type"] = "json_schema",
            ["json_schema"] = jsonSchema,
        };
    }

    private static string? ResolveEffort(
        JsonElement? openai,
        string? reasoning,
        OpenAILanguageModelCapabilities capabilities,
        string modelId,
        List<OpenAICallWarning> warnings)
    {
        var effort = OpenAIJson.String(openai, "reasoningEffort");
        if (effort == null && !string.IsNullOrEmpty(reasoning) && reasoning != "provider-default")
        {
            effort = reasoning;
        }

        if (effort != null && capabilities.SupportedReasoningEfforts != null && !Contains(capabilities.SupportedReasoningEfforts, effort))
        {
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "reasoningEffort",
                modelId + " only supports the following reasoning efforts: " + string.Join(", ", capabilities.SupportedReasoningEfforts)));
            return null;
        }

        return effort;
    }

    private static bool Contains(IReadOnlyList<string> values, string effort)
    {
        foreach (var value in values)
        {
            if (value == effort)
            {
                return true;
            }
        }

        return false;
    }

    private static (bool Enabled, int? Top) ReadLogprobs(JsonElement? openai)
    {
        var child = OpenAIJson.Child(openai, "logprobs");
        if (child == null)
        {
            return (false, null);
        }

        if (child.Value.ValueKind == JsonValueKind.True)
        {
            return (true, 0);
        }

        if (child.Value.ValueKind == JsonValueKind.False)
        {
            return (false, null);
        }

        if (child.Value.ValueKind == JsonValueKind.Number && child.Value.TryGetInt32(out var number))
        {
            return (true, number);
        }

        return (false, null);
    }

    private static void Copy(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Copy(JsonObject body, string name, JsonNode? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Remove(
        JsonObject body,
        List<OpenAICallWarning> warnings,
        string field,
        string type,
        string? feature,
        string? details,
        string? message = null)
    {
        if (body[field] == null)
        {
            return;
        }

        body.Remove(field);
        warnings.Add(new OpenAICallWarning(type, feature, details, message));
    }

    private static string? StringOrNull(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static bool IsJsonObject(string data)
    {
        try
        {
            using var document = JsonDocument.Parse(data);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsOutputChunk(string data)
    {
        if (!IsJsonObject(data))
        {
            return false;
        }

        using var document = JsonDocument.Parse(data);
        var root = document.RootElement;
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            return false;
        }

        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var choice in choices.EnumerateArray())
        {
            if (!choice.TryGetProperty("delta", out var delta) || delta.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String && (content.GetString() ?? string.Empty).Length > 0)
            {
                return true;
            }

            if (delta.TryGetProperty("tool_calls", out var tools) && tools.ValueKind == JsonValueKind.Array && tools.GetArrayLength() > 0)
            {
                return true;
            }

            if (delta.TryGetProperty("annotations", out var annotations) && annotations.ValueKind == JsonValueKind.Array && annotations.GetArrayLength() > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetError(string data, out Exception error)
    {
        error = null!;
        if (!IsJsonObject(data))
        {
            return false;
        }

        using var document = JsonDocument.Parse(data);
        var root = document.RootElement;
        if (!root.TryGetProperty("error", out var value) || value.ValueKind != JsonValueKind.Object || !value.TryGetProperty("message", out _))
        {
            return false;
        }

        var status = 500;
        if (value.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.Number && code.TryGetInt32(out var numeric) && numeric >= 100 && numeric <= 599)
        {
            status = numeric;
        }

        error = ProviderHttp.MapStatus(status, data);
        return true;
    }

    private sealed class StreamState
    {
        private readonly StreamingToolCallTracker _tools = new StreamingToolCallTracker();
        private bool _metadata;
        private bool _text;
        private FinishReason _finish = FinishReason.Other;
        private string? _rawFinish;
        private LanguageModelUsage? _usage;
        private readonly JsonObject _metadataObject = new JsonObject();
        private bool _failed;

        public IEnumerable<LanguageModelStreamPart> Read(string data, bool includeRaw)
        {
            if (includeRaw)
            {
                yield return new RawStreamPart(data);
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(data);
            }
            catch (JsonException)
            {
                _failed = true;
                _finish = FinishReason.Error;
                yield return new ErrorStreamPart("JSON parsing failed: Text: " + data + ".");
                yield break;
            }

            using (document)
            {
                var root = document.RootElement;
                if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
                {
                    _failed = true;
                    _finish = FinishReason.Error;
                    var message = error.TryGetProperty("message", out var messageElement) && messageElement.ValueKind == JsonValueKind.String
                        ? messageElement.GetString() ?? "stream error"
                        : "stream error";
                    yield return new ErrorStreamPart(message);
                    yield break;
                }

                if (!_metadata)
                {
                    var id = StringOrNull(root, "id");
                    var model = StringOrNull(root, "model");
                    var created = OpenAIJson.UnixSeconds(OpenAIJson.Unix(root, "created"));
                    if (id != null || model != null || created != null)
                    {
                        _metadata = true;
                        yield return new ResponseMetadataStreamPart(id, model, created);
                    }
                }

                if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
                {
                    _usage = OpenAIJson.ChatUsage(usage);
                    if (usage.TryGetProperty("completion_tokens_details", out var details) && details.ValueKind == JsonValueKind.Object)
                    {
                        if (details.TryGetProperty("accepted_prediction_tokens", out var accepted) && accepted.ValueKind == JsonValueKind.Number)
                        {
                            _metadataObject["acceptedPredictionTokens"] = accepted.GetInt32();
                        }

                        if (details.TryGetProperty("rejected_prediction_tokens", out var rejected) && rejected.ValueKind == JsonValueKind.Number)
                        {
                            _metadataObject["rejectedPredictionTokens"] = rejected.GetInt32();
                        }
                    }
                }

                if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
                {
                    yield break;
                }

                var choice = choices[0];
                if (choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String)
                {
                    _rawFinish = finish.GetString();
                    _finish = FinishReasons.Parse(_rawFinish);
                }

                if (choice.TryGetProperty("logprobs", out var logprobs)
                    && logprobs.ValueKind == JsonValueKind.Object
                    && logprobs.TryGetProperty("content", out var logprobContent)
                    && logprobContent.ValueKind != JsonValueKind.Null)
                {
                    _metadataObject["logprobs"] = JsonNode.Parse(logprobContent.GetRawText());
                }

                if (!choice.TryGetProperty("delta", out var delta) || delta.ValueKind != JsonValueKind.Object)
                {
                    yield break;
                }

                if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                {
                    var text = content.GetString() ?? string.Empty;
                    if (!_text)
                    {
                        _text = true;
                        yield return new TextStartStreamPart("0");
                    }

                    yield return new TextDeltaStreamPart("0", text);
                }

                if (delta.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
                {
                    foreach (var call in toolCalls.EnumerateArray())
                    {
                        int? index = call.TryGetProperty("index", out var indexElement) && indexElement.TryGetInt32(out var indexValue) ? indexValue : null;
                        var id = call.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String ? idElement.GetString() : null;
                        string? name = null;
                        string? arguments = null;
                        var hasArguments = false;
                        if (call.TryGetProperty("function", out var function) && function.ValueKind == JsonValueKind.Object)
                        {
                            if (function.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
                            {
                                name = nameElement.GetString();
                            }

                            if (function.TryGetProperty("arguments", out var argumentsElement) && argumentsElement.ValueKind == JsonValueKind.String)
                            {
                                arguments = argumentsElement.GetString();
                                hasArguments = true;
                            }
                        }

                        _tools.ProcessDelta(new StreamingToolCallDelta(index, id, name, hasArguments ? arguments : null));
                    }
                }

                if (delta.TryGetProperty("annotations", out var annotations) && annotations.ValueKind == JsonValueKind.Array)
                {
                    foreach (var annotation in annotations.EnumerateArray())
                    {
                        if (!annotation.TryGetProperty("url_citation", out var citation))
                        {
                            continue;
                        }

                        var url = citation.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
                        var title = citation.TryGetProperty("title", out var titleElement) && titleElement.ValueKind == JsonValueKind.String
                            ? titleElement.GetString()
                            : null;
                        yield return new SourceStreamPart(JsonValues.GenerateId("source_"), url, title);
                    }
                }
            }
        }

        public IEnumerable<LanguageModelStreamPart> Finish()
        {
            if (_text)
            {
                yield return new TextEndStreamPart("0");
            }

            foreach (var call in _tools.Flush())
            {
                yield return call;
            }

            if (_failed)
            {
                _finish = FinishReason.Error;
            }

            yield return new FinishStreamPart(
                _finish,
                _usage ?? new LanguageModelUsage(null, null, null),
                _rawFinish,
                OpenAIJson.ProviderMetadata(_metadataObject));
        }
    }
}
