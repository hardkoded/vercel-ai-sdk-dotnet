// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.OpenResponses;

/// <summary>Posts to <c>/responses</c>.</summary>
public sealed class OpenResponsesLanguageModel : ILanguageModel
{
    private const string ReplayKind = "open-responses.extension-replay";

    private static readonly Dictionary<string, string> ReasoningEffortMap = new Dictionary<string, string>
    {
        { "minimal", "low" },
        { "low", "low" },
        { "medium", "medium" },
        { "high", "high" },
        { "xhigh", "xhigh" },
        { "max", "max" },
    };

    private readonly OpenResponsesProvider _provider;

    /// <summary>Creates a model.</summary>
    public OpenResponsesLanguageModel(OpenResponsesProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => OpenResponsesProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    private string ProviderOptionsName => _provider.Options.Name;

    private OpenResponsesExtensionRegistry Registry => _provider.ExtensionRegistry;

    /// <summary>
    /// Serializes the model id and config for a workflow boundary. Maps to <c>[WORKFLOW_SERIALIZE]</c>.
    /// Throws a <see cref="SerializationError"/> when extensions are registered, because their codecs cannot cross the boundary.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        if (Registry.ByExtensionId.Count > 0)
        {
            throw new SerializationError(
                "Open Responses models with registered extensions cannot be serialized across workflow boundaries. Recreate the provider with its extension codecs inside the workflow step.");
        }

        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["providerOptionsName"] = ProviderOptionsName,
            ["url"] = ApiKeys.Combine(_provider.Options.BaseUrl, "responses").ToString(),
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    private sealed class RegisteredProviderTool
    {
        public RegisteredProviderTool(string toolType, OpenResponsesExtension extension, LanguageModelTool tool)
        {
            ToolType = toolType;
            Extension = extension;
            Tool = tool;
        }

        public string ToolType { get; }

        public OpenResponsesExtension Extension { get; }

        public LanguageModelTool Tool { get; }
    }

    private JsonObject GetArgs(LanguageModelCallOptions options, List<CallWarning> warnings)
    {
        if (options.StopSequences != null)
        {
            warnings.Add(new CallWarning("unsupported", "stopSequences"));
        }

        if (options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        if (options.Seed != null)
        {
            warnings.Add(new CallWarning("unsupported", "seed"));
        }

        var providerToolsByName = new Dictionary<string, LanguageModelTool>(StringComparer.Ordinal);
        foreach (var tool in options.Tools ?? Array.Empty<LanguageModelTool>())
        {
            if (tool.IsProviderDefined)
            {
                providerToolsByName[tool.Name] = tool;
            }
        }

        var converted = OpenResponsesInput.Convert(options.Prompt, ProviderOptionsName, Registry, providerToolsByName, _provider.Options.StrictResponseInput);
        warnings.AddRange(converted.Warnings);

        var convertedTools = new JsonArray();
        var encodedProviderTools = new Dictionary<string, RegisteredProviderTool>(StringComparer.Ordinal);
        foreach (var tool in options.Tools ?? Array.Empty<LanguageModelTool>())
        {
            if (tool.IsProviderDefined)
            {
                JsonObject? encoded = null;
                OpenResponsesExtension? extension;
                if (Registry.ByProviderToolId.TryGetValue(tool.ProviderToolId!, out extension) && extension != null)
                {
                    try
                    {
                        var fields = extension.EncodeTool!(new OpenResponsesToolContext(tool.Name, tool.Args));
                        if (fields != null)
                        {
                            encoded = WithType(fields, extension.ToolType!);
                        }
                    }
                    catch (Exception)
                    {
                        // Encoding failures are reported as unsupported below.
                    }
                }

                if (encoded == null)
                {
                    warnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.ProviderToolId));
                }
                else
                {
                    convertedTools.Add(encoded);
                    encodedProviderTools[tool.Name] = new RegisteredProviderTool(extension!.ToolType!, extension, tool);
                }
            }
            else
            {
                var function = new JsonObject { ["type"] = "function", ["name"] = tool.Name };
                if (tool.Description != null)
                {
                    function["description"] = tool.Description;
                }

                function["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText());
                if (tool.Strict != null)
                {
                    function["strict"] = tool.Strict;
                }

                convertedTools.Add(function);
            }
        }

        JsonNode? toolChoice = null;
        if (options.ToolChoice is ToolChoice.NamedChoice named)
        {
            if (!encodedProviderTools.TryGetValue(named.ToolName, out var registered))
            {
                if (!providerToolsByName.ContainsKey(named.ToolName))
                {
                    toolChoice = new JsonObject { ["type"] = "function", ["name"] = named.ToolName };
                }
            }
            else
            {
                JsonObject? fields = new JsonObject();
                var encodeToolChoice = registered.Extension.EncodeToolChoice;
                if (encodeToolChoice != null)
                {
                    try
                    {
                        fields = encodeToolChoice(new OpenResponsesToolContext(registered.Tool.Name, registered.Tool.Args));
                    }
                    catch (Exception)
                    {
                        fields = null;
                    }
                }

                if (encodeToolChoice != null && fields == null)
                {
                    warnings.Add(new CallWarning("unsupported", "tool choice for provider-defined tool " + registered.Tool.ProviderToolId));
                }
                else
                {
                    toolChoice = WithType(fields ?? new JsonObject(), registered.ToolType);
                }
            }
        }
        else if (options.ToolChoice != null)
        {
            toolChoice = options.ToolChoice.Type;
        }

        JsonObject? textFormat = null;
        if (options.JsonSchema is { } schema)
        {
            textFormat = new JsonObject
            {
                ["type"] = "json_schema",
                ["name"] = options.JsonSchemaName ?? "response",
            };
            if (options.JsonSchemaDescription != null)
            {
                textFormat["description"] = options.JsonSchemaDescription;
            }

            textFormat["schema"] = JsonNode.Parse(schema.GetRawText());
            textFormat["strict"] = true;
        }

        string? providerEffort = null;
        string? providerSummary = null;
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(ProviderOptionsName, out var configured)
            && configured.ValueKind == JsonValueKind.Object)
        {
            if (configured.TryGetProperty("reasoningEffort", out var effortValue) && effortValue.ValueKind == JsonValueKind.String)
            {
                providerEffort = effortValue.GetString();
            }

            if (configured.TryGetProperty("reasoningSummary", out var summaryValue) && summaryValue.ValueKind == JsonValueKind.String)
            {
                providerSummary = summaryValue.GetString();
            }
        }

        var effort = providerEffort;
        if (effort == null && ReasoningMap.IsCustomReasoning(options.Reasoning))
        {
            if (options.Reasoning == "none")
            {
                effort = "none";
            }
            else
            {
                var mappingWarnings = new List<ModelWarning>();
                effort = ReasoningMap.MapReasoningToProviderEffort(options.Reasoning!, ReasoningEffortMap, mappingWarnings);
                foreach (var warning in mappingWarnings)
                {
                    warnings.Add(new CallWarning(warning.Type, (warning as CompatibilityWarning)?.Details ?? (warning as UnsupportedWarning)?.Details ?? string.Empty));
                }
            }
        }

        var body = new JsonObject { ["model"] = ModelId, ["input"] = converted.Input };
        Set(body, "instructions", converted.Instructions);
        Set(body, "max_output_tokens", options.MaxOutputTokens);
        Set(body, "temperature", options.Temperature);
        Set(body, "top_p", options.TopP);
        Set(body, "presence_penalty", options.PresencePenalty);
        Set(body, "frequency_penalty", options.FrequencyPenalty);
        if (effort != null || providerSummary != null)
        {
            var reasoning = new JsonObject();
            if (effort != null)
            {
                reasoning["effort"] = effort;
            }

            if (providerSummary != null)
            {
                reasoning["summary"] = providerSummary;
            }

            body["reasoning"] = reasoning;
        }

        if (convertedTools.Count > 0)
        {
            body["tools"] = convertedTools;
        }

        if (toolChoice != null)
        {
            body["tool_choice"] = toolChoice;
        }

        if (textFormat != null)
        {
            body["text"] = new JsonObject { ["format"] = textFormat };
        }

        return body;
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var warnings = new List<CallWarning>();
        var body = GetArgs(options, warnings);

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
            body.ToJsonString(),
            Headers(options),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            throw ProviderHttp.MapStatus(400, Str(error, "message") ?? "Responses API returned an error", response.Body);
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            var detail = root.TryGetProperty("incomplete_details", out var incomplete) && incomplete.ValueKind == JsonValueKind.Object
                ? Str(incomplete, "reason")
                : null;
            detail ??= Str(root, "status");
            throw ProviderHttp.MapStatus(
                500,
                string.IsNullOrEmpty(detail) ? "Responses API returned no output" : "Responses API returned no output (" + detail + ")",
                response.Body);
        }

