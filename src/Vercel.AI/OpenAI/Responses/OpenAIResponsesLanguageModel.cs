// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Responses API request body and the warnings produced while building it.</summary>
public sealed class OpenAIResponsesPreparedRequest
{
    internal OpenAIResponsesPreparedRequest(JsonObject body, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Body = body;
        Warnings = warnings;
    }

    /// <summary>JSON body sent to <c>/responses</c>.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>OpenAI Responses API language model.</summary>
public sealed class OpenAIResponsesLanguageModel : ILanguageModel
{
    private readonly ProviderHttp _http;
    private readonly string _providerName;
    private readonly string _providerOptionsName;
    private readonly Func<Uri> _url;
    private readonly Func<IReadOnlyDictionary<string, string?>?, Dictionary<string, string?>> _headers;

    /// <summary>Creates a Responses model.</summary>
    public OpenAIResponsesLanguageModel(OpenAIProvider provider, string modelId)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _http = provider.Http;
        _providerName = provider.Name;
        _providerOptionsName = "openai";
        _url = () => ApiKeys.Combine(provider.Options.BaseUrl, "responses");
        _headers = provider.CreateOpenAIHeaders;
    }

    /// <summary>
    /// Creates an Azure OpenAI Responses model. Message and request options are read from the
    /// <c>azure</c> provider options first and fall back to <c>openai</c> when <c>azure</c> is absent.
    /// </summary>
    public OpenAIResponsesLanguageModel(Azure.AzureOpenAIProvider provider, string modelId)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _http = provider.Http;
        _providerName = provider.Name;
        _providerOptionsName = "azure";
        _url = () => ApiKeys.Combine(
            provider.Options.BaseUrl,
            "openai/v1/responses?api-version=" + provider.Options.AzureApiVersion);
        _headers = provider.CreateHeaders;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _providerName + ".responses";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>
    /// Prepares Responses function tools. Strict is false unless the tool sets <see cref="LanguageModelTool.Strict"/>.
    /// </summary>
    public static PreparedResponsesTools PrepareResponsesTools(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
    {
        if (tools == null || tools.Count == 0)
        {
            return new PreparedResponsesTools(null, null, Array.Empty<CallWarning>());
        }

        var prepared = new JsonArray();
        var warnings = new List<CallWarning>();
        foreach (var tool in tools)
        {
            var normalized = OpenAIJsonSchema.Normalize(tool.InputSchema);
            foreach (var warning in normalized.Warnings)
            {
                warnings.Add(warning.ToCallWarning());
            }

            prepared.Add(new JsonObject
            {
                ["type"] = "function",
                ["name"] = tool.Name,
                ["description"] = tool.Description,
                ["parameters"] = normalized.Schema,
                ["strict"] = tool.Strict ?? false,
            });
        }

        return new PreparedResponsesTools(prepared, toolChoice, warnings);
    }

    /// <summary>Builds the Responses body for <paramref name="modelId"/>. Generate calls omit <c>stream</c>.</summary>
    public static OpenAIResponsesPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream)
    {
        return Prepare(modelId, options, stream, "openai");
    }

