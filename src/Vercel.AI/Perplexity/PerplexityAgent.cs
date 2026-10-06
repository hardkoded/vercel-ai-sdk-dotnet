// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Perplexity;

/// <summary>Parses Perplexity Agent API JSON without treating native tool traces as web search results.</summary>
internal static class PerplexityAgent
{
    private static readonly string[] Presets = { "fast", "low", "medium", "high", "xhigh" };
    private static readonly string[] Efforts = { "minimal", "low", "medium", "high", "xhigh" };
    private static readonly string[] Recency = { "hour", "day", "week", "month", "year" };
    private static readonly string[] ContextSizes = { "low", "medium", "high" };
    private static readonly string[] BuiltinSkills = { "office", "office/docx", "office/pdf", "office/pptx", "office/xlsx" };
    private static readonly string[] HandledTypes = { "message", "search_results", "fetch_url_results", "function_call" };

    public static PerplexityModelSelection GetModelSelection(string modelId)
    {
        for (var index = 0; index < Presets.Length; index++)
        {
            if (string.Equals(Presets[index], modelId, StringComparison.Ordinal))
            {
                return new PerplexityModelSelection(modelId, null);
            }
        }

        return new PerplexityModelSelection(null, modelId);
    }

    public static string? MapReasoningEffort(string? reasoning, IList<CallWarning> warnings)
    {
        if (string.IsNullOrEmpty(reasoning))
        {
            return null;
        }

        if (string.Equals(reasoning, "none", StringComparison.Ordinal))
        {
            warnings.Add(new CallWarning("unsupported", "reasoning \"none\""));
            return null;
        }

        if (!Contains(Efforts, reasoning))
        {
            warnings.Add(new CallWarning("unsupported", "reasoning \"" + reasoning + "\""));
            return null;
        }

        return reasoning;
    }

    public static void ValidateProviderOptions(JsonElement options)
    {
        if (options.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: expected an object.");
        }

        foreach (var property in options.EnumerateObject())
        {
            switch (property.Name)
            {
                case "instructions":
                case "previous_response_id":
                case "language_preference":
                    RequireString(property);
                    break;
                case "models":
                    RequireStringArray(property);
                    break;
                case "max_steps":
                    RequireInteger(property, minimum: 1, "max_steps must be a positive integer.");
                    break;
                case "max_tool_calls":
                    RequireInteger(property, minimum: 0, "max_tool_calls must be a non-negative integer.");
                    break;
                case "store":
                    RequireBool(property);
                    break;
                case "reasoning":
                    ValidateReasoning(property.Value);
                    break;
                case "tools":
                    ValidateNativeTools(property.Value);
                    break;
                case "skills":
                    ValidateSkills(property.Value);
                    break;
            }
        }
    }

    public static bool TryReadResponse(JsonElement root, out AgentResponse response)
    {
        response = new AgentResponse();
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!TryRequiredString(root, "id", out var id)
            || !TryRequiredString(root, "model", out var model)
            || !TryRequiredString(root, "status", out var status)
            || !TryUnix(root, "created_at", out var createdAt))
        {
            return false;
        }

        if (!root.TryGetProperty("object", out var objectName)
            || objectName.ValueKind != JsonValueKind.String
            || !string.Equals(objectName.GetString(), "response", StringComparison.Ordinal))
        {
            return false;
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        if (!TryNullishObject(root, "incomplete_details", out var incomplete, out var incompleteOk) || !incompleteOk)
        {
            return false;
        }

        string? incompleteReason = null;
        if (incomplete is { } incompleteElement)
        {
            if (!TryRequiredString(incompleteElement, "reason", out var reason))
            {
                return false;
            }

            incompleteReason = reason;
        }

        if (!TryNullishObject(root, "error", out var error, out var errorOk) || !errorOk)
        {
            return false;
        }

        string? errorMessage = null;
        var hasError = false;
        if (error is { } errorElement)
        {
            hasError = true;
            if (!TryRequiredString(errorElement, "message", out var message))
            {
                return false;
            }

            errorMessage = message;
        }

        if (!TryNullishUsage(root, out var usage))
        {
            return false;
        }

        var items = new List<AgentItem>();
        foreach (var item in output.EnumerateArray())
        {
            if (!TryReadItem(item, out var parsed))
            {
                return false;
            }

            items.Add(parsed!);
        }

        response = new AgentResponse
        {
            Id = id,
            Model = model,
            Status = status,
            CreatedAt = createdAt,
            IncompleteReason = incompleteReason,
            ErrorMessage = errorMessage,
            HasError = hasError,
            Usage = usage,
            Output = items,
        };
        return true;
    }

