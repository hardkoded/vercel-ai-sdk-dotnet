// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>A parsed Converse response.</summary>
public sealed class AmazonBedrockParsedResponse
{
    /// <summary>Creates a parsed response.</summary>
    public AmazonBedrockParsedResponse(
        IReadOnlyList<GeneratedContent> content,
        FinishReason finishReason,
        string? rawFinishReason,
        LanguageModelUsage usage,
        JsonElement? providerMetadata,
        string? responseId,
        DateTimeOffset? timestamp)
    {
        Content = content ?? Array.Empty<GeneratedContent>();
        FinishReason = finishReason;
        RawFinishReason = rawFinishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        ProviderMetadata = providerMetadata;
        ResponseId = responseId;
        Timestamp = timestamp;
    }

    /// <summary>Generated content.</summary>
    public IReadOnlyList<GeneratedContent> Content { get; }

    /// <summary>Normalized finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Provider stop reason.</summary>
    public string? RawFinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider metadata for <c>amazonBedrock</c> and <c>bedrock</c>.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response id, usually <c>x-amzn-requestid</c>.</summary>
    public string? ResponseId { get; }

    /// <summary>Response timestamp from the <c>date</c> header.</summary>
    public DateTimeOffset? Timestamp { get; }
}

/// <summary>Parses a Converse JSON response.</summary>
public static class AmazonBedrockResponseParser
{
    /// <summary>Parses <paramref name="root"/>.</summary>
    public static AmazonBedrockParsedResponse Parse(JsonElement root, string modelId, bool usesJsonResponseTool, Func<string>? generateId, IReadOnlyDictionary<string, string>? headers)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new AmazonBedrockResponseException("Invalid JSON response: response must be an object.");
        }

        var content = new List<GeneratedContent>();
        var reasoningMetadata = new JsonArray();
        var isJsonResponseFromTool = false;
        var isMistral = AmazonBedrockToolCallId.IsMistralModel(modelId);
        if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Object && output.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object && message.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                if (part.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (var text in ReadText(part))
                {
                    content.Add(new GeneratedText(text));
                }

                if (part.TryGetProperty("reasoningContent", out var reasoning) && reasoning.ValueKind == JsonValueKind.Object)
                {
                    AppendReasoning(content, reasoningMetadata, reasoning);
                }

                if (part.TryGetProperty("toolUse", out var toolUse) && toolUse.ValueKind == JsonValueKind.Object)
                {
                    var name = toolUse.TryGetProperty("name", out var toolName) && toolName.ValueKind == JsonValueKind.String
                        ? toolName.GetString() ?? string.Empty
                        : "tool-" + NextId(generateId);
                    if (usesJsonResponseTool && name == "json")
                    {
                        isJsonResponseFromTool = true;
                        var input = toolUse.TryGetProperty("input", out var jsonInput) ? jsonInput : default;
                        content.Add(new GeneratedText(input.ValueKind == JsonValueKind.Undefined ? "{}" : input.GetRawText()));
                    }
                    else
                    {
                        var rawId = toolUse.TryGetProperty("toolUseId", out var idElement) && idElement.ValueKind == JsonValueKind.String
                            ? idElement.GetString() ?? string.Empty
                            : string.Empty;
                        if (rawId.Length == 0)
                        {
                            rawId = NextId(generateId);
                        }

                        var arguments = toolUse.TryGetProperty("input", out var inputElement) && inputElement.ValueKind != JsonValueKind.Undefined
                            ? inputElement.GetRawText()
                            : "{}";
                        content.Add(new GeneratedToolCall(AmazonBedrockToolCallId.Normalize(rawId, isMistral), name, arguments));
                    }
                }
            }
        }

        var rawFinish = root.TryGetProperty("stopReason", out var stop) && stop.ValueKind == JsonValueKind.String ? stop.GetString() : null;
        JsonElement? usageElement = root.TryGetProperty("usage", out var usageProperty) ? usageProperty : null;
        var usage = AmazonBedrockUsage.Convert(usageElement);
        string? stopSequence = null;
        var hasStopSequence = false;
        if (root.TryGetProperty("additionalModelResponseFields", out var extra) && extra.ValueKind == JsonValueKind.Object && extra.TryGetProperty("delta", out var delta) && delta.ValueKind == JsonValueKind.Object && delta.TryGetProperty("stop_sequence", out var sequence))
        {
            hasStopSequence = true;
            stopSequence = sequence.ValueKind == JsonValueKind.String ? sequence.GetString() : null;
        }

        var metadata = BuildMetadata(root, usageElement, isJsonResponseFromTool, hasStopSequence, stopSequence, reasoningMetadata);
        string? responseId = null;
        DateTimeOffset? timestamp = null;
        if (headers != null)
        {
            foreach (var header in headers)
            {
                if (string.Equals(header.Key, "x-amzn-requestid", StringComparison.OrdinalIgnoreCase))
                {
                    responseId = header.Value;
                }
                else if (string.Equals(header.Key, "date", StringComparison.OrdinalIgnoreCase) && DateTimeOffset.TryParse(header.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                {
                    timestamp = parsed;
                }
            }
        }

        return new AmazonBedrockParsedResponse(content, AmazonBedrockFinishReason.Map(rawFinish, isJsonResponseFromTool), rawFinish, usage.ToLanguageModelUsage(), metadata, responseId, timestamp);
    }

    private static IEnumerable<string> ReadText(JsonElement part)
    {
        if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            yield return text.GetString() ?? string.Empty;
            yield break;
        }

        if (!part.TryGetProperty("citationsContent", out var citations) || citations.ValueKind != JsonValueKind.Object || !citations.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("text", out var cited) && cited.ValueKind == JsonValueKind.String)
            {
                yield return cited.GetString() ?? string.Empty;
            }
        }
    }

    private static void AppendReasoning(List<GeneratedContent> content, JsonArray metadata, JsonElement reasoning)
    {
        if (reasoning.TryGetProperty("reasoningText", out var reasoningText) && reasoningText.ValueKind == JsonValueKind.Object)
        {
            var text = reasoningText.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String
                ? textElement.GetString() ?? string.Empty
                : string.Empty;
            content.Add(new GeneratedReasoning(text));
            if (reasoningText.TryGetProperty("signature", out var signature) && signature.ValueKind == JsonValueKind.String)
            {
                metadata.Add(new JsonObject { ["signature"] = signature.GetString() });
            }
            else
            {
                metadata.Add(new JsonObject());
            }

            return;
        }

        if (reasoning.TryGetProperty("redactedReasoning", out var redacted) && redacted.ValueKind == JsonValueKind.Object)
        {
            var data = redacted.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.String
                ? dataElement.GetString() ?? string.Empty
                : string.Empty;
            content.Add(new GeneratedReasoning(string.Empty));
            metadata.Add(new JsonObject { ["redactedData"] = data });
            return;
        }

        if (reasoning.TryGetProperty("redactedContent", out var redactedContent) && redactedContent.ValueKind == JsonValueKind.String)
        {
            content.Add(new GeneratedReasoning(string.Empty));
            metadata.Add(new JsonObject { ["redactedContent"] = redactedContent.GetString() });
        }
    }

    private static JsonElement? BuildMetadata(JsonElement root, JsonElement? usage, bool isJsonResponseFromTool, bool hasStopSequence, string? stopSequence, JsonArray reasoning)
    {
        var hasTrace = root.TryGetProperty("trace", out var trace) && trace.ValueKind == JsonValueKind.Object;
        var hasPerformance = root.TryGetProperty("performanceConfig", out var performance) && performance.ValueKind != JsonValueKind.Null && performance.ValueKind != JsonValueKind.Undefined;
        var hasServiceTier = root.TryGetProperty("serviceTier", out var serviceTier) && serviceTier.ValueKind != JsonValueKind.Null && serviceTier.ValueKind != JsonValueKind.Undefined;
        var hasUsage = usage != null && usage.Value.ValueKind == JsonValueKind.Object;
        if (!hasTrace && !hasPerformance && !hasServiceTier && !hasUsage && !isJsonResponseFromTool && !hasStopSequence && reasoning.Count == 0)
        {
            return null;
        }

        var payload = new JsonObject();
        if (hasTrace)
        {
            payload["trace"] = JsonNode.Parse(trace.GetRawText());
        }

        if (hasPerformance)
        {
            payload["performanceConfig"] = JsonNode.Parse(performance.GetRawText());
        }

        if (hasServiceTier)
        {
            payload["serviceTier"] = JsonNode.Parse(serviceTier.GetRawText());
        }

        if (hasUsage && usage!.Value.ValueKind == JsonValueKind.Object)
        {
            var usageNode = new JsonObject();
            var includeUsage = false;
            if (usage.Value.TryGetProperty("cacheWriteInputTokens", out var cacheWrite) && cacheWrite.ValueKind != JsonValueKind.Null && cacheWrite.ValueKind != JsonValueKind.Undefined)
            {
                usageNode["cacheWriteInputTokens"] = JsonNode.Parse(cacheWrite.GetRawText());
                includeUsage = true;
            }

            if (usage.Value.TryGetProperty("cacheDetails", out var cacheDetails) && cacheDetails.ValueKind != JsonValueKind.Null && cacheDetails.ValueKind != JsonValueKind.Undefined)
            {
                usageNode["cacheDetails"] = JsonNode.Parse(cacheDetails.GetRawText());
                includeUsage = true;
            }

            if (includeUsage)
            {
                payload["usage"] = usageNode;
            }
        }

        if (isJsonResponseFromTool)
        {
            payload["isJsonResponseFromTool"] = true;
        }

        if (hasUsage || hasTrace || hasPerformance || hasServiceTier || isJsonResponseFromTool || hasStopSequence)
        {
            payload["stopSequence"] = hasStopSequence && stopSequence != null ? JsonValue.Create(stopSequence) : JsonNode.Parse("null");
        }

        if (reasoning.Count > 0)
        {
            payload["reasoning"] = reasoning;
        }

        var metadata = new JsonObject
        {
            ["amazonBedrock"] = payload.DeepClone(),
            ["bedrock"] = payload,
        };
        using var document = JsonDocument.Parse(metadata.ToJsonString());
        return document.RootElement.Clone();
    }

    private static string NextId(Func<string>? generateId)
    {
        var id = generateId?.Invoke();
        return string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString("N") : id!;
    }
}
