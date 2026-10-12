// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.OpenResponses;

/// <summary>Posts to <c>/responses</c>.</summary>
public sealed class OpenResponsesLanguageModel : ILanguageModel
{
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

    private static string Name => OpenResponsesProvider.ProviderName;

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var (body, warnings) = GetArgs(options);
        var url = ApiKeys.Combine(_provider.Options.BaseUrl, "responses");
        var response = await _provider.Http.SendJsonStringAsync(HttpMethod.Post, url, body.ToJsonString(), CreateHeaders(options), cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            throw new ApiException(StringOf(error, "message") ?? "Responses API returned an error", 400, response.Body);
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            var detail = root.TryGetProperty("incomplete_details", out var incomplete) && incomplete.ValueKind == JsonValueKind.Object
                ? StringOf(incomplete, "reason")
                : null;
            detail ??= StringOf(root, "status");
            throw new ApiException(
                string.IsNullOrEmpty(detail) ? "Responses API returned no output" : "Responses API returned no output (" + detail + ")",
                500,
                response.Body);
        }

        var content = new List<GeneratedContent>();
        var hasToolCalls = false;
        foreach (var part in output.EnumerateArray())
        {
            switch (StringOf(part, "type"))
            {
                case "reasoning":
                    AddReasoning(content, part);
                    break;
                case "message":
                    foreach (var contentPart in part.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array ? parts.EnumerateArray() : default)
                    {
                        if (StringOf(contentPart, "type") != "output_text")
                        {
                            continue;
                        }

                        content.Add(new GeneratedText(StringOf(contentPart, "text") ?? string.Empty, null, TextMetadata(StringOf(part, "id"), Annotations(contentPart))));
                    }

                    break;
                case "function_call":
                    hasToolCalls = true;
                    content.Add(new GeneratedToolCall(
                        StringOf(part, "call_id") ?? string.Empty,
                        StringOf(part, "name") ?? string.Empty,
                        StringOf(part, "arguments") ?? "{}",
                        Metadata(new JsonObject { ["itemId"] = StringOf(part, "id") })));
                    break;
            }
        }

