// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Cohere;

/// <summary>Maps prompts, tools, usage, and reasoning onto the Cohere chat API.</summary>
public static class CohereChatMapping
{
    private const int MaxOutputTokens = 32768;

    /// <summary>Converts prompt messages into Cohere messages and documents.</summary>
    public static CoherePromptConversion Convert(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new JsonArray();
        var documents = new JsonArray();
        var warnings = new List<CohereWarning>();
        if (prompt != null)
        {
            foreach (var message in prompt)
            {
                Append(message, messages, documents);
            }
        }

        return new CoherePromptConversion(messages.ToJsonString(), documents.ToJsonString(), warnings);
    }

    /// <summary>Prepares function tools and maps tool choice onto <c>NONE</c> or <c>REQUIRED</c>.</summary>
    public static CoherePreparedTools PrepareTools(IReadOnlyList<CohereToolDefinition>? tools, ToolChoice? toolChoice)
    {
        if (tools == null || tools.Count == 0)
        {
            return new CoherePreparedTools(null, null, Array.Empty<CohereWarning>());
        }

        var warnings = new List<CohereWarning>();
        var mapped = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.Kind == "provider")
            {
                warnings.Add(new CohereWarning("unsupported", "provider-defined tool " + tool.ProviderId, null));
                continue;
            }

            var function = new JsonObject { ["name"] = tool.Name };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            function["parameters"] = tool.Parameters.ValueKind == JsonValueKind.Undefined
                ? new JsonObject()
                : JsonNode.Parse(tool.Parameters.GetRawText());
            mapped.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        string? choice = null;
        if (toolChoice != null)
        {
            if (toolChoice.Type == "none")
            {
                choice = "NONE";
            }
            else if (toolChoice.Type == "required")
            {
                choice = "REQUIRED";
            }
            else if (toolChoice.Type == "tool" && toolChoice is ToolChoice.NamedChoice named)
            {
                choice = "REQUIRED";
                mapped = Filter(tools, named.ToolName);
            }
        }

