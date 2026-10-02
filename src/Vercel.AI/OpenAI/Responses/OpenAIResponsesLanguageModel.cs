// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a Responses model.</summary>
    public OpenAIResponsesLanguageModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name + ".responses";

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
        options ??= new LanguageModelCallOptions();
        var warnings = new List<OpenAICallWarning>();
        var openai = OpenAIJson.Provider(options.ProviderOptions);
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

        var reasoning = OpenAIJson.Bool(openai, "forceReasoning") ?? capabilities.IsReasoningModel;
        if (options.TopK != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "topK", null));
        }

        var systemMode = OpenAIJson.String(openai, "systemMessageMode") ?? (reasoning ? "developer" : capabilities.SystemMessageMode);
        var explicitItem = OpenAIJson.Bool(openai, "explicitMessageItemType") == true;
        var converted = ConvertInput(options.Prompt, systemMode, explicitItem);
        warnings.AddRange(converted.Warnings);
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
        if (reasoning && effort != null)
        {
            body["reasoning"] = new JsonObject { ["effort"] = effort };
        }
        else if (!reasoning && OpenAIJson.String(openai, "reasoningEffort") != null)
        {
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "reasoningEffort",
                "reasoningEffort is not supported for non-reasoning models"));
        }

        if (capabilities.SupportsConfigurationUpdate && body["prompt_cache_retention"] != null)
        {
            body.Remove("prompt_cache_retention");
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "promptCacheRetention",
                "promptCacheRetention is not supported by GPT-6 and later models; use promptCacheOptions instead"));
        }

        if (reasoning && !(effort == "none" && capabilities.SupportsNonReasoningParameters))
        {
            Remove(body, warnings, "temperature", "temperature", "temperature is not supported for reasoning models");
            Remove(body, warnings, "top_p", "topP", "topP is not supported for reasoning models");
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
        var prepared = Prepare(ModelId, options, false);
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
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
        var prepared = Prepare(ModelId, options, true);
        yield return new StreamStartStreamPart(ToCallWarnings(prepared.Warnings));
        var text = false;
        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
            prepared.Body.ToJsonString(),
            _provider.CreateOpenAIHeaders(options.Headers),
            cancellationToken).ConfigureAwait(false))
        {
            JsonObject? node;
            try
            {
                node = JsonNode.Parse(data) as JsonObject;
            }
            catch (JsonException)
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

    private static ResponsesInput ConvertInput(IReadOnlyList<ModelMessage> prompt, string systemMessageMode, bool explicitItem)
    {
        var input = new JsonArray();
        var warnings = new List<OpenAICallWarning>();
        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
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