        var incompleteReason = root.TryGetProperty("incomplete_details", out var details) && details.ValueKind == JsonValueKind.Object
            ? StringOf(details, "reason")
            : null;
        DateTimeOffset? timestamp = root.TryGetProperty("created_at", out var created) && created.ValueKind == JsonValueKind.Number
            ? DateTimeOffset.FromUnixTimeSeconds(created.GetInt64())
            : null;
        return new LanguageModelGenerateResult(
            content,
            MapFinishReason(incompleteReason, hasToolCalls),
            ParseUsage(root),
            incompleteReason,
            warnings,
            StringOf(root, "id"),
            null,
            response.Body,
            StringOf(root, "model"),
            timestamp,
            new Dictionary<string, string>(response.Headers.ToDictionary(pair => pair.Key, pair => pair.Value)));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var (body, warnings) = GetArgs(options);
        body["stream"] = true;
        var url = ApiKeys.Combine(_provider.Options.BaseUrl, "responses");
        var enumerator = _provider.Http.SendSseAsync(url, body.ToJsonString(), CreateHeaders(options), cancellationToken).GetAsyncEnumerator(cancellationToken);
        try
        {
            var more = await enumerator.MoveNextAsync().ConfigureAwait(false);
            yield return new StreamStartStreamPart(warnings);

            var usage = LanguageModelUsage.Empty;
            var finish = FinishReason.Other;
            string? rawFinish = null;
            var hasToolCalls = false;
            string? activeReasoningId = null;
            var toolCalls = new Dictionary<string, ToolCallState>(StringComparer.Ordinal);

            while (more)
            {
                var data = enumerator.Current;
                if (options.IncludeRawChunks)
                {
                    yield return new RawStreamPart(data);
                }

                JsonDocument? document = null;
                string? parseError = null;
                try
                {
                    document = JsonDocument.Parse(data);
                }
                catch (JsonException exception)
                {
                    parseError = exception.Message;
                }

                if (parseError != null)
                {
                    yield return new ErrorStreamPart(parseError);
                }

                if (document != null)
                {
                    using (document)
                    {
                        var chunk = document.RootElement;
                        var type = StringOf(chunk, "type");
                        var item = chunk.TryGetProperty("item", out var itemValue) && itemValue.ValueKind == JsonValueKind.Object ? itemValue : default;
                        var itemType = item.ValueKind == JsonValueKind.Object ? StringOf(item, "type") : null;
                        var itemId = item.ValueKind == JsonValueKind.Object ? StringOf(item, "id") : null;

                        if (type == "response.output_item.added" && itemType == "function_call")
                        {
                            toolCalls[itemId ?? string.Empty] = new ToolCallState
                            {
                                ToolName = StringOf(item, "name"),
                                ToolCallId = StringOf(item, "call_id"),
                                Arguments = StringOf(item, "arguments"),
                            };
                        }
                        else if (type == "response.function_call_arguments.delta")
                        {
                            var key = StringOf(chunk, "item_id") ?? string.Empty;
                            if (!toolCalls.TryGetValue(key, out var state))
                            {
                                toolCalls[key] = new ToolCallState { Arguments = StringOf(chunk, "delta") };
                            }
                            else
                            {
                                state.Arguments = (state.Arguments ?? string.Empty) + StringOf(chunk, "delta");
                            }
                        }
                        else if (type == "response.function_call_arguments.done")
                        {
                            var key = StringOf(chunk, "item_id") ?? string.Empty;
                            if (!toolCalls.TryGetValue(key, out var state))
                            {
                                toolCalls[key] = new ToolCallState { Arguments = StringOf(chunk, "arguments") };
                            }
                            else
                            {
                                state.Arguments = StringOf(chunk, "arguments");
                            }
                        }
                        else if (type == "response.output_item.done" && itemType == "function_call")
                        {
                            toolCalls.TryGetValue(itemId ?? string.Empty, out var state);
                            yield return new ToolCallStreamPart(
                                state?.ToolCallId ?? StringOf(item, "call_id") ?? string.Empty,
                                state?.ToolName ?? StringOf(item, "name") ?? string.Empty,
                                state?.Arguments ?? StringOf(item, "arguments") ?? string.Empty,
                                Metadata(new JsonObject { ["itemId"] = itemId }));
                            hasToolCalls = true;
                            toolCalls.Remove(itemId ?? string.Empty);
                        }
                        else if (type == "response.output_item.added" && itemType == "reasoning")
                        {
                            yield return new ReasoningStartStreamPart(itemId ?? string.Empty);
                            activeReasoningId = itemId;
                        }
                        else if (type == "response.reasoning_summary_text.delta" || type == "response.reasoning_text.delta")
                        {
                            yield return new ReasoningDeltaStreamPart(StringOf(chunk, "item_id") ?? string.Empty, StringOf(chunk, "delta") ?? string.Empty);
                        }
                        else if (type == "response.output_item.done" && itemType == "reasoning")
                        {
                            yield return new ReasoningEndStreamPart(itemId ?? string.Empty, ReasoningMetadata(item, null));
                            if (activeReasoningId == itemId)
                            {
                                activeReasoningId = null;
                            }
                        }
                        else if (type == "response.output_item.added" && itemType == "message")
                        {
                            yield return new TextStartStreamPart(itemId ?? string.Empty);
                        }
                        else if (type == "response.output_text.delta")
                        {
                            yield return new TextDeltaStreamPart(StringOf(chunk, "item_id") ?? string.Empty, StringOf(chunk, "delta") ?? string.Empty);
                        }
                        else if (type == "response.output_item.done" && itemType == "message")
                        {
                            var annotations = new JsonArray();
                            if (item.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var part in parts.EnumerateArray())
                                {
                                    foreach (var annotation in Annotations(part))
                                    {
                                        annotations.Add(annotation);
                                    }
                                }
                            }

                            yield return new TextEndStreamPart(itemId ?? string.Empty, null, TextMetadata(itemId, annotations));
                        }
                        else if (type == "response.completed" || type == "response.incomplete")
                        {
                            var response = chunk.TryGetProperty("response", out var responseValue) ? responseValue : default;
                            rawFinish = response.ValueKind == JsonValueKind.Object
                                && response.TryGetProperty("incomplete_details", out var incomplete)
                                && incomplete.ValueKind == JsonValueKind.Object
                                    ? StringOf(incomplete, "reason")
                                    : null;
                            finish = MapFinishReason(rawFinish, hasToolCalls);
                            if (response.ValueKind == JsonValueKind.Object)
                            {
                                usage = ParseUsage(response);
                            }
                        }
                        else if (type == "response.failed")
                        {
                            var response = chunk.TryGetProperty("response", out var responseValue) ? responseValue : default;
                            var error = response.ValueKind == JsonValueKind.Object && response.TryGetProperty("error", out var errorValue) && errorValue.ValueKind == JsonValueKind.Object
                                ? errorValue
                                : default;
                            finish = FinishReason.Error;
                            rawFinish = error.ValueKind == JsonValueKind.Object ? StringOf(error, "code") : StringOf(response, "status");
                            if (response.ValueKind == JsonValueKind.Object)
                            {
                                usage = ParseUsage(response);
                            }

                            if (error.ValueKind == JsonValueKind.Object)
                            {
                                yield return new ErrorStreamPart(StringOf(error, "message") ?? "Unknown provider error.");
                            }
                        }
                        else if (type == "error")
                        {
                            var error = chunk.TryGetProperty("error", out var errorValue) && errorValue.ValueKind == JsonValueKind.Object ? errorValue : default;
                            finish = FinishReason.Error;
                            rawFinish = error.ValueKind == JsonValueKind.Object ? StringOf(error, "code") : null;
                            yield return new ErrorStreamPart(error.ValueKind == JsonValueKind.Object ? StringOf(error, "message") ?? "Unknown provider error." : "Unknown provider error.");
                        }
                    }
                }

                more = await enumerator.MoveNextAsync().ConfigureAwait(false);
            }