        return new CoherePreparedTools(mapped.ToJsonString(), choice, warnings);
    }

    /// <summary>Maps a Cohere finish reason. <c>TOOL_CALL</c> becomes tool calls.</summary>
    public static FinishReason MapFinishReason(string? finishReason)
    {
        switch (finishReason)
        {
            case "COMPLETE":
            case "STOP_SEQUENCE":
                return FinishReason.Stop;
            case "MAX_TOKENS":
                return FinishReason.Length;
            case "ERROR":
                return FinishReason.Error;
            case "TOOL_CALL":
                return FinishReason.ToolCalls;
            default:
                return FinishReason.Other;
        }
    }

    /// <summary>Reads token counts from <c>usage.tokens</c>. Cached tokens must be a number when present.</summary>
    public static LanguageModelUsage ConvertUsage(JsonElement usage)
    {
        ValidateUsage(usage);
        var tokens = usage.GetProperty("tokens");
        var input = tokens.GetProperty("input_tokens").GetInt32();
        var output = tokens.GetProperty("output_tokens").GetInt32();
        return new LanguageModelUsage(input, output, null, null, null, null, usage.Clone());
    }

    /// <summary>Throws when usage numbers are the wrong JSON type.</summary>
    public static void ValidateUsage(JsonElement usage)
    {
        if (usage.ValueKind != JsonValueKind.Object
            || !usage.TryGetProperty("tokens", out var tokens)
            || tokens.ValueKind != JsonValueKind.Object
            || !IsNumber(tokens, "input_tokens")
            || !IsNumber(tokens, "output_tokens"))
        {
            throw new CohereUsageException("Invalid Cohere usage.");
        }

        if (usage.TryGetProperty("cached_tokens", out var cached)
            && cached.ValueKind != JsonValueKind.Null
            && cached.ValueKind != JsonValueKind.Number)
        {
            throw new CohereUsageException("Invalid Cohere usage.");
        }

        if (usage.TryGetProperty("billed_units", out var billed) && billed.ValueKind != JsonValueKind.Null && billed.ValueKind != JsonValueKind.Undefined)
        {
            if (billed.ValueKind != JsonValueKind.Object)
            {
                throw new CohereUsageException("Invalid Cohere usage.");
            }

            if (billed.TryGetProperty("input_tokens", out var billedInput) && billedInput.ValueKind != JsonValueKind.Number && billedInput.ValueKind != JsonValueKind.Null)
            {
                throw new CohereUsageException("Invalid Cohere usage.");
            }

            if (billed.TryGetProperty("output_tokens", out var billedOutput) && billedOutput.ValueKind != JsonValueKind.Number && billedOutput.ValueKind != JsonValueKind.Null)
            {
                throw new CohereUsageException("Invalid Cohere usage.");
            }
        }
    }

    /// <summary>Resolves Cohere thinking from provider options or a top-level reasoning level.</summary>
    public static JsonObject? ResolveThinking(string? reasoning, JsonElement? cohereOptions, List<CohereWarning> warnings)
    {
        if (cohereOptions is { ValueKind: JsonValueKind.Object } options && options.TryGetProperty("thinking", out var thinking) && thinking.ValueKind == JsonValueKind.Object)
        {
            var resolved = new JsonObject { ["type"] = String(thinking, "type") ?? "enabled" };
            if (thinking.TryGetProperty("tokenBudget", out var budget) && budget.ValueKind == JsonValueKind.Number)
            {
                resolved["token_budget"] = budget.GetInt32();
            }

            return resolved;
        }

        if (string.IsNullOrEmpty(reasoning) || reasoning == "provider-default")
        {
            return null;
        }

        if (reasoning == "none")
        {
            return new JsonObject { ["type"] = "disabled" };
        }

        var percent = BudgetPercent(reasoning);
        if (percent is null)
        {
            warnings?.Add(new CohereWarning("unsupported", "reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        var budgetTokens = (int)Math.Min(MaxOutputTokens, Math.Max(1024, Math.Round(MaxOutputTokens * percent.Value)));
        return new JsonObject { ["type"] = "enabled", ["token_budget"] = budgetTokens };
    }

    /// <summary>Parses JSON and rejects <c>__proto__</c> and constructor prototype keys.</summary>
    public static JsonElement SecureParse(string text)
    {
        var source = string.IsNullOrWhiteSpace(text) ? "{}" : text;
        using var document = JsonDocument.Parse(source);
        RejectPrototype(document.RootElement);
        return document.RootElement.Clone();
    }

    /// <summary>Serializes tool arguments. A <c>null</c> JSON value becomes <c>{}</c> only when the caller asks.</summary>
    public static string Reserialize(string arguments)
    {
        var trimmed = string.IsNullOrWhiteSpace(arguments) ? "{}" : arguments.Trim();
        var parsed = SecureParse(trimmed);
        if (parsed.ValueKind == JsonValueKind.Null || parsed.ValueKind == JsonValueKind.Undefined)
        {
            return "null";
        }

        return JsonSerializer.Serialize(parsed);
    }

    private static JsonArray Filter(IReadOnlyList<CohereToolDefinition> tools, string name)
    {
        var filtered = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.Kind == "provider" || tool.Name != name)
            {
                continue;
            }

            var function = new JsonObject { ["name"] = tool.Name };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            function["parameters"] = JsonNode.Parse(tool.Parameters.GetRawText());
            filtered.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        return filtered;
    }

    private static void Append(ModelMessage message, JsonArray messages, JsonArray documents)
    {
        if (message is SystemModelMessage system)
        {
            messages.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
            return;
        }

        if (message is UserModelMessage user)
        {
            AppendUser(user.Content, messages, documents);
            return;
        }

        if (message is AssistantModelMessage assistant)
        {
            if (assistant.ToolCalls.Count > 0)
            {
                var calls = new JsonArray();
                foreach (var call in assistant.ToolCalls)
                {
                    calls.Add(new JsonObject
                    {
                        ["id"] = call.ToolCallId,
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = call.ToolName,
                            ["arguments"] = Reserialize(call.ArgumentsJson),
                        },
                    });
                }

                messages.Add(new JsonObject { ["role"] = "assistant", ["tool_calls"] = calls });
                return;
            }

            messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = assistant.Text ?? string.Empty });
            return;
        }

        if (message is CohereToolMessage group)
        {
            foreach (var result in group.Results)
            {
                messages.Add(ToolMessage(result));
            }

            return;
        }

        if (message is ToolModelMessage tool)
        {
            messages.Add(new JsonObject
            {
                ["role"] = "tool",
                ["content"] = tool.OutputJson == "null" ? "{}" : tool.OutputJson,
                ["tool_call_id"] = tool.ToolCallId,
            });
        }
    }

    private static void AppendUser(IReadOnlyList<UserContentPart> parts, JsonArray messages, JsonArray documents)
    {
        var content = new JsonArray();
        var hasImage = false;
        foreach (var part in parts)
        {
            if (part is TextContentPart text)
            {
                if (text.Text.Length > 0)
                {
                    content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                }
            }
            else if (part is CohereTextPart cohereText)
            {
                if (cohereText.Text.Length > 0)
                {
                    content.Add(new JsonObject { ["type"] = "text", ["text"] = cohereText.Text });
                }
            }
            else if (part is CohereFilePart file)
            {
                if (IsImage(file.MediaType, file.Bytes))
                {
                    hasImage = true;
                    content.Add(ImagePart(file));
                }
                else
                {
                    documents.Add(Document(file));
                }
            }
            else if (part is FileContentPart plain)
            {
                if (IsImage(plain.MediaType, plain.Data))
                {
                    hasImage = true;
                    content.Add(ImageFromFile(plain));
                }
                else
                {
                    documents.Add(DocumentFromFile(plain));
                }
            }
        }

        if (hasImage)
        {
            messages.Add(new JsonObject { ["role"] = "user", ["content"] = content });
            return;
        }

        var joined = new StringBuilder();
        foreach (var item in content)
        {
            if (item is JsonObject obj && obj["type"]?.GetValue<string>() == "text")
            {
                joined.Append(obj["text"]?.GetValue<string>());
            }
        }

        messages.Add(new JsonObject { ["role"] = "user", ["content"] = joined.ToString() });
    }

    private static JsonObject ImagePart(CohereFilePart file)
    {
        var image = new JsonObject { ["url"] = ImageUrl(file) };
        if (!string.IsNullOrEmpty(file.Detail))
        {
            image["detail"] = file.Detail;
        }

        return new JsonObject { ["type"] = "image_url", ["image_url"] = image };
    }

    private static string ImageUrl(CohereFilePart file)
    {
        if (file.ProviderReference)
        {
            throw new InvalidOperationException("'image file parts with provider references' functionality not supported.");
        }

        if (!string.IsNullOrEmpty(file.Url))
        {
            return file.Url!;
        }

        if (file.Text != null)
        {
            throw new InvalidOperationException("'image file parts with text data' functionality not supported.");
        }

        var media = ResolveMediaType(file.MediaType, file.Bytes);
        if (file.StringData != null)
        {
            return "data:" + media + ";base64," + file.StringData;
        }

        return "data:" + media + ";base64," + System.Convert.ToBase64String(file.Bytes ?? Array.Empty<byte>());
    }

    private static JsonObject ImageFromFile(FileContentPart file)
    {
        string url;
        if (!string.IsNullOrEmpty(file.Url))
        {
            url = file.Url!;
        }
        else
        {
            url = "data:" + ResolveMediaType(file.MediaType, file.Data) + ";base64," + System.Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
        }

        return new JsonObject
        {
            ["type"] = "image_url",
            ["image_url"] = new JsonObject { ["url"] = url },
        };
    }

    private static JsonObject Document(CohereFilePart file)
    {
        if (file.ProviderReference)
        {
            throw new InvalidOperationException("'file parts with provider references' functionality not supported.");
        }

        if (!string.IsNullOrEmpty(file.Url) && file.Bytes == null && file.Text == null && file.StringData == null)
        {
            throw new InvalidOperationException("URLs should be downloaded by the AI SDK and not reach this point. This indicates a configuration issue.");
        }

        var text = file.Text ?? file.StringData;
        if (text == null && file.Bytes != null)
        {
            text = Encoding.UTF8.GetString(file.Bytes);
        }

        var data = new JsonObject { ["text"] = text ?? string.Empty };
        if (file.FileName != null)
        {
            data["title"] = file.FileName;
        }

        return new JsonObject { ["data"] = data };
    }

    private static JsonObject DocumentFromFile(FileContentPart file)
    {
        var data = new JsonObject { ["text"] = file.Data == null ? string.Empty : Encoding.UTF8.GetString(file.Data) };
        if (file.FileName != null)
        {
            data["title"] = file.FileName;
        }

        return new JsonObject { ["data"] = data };
    }

    private static JsonObject ToolMessage(CohereToolResultPart result)
    {
        string content;
        if (result.Kind == "text" || result.Kind == "error-text")
        {
            content = result.Text ?? string.Empty;
        }
        else if (result.Kind == "execution-denied")
        {
            content = result.Reason ?? "Tool call execution denied.";
        }
        else if (result.Json is { } json)
        {
            content = JsonSerializer.Serialize(json);
        }
        else
        {
            content = result.Text ?? "null";
        }

        return new JsonObject
        {
            ["role"] = "tool",
            ["content"] = content,
            ["tool_call_id"] = result.ToolCallId,
        };
    }

    private static bool IsImage(string? mediaType, byte[]? bytes)
    {
        var top = TopLevel(mediaType);
        if (top == "image")
        {
            return true;
        }

        return false;
    }

    private static string ResolveMediaType(string? mediaType, byte[]? bytes)
    {
        if (!string.IsNullOrEmpty(mediaType) && mediaType!.IndexOf('/') >= 0)
        {
            return mediaType;
        }

        if (IsPng(bytes))
        {
            return "image/png";
        }

        return string.IsNullOrEmpty(mediaType) ? "image/png" : "image/" + mediaType;
    }

    private static bool IsPng(byte[]? bytes)
    {
        return bytes != null
            && bytes.Length >= 8
            && bytes[0] == 0x89
            && bytes[1] == 0x50
            && bytes[2] == 0x4E
            && bytes[3] == 0x47
            && bytes[4] == 0x0D
            && bytes[5] == 0x0A
            && bytes[6] == 0x1A
            && bytes[7] == 0x0A;
    }

    private static string TopLevel(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return string.Empty;
        }

        var slash = mediaType!.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    private static void RejectPrototype(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("__proto__"))
                {
                    throw new FormatException("Object contains forbidden prototype property");
                }

                if (property.NameEquals("constructor")
                    && property.Value.ValueKind == JsonValueKind.Object
                    && property.Value.TryGetProperty("prototype", out _))
                {
                    throw new FormatException("Object contains forbidden prototype property");
                }

                RejectPrototype(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                RejectPrototype(item);
            }
        }
    }

    private static bool IsNumber(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number;
    }

    private static string? String(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }

    private static double? BudgetPercent(string reasoning)
    {
        switch (reasoning)
        {
            case "minimal":
                return 0.02;
            case "low":
                return 0.1;
            case "medium":
                return 0.3;
            case "high":
                return 0.6;
            case "xhigh":
                return 0.9;
            default:
                return null;
        }
    }
}

/// <summary>Usage that failed Cohere's numeric schema.</summary>
public sealed class CohereUsageException : FormatException
{
    /// <summary>Creates the exception.</summary>
    public CohereUsageException(string message)
        : base(message)
    {
    }
}