    public static bool TryReadChunk(JsonElement root, out AgentChunk chunk)
    {
        chunk = new AgentChunk();
        if (root.ValueKind != JsonValueKind.Object || !TryRequiredString(root, "type", out var type))
        {
            return false;
        }

        if (!TryNullishNumber(root, "sequence_number", out var sequence)
            || !TryNullishNumber(root, "output_index", out var outputIndex)
            || !TryNullishNumber(root, "content_index", out var contentIndex)
            || !TryNullishString(root, "item_id", out var itemId)
            || !TryNullishString(root, "delta", out var delta)
            || !TryNullishString(root, "text", out var text)
            || !TryNullishString(root, "thought", out var thought)
            || !TryNullishStringArray(root, "queries")
            || !TryNullishStringArray(root, "urls"))
        {
            return false;
        }

        AgentResponse? response = null;
        if (root.TryGetProperty("response", out var responseElement) && responseElement.ValueKind != JsonValueKind.Null)
        {
            if (!TryReadResponse(responseElement, out var parsedResponse))
            {
                return false;
            }

            response = parsedResponse;
        }

        AgentItem? item = null;
        if (root.TryGetProperty("item", out var itemElement) && itemElement.ValueKind != JsonValueKind.Null)
        {
            if (!TryReadItem(itemElement, out var parsedItem))
            {
                return false;
            }

            item = parsedItem;
        }

        if (!TryNullableSearchResults(root, "results", out var results) || !TryNullableFetchedContents(root, "contents", out var contents))
        {
            return false;
        }

        string? errorMessage = null;
        if (root.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
        {
            if (error.ValueKind != JsonValueKind.Object
                || !TryRequiredString(error, "message", out var message)
                || !TryNullishString(error, "code", out _)
                || !TryNullishString(error, "type", out _))
            {
                return false;
            }

            errorMessage = message;
        }

        chunk = new AgentChunk
        {
            Type = type,
            SequenceNumber = sequence,
            OutputIndex = outputIndex,
            ContentIndex = contentIndex,
            ItemId = itemId,
            Delta = delta,
            Text = text,
            Thought = thought,
            Response = response,
            Item = item,
            Results = results,
            Contents = contents,
            ErrorMessage = errorMessage,
        };
        return true;
    }

    public static LanguageModelUsage ConvertPerplexityUsage(JsonElement? usage)
    {
        if (usage is not { } element || element.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var input = ReadInt(element, "input_tokens");
        var output = ReadInt(element, "output_tokens");
        int? total = null;
        if (element.TryGetProperty("total_tokens", out var totalElement) && totalElement.ValueKind == JsonValueKind.Number)
        {
            total = ReadIntValue(totalElement);
        }
        var cacheRead = 0;
        var cacheWrite = 0;
        var reasoning = 0;
        if (element.TryGetProperty("input_tokens_details", out var inputDetails) && inputDetails.ValueKind == JsonValueKind.Object)
        {
            if (inputDetails.TryGetProperty("cache_read_input_tokens", out var cacheReadElement) && cacheReadElement.ValueKind == JsonValueKind.Number)
            {
                cacheRead = ReadIntValue(cacheReadElement);
            }
            else if (inputDetails.TryGetProperty("cached_tokens", out var cached) && cached.ValueKind == JsonValueKind.Number)
            {
                cacheRead = ReadIntValue(cached);
            }

            if (inputDetails.TryGetProperty("cache_creation_input_tokens", out var cacheWriteElement) && cacheWriteElement.ValueKind == JsonValueKind.Number)
            {
                cacheWrite = ReadIntValue(cacheWriteElement);
            }
        }

        if (element.TryGetProperty("output_tokens_details", out var outputDetails)
            && outputDetails.ValueKind == JsonValueKind.Object
            && outputDetails.TryGetProperty("reasoning_tokens", out var reasoningElement)
            && reasoningElement.ValueKind == JsonValueKind.Number)
        {
            reasoning = ReadIntValue(reasoningElement);
        }

        return new LanguageModelUsage(input, output, total, cacheRead, cacheWrite, reasoning, element.Clone(), input - cacheRead - cacheWrite, output - reasoning);
    }

    public static JsonElement ProviderMetadata(JsonElement? usage)
    {
        var perplexity = new JsonObject
        {
            ["usage"] = new JsonObject
            {
                ["citationTokens"] = JsonNull(),
                ["numSearchQueries"] = SearchQueryCount(usage),
            },
            ["images"] = JsonNull(),
            ["cost"] = CostNode(usage),
            ["toolCalls"] = ToolCallsNode(usage),
        };
        return ToElement(new JsonObject { ["perplexity"] = perplexity });
    }

    public static GeneratedSource CreateSource(AgentSearchResult result, Func<string> generateId)
    {
        var id = result.Id is { } number
            ? number.ToString(CultureInfo.InvariantCulture)
            : generateId();
        return new GeneratedSource(id, result.Url, result.Title, SearchMetadata(result));
    }

    public static GeneratedSource CreateFetchSource(AgentFetched result, Func<string> generateId)
    {
        var perplexity = new JsonObject
        {
            ["snippet"] = result.Snippet is null ? JsonNull() : JsonValue.Create(result.Snippet),
        };
        return new GeneratedSource(generateId(), result.Url, result.Title, ToElement(new JsonObject { ["perplexity"] = perplexity }));
    }

    public static GeneratedSource CreateAnnotationSource(string url, string? title, Func<string> generateId)
    {
        return new GeneratedSource(generateId(), url, title);
    }

    public static bool HasSearchResultId(GeneratedSource? source)
    {
        if (source?.ProviderMetadata is not { } metadata
            || metadata.ValueKind != JsonValueKind.Object
            || !metadata.TryGetProperty("perplexity", out var perplexity)
            || perplexity.ValueKind != JsonValueKind.Object
            || !perplexity.TryGetProperty("resultId", out var resultId))
        {
            return false;
        }

        return resultId.ValueKind == JsonValueKind.Number;
    }

    public static JsonElement ToolMetadata(string? itemId, string? thoughtSignature)
    {
        var perplexity = new JsonObject
        {
            ["itemId"] = itemId is null ? JsonNull() : JsonValue.Create(itemId),
        };
        if (!string.IsNullOrEmpty(thoughtSignature))
        {
            perplexity["thoughtSignature"] = thoughtSignature;
        }

        return ToElement(new JsonObject { ["perplexity"] = perplexity });
    }

    public static string? ThoughtSignature(JsonElement? metadata)
    {
        if (metadata is not { } element
            || element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty("perplexity", out var perplexity)
            || perplexity.ValueKind != JsonValueKind.Object
            || !perplexity.TryGetProperty("thoughtSignature", out var signature)
            || signature.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return signature.GetString();
    }

    public static FinishReason MapPerplexityFinishReason(string? status, string? incompleteReason, bool hasFunctionCall, out string? raw)
    {
        raw = incompleteReason ?? status;
        if (string.Equals(incompleteReason, "max_output_tokens", StringComparison.Ordinal))
        {
            return FinishReason.Length;
        }

        if (string.Equals(incompleteReason, "content_filter", StringComparison.Ordinal))
        {
            return FinishReason.ContentFilter;
        }

        switch (status)
        {
            case "completed":
                return hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Stop;
            case "requires_action":
                return FinishReason.ToolCalls;
            case "failed":
                return FinishReason.Error;
            default:
                return hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Other;
        }
    }

    public static string TextId(string? itemId, int? outputIndex, int? contentIndex)
    {
        var id = itemId ?? (outputIndex.HasValue ? outputIndex.Value.ToString(CultureInfo.InvariantCulture) : "text");
        if (!contentIndex.HasValue || contentIndex.Value == 0)
        {
            return id;
        }

        return id + ":" + contentIndex.Value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool StartsWithOrdinal(string value, string prefix)
    {
        return value.Length >= prefix.Length && string.CompareOrdinal(value, 0, prefix, 0, prefix.Length) == 0;
    }

    private static JsonElement SearchMetadata(AgentSearchResult result)
    {
        var perplexity = new JsonObject
        {
            ["resultId"] = result.IdElement ?? JsonNull(),
            ["snippet"] = result.Snippet is null ? JsonNull() : JsonValue.Create(result.Snippet),
            ["date"] = result.Date is null ? JsonNull() : JsonValue.Create(result.Date),
            ["lastUpdated"] = result.LastUpdated is null ? JsonNull() : JsonValue.Create(result.LastUpdated),
            ["source"] = result.Source is null ? JsonNull() : JsonValue.Create(result.Source),
        };
        return ToElement(new JsonObject { ["perplexity"] = perplexity });
    }

    private static JsonNode SearchQueryCount(JsonElement? usage)
    {
        if (usage is not { } element
            || element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty("tool_calls_details", out var details)
            || details.ValueKind != JsonValueKind.Object)
        {
            return JsonNull();
        }

        var total = 0;
        foreach (var tool in details.EnumerateObject())
        {
            if (tool.Name.IndexOf("search", StringComparison.Ordinal) < 0
                || tool.Value.ValueKind != JsonValueKind.Object
                || !tool.Value.TryGetProperty("invocation", out var invocation)
                || invocation.ValueKind != JsonValueKind.Number)
            {
                continue;
            }

            total += ReadIntValue(invocation);
        }

        return JsonValue.Create(total);
    }

    private static JsonNode CostNode(JsonElement? usage)
    {
        if (usage is not { } element
            || element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty("cost", out var cost)
            || cost.ValueKind != JsonValueKind.Object)
        {
            return JsonNull();
        }

        return new JsonObject
        {
            ["inputTokensCost"] = CloneOrNull(cost, "input_cost"),
            ["outputTokensCost"] = CloneOrNull(cost, "output_cost"),
            ["requestCost"] = JsonNull(),
            ["totalCost"] = CloneOrNull(cost, "total_cost"),
            ["currency"] = CloneOrNull(cost, "currency"),
            ["cacheCreationCost"] = CloneOrNull(cost, "cache_creation_cost"),
            ["cacheReadCost"] = CloneOrNull(cost, "cache_read_cost"),
            ["toolCallsCost"] = CloneOrNull(cost, "tool_calls_cost"),
        };
    }

    private static JsonNode ToolCallsNode(JsonElement? usage)
    {
        if (usage is not { } element
            || element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty("tool_calls_details", out var details)
            || details.ValueKind != JsonValueKind.Object)
        {
            return JsonNull();
        }

        var calls = new JsonObject();
        foreach (var tool in details.EnumerateObject())
        {
            JsonNode invocation = JsonNull();
            if (tool.Value.ValueKind == JsonValueKind.Object
                && tool.Value.TryGetProperty("invocation", out var value)
                && value.ValueKind == JsonValueKind.Number)
            {
                invocation = JsonNode.Parse(value.GetRawText())!;
            }

            calls[tool.Name] = new JsonObject { ["invocation"] = invocation };
        }

        return calls;
    }

    private static bool TryReadItem(JsonElement element, out AgentItem? item)
    {
        item = null;
        if (element.ValueKind != JsonValueKind.Object || !TryRequiredString(element, "type", out var type))
        {
            return false;
        }

        if (!Contains(HandledTypes, type))
        {
            item = new AgentItem { Type = "unhandled" };
            return true;
        }

        switch (type)
        {
            case "message":
                return TryReadMessage(element, out item);
            case "search_results":
                return TryReadSearchItem(element, out item);
            case "fetch_url_results":
                return TryReadFetchItem(element, out item);
            default:
                return TryReadFunctionCall(element, out item);
        }
    }

    private static bool TryReadMessage(JsonElement element, out AgentItem? item)
    {
        item = null;
        if (!TryNullishString(element, "id", out var id) || !TryNullishContent(element, out var content))
        {
            return false;
        }

        item = new AgentItem { Type = "message", Id = id, Content = content };
        return true;
    }

    private static bool TryReadSearchItem(JsonElement element, out AgentItem? item)
    {
        item = null;
        if (!TryNullishSearchResults(element, "results", out var results))
        {
            return false;
        }

        item = new AgentItem { Type = "search_results", Results = results };
        return true;
    }

    private static bool TryReadFetchItem(JsonElement element, out AgentItem? item)
    {
        item = null;
        if (!TryNullishFetched(element, "contents", out var contents))
        {
            return false;
        }

        item = new AgentItem { Type = "fetch_url_results", Contents = contents };
        return true;
    }

    private static bool TryReadFunctionCall(JsonElement element, out AgentItem? item)
    {
        item = null;
        if (!TryNullishString(element, "id", out var id)
            || !TryNullishString(element, "call_id", out var callId)
            || !TryNullishString(element, "name", out var name)
            || !TryNullishString(element, "arguments", out var arguments)
            || !TryNullishString(element, "thought_signature", out var signature))
        {
            return false;
        }

        item = new AgentItem
        {
            Type = "function_call",
            Id = id,
            CallId = callId,
            Name = name,
            Arguments = arguments,
            ThoughtSignature = signature,
        };
        return true;
    }

    private static bool TryNullishContent(JsonElement element, out List<AgentContentPart> content)
    {
        content = new List<AgentContentPart>();
        if (!element.TryGetProperty("content", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var part in value.EnumerateArray())
        {
            if (part.ValueKind != JsonValueKind.Object || !TryRequiredString(part, "type", out var type) || !TryOptionalString(part, "text", out var text))
            {
                return false;
            }

            if (!TryNullishAnnotations(part, out var annotations))
            {
                return false;
            }

            content.Add(new AgentContentPart { Type = type, Text = text, Annotations = annotations });
        }

        return true;
    }

    private static bool TryNullishAnnotations(JsonElement element, out List<AgentAnnotation> annotations)
    {
        annotations = new List<AgentAnnotation>();
        if (!element.TryGetProperty("annotations", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var annotation in value.EnumerateArray())
        {
            if (annotation.ValueKind != JsonValueKind.Object || !TryOptionalString(annotation, "url", out var url) || !TryOptionalString(annotation, "title", out var title))
            {
                return false;
            }

            annotations.Add(new AgentAnnotation { Url = url, Title = title });
        }

        return true;
    }

    private static bool TryNullishSearchResults(JsonElement element, string name, out List<AgentSearchResult> results)
    {
        results = new List<AgentSearchResult>();
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        return TrySearchArray(value, results);
    }

    private static bool TryNullableSearchResults(JsonElement element, string name, out List<AgentSearchResult>? results)
    {
        results = null;
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        var parsed = new List<AgentSearchResult>();
        if (!TrySearchArray(value, parsed))
        {
            return false;
        }

        results = parsed;
        return true;
    }

    private static bool TrySearchArray(JsonElement value, List<AgentSearchResult> results)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var result in value.EnumerateArray())
        {
            if (!TrySearchResult(result, out var parsed))
            {
                return false;
            }

            results.Add(parsed!);
        }

        return true;
    }

    private static bool TrySearchResult(JsonElement element, out AgentSearchResult? result)
    {
        result = null;
        if (element.ValueKind != JsonValueKind.Object
            || !TryRequiredString(element, "title", out var title)
            || !TryRequiredString(element, "url", out var url)
            || !TryOptionalString(element, "snippet", out var snippet)
            || !TryOptionalString(element, "source", out var source)
            || !TryNullishString(element, "date", out var date)
            || !TryNullishString(element, "last_updated", out var lastUpdated)
            || !TryOptionalNumber(element, "id", out var id))
        {
            return false;
        }

        JsonNode? idElement = null;
        if (element.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.Number)
        {
            idElement = JsonNode.Parse(idValue.GetRawText());
        }

        result = new AgentSearchResult
        {
            Id = id.HasValue ? (long)id.Value : null,
            IdElement = idElement,
            Title = title,
            Url = url,
            Snippet = snippet,
            Date = date,
            LastUpdated = lastUpdated,
            Source = source,
        };
        return true;
    }

    private static bool TryNullishFetched(JsonElement element, string name, out List<AgentFetched> contents)
    {
        contents = new List<AgentFetched>();
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        return TryFetchedArray(value, contents);
    }

    private static bool TryNullableFetchedContents(JsonElement element, string name, out List<AgentFetched>? contents)
    {
        contents = null;
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        var parsed = new List<AgentFetched>();
        if (!TryFetchedArray(value, parsed))
        {
            return false;
        }

        contents = parsed;
        return true;
    }

    private static bool TryFetchedArray(JsonElement value, List<AgentFetched> contents)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object
                || !TryRequiredString(item, "title", out var title)
                || !TryRequiredString(item, "url", out var url)
                || !TryOptionalString(item, "snippet", out var snippet))
            {
                return false;
            }

            contents.Add(new AgentFetched { Title = title, Url = url, Snippet = snippet });
        }

        return true;
    }

    private static bool TryNullishUsage(JsonElement root, out JsonElement? usage)
    {
        usage = null;
        if (!root.TryGetProperty("usage", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (value.ValueKind != JsonValueKind.Object
            || !IsNumber(value, "input_tokens")
            || !IsNumber(value, "output_tokens")
            || !IsNumber(value, "total_tokens")
            || !TryNullishObject(value, "input_tokens_details", out _, out var inputOk)
            || !inputOk
            || !TryNullishObject(value, "output_tokens_details", out _, out var outputOk)
            || !outputOk
            || !TryNullishObject(value, "cost", out _, out var costOk)
            || !costOk
            || !TryNullishObject(value, "tool_calls_details", out _, out var toolsOk)
            || !toolsOk)
        {
            return false;
        }

        usage = value.Clone();
        return true;
    }

    private static bool TryNullishObject(JsonElement element, string name, out JsonElement? value, out bool ok)
    {
        value = null;
        ok = true;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.Object)
        {
            ok = false;
            return false;
        }

        value = property;
        return true;
    }

    private static bool TryRequiredString(JsonElement element, string name, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return true;
    }

    private static bool TryOptionalString(JsonElement element, string name, out string? value)
    {
        value = null;
        if (!element.TryGetProperty(name, out var property))
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return true;
    }

    private static bool TryNullishString(JsonElement element, string name, out string? value)
    {
        value = null;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return true;
    }

    private static bool TryOptionalNumber(JsonElement element, string name, out double? value)
    {
        value = null;
        if (!element.TryGetProperty(name, out var property))
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        value = property.GetDouble();
        return true;
    }

    private static bool TryNullishNumber(JsonElement element, string name, out double? value)
    {
        value = null;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        value = property.GetDouble();
        return true;
    }

    private static bool TryNullishStringArray(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryUnix(JsonElement element, string name, out long seconds)
    {
        seconds = 0;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        if (property.TryGetInt64(out seconds))
        {
            return true;
        }

        seconds = (long)property.GetDouble();
        return true;
    }

    private static bool IsNumber(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number;
    }

    private static int ReadInt(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var property) ? ReadIntValue(property) : 0;
    }

    private static int ReadIntValue(JsonElement value)
    {
        if (value.TryGetInt32(out var number))
        {
            return number;
        }

        var wide = value.GetDouble();
        if (wide > int.MaxValue)
        {
            return int.MaxValue;
        }

        if (wide < int.MinValue)
        {
            return int.MinValue;
        }

        return (int)wide;
    }

    private static void ValidateReasoning(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: reasoning must be an object.");
        }

        if (!value.TryGetProperty("effort", out var effort))
        {
            return;
        }

        if (effort.ValueKind != JsonValueKind.String || !Contains(Efforts, effort.GetString()))
        {
            throw new AiSdkException("Invalid perplexity provider option: reasoning.effort is not supported.");
        }
    }

    private static void ValidateNativeTools(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid perplexity provider option: tools must be an array.");
        }

        foreach (var tool in value.EnumerateArray())
        {
            if (tool.ValueKind != JsonValueKind.Object || !TryRequiredString(tool, "type", out var type))
            {
                throw new AiSdkException("Invalid perplexity provider option: each native tool needs a type.");
            }

            switch (type)
            {
                case "web_search":
                    ValidateWebSearch(tool);
                    break;
                case "fetch_url":
                    RequireOptionalNumber(tool, "max_urls");
                    break;
                case "people_search":
                case "finance_search":
                case "sandbox":
                    break;
                case "mcp":
                    RequirePresentString(tool, "server_label");
                    RequirePresentString(tool, "server_url");
                    break;
                case "connector":
                    RequirePresentString(tool, "id");
                    RequirePresentString(tool, "server_label");
                    break;
                default:
                    throw new AiSdkException("Invalid perplexity provider option: unsupported native tool '" + type + "'.");
            }
        }
    }