            if (activeReasoningId != null)
            {
                yield return new ReasoningEndStreamPart(activeReasoningId);
            }

            yield return new FinishStreamPart(finish, usage, rawFinish);
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    private IReadOnlyDictionary<string, string?> CreateHeaders(LanguageModelCallOptions options)
    {
        var headers = new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer " + ApiKeys.Require(_provider.Options.ApiKey, _provider.Options.ApiKeyEnvironmentVariable),
        };
        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private (JsonObject Body, List<CallWarning> Warnings) GetArgs(LanguageModelCallOptions options)
    {
        var warnings = new List<CallWarning>();
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

        var converted = OpenResponsesInput.Convert(options.Prompt, Name, _provider.Options.StrictResponseInput);
        warnings.AddRange(converted.Warnings);

        string? providerEffort = null;
        string? providerSummary = null;
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(Name, out var configured)
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
        if (converted.Instructions != null)
        {
            body["instructions"] = converted.Instructions;
        }

        if (options.MaxOutputTokens != null)
        {
            body["max_output_tokens"] = options.MaxOutputTokens;
        }

        if (options.Temperature != null)
        {
            body["temperature"] = options.Temperature;
        }

        if (options.TopP != null)
        {
            body["top_p"] = options.TopP;
        }

        if (options.PresencePenalty != null)
        {
            body["presence_penalty"] = options.PresencePenalty;
        }

        if (options.FrequencyPenalty != null)
        {
            body["frequency_penalty"] = options.FrequencyPenalty;
        }

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

        if (options.Tools is { Count: > 0 } tools)
        {
            var array = new JsonArray();
            foreach (var tool in tools)
            {
                var item = new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                };
                if (tool.Strict != null)
                {
                    item["strict"] = tool.Strict;
                }

                array.Add(item);
            }

            body["tools"] = array;
        }

        if (options.ToolChoice is ToolChoice.NamedChoice named)
        {
            body["tool_choice"] = new JsonObject { ["type"] = "function", ["name"] = named.ToolName };
        }
        else if (options.ToolChoice != null)
        {
            body["tool_choice"] = options.ToolChoice.Type;
        }

        if (options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Undefined && schema.ValueKind != JsonValueKind.Null)
        {
            body["text"] = new JsonObject
            {
                ["format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["name"] = string.IsNullOrEmpty(options.JsonSchemaName) ? "response" : options.JsonSchemaName,
                    ["description"] = options.JsonSchemaDescription,
                    ["schema"] = JsonNode.Parse(schema.GetRawText()),
                    ["strict"] = true,
                },
            };
        }

