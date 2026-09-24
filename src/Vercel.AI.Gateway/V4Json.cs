// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Gateway;

/// <summary>Serializes call options and parses results in the Language Model V4 JSON shape the Gateway expects.</summary>
public static class V4Json
{
    /// <summary>Serializes <paramref name="options"/> as a V4 call body.</summary>
    public static string CallOptions(LanguageModelCallOptions options)
    {
        var prompt = new JsonArray();
        foreach (var message in options.Prompt)
        {
            prompt.Add(Message(message));
        }

        var body = new JsonObject { ["prompt"] = prompt };
        if (options.MaxOutputTokens is { } max)
        {
            body["maxOutputTokens"] = max;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["topP"] = topP;
        }

        if (options.TopK is { } topK)
        {
            body["topK"] = topK;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presencePenalty"] = presence;
        }

        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequencyPenalty"] = frequency;
        }

        if (options.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stopSequences"] = stop;
        }

        if (options.JsonSchema is { } schema)
        {
            body["responseFormat"] = new JsonObject
            {
                ["type"] = "json",
                ["name"] = options.JsonSchemaName,
                ["schema"] = PrepareSchema(JsonNode.Parse(schema.GetRawText())),
            };
        }
        else
        {
            body["responseFormat"] = new JsonObject { ["type"] = "text" };
        }