    private static void ValidateWebSearch(JsonElement tool)
    {
        RequireOptionalNumber(tool, "max_tokens");
        RequireOptionalNumber(tool, "max_tokens_per_page");
        if (tool.TryGetProperty("max_results", out var maxResults))
        {
            if (maxResults.ValueKind != JsonValueKind.Number || !maxResults.TryGetInt32(out var count) || count <= 0)
            {
                throw new AiSdkException("Invalid perplexity provider option: max_results must be a positive integer.");
            }
        }

        if (tool.TryGetProperty("search_context_size", out var size)
            && (size.ValueKind != JsonValueKind.String || !Contains(ContextSizes, size.GetString())))
        {
            throw new AiSdkException("Invalid perplexity provider option: search_context_size is not supported.");
        }

        if (!tool.TryGetProperty("filters", out var filters))
        {
            return;
        }

        if (filters.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: web_search filters must be an object.");
        }

        if (filters.TryGetProperty("search_recency_filter", out var recency)
            && (recency.ValueKind != JsonValueKind.String || !Contains(Recency, recency.GetString())))
        {
            throw new AiSdkException("Invalid perplexity provider option: search_recency_filter is not supported.");
        }
    }

    private static void ValidateSkills(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid perplexity provider option: skills must be an array.");
        }