        var content = new List<GeneratedContent>();
        var hasToolCalls = false;
        foreach (var part in output.EnumerateArray())
        {
            var type = Str(part, "type");
            switch (type)
            {
                case "reasoning":
                {
                    var item = JsonNode.Parse(part.GetRawText())!.AsObject();
                    var reasoningContent = item["content"] as JsonArray;
                    if (reasoningContent != null && reasoningContent.Count > 0)
                    {
                        foreach (var contentPart in reasoningContent)
                        {
                            content.Add(new GeneratedReasoning(
                                (string?)contentPart!["text"] ?? string.Empty,
                                ReasoningMetadata(item, new JsonArray(contentPart.DeepClone()))));
                        }
                    }
                    else
                    {
                        var summary = string.Concat(((item["summary"] as JsonArray) ?? new JsonArray()).Select(summaryPart => (string?)summaryPart!["text"]));
                        content.Add(new GeneratedReasoning(summary, ReasoningMetadata(item, item["content"] == null ? null : item["content"]!.DeepClone().AsArray())));
                    }

                    break;
                }

                case "message":
                {
                    var itemId = Str(part, "id");
                    if (part.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var contentPart in parts.EnumerateArray())
                        {
                            var annotations = OutputTextAnnotations(contentPart);
                            var metadata = new JsonObject { ["itemId"] = itemId };
                            if (annotations.Count > 0)
                            {
                                metadata["annotations"] = annotations;
                            }

                            content.Add(new GeneratedText(Str(contentPart, "text") ?? string.Empty)
                            {
                                ProviderMetadata = Provider_(metadata),
                            });
                        }
                    }

                    break;
                }

                case "function_call":
                {
                    hasToolCalls = true;
                    content.Add(new GeneratedToolCall(
                        Str(part, "call_id") ?? string.Empty,
                        Str(part, "name") ?? string.Empty,
                        Str(part, "arguments") ?? string.Empty,
                        Provider_(new JsonObject { ["itemId"] = Str(part, "id") })));
                    break;
                }

                default:
                {
                    var item = JsonNode.Parse(part.GetRawText())!.AsObject();
                    if (!OpenResponsesJson.IsExtensionItem(item))
                    {
                        break;
                    }

                    var decoded = DecodeExtensionItem(item, OpenResponsesItemMode.Generate);
                    if (decoded != null)
                    {
                        content.AddRange(decoded);
                        hasToolCalls |= decoded.Any(decodedPart => decodedPart is GeneratedToolCall);
                    }

                    break;
                }
            }
        }

        string? rawReason = null;
        if (root.TryGetProperty("incomplete_details", out var details) && details.ValueKind == JsonValueKind.Object)
        {
            rawReason = Str(details, "reason");
        }

        var usage = root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object
            ? ConvertUsage(usageElement)
            : new LanguageModelUsage(null, null, null);
        DateTimeOffset? timestamp = root.TryGetProperty("created_at", out var created) && created.ValueKind == JsonValueKind.Number
            ? DateTimeOffset.FromUnixTimeSeconds(created.GetInt64())
            : null;

        return new LanguageModelGenerateResult(
            content,
            MapFinishReason(rawReason, hasToolCalls),
            usage,
            rawReason,
            warnings,
            Str(root, "id"),
            null,
            response.Body,
            Str(root, "model"),
            timestamp,
            response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var warnings = new List<CallWarning>();
        var body = GetArgs(options, warnings);
        body["stream"] = true;

        yield return new StreamStartStreamPart(warnings);

        string? activeReasoningId = null;
        var hasToolCalls = false;
        var finishReason = FinishReason.Other;
        string? rawFinishReason = null;
        var usage = new LanguageModelUsage(null, null, null);
        var functionCalls = new Dictionary<string, FunctionCallState>(StringComparer.Ordinal);
        var extensionState = new Dictionary<string, object?>(StringComparer.Ordinal);

        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
            body.ToJsonString(),
            Headers(options),
            cancellationToken).ConfigureAwait(false))
        {
            if (options.IncludeRawChunks)
            {
                yield return new RawStreamPart(data);
            }

            JsonNode? node = null;
            string? parseFailure = null;
            try
            {
                node = JsonNode.Parse(data);
            }
            catch (JsonException exception)
            {
                parseFailure = exception.Message;
            }

            if (parseFailure != null)
            {
                yield return new ErrorStreamPart(parseFailure);
                continue;
            }

            if (node is not JsonObject chunk)
            {
                continue;
            }

            var chunkType = (string?)chunk["type"];

            if (OpenResponsesJson.IsExtensionEvent(chunk))
            {
                if (chunkType != null && Registry.ByEventType.TryGetValue(chunkType, out var eventExtension) && eventExtension.DecodeEvent != null)
                {
                    IReadOnlyList<LanguageModelStreamPart>? decodedParts = null;
                    string? failure = null;
                    try
                    {
                        decodedParts = eventExtension.DecodeEvent(new OpenResponsesDecodeEventContext(chunk, extensionState));
                    }
                    catch (Exception exception)
                    {
                        failure = exception.Message;
                    }

                    foreach (var decodedPart in decodedParts ?? Array.Empty<LanguageModelStreamPart>())
                    {
                        yield return decodedPart;
                        hasToolCalls |= decodedPart is ToolCallStreamPart || decodedPart is ToolInputStartStreamPart;
                    }

                    if (failure != null)
                    {
                        yield return new ErrorStreamPart(failure);
                    }
                }

                continue;
            }

            var item = chunk["item"] as JsonObject;
            var itemType = item == null ? null : (string?)item["type"];

            if (chunkType == "response.output_item.added" && itemType == "function_call")
            {
                functionCalls[(string)item!["id"]!] = new FunctionCallState
                {
                    ToolName = (string?)item["name"],
                    ToolCallId = (string?)item["call_id"],
                    Arguments = (string?)item["arguments"],
                };
            }
            else if (chunkType == "response.function_call_arguments.delta")
            {
                var itemId = (string)chunk["item_id"]!;
                var delta = (string?)chunk["delta"] ?? string.Empty;
                if (!functionCalls.TryGetValue(itemId, out var call))
                {
                    functionCalls[itemId] = new FunctionCallState { Arguments = delta };
                }
                else
                {
                    call.Arguments = (call.Arguments ?? string.Empty) + delta;
                }
            }
            else if (chunkType == "response.function_call_arguments.done")
            {
                var itemId = (string)chunk["item_id"]!;
                var finalArguments = (string?)chunk["arguments"];
                if (!functionCalls.TryGetValue(itemId, out var call))
                {
                    functionCalls[itemId] = new FunctionCallState { Arguments = finalArguments };
                }
                else
                {
                    call.Arguments = finalArguments;
                }
            }
            else if (chunkType == "response.output_item.done" && itemType == "function_call")
            {
                var itemId = (string)item!["id"]!;
                functionCalls.TryGetValue(itemId, out var call);
                yield return new ToolCallStreamPart(
                    call?.ToolCallId ?? (string?)item["call_id"] ?? string.Empty,
                    call?.ToolName ?? (string?)item["name"] ?? string.Empty,
                    call?.Arguments ?? (string?)item["arguments"] ?? string.Empty,
                    Provider_(new JsonObject { ["itemId"] = itemId }));
                hasToolCalls = true;
                functionCalls.Remove(itemId);
            }
            else if (chunkType == "response.output_item.done" && item != null && OpenResponsesJson.IsExtensionItem(item))
            {
                List<GeneratedContent>? decoded = null;
                string? failure = null;
                try
                {
                    decoded = DecodeExtensionItem(item, OpenResponsesItemMode.Stream);
                }
                catch (Exception exception)
                {
                    failure = exception.Message;
                }

                foreach (var decodedPart in decoded ?? new List<GeneratedContent>())
                {
                    yield return ToStreamPart(decodedPart);
                    hasToolCalls |= decodedPart is GeneratedToolCall;
                }

                if (failure != null)
                {
                    yield return new ErrorStreamPart(failure);
                }
            }
            else if (chunkType == "response.output_item.added" && itemType == "reasoning")
            {
                var id = (string)item!["id"]!;
                yield return new ReasoningStartStreamPart(id);
                activeReasoningId = id;
            }
            else if (chunkType == "response.reasoning_summary_text.delta" || chunkType == "response.reasoning_text.delta")
            {
                yield return new ReasoningDeltaStreamPart((string)chunk["item_id"]!, (string?)chunk["delta"] ?? string.Empty);
            }
            else if (chunkType == "response.output_item.done" && itemType == "reasoning")
            {
                var id = (string)item!["id"]!;
                yield return new ReasoningEndStreamPart(id, ReasoningMetadata(item, null));
                if (activeReasoningId == id)
                {
                    activeReasoningId = null;
                }
            }
            else if (chunkType == "response.output_item.added" && itemType == "message")
            {
                yield return new TextStartStreamPart((string)item!["id"]!);
            }
            else if (chunkType == "response.output_text.delta")
            {
                yield return new TextDeltaStreamPart((string)chunk["item_id"]!, (string?)chunk["delta"] ?? string.Empty);
            }
            else if (chunkType == "response.output_item.done" && itemType == "message")
            {
                var itemId = (string)item!["id"]!;
                var annotations = new JsonArray();
                foreach (var contentPart in (item["content"] as JsonArray) ?? new JsonArray())
                {
                    foreach (var annotation in OutputTextAnnotations(JsonSerializer.SerializeToElement(contentPart)))
                    {
                        annotations.Add(annotation!.DeepClone());
                    }
                }

                var metadata = new JsonObject { ["itemId"] = itemId };
                if (annotations.Count > 0)
                {
                    metadata["annotations"] = annotations;
                }

                yield return new TextEndStreamPart(itemId, null, Provider_(metadata));
            }
            else if (chunkType == "response.completed" || chunkType == "response.incomplete")
            {
                var response = chunk["response"] as JsonObject;
                rawFinishReason = response?["incomplete_details"]?["reason"]?.GetValue<string>();
                finishReason = MapFinishReason(rawFinishReason, hasToolCalls);
                if (response?["usage"] is JsonObject usageNode)
                {
                    usage = ConvertUsage(JsonSerializer.SerializeToElement(usageNode));
                }
            }
            else if (chunkType == "response.failed")
            {
                var response = chunk["response"] as JsonObject;
                finishReason = FinishReason.Error;
                rawFinishReason = response?["error"]?["code"]?.GetValue<string>() ?? (string?)response?["status"];
                if (response?["usage"] is JsonObject usageNode)
                {
                    usage = ConvertUsage(JsonSerializer.SerializeToElement(usageNode));
                }

                if (response?["error"] is JsonObject failed)
                {
                    yield return new ErrorStreamPart((string?)failed["message"] ?? "Responses API stream failed");
                }
            }
            else if (chunkType == "error")
            {
                finishReason = FinishReason.Error;
                var errorNode = chunk["error"] as JsonObject;
                rawFinishReason = (string?)errorNode?["code"];
                yield return new ErrorStreamPart((string?)errorNode?["message"] ?? "Responses API stream error");
            }
        }

        if (activeReasoningId != null)
        {
            yield return new ReasoningEndStreamPart(activeReasoningId);
        }

        yield return new FinishStreamPart(finishReason, usage, rawFinishReason);
    }

    private sealed class FunctionCallState
    {
        public string? ToolName { get; set; }

        public string? ToolCallId { get; set; }

        public string? Arguments { get; set; }
    }

    private IReadOnlyDictionary<string, string?> Headers(LanguageModelCallOptions options)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = "Bearer " + ApiKeys.Require(_provider.Options.ApiKey, _provider.Options.ApiKeyEnvironmentVariable),
        };
        foreach (var pair in _provider.Options.Headers ?? new Dictionary<string, string>())
        {
            headers[pair.Key] = pair.Value;
        }

        foreach (var pair in options.Headers ?? new Dictionary<string, string?>())
        {
            headers[pair.Key] = pair.Value;
        }

        return headers;
    }

    private static void Set(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Set(JsonObject body, string name, int? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Set(JsonObject body, string name, double? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static JsonObject WithType(JsonObject fields, string type)
    {
        var result = new JsonObject { ["type"] = type };
        foreach (var pair in fields)
        {
            if (pair.Key != "type")
            {
                result[pair.Key] = pair.Value?.DeepClone();
            }
        }

        return result;
    }

    private static string? Str(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }

    private static int? Int(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetInt32()
                : null;
    }

    private JsonElement Provider_(JsonObject inner)
    {
        return JsonSerializer.SerializeToElement(new JsonObject { [ProviderOptionsName] = inner });
    }

    private static FinishReason MapFinishReason(string? reason, bool hasToolCalls)
    {
        switch (reason)
        {
            case null:
                return hasToolCalls ? FinishReason.ToolCalls : FinishReason.Stop;
            case "max_output_tokens":
                return FinishReason.Length;
            case "content_filter":
                return FinishReason.ContentFilter;
            default:
                return hasToolCalls ? FinishReason.ToolCalls : FinishReason.Other;
        }
    }

    private static LanguageModelUsage ConvertUsage(JsonElement usage)
    {
        var input = Int(usage, "input_tokens");
        var output = Int(usage, "output_tokens");
        int? cached = usage.TryGetProperty("input_tokens_details", out var inputDetails) ? Int(inputDetails, "cached_tokens") : null;
        int? reasoning = usage.TryGetProperty("output_tokens_details", out var outputDetails) ? Int(outputDetails, "reasoning_tokens") : null;
        return new LanguageModelUsage(
            input,
            output,
            null,
            cached,
            null,
            reasoning,
            usage.Clone(),
            (input ?? 0) - (cached ?? 0),
            (output ?? 0) - (reasoning ?? 0));
    }

    private JsonElement ReasoningMetadata(JsonObject part, JsonArray? reasoningContent)
    {
        reasoningContent ??= part["content"] as JsonArray;
        var summary = new JsonArray();
        foreach (var summaryPart in (part["summary"] as JsonArray) ?? new JsonArray())
        {
            summary.Add(new JsonObject { ["type"] = "summary_text", ["text"] = (string?)summaryPart!["text"] });
        }

        JsonNode? contentNode = null;
        if (reasoningContent != null)
        {
            var array = new JsonArray();
            foreach (var contentPart in reasoningContent)
            {
                array.Add(new JsonObject { ["type"] = "reasoning_text", ["text"] = (string?)contentPart!["text"] });
            }

            contentNode = array;
        }

        var inner = new JsonObject
        {
            ["itemId"] = (string?)part["id"],
            ["reasoningSummary"] = summary,
            ["reasoningContent"] = contentNode,
        };
        if ((string?)part["encrypted_content"] is { } encrypted)
        {
            inner["reasoningEncryptedContent"] = encrypted;
        }

        return Provider_(inner);
    }

    private static JsonArray OutputTextAnnotations(JsonElement value)
    {
        var result = new JsonArray();
        if (value.ValueKind != JsonValueKind.Object
            || !value.TryGetProperty("annotations", out var annotations)
            || annotations.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var annotation in annotations.EnumerateArray())
        {
            if (annotation.ValueKind != JsonValueKind.Object
                || Str(annotation, "type") != "url_citation"
                || !annotation.TryGetProperty("start_index", out var start) || start.ValueKind != JsonValueKind.Number
                || !annotation.TryGetProperty("end_index", out var end) || end.ValueKind != JsonValueKind.Number
                || Str(annotation, "url") == null
                || Str(annotation, "title") == null)
            {
                return new JsonArray();
            }
        }

        foreach (var annotation in annotations.EnumerateArray())
        {
            result.Add(new JsonObject
            {
                ["type"] = "url_citation",
                ["start_index"] = annotation.GetProperty("start_index").GetInt32(),
                ["end_index"] = annotation.GetProperty("end_index").GetInt32(),
                ["url"] = Str(annotation, "url"),
                ["title"] = Str(annotation, "title"),
            });
        }

        return result;
    }

    private List<GeneratedContent>? DecodeExtensionItem(JsonObject item, OpenResponsesItemMode mode)
    {
        var type = (string)item["type"]!;
        if (!Registry.ByItemType.TryGetValue(type, out var extension) || extension.DecodeItem == null)
        {
            return null;
        }

        var decoded = extension.DecodeItem(new OpenResponsesDecodeItemContext(item, mode));
        if (decoded == null)
        {
            return null;
        }

        var result = new List<GeneratedContent>
        {
            new GeneratedCustom(
                ReplayKind,
                Provider_(new JsonObject
                {
                    ["openResponsesExtension"] = new JsonObject { ["id"] = extension.Id, ["item"] = item.DeepClone() },
                })),
        };
        foreach (var part in decoded)
        {
            result.Add(WithExtensionReference(part, extension, item));
        }

        return result;
    }

    private GeneratedContent WithExtensionReference(GeneratedContent part, OpenResponsesExtension extension, JsonObject item)
    {
        JsonElement? existing;
        switch (part)
        {
            case GeneratedText text:
                existing = text.ProviderMetadata;
                break;
            case GeneratedReasoning reasoning:
                existing = reasoning.ProviderMetadata;
                break;
            case GeneratedToolCall call:
                existing = call.ProviderMetadata;
                break;
            case GeneratedToolResult toolResult:
                existing = toolResult.ProviderMetadata;
                break;
            case GeneratedSource source:
                existing = source.ProviderMetadata;
                break;
            case GeneratedDocumentSource documentSource:
                existing = documentSource.ProviderMetadata;
                break;
            case GeneratedCustom custom:
                existing = custom.ProviderMetadata;
                break;
            default:
                return part;
        }

        var metadata = existing is { ValueKind: JsonValueKind.Object } element
            ? JsonNode.Parse(element.GetRawText())!.AsObject()
            : new JsonObject();
        var inner = metadata[ProviderOptionsName] as JsonObject ?? new JsonObject();
        inner["openResponsesExtension"] = new JsonObject { ["id"] = extension.Id, ["itemId"] = (string)item["id"]! };
        metadata[ProviderOptionsName] = inner;
        var merged = JsonSerializer.SerializeToElement(metadata);

        switch (part)
        {
            case GeneratedText text:
                return new GeneratedText(text.Text, text.Citations) { ProviderMetadata = merged };
            case GeneratedReasoning reasoning:
                return new GeneratedReasoning(reasoning.Text, merged);
            case GeneratedToolCall call:
                return new GeneratedToolCall(call.ToolCallId, call.ToolName, call.ArgumentsJson, merged) { ProviderExecuted = call.ProviderExecuted };
            case GeneratedToolResult toolResult:
                return new GeneratedToolResult(toolResult.ToolCallId, toolResult.ToolName, toolResult.ResultJson, toolResult.IsError) { ProviderMetadata = merged };
            case GeneratedSource source:
                return new GeneratedSource(source.Id, source.Url, source.Title, merged);
            case GeneratedDocumentSource documentSource:
                return new GeneratedDocumentSource(documentSource.Id, documentSource.MediaType, documentSource.Title, documentSource.Filename, merged);
            default:
                return new GeneratedCustom(((GeneratedCustom)part).Kind, merged);
        }
    }

    private static LanguageModelStreamPart ToStreamPart(GeneratedContent part)
    {
        switch (part)
        {
            case GeneratedToolCall call:
                return new ToolCallStreamPart(call.ToolCallId, call.ToolName, call.ArgumentsJson, call.ProviderMetadata) { ProviderExecuted = call.ProviderExecuted };
            case GeneratedToolResult toolResult:
                return new ToolResultStreamPart(toolResult.ToolCallId, toolResult.ToolName, JsonValues.ParseOrEmpty(toolResult.ResultJson)) { ProviderMetadata = toolResult.ProviderMetadata };
            case GeneratedSource source:
                return new SourceStreamPart(source.Id, source.Url, source.Title, source.ProviderMetadata);
            case GeneratedDocumentSource documentSource:
                return new DocumentSourceStreamPart(documentSource.Id, documentSource.MediaType, documentSource.Title, documentSource.Filename, documentSource.ProviderMetadata);
            case GeneratedCustom custom:
                return new CustomStreamPart(custom.Kind, custom.ProviderMetadata);
            case GeneratedReasoning reasoning:
                return new ReasoningDeltaStreamPart(string.Empty, reasoning.Text);
            case GeneratedText text:
                return new TextDeltaStreamPart(string.Empty, text.Text, text.ProviderMetadata);
            default:
                throw new InvalidOperationException("Open Responses extension content of type " + part.Type + " cannot be streamed.");
        }
    }
}