        if (options.Tools is { Count: > 0 })
        {
            var tools = new JsonArray();
            foreach (var tool in options.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["inputSchema"] = PrepareSchema(JsonNode.Parse(tool.InputSchema.GetRawText())),
                });
            }

            body["tools"] = tools;
        }

        if (options.ToolChoice != null)
        {
            var choice = new JsonObject { ["type"] = options.ToolChoice.Type };
            if (options.ToolChoice is ToolChoice.NamedChoice named)
            {
                choice["toolName"] = named.ToolName;
            }

            body["toolChoice"] = choice;
        }

        return body.ToJsonString();
    }

    /// <summary>Parses a V4 generate result.</summary>
    public static LanguageModelGenerateResult ParseGenerate(JsonElement root)
    {
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("content", out var parts))
        {
            foreach (var part in parts.EnumerateArray())
            {
                var parsed = Content(part);
                if (parsed != null)
                {
                    content.Add(parsed);
                }
            }
        }

        var raw = root.TryGetProperty("finishReason", out var finish) ? ReadFinishReason(finish) : null;
        var usage = ReadUsage(root);
        var id = root.TryGetProperty("response", out var response) && response.TryGetProperty("id", out var responseId)
            ? responseId.GetString()
            : null;
        return new LanguageModelGenerateResult(content, FinishReasons.Parse(raw), usage, raw, responseId: id);
    }

    /// <summary>Parses one SSE JSON object into a stream part.</summary>
    public static LanguageModelStreamPart? ParseStreamPart(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
        switch (type)
        {
            case "text-delta":
                return new TextDeltaStreamPart(
                    root.TryGetProperty("id", out var id) ? id.GetString() ?? "text" : "text",
                    ReadDelta(root));
            case "reasoning-delta":
                return new ReasoningDeltaStreamPart(
                    root.TryGetProperty("id", out var reasoningId) ? reasoningId.GetString() ?? "reasoning" : "reasoning",
                    ReadDelta(root));
            case "tool-call":
                return new ToolCallStreamPart(
                    root.GetProperty("toolCallId").GetString() ?? "call",
                    root.GetProperty("toolName").GetString() ?? string.Empty,
                    ReadToolInput(root));
            case "source":
            case "source-url":
                return new SourceStreamPart(
                    root.TryGetProperty("id", out var sourceId) ? sourceId.GetString() ?? string.Empty : string.Empty,
                    root.TryGetProperty("url", out var url) ? url.GetString() ?? string.Empty : string.Empty,
                    root.TryGetProperty("title", out var title) ? title.GetString() : null);
            case "finish":
            case "finish-step":
                var raw = root.TryGetProperty("finishReason", out var finish) ? ReadFinishReason(finish) ?? "stop" : "stop";
                return new FinishStreamPart(FinishReasons.Parse(raw), ReadUsage(root), raw);
            case "error":
                var message = root.TryGetProperty("error", out var error) ? error.ToString() : "Provider stream error.";
                return new ErrorStreamPart(message ?? "Provider stream error.");
            default:
                return null;
        }
    }

    private static JsonObject Message(ModelMessage message)
    {
        switch (message)
        {
            case SystemModelMessage system:
                return new JsonObject { ["role"] = "system", ["content"] = system.Content };
            case UserModelMessage user:
                var userParts = new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        userParts.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    }
                    else if (part is FileContentPart file)
                    {
                        userParts.Add(new JsonObject
                        {
                            ["type"] = "file",
                            ["mediaType"] = file.MediaType,
                            ["data"] = file.Url ?? (file.Data is null ? null : Convert.ToBase64String(file.Data)),
                        });
                    }
                }

                return new JsonObject { ["role"] = "user", ["content"] = userParts };
            case AssistantModelMessage assistant:
                var assistantParts = new JsonArray();
                if (!string.IsNullOrEmpty(assistant.Reasoning))
                {
                    assistantParts.Add(new JsonObject { ["type"] = "reasoning", ["text"] = assistant.Reasoning });
                }

                if (!string.IsNullOrEmpty(assistant.Text))
                {
                    assistantParts.Add(new JsonObject { ["type"] = "text", ["text"] = assistant.Text });
                }

                foreach (var call in assistant.ToolCalls)
                {
                    assistantParts.Add(new JsonObject
                    {
                        ["type"] = "tool-call",
                        ["toolCallType"] = "function",
                        ["toolCallId"] = call.ToolCallId,
                        ["toolName"] = call.ToolName,
                        ["input"] = JsonValue(call.ArgumentsJson),
                    });
                }

                return new JsonObject { ["role"] = "assistant", ["content"] = assistantParts };
            case ToolModelMessage tool:
                return new JsonObject
                {
                    ["role"] = "tool",
                    ["content"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "tool-result",
                            ["toolCallId"] = tool.ToolCallId,
                            ["toolName"] = tool.ToolName,
                            ["output"] = ToolOutput(tool.OutputJson, tool.IsError),
                        },
                    },
                };
            default:
                throw new AiSdkException("Unsupported message role '" + message.Role + "'.");
        }
    }

    private static GeneratedContent? Content(JsonElement part)
    {
        var type = part.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
        switch (type)
        {
            case "text":
                return new GeneratedText(part.GetProperty("text").GetString() ?? string.Empty);
            case "reasoning":
                return new GeneratedReasoning(part.GetProperty("text").GetString() ?? string.Empty);
            case "tool-call":
                var args = ReadToolInput(part);
                return new GeneratedToolCall(
                    part.GetProperty("toolCallId").GetString() ?? "call",
                    part.GetProperty("toolName").GetString() ?? string.Empty,
                    args);
            case "source":
                return new GeneratedSource(
                    part.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                    part.TryGetProperty("url", out var url) ? url.GetString() ?? string.Empty : string.Empty,
                    part.TryGetProperty("title", out var title) ? title.GetString() : null);
            case "file":
                var mediaType = part.TryGetProperty("mediaType", out var media) ? media.GetString() ?? "application/octet-stream" : "application/octet-stream";
                var data = part.TryGetProperty("data", out var dataElement) ? dataElement.GetString() : null;
                return new GeneratedFile(data is null ? Array.Empty<byte>() : Convert.FromBase64String(data), mediaType);
            default:
                return null;
        }
    }

    private static string ReadDelta(JsonElement root)
    {
        if (root.TryGetProperty("delta", out var delta))
        {
            return delta.GetString() ?? string.Empty;
        }

        if (root.TryGetProperty("textDelta", out var textDelta))
        {
            return textDelta.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static LanguageModelUsage ReadUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usage", out var usage) || usage.ValueKind != JsonValueKind.Object)
        {
            return LanguageModelUsage.Empty;
        }

        int? input = ReadTokenCount(usage, "inputTokens");
        int? output = ReadTokenCount(usage, "outputTokens");
        int? total = ReadTokenCount(usage, "totalTokens");
        return new LanguageModelUsage(input, output, total);
    }

    /// <summary>Reads a finish reason that is either a string or <c>{ unified, raw }</c>.</summary>
    private static string? ReadFinishReason(JsonElement finish)
    {
        if (finish.ValueKind == JsonValueKind.String)
        {
            return finish.GetString();
        }

        if (finish.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (finish.TryGetProperty("unified", out var unified) && unified.ValueKind == JsonValueKind.String)
        {
            return unified.GetString();
        }

        if (finish.TryGetProperty("raw", out var raw) && raw.ValueKind == JsonValueKind.String)
        {
            return raw.GetString();
        }

        return null;
    }

    /// <summary>Reads a token count that is either a number or <c>{ total }</c>.</summary>
    private static int? ReadTokenCount(JsonElement usage, string name)
    {
        if (!usage.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("total", out var total)
            && total.ValueKind == JsonValueKind.Number
            && total.TryGetInt32(out var nested))
        {
            return nested;
        }

        return null;
    }

    /// <summary>Adds <c>additionalProperties: false</c> on object schemas, which providers require for tools.</summary>
    private static JsonNode? PrepareSchema(JsonNode? node)
    {
        if (node is not JsonObject schema)
        {
            return node;
        }

        var typeName = schema["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var type) ? type : null;
        if (typeName == "object" || schema["properties"] is JsonObject)
        {
            if (schema["additionalProperties"] == null)
            {
                schema["additionalProperties"] = false;
            }

            if (schema["properties"] is JsonObject properties)
            {
                foreach (var property in properties.ToList())
                {
                    if (property.Value != null)
                    {
                        properties[property.Key] = PrepareSchema(property.Value);
                    }
                }

                if (schema["required"] == null && properties.Count > 0)
                {
                    var required = new JsonArray();
                    foreach (var property in properties)
                    {
                        required.Add(property.Key);
                    }

                    schema["required"] = required;
                }
            }
        }

        return schema;
    }

    private static JsonNode? JsonValue(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return json;
        }
    }

    private static JsonObject ToolOutput(string? outputJson, bool isError)
    {
        JsonNode? value = null;
        var json = false;
        if (!string.IsNullOrWhiteSpace(outputJson))
        {
            try
            {
                value = JsonNode.Parse(outputJson);
                json = true;
            }
            catch (JsonException)
            {
                value = outputJson;
            }
        }

        var type = isError ? (json ? "error-json" : "error-text") : (json ? "json" : "text");
        return new JsonObject
        {
            ["type"] = type,
            ["value"] = value,
        };
    }

    private static string ReadToolInput(JsonElement part)
    {
        if (!part.TryGetProperty("input", out var input) && !part.TryGetProperty("args", out input))
        {
            return "{}";
        }

        return input.ValueKind == JsonValueKind.String ? input.GetString() ?? "{}" : input.GetRawText();
    }
}