        foreach (var skill in value.EnumerateArray())
        {
            if (skill.ValueKind != JsonValueKind.Object || !TryRequiredString(skill, "type", out var type))
            {
                throw new AiSdkException("Invalid perplexity provider option: each skill needs a type.");
            }

            if (string.Equals(type, "builtin", StringComparison.Ordinal))
            {
                if (!skill.TryGetProperty("name", out var name)
                    || name.ValueKind != JsonValueKind.String
                    || !Contains(BuiltinSkills, name.GetString()))
                {
                    throw new AiSdkException("Invalid perplexity provider option: builtin skill name is not supported.");
                }
            }
            else if (string.Equals(type, "inline", StringComparison.Ordinal))
            {
                RequirePresentString(skill, "name");
                RequirePresentString(skill, "description");
                RequirePresentString(skill, "instructions");
            }
            else
            {
                throw new AiSdkException("Invalid perplexity provider option: unsupported skill type '" + type + "'.");
            }
        }
    }

    private static void RequireString(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.String)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + property.Name + " must be a string.");
        }
    }

    private static void RequireBool(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.True && property.Value.ValueKind != JsonValueKind.False)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + property.Name + " must be a boolean.");
        }
    }

    private static void RequireStringArray(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + property.Name + " must be an array of strings.");
        }

        foreach (var item in property.Value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("Invalid perplexity provider option: " + property.Name + " must be an array of strings.");
            }
        }
    }

    private static void RequireInteger(JsonProperty property, int minimum, string message)
    {
        if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out var number) || number < minimum)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + message);
        }
    }

    private static void RequirePresentString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(value.GetString()))
        {
            throw new AiSdkException("Invalid perplexity provider option: " + name + " is required.");
        }
    }

    private static void RequireOptionalNumber(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Number)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + name + " must be a number.");
        }
    }

    private static bool Contains(string[] values, string? candidate)
    {
        if (candidate is null)
        {
            return false;
        }

        for (var index = 0; index < values.Length; index++)
        {
            if (string.Equals(values[index], candidate, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static JsonNode CloneOrNull(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return JsonNull();
        }

        return JsonNode.Parse(value.GetRawText())!;
    }

    private static JsonNode JsonNull()
    {
        return JsonNode.Parse("null")!;
    }

    private static JsonElement ToElement(JsonNode node)
    {
        return JsonDocument.Parse(node.ToJsonString()).RootElement.Clone();
    }
}

internal readonly struct PerplexityModelSelection
{
    public PerplexityModelSelection(string? preset, string? model)
    {
        Preset = preset;
        Model = model;
    }

    public string? Preset { get; }

    public string? Model { get; }
}

internal sealed class AgentResponse
{
    public string Id { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public long CreatedAt { get; set; }

    public string? IncompleteReason { get; set; }

    public string? ErrorMessage { get; set; }

    public bool HasError { get; set; }

    public JsonElement? Usage { get; set; }

    public List<AgentItem> Output { get; set; } = new();
}

internal sealed class AgentChunk
{
    public string Type { get; set; } = string.Empty;

    public double? SequenceNumber { get; set; }

    public double? OutputIndex { get; set; }

    public double? ContentIndex { get; set; }

    public string? ItemId { get; set; }

    public string? Delta { get; set; }

    public string? Text { get; set; }

    public string? Thought { get; set; }

    public AgentResponse? Response { get; set; }

    public AgentItem? Item { get; set; }

    public List<AgentSearchResult>? Results { get; set; }

    public List<AgentFetched>? Contents { get; set; }

    public string? ErrorMessage { get; set; }
}

internal sealed class AgentItem
{
    public string Type { get; set; } = string.Empty;

    public string? Id { get; set; }

    public List<AgentContentPart> Content { get; set; } = new();

    public List<AgentSearchResult> Results { get; set; } = new();

    public List<AgentFetched> Contents { get; set; } = new();

    public string? CallId { get; set; }

    public string? Name { get; set; }

    public string? Arguments { get; set; }

    public string? ThoughtSignature { get; set; }
}

internal sealed class AgentContentPart
{
    public string Type { get; set; } = string.Empty;

    public string? Text { get; set; }

    public List<AgentAnnotation> Annotations { get; set; } = new();
}

internal sealed class AgentAnnotation
{
    public string? Url { get; set; }

    public string? Title { get; set; }
}

internal sealed class AgentSearchResult
{
    public long? Id { get; set; }

    public JsonNode? IdElement { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Snippet { get; set; }

    public string? Date { get; set; }

    public string? LastUpdated { get; set; }

    public string? Source { get; set; }
}

internal sealed class AgentFetched
{
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Snippet { get; set; }
}