    /// <summary>
    /// Builds the Responses body. Options come from <paramref name="providerOptionsName"/> and fall back to
    /// <c>openai</c> when that key is absent.
    /// </summary>
    public static OpenAIResponsesPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream, string providerOptionsName)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<OpenAICallWarning>();
        var openai = ProviderOptionsFor(options.ProviderOptions, providerOptionsName);
        var capabilities = OpenAILanguageModelCapabilities.ForModel(modelId);
        var effort = OpenAIJson.String(openai, "reasoningEffort");
        if (effort == null && !string.IsNullOrEmpty(options.Reasoning) && options.Reasoning != "provider-default")
        {
            effort = options.Reasoning;
        }

        if (effort != null && capabilities.SupportedReasoningEfforts != null && !Contains(capabilities.SupportedReasoningEfforts, effort))
        {
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "reasoningEffort",
                modelId + " only supports the following reasoning efforts: " + string.Join(", ", capabilities.SupportedReasoningEfforts)));
            effort = null;
        }

        // An explicit null reasoningSummary turns the default off.
        var summaryExplicit = openai is { ValueKind: JsonValueKind.Object } summaryHolder
            && summaryHolder.TryGetProperty("reasoningSummary", out _);
        var reasoningSummary = summaryExplicit
            ? OpenAIJson.String(openai, "reasoningSummary")
            : effort != null && effort != "none" ? "detailed" : null;
        var reasoning = OpenAIJson.Bool(openai, "forceReasoning") ?? capabilities.IsReasoningModel;
        if (OpenAIJson.String(openai, "conversation") != null && OpenAIJson.String(openai, "previousResponseId") != null)
        {
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "conversation",
                "conversation and previousResponseId cannot be used together"));
        }

        if (options.TopK != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "topK", null));
        }

        var systemMode = OpenAIJson.String(openai, "systemMessageMode") ?? (reasoning ? "developer" : capabilities.SystemMessageMode);
        var explicitItem = OpenAIJson.Bool(openai, "explicitMessageItemType") == true;
        var configurationUpdateUnsupportedReason = GetConfigurationUpdateUnsupportedReason(capabilities, openai);
        var hasContinuation = OpenAIJson.String(openai, "conversation") != null || OpenAIJson.String(openai, "previousResponseId") != null;
        var converted = ConvertInput(
            options.Prompt,
            systemMode,
            explicitItem,
            configurationUpdateUnsupportedReason,
            providerOptionsName,
            hasContinuation,
            OpenAIJson.Bool(openai, "store") ?? true);
        warnings.AddRange(converted.Warnings);

        // The schema accepts update efforts supported by any model. Check this model's list.
        string? UpdateEffortUnsupportedReason(string? updateEffort) =>
            updateEffort != null
            && capabilities.SupportedReasoningEfforts != null
            && !Contains(capabilities.SupportedReasoningEfforts, updateEffort)
                ? modelId + " only supports the following reasoning efforts: " + string.Join(", ", capabilities.SupportedReasoningEfforts)
                : null;

        foreach (var item in converted.Input)
        {
            if (item is JsonObject { } update && update["type"]?.GetValue<string>() == "configuration_update")
            {
                var reason = UpdateEffortUnsupportedReason(update["reasoning"]!["effort"]!.GetValue<string>());
                if (reason != null)
                {
                    throw new UnsupportedFunctionalityException("Message-level reasoningEffortUpdate", reason);
                }
            }
        }

        var effortUpdate = OpenAIJson.String(openai, "reasoningEffortUpdate");
        if (effortUpdate != null)
        {
            ValidateUpdateEffort(effortUpdate);
            var requestReason = configurationUpdateUnsupportedReason ?? UpdateEffortUnsupportedReason(effortUpdate);
            if (requestReason != null)
            {
                warnings.Add(new OpenAICallWarning("unsupported", "reasoningEffortUpdate", requestReason));
            }
            else if (!(converted.Input.Count > 0
                && converted.Input[0] is JsonObject { } first
                && first["type"]?.GetValue<string>() == "configuration_update"
                && first["reasoning"]!["effort"]!.GetValue<string>() == effortUpdate))
            {
                // An identical first item would otherwise leave two adjacent updates, which OpenAI rejects.
                converted.Input.Insert(0, ConfigurationUpdate(effortUpdate));
            }
        }

        for (var i = 1; i < converted.Input.Count; i++)
        {
            if (IsConfigurationUpdate(converted.Input[i - 1]) && IsConfigurationUpdate(converted.Input[i]))
            {
                throw new UnsupportedFunctionalityException(
                    "Adjacent reasoning effort configuration updates",
                    "Adjacent reasoning effort configuration updates are not supported.");
            }
        }
        // A compaction trigger is a request control and must be the final input item.
        if (OpenAIJson.Bool(openai, "compactionTrigger") == true)
        {
            converted.Input.Add(new JsonObject { ["type"] = "compaction_trigger" });
        }

        var body = new JsonObject
        {
            ["model"] = modelId,
            ["input"] = converted.Input,
        };
        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.MaxOutputTokens is { } maxOutput)
        {
            body["max_output_tokens"] = maxOutput;
        }

        if (OpenAIJson.Int(openai, "maxToolCalls") is { } maxToolCalls)
        {
            body["max_tool_calls"] = maxToolCalls;
        }

        Copy(body, "metadata", OpenAIJson.Node(OpenAIJson.Child(openai, "metadata")));
        if (OpenAIJson.Bool(openai, "parallelToolCalls") is { } parallel)
        {
            body["parallel_tool_calls"] = parallel;
        }

        Copy(body, "conversation", OpenAIJson.String(openai, "conversation"));
        Copy(body, "previous_response_id", OpenAIJson.String(openai, "previousResponseId"));
        if (OpenAIJson.Bool(openai, "store") is { } store)
        {
            body["store"] = store;
        }

        Copy(body, "user", OpenAIJson.String(openai, "user"));
        Copy(body, "instructions", OpenAIJson.String(openai, "instructions"));
        Copy(body, "service_tier", OpenAIJson.String(openai, "serviceTier"));
        Copy(body, "prompt_cache_key", OpenAIJson.String(openai, "promptCacheKey"));
        Copy(body, "prompt_cache_options", OpenAIJson.Node(OpenAIJson.Child(openai, "promptCacheOptions")));
        Copy(body, "prompt_cache_retention", OpenAIJson.String(openai, "promptCacheRetention"));
        Copy(body, "safety_identifier", OpenAIJson.String(openai, "safetyIdentifier"));
        Copy(body, "truncation", OpenAIJson.String(openai, "truncation"));
        if (OpenAIJson.Child(openai, "contextManagement") is { ValueKind: JsonValueKind.Array } contextManagement)
        {
            var managed = new JsonArray();
            foreach (var entry in contextManagement.EnumerateArray())
            {
                var managedEntry = new JsonObject { ["type"] = OpenAIJson.String(entry, "type") };
                if (OpenAIJson.Child(entry, "compactThreshold") is { } threshold)
                {
                    managedEntry["compact_threshold"] = OpenAIJson.Node(threshold);
                }

                managed.Add(managedEntry);
            }

            body["context_management"] = managed;
        }

        var reasoningMode = OpenAIJson.String(openai, "reasoningMode");
        var reasoningContext = OpenAIJson.String(openai, "reasoningContext");
        if (reasoning && (effort != null || reasoningSummary != null || reasoningMode != null || reasoningContext != null))
        {
            var reasoningBody = new JsonObject();
            if (effort != null)
            {
                reasoningBody["effort"] = effort;
            }

            if (reasoningSummary != null)
            {
                reasoningBody["summary"] = reasoningSummary;
            }

            if (reasoningMode != null)
            {
                reasoningBody["mode"] = reasoningMode;
            }

            if (reasoningContext != null)
            {
                reasoningBody["context"] = reasoningContext;
            }

            body["reasoning"] = reasoningBody;
        }
        else if (!reasoning && OpenAIJson.String(openai, "reasoningEffort") != null)
        {
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "reasoningEffort",
                "reasoningEffort is not supported for non-reasoning models"));
        }

        if (!reasoning)
        {
            if (summaryExplicit && reasoningSummary != null)
            {
                warnings.Add(new OpenAICallWarning(
                    "unsupported",
                    "reasoningSummary",
                    "reasoningSummary is not supported for non-reasoning models"));
            }

            if (reasoningMode != null)
            {
                warnings.Add(new OpenAICallWarning(
                    "unsupported",
                    "reasoningMode",
                    "reasoningMode is not supported for non-reasoning models"));
            }

            if (reasoningContext != null)
            {
                warnings.Add(new OpenAICallWarning(
                    "unsupported",
                    "reasoningContext",
                    "reasoningContext is not supported for non-reasoning models"));
            }
        }

        if (capabilities.SupportsConfigurationUpdate && body["prompt_cache_retention"] != null)
        {
            body.Remove("prompt_cache_retention");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "promptCacheRetention",
                "promptCacheRetention is not supported by GPT-6 and later models; use promptCacheOptions instead"));
        }

        var topLogprobs = OpenAIJson.Int(openai, "logprobs") ?? (OpenAIJson.Bool(openai, "logprobs") == true ? 20 : null);
        var include = new List<string>();
        if (OpenAIJson.Child(openai, "include") is { ValueKind: JsonValueKind.Array } includeValues)
        {
            foreach (var value in includeValues.EnumerateArray())
            {
                if (value.ValueKind == JsonValueKind.String && !include.Contains(value.GetString()!))
                {
                    include.Add(value.GetString()!);
                }
            }
        }

        if (topLogprobs is > 0)
        {
            body["top_logprobs"] = topLogprobs;
            if (!include.Contains("message.output_text.logprobs"))
            {
                include.Add("message.output_text.logprobs");
            }
        }

        if (include.Count > 0)
        {
            var includeArray = new JsonArray();
            foreach (var value in include)
            {
                includeArray.Add(value);
            }

            body["include"] = includeArray;
        }

        if (reasoning)
        {
            // Input updates change the effort used to validate sampling parameters.
            var effectiveEffort = effort;
            foreach (var item in converted.Input)
            {
                if (IsConfigurationUpdate(item))
                {
                    effectiveEffort = item!["reasoning"]!["effort"]!.GetValue<string>();
                }
            }

            if (!(effectiveEffort == "none" && capabilities.SupportsNonReasoningParameters))
            {
                Remove(body, warnings, "temperature", "temperature", "temperature is not supported for reasoning models");
                Remove(body, warnings, "top_p", "topP", "topP is not supported for reasoning models");
                if (capabilities.SupportedReasoningEfforts != null
                    && (body["top_logprobs"] != null || include.Contains("message.output_text.logprobs")))
                {
                    body.Remove("top_logprobs");
                    include.Remove("message.output_text.logprobs");
                    if (include.Count == 0)
                    {
                        body.Remove("include");
                    }
                    else
                    {
                        var kept = new JsonArray();
                        foreach (var value in include)
                        {
                            kept.Add(value);
                        }

                        body["include"] = kept;
                    }

                    warnings.Add(new OpenAICallWarning("unsupported", "logprobs", "logprobs is not supported for reasoning models"));
                }
            }
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

        var prepared = PrepareResponsesTools(options.Tools, options.ToolChoice);
        foreach (var warning in prepared.ToolWarnings)
        {
            warnings.Add(new OpenAICallWarning(warning.Type, null, warning.Message));
        }

        if (prepared.Tools is { Count: > 0 })
        {
            body["tools"] = prepared.Tools;
        }

        var choice = MapChoice(options.ToolChoice);
        if (choice != null)
        {
            body["tool_choice"] = choice;
        }

        if (stream)
        {
            body["stream"] = true;
        }

        return new OpenAIResponsesPreparedRequest(body, warnings);
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, options, false, _providerOptionsName);
        var response = await _http.SendJsonStringAsync(
            HttpMethod.Post,
            _url(),
            prepared.Body.ToJsonString(),
            _headers(options.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        return Parse(document.RootElement, prepared.Warnings, response.Body, response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, options, true, _providerOptionsName);
        yield return new StreamStartStreamPart(ToCallWarnings(prepared.Warnings));
        var text = false;
        await foreach (var data in _http.SendSseAsync(
            _url(),
            prepared.Body.ToJsonString(),
            _headers(options.Headers),
            cancellationToken).ConfigureAwait(false))
        {
            JsonObject? node = null;
            var invalidJson = false;
            try
            {
                node = JsonNode.Parse(data) as JsonObject;
            }
            catch (JsonException)
            {
                invalidJson = true;
            }

            if (invalidJson)
            {
                yield return new ErrorStreamPart("JSON parsing failed: Text: " + data + ".");
                continue;
            }

            var type = node?["type"]?.GetValue<string>();
            if (type == "response.output_text.delta")
            {
                var delta = node?["delta"]?.GetValue<string>();
                if (!text)
                {
                    text = true;
                    yield return new TextStartStreamPart("text");
                }

                if (!string.IsNullOrEmpty(delta))
                {
                    yield return new TextDeltaStreamPart("text", delta!);
                }
            }
            else if (type == "response.completed")
            {
                if (text)
                {
                    yield return new TextEndStreamPart("text");
                    text = false;
                }

                yield return new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty);
            }
        }

        if (text)
        {
            yield return new TextEndStreamPart("text");
        }
    }

    internal static LanguageModelGenerateResult Parse(
        JsonElement root,
        IReadOnlyList<OpenAICallWarning> warnings,
        string raw,
        IReadOnlyDictionary<string, string> headers)
    {
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            var message = error.TryGetProperty("message", out var messageElement) && messageElement.ValueKind == JsonValueKind.String
                ? messageElement.GetString() ?? "Responses API error"
                : "Responses API error";
            throw new BadRequestException(message, raw);
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            string? detail = null;
            if (root.TryGetProperty("incomplete_details", out var incomplete)
                && incomplete.ValueKind == JsonValueKind.Object
                && incomplete.TryGetProperty("reason", out var reason)
                && reason.ValueKind == JsonValueKind.String)
            {
                detail = reason.GetString();
            }

            var message = string.IsNullOrEmpty(detail)
                ? "Responses API returned no output"
                : "Responses API returned no output (" + detail + ")";
            throw new InternalServerException(message, 500, raw);
        }

        var content = new List<GeneratedContent>();
        var hasFunctionCall = false;
        foreach (var item in output.EnumerateArray())
        {
            var type = item.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
            if (type == "message" && item.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    {
                        var value = text.GetString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            content.Add(new GeneratedText(value!));
                        }
                    }
                }
            }
            else if (type == "function_call")
            {
                hasFunctionCall = true;
                var id = item.TryGetProperty("call_id", out var idElement) && idElement.ValueKind == JsonValueKind.String
                    ? idElement.GetString() ?? "call"
                    : "call";
                var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
                var arguments = item.TryGetProperty("arguments", out var args) && args.ValueKind == JsonValueKind.String
                    ? args.GetString() ?? "{}"
                    : "{}";
                var itemId = item.TryGetProperty("id", out var itemIdElement) && itemIdElement.ValueKind == JsonValueKind.String
                    ? itemIdElement.GetString()
                    : null;
                JsonElement? metadata = null;
                if (!string.IsNullOrEmpty(itemId))
                {
                    metadata = OpenAIJson.ProviderMetadata(new JsonObject { ["itemId"] = itemId });
                }

                content.Add(new GeneratedToolCall(id, name, arguments, metadata));
            }
            else if (type == "reasoning" && item.TryGetProperty("summary", out var summary) && summary.ValueKind == JsonValueKind.Array)
            {
                if (summary.GetArrayLength() == 0)
                {
                    content.Add(new GeneratedReasoning(string.Empty));
                }

                foreach (var part in summary.EnumerateArray())
                {
                    var text = part.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String
                        ? textElement.GetString() ?? string.Empty
                        : string.Empty;
                    content.Add(new GeneratedReasoning(text));
                }
            }
        }

        string? incompleteReason = null;
        if (root.TryGetProperty("incomplete_details", out var incompleteDetails)
            && incompleteDetails.ValueKind == JsonValueKind.Object
            && incompleteDetails.TryGetProperty("reason", out var reasonElement)
            && reasonElement.ValueKind == JsonValueKind.String)
        {
            incompleteReason = reasonElement.GetString();
        }

        FinishReason finish;
        switch (incompleteReason)
        {
            case null:
                finish = hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Stop;
                break;
            case "max_output_tokens":
                finish = FinishReason.Length;
                break;
            case "content_filter":
                finish = FinishReason.ContentFilter;
                break;
            default:
                finish = hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Other;
                break;
        }

        var responseId = root.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.String ? idValue.GetString() : null;
        var responseModel = root.TryGetProperty("model", out var modelValue) && modelValue.ValueKind == JsonValueKind.String ? modelValue.GetString() : null;
        return new LanguageModelGenerateResult(
            content,
            finish,
            OpenAIJson.ResponsesUsage(root.TryGetProperty("usage", out var usage) ? usage : (JsonElement?)null),
            incompleteReason,
            ToCallWarnings(warnings),
            string.IsNullOrEmpty(responseId) ? null : responseId,
            OpenAIJson.ProviderMetadata(new JsonObject()),
            raw,
            string.IsNullOrEmpty(responseModel) ? null : responseModel,
            OpenAIJson.UnixSeconds(OpenAIJson.Unix(root, "created_at")),
            headers);
    }

    private static readonly string[] UpdateEfforts = { "none", "low", "medium", "high", "xhigh", "max" };

    private static string? GetConfigurationUpdateUnsupportedReason(OpenAILanguageModelCapabilities capabilities, JsonElement? openai)
    {
        if (!capabilities.SupportsConfigurationUpdate)
        {
            return "reasoningEffortUpdate is only supported by GPT-6 and later models";
        }

        if (OpenAIJson.String(openai, "reasoningMode") == "pro"
            || OpenAIJson.Child(openai, "contextManagement") != null
            || OpenAIJson.String(openai, "truncation") == "auto")
        {
            return "reasoningEffortUpdate requires standard reasoning mode without automatic compaction or automatic truncation";
        }

        return null;
    }

    private static void ValidateUpdateEffort(string effort)
    {
        if (Array.IndexOf(UpdateEfforts, effort) < 0)
        {
            throw new InvalidArgumentException("providerOptions", effort, "invalid openai provider options");
        }
    }

    private static JsonObject ConfigurationUpdate(string effort) => new()
    {
        ["type"] = "configuration_update",
        ["reasoning"] = new JsonObject { ["effort"] = effort },
    };

    private static bool IsConfigurationUpdate(JsonNode? item) =>
        item is JsonObject obj && obj["type"]?.GetValue<string>() == "configuration_update";

    private static JsonElement? ProviderOptionsFor(IReadOnlyDictionary<string, JsonElement>? providerOptions, string name)
    {
        if (providerOptions != null)
        {
            if (providerOptions.TryGetValue(name, out var named))
            {
                return named;
            }

            if (name != "openai" && providerOptions.TryGetValue("openai", out var fallback))
            {
                return fallback;
            }
        }

        return null;
    }

    private static ResponsesInput ConvertInput(
        IReadOnlyList<ModelMessage> prompt,
        string systemMessageMode,
        bool explicitItem,
        string? configurationUpdateUnsupportedReason,
        string providerOptionsName,
        bool hasContinuation,
        bool store)
    {
        var input = new JsonArray();
        var warnings = new List<OpenAICallWarning>();
        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    var messageEffort = OpenAIJson.String(ProviderOptionsFor(system.ProviderOptions, providerOptionsName), "reasoningEffortUpdate");
                    if (messageEffort != null)
                    {
                        ValidateUpdateEffort(messageEffort);
                        var reason = system.Content != string.Empty
                            ? "Message-level reasoningEffortUpdate requires empty system message content."
                            : configurationUpdateUnsupportedReason;
                        if (reason != null)
                        {
                            throw new UnsupportedFunctionalityException("Message-level reasoningEffortUpdate", reason);
                        }

                        // The control is independent of systemMessageMode's text handling.
                        input.Add(ConfigurationUpdate(messageEffort));
                        break;
                    }

                    if (systemMessageMode == "remove")
                    {
                        warnings.Add(new OpenAICallWarning("other", null, null, "system messages are removed for this model"));
                        break;
                    }

                    if (systemMessageMode != "system" && systemMessageMode != "developer")
                    {
                        throw new AiSdkException("Unsupported system message mode: " + systemMessageMode);
                    }

                    input.Add(MessageItem(systemMessageMode, JsonValue.Create(system.Content)!, explicitItem));
                    break;
                case UserModelMessage user:
                    var parts = new JsonArray();
                    foreach (var part in user.Content)
                    {
                        if (part is TextContentPart text)
                        {
                            parts.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
                        }
                        else if (part is FileContentPart file)
                        {
                            parts.Add(ConvertFile(file));
                        }
                    }

                    input.Add(MessageItem("user", parts, explicitItem));
                    break;
                case AssistantModelMessage assistant:
                    AddStoredReasoning(input, assistant, providerOptionsName, hasContinuation, store);
                    if (!string.IsNullOrEmpty(assistant.Text))
                    {
                        input.Add(MessageItem("assistant", JsonValue.Create(assistant.Text)!, explicitItem));
                    }

                    foreach (var call in assistant.ToolCalls)
                    {
                        input.Add(new JsonObject
                        {
                            ["type"] = "function_call",
                            ["call_id"] = call.ToolCallId,
                            ["name"] = call.ToolName,
                            ["arguments"] = string.IsNullOrEmpty(call.ArgumentsJson) ? "{}" : call.ArgumentsJson,
                        });
                    }

                    break;
                case ToolModelMessage tool:
                    input.Add(new JsonObject
                    {
                        ["type"] = "function_call_output",
                        ["call_id"] = tool.ToolCallId,
                        ["output"] = tool.OutputJson ?? string.Empty,
                    });
                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + message.Role);
            }
        }

        return new ResponsesInput(input, warnings);
    }

    // Reasoning already held by a conversation or previous response is filtered out.
    private static void AddStoredReasoning(JsonArray input, AssistantModelMessage assistant, string providerOptionsName, bool hasContinuation, bool store)
    {
        var options = ProviderOptionsFor(assistant.ReasoningProviderOptions, providerOptionsName);
        var itemId = OpenAIJson.String(options, "itemId");
        if (itemId == null || hasContinuation)
        {
            return;
        }

        if (store)
        {
            input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = itemId });
            return;
        }

        var summary = new JsonArray();
        if (!string.IsNullOrEmpty(assistant.Reasoning))
        {
            summary.Add(new JsonObject { ["type"] = "summary_text", ["text"] = assistant.Reasoning });
        }

        var item = new JsonObject { ["type"] = "reasoning", ["id"] = itemId };
        if (OpenAIJson.String(options, "reasoningEncryptedContent") is { } encrypted)
        {
            item["encrypted_content"] = encrypted;
        }

        item["summary"] = summary;
        input.Add(item);
    }

    private static JsonObject ConvertFile(FileContentPart file)
    {
        var mediaType = file.MediaType ?? "application/octet-stream";
        var inline = file.Data != null;
        var full = OpenAIJson.ResolveFullMediaType(mediaType, file.Data, inline && string.IsNullOrEmpty(file.Url));
        if (OpenAIJson.TopLevel(full) == "image")
        {
            var image = new JsonObject { ["type"] = "input_image" };
            if (!string.IsNullOrEmpty(file.Url))
            {
                image["image_url"] = file.Url;
            }
            else
            {
                image["image_url"] = "data:" + full + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
            }

            return image;
        }

        if (!string.IsNullOrEmpty(file.Url))
        {
            return new JsonObject { ["type"] = "input_file", ["file_url"] = file.Url };
        }

        if (full != "application/pdf")
        {
            throw new AiSdkException("file part media type " + full);
        }

        return new JsonObject
        {
            ["type"] = "input_file",
            ["filename"] = string.IsNullOrEmpty(file.FileName) ? "data" : file.FileName,
            ["file_data"] = "data:" + full + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>()),
        };
    }

    private static JsonObject MessageItem(string role, JsonNode content, bool explicitItem)
    {
        var item = new JsonObject { ["role"] = role, ["content"] = content };
        if (explicitItem)
        {
            item["type"] = "message";
        }

        return item;
    }

    private static JsonNode? MapChoice(ToolChoice? toolChoice)
    {
        if (toolChoice == null)
        {
            return null;
        }

        if (toolChoice.Type == "auto" || toolChoice.Type == "none" || toolChoice.Type == "required")
        {
            return JsonValue.Create(toolChoice.Type);
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            return new JsonObject { ["type"] = "function", ["name"] = named.ToolName };
        }

        return null;
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

    private static void Remove(JsonObject body, List<OpenAICallWarning> warnings, string field, string feature, string details)
    {
        if (body[field] == null)
        {
            return;
        }

        body.Remove(field);
        warnings.Add(new OpenAICallWarning("unsupported", feature, details));
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

    private static List<CallWarning> ToCallWarnings(IReadOnlyList<OpenAICallWarning> warnings)
    {
        var result = new List<CallWarning>(warnings.Count);
        foreach (var warning in warnings)
        {
            result.Add(warning.ToCallWarning());
        }

        return result;
    }

    private sealed class ResponsesInput
    {
        public ResponsesInput(JsonArray input, IReadOnlyList<OpenAICallWarning> warnings)
        {
            Input = input;
            Warnings = warnings;
        }

        public JsonArray Input { get; }

        public IReadOnlyList<OpenAICallWarning> Warnings { get; }
    }
}