        return (body, warnings);
    }

    private static void AddReasoning(List<GeneratedContent> content, JsonElement part)
    {
        if (part.TryGetProperty("content", out var reasoningContent) && reasoningContent.ValueKind == JsonValueKind.Array && reasoningContent.GetArrayLength() > 0)
        {
            foreach (var contentPart in reasoningContent.EnumerateArray())
            {
                content.Add(new GeneratedReasoning(StringOf(contentPart, "text") ?? string.Empty, ReasoningMetadata(part, new[] { contentPart })));
            }

            return;
        }

        var summary = new System.Text.StringBuilder();
        if (part.TryGetProperty("summary", out var summaryParts) && summaryParts.ValueKind == JsonValueKind.Array)
        {
            foreach (var summaryPart in summaryParts.EnumerateArray())
            {
                summary.Append(StringOf(summaryPart, "text"));
            }
        }

        content.Add(new GeneratedReasoning(summary.ToString(), ReasoningMetadata(part, null, useItemContent: false)));
    }

    // Reasoning metadata carries what a later turn needs to replay the item. A null content list serializes as null.
    private static JsonElement ReasoningMetadata(JsonElement part, IReadOnlyList<JsonElement>? contentOverride, bool useItemContent = true)
    {
        var summary = new JsonArray();
        if (part.TryGetProperty("summary", out var summaryParts) && summaryParts.ValueKind == JsonValueKind.Array)
        {
            foreach (var summaryPart in summaryParts.EnumerateArray())
            {
                summary.Add(new JsonObject { ["type"] = "summary_text", ["text"] = StringOf(summaryPart, "text") });
            }
        }

        IEnumerable<JsonElement>? contentSource = contentOverride;
        if (contentSource == null && useItemContent && part.TryGetProperty("content", out var itemContent) && itemContent.ValueKind == JsonValueKind.Array)
        {
            contentSource = itemContent.EnumerateArray();
        }

        JsonNode? reasoningContent = null;
        if (contentSource != null)
        {
            var array = new JsonArray();
            foreach (var contentPart in contentSource)
            {
                array.Add(new JsonObject { ["type"] = "reasoning_text", ["text"] = StringOf(contentPart, "text") });
            }

            reasoningContent = array;
        }

        var metadata = new JsonObject
        {
            ["itemId"] = StringOf(part, "id"),
            ["reasoningSummary"] = summary,
            ["reasoningContent"] = reasoningContent,
        };
        if (StringOf(part, "encrypted_content") is { } encrypted)
        {
            metadata["reasoningEncryptedContent"] = encrypted;
        }

        return Metadata(metadata);
    }

    private static JsonElement TextMetadata(string? itemId, JsonArray annotations)
    {
        var metadata = new JsonObject { ["itemId"] = itemId };
        if (annotations.Count > 0)
        {
            metadata["annotations"] = annotations;
        }

        return Metadata(metadata);
    }

    private static JsonArray Annotations(JsonElement contentPart)
    {
        var result = new JsonArray();
        if (!contentPart.TryGetProperty("annotations", out var list) || list.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var entry in list.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object
                || StringOf(entry, "type") != "url_citation"
                || !entry.TryGetProperty("start_index", out var start) || start.ValueKind != JsonValueKind.Number
                || !entry.TryGetProperty("end_index", out var end) || end.ValueKind != JsonValueKind.Number
                || StringOf(entry, "url") is not { } url
                || StringOf(entry, "title") is not { } title)
            {
                return new JsonArray();
            }

            result.Add(new JsonObject
            {
                ["type"] = "url_citation",
                ["start_index"] = JsonNode.Parse(start.GetRawText()),
                ["end_index"] = JsonNode.Parse(end.GetRawText()),
                ["url"] = url,
                ["title"] = title,
            });
        }

        return result;
    }

    private static JsonElement Metadata(JsonObject value)
    {
        using var document = JsonDocument.Parse(new JsonObject { [Name] = value }.ToJsonString());
        return document.RootElement.Clone();
    }

    private static LanguageModelUsage ParseUsage(JsonElement response)
    {
        if (!response.TryGetProperty("usage", out var usage) || usage.ValueKind != JsonValueKind.Object)
        {
            return LanguageModelUsage.Empty;
        }

        var input = IntOf(usage, "input_tokens");
        var output = IntOf(usage, "output_tokens");
        var cached = usage.TryGetProperty("input_tokens_details", out var inputDetails) && inputDetails.ValueKind == JsonValueKind.Object ? IntOf(inputDetails, "cached_tokens") : null;
        var reasoning = usage.TryGetProperty("output_tokens_details", out var outputDetails) && outputDetails.ValueKind == JsonValueKind.Object ? IntOf(outputDetails, "reasoning_tokens") : null;
        return new LanguageModelUsage(
            input,
            output,
            IntOf(usage, "total_tokens"),
            cached,
            null,
            reasoning,
            usage.Clone(),
            (input ?? 0) - (cached ?? 0),
            (output ?? 0) - (reasoning ?? 0));
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

    private static string? StringOf(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static int? IntOf(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private sealed class ToolCallState
    {
        public string? ToolName { get; set; }

        public string? ToolCallId { get; set; }

        public string? Arguments { get; set; }
    }
}
