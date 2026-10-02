// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.OpenTelemetry;

/// <summary>A tool call included in <c>gen_ai.output.messages</c>.</summary>
public sealed class GenAiToolCall
{
    /// <summary>Creates a tool call.</summary>
    public GenAiToolCall(string toolCallId, string toolName, JsonNode? input)
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        Input = input;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Tool arguments.</summary>
    public JsonNode? Input { get; }
}

/// <summary>A tool result included in <c>gen_ai.output.messages</c>.</summary>
public sealed class GenAiToolResult
{
    /// <summary>Creates a tool result.</summary>
    public GenAiToolResult(string toolCallId, JsonNode? output)
    {
        ToolCallId = toolCallId ?? string.Empty;
        Output = output;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool output.</summary>
    public JsonNode? Output { get; }
}

/// <summary>A generated file included in <c>gen_ai.output.messages</c>.</summary>
public sealed class GenAiOutputFile
{
    /// <summary>Creates a file.</summary>
    public GenAiOutputFile(string mediaType, string base64)
    {
        MediaType = mediaType ?? string.Empty;
        Base64 = base64 ?? string.Empty;
    }

    /// <summary>Media type.</summary>
    public string MediaType { get; }

    /// <summary>Base64 payload.</summary>
    public string Base64 { get; }
}

/// <summary>Step output formatted as <c>gen_ai.output.messages</c>.</summary>
public sealed class GenAiOutput
{
    /// <summary>Assistant text. Empty text is omitted.</summary>
    public string? Text { get; set; }

    /// <summary>Reasoning strings. Empty strings are omitted.</summary>
    public IReadOnlyList<string>? Reasoning { get; set; }

    /// <summary>Tool calls.</summary>
    public IReadOnlyList<GenAiToolCall>? ToolCalls { get; set; }

    /// <summary>Tool results.</summary>
    public IReadOnlyList<GenAiToolResult>? ToolResults { get; set; }

    /// <summary>Generated files.</summary>
    public IReadOnlyList<GenAiOutputFile>? Files { get; set; }

    /// <summary>SDK finish reason.</summary>
    public string FinishReason { get; set; } = "stop";
}

/// <summary>Maps provider and operation names and formats GenAI semantic-convention messages.</summary>
public static class GenAiTelemetry
{
    private static readonly (string Prefix, string Name)[] Providers =
    {
        ("google.vertex", "gcp.vertex_ai"),
        ("google.generative-ai", "gcp.gemini"),
        ("google-vertex", "gcp.vertex_ai"),
        ("amazon-bedrock", "aws.bedrock"),
        ("azure-openai", "azure.ai.openai"),
        ("anthropic", "anthropic"),
        ("openai", "openai"),
        ("azure", "azure.ai.inference"),
        ("google", "gcp.gemini"),
        ("mistral", "mistral_ai"),
        ("cohere", "cohere"),
        ("bedrock", "aws.bedrock"),
        ("groq", "groq"),
        ("deepseek", "deepseek"),
        ("perplexity", "perplexity"),
        ("xai", "x_ai"),
    };

    private static readonly Dictionary<string, string> Operations = new(StringComparer.Ordinal)
    {
        ["ai.generateText"] = "invoke_agent",
        ["ai.streamText"] = "invoke_agent",
        ["ai.generateObject"] = "invoke_agent",
        ["ai.streamObject"] = "invoke_agent",
        ["ai.embed"] = "embeddings",
        ["ai.embedMany"] = "embeddings",
        ["ai.rerank"] = "rerank",
    };

    private static readonly Dictionary<string, string> FinishReasons = new(StringComparer.Ordinal)
    {
        ["stop"] = "stop",
        ["length"] = "length",
        ["content-filter"] = "content_filter",
        ["tool-calls"] = "tool_call",
        ["error"] = "error",
        ["other"] = "stop",
        ["unknown"] = "stop",
    };

    /// <summary>Maps an SDK provider string to a GenAI <c>gen_ai.provider.name</c> value.</summary>
    public static string MapProviderName(string provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        var lower = provider.ToLowerInvariant();
        for (var index = 0; index < Providers.Length; index++)
        {
            var prefix = Providers[index].Prefix;
            if (lower == prefix || lower.StartsWith(prefix + ".", StringComparison.Ordinal) || lower.StartsWith(prefix + "-", StringComparison.Ordinal))
            {
                return Providers[index].Name;
            }
        }

        return provider;
    }

    /// <summary>Maps an SDK operation id to <c>gen_ai.operation.name</c>.</summary>
    public static string MapOperationName(string operationId)
    {
        if (operationId is null)
        {
            throw new ArgumentNullException(nameof(operationId));
        }

        if (Operations.TryGetValue(operationId, out var mapped))
        {
            return mapped;
        }

        return operationId;
    }

    /// <summary>Formats a system string, system message, or list of system messages.</summary>
    public static JsonArray FormatSystemInstructions(JsonNode? system)
    {
        var result = new JsonArray();
        if (system is JsonValue value && value.TryGetValue<string>(out var text))
        {
            result.Add(Instruction(text));
            return result;
        }

        if (system is JsonArray array)
        {
            foreach (var item in array)
            {
                result.Add(Instruction(ContentText(item)));
            }

            return result;
        }

        if (system is JsonObject)
        {
            result.Add(Instruction(ContentText(system)));
        }

        return result;
    }

    /// <summary>Returns the first system message content in <paramref name="prompt"/>, or null.</summary>
    public static string? ExtractSystemFromPrompt(JsonArray? prompt)
    {
        if (prompt == null)
        {
            return null;
        }

        foreach (var message in prompt)
        {
            if (message is JsonObject obj && Role(obj) == "system")
            {
                return ContentText(obj);
            }
        }

        return null;
    }

    /// <summary>Converts a provider prompt to <c>gen_ai.input.messages</c>, preserving order.</summary>
    public static JsonArray FormatInputMessages(JsonArray prompt)
    {
        if (prompt is null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        var result = new JsonArray();
        foreach (var messageNode in prompt)
        {
            if (messageNode is not JsonObject message)
            {
                continue;
            }

            var role = Role(message);
            if (role == "system")
            {
                result.Add(Message(role, new JsonArray { TextPart(ContentText(message)) }));
                continue;
            }

            var parts = new JsonArray();
            if (message["content"] is JsonArray content)
            {
                foreach (var part in content)
                {
                    if (part is JsonObject obj)
                    {
                        parts.Add(ConvertPart(obj));
                    }
                }
            }

            result.Add(Message(role, parts));
        }

        return result;
    }

    /// <summary>Converts a prompt string or message list, then <paramref name="messages"/>, preserving order.</summary>
    public static JsonArray FormatModelMessages(JsonNode? prompt, JsonArray? messages)
    {
        var result = new JsonArray();
        if (prompt is JsonValue promptValue && promptValue.TryGetValue<string>(out var text))
        {
            result.Add(Message("user", new JsonArray { TextPart(text) }));
        }
        else if (prompt is JsonArray promptMessages)
        {
            AppendModelMessages(result, promptMessages);
        }

        if (messages != null)
        {
            AppendModelMessages(result, messages);
        }

        return result;
    }

    /// <summary>Converts step output to one assistant <c>gen_ai.output.messages</c> entry.</summary>
    public static JsonArray FormatOutputMessages(GenAiOutput output)
    {
        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        var parts = new JsonArray();
        if (output.Reasoning != null)
        {
            for (var index = 0; index < output.Reasoning.Count; index++)
            {
                var text = output.Reasoning[index];
                if (!string.IsNullOrEmpty(text))
                {
                    parts.Add(new JsonObject
                    {
                        ["type"] = "reasoning",
                        ["content"] = text,
                    });
                }
            }
        }

        if (!string.IsNullOrEmpty(output.Text))
        {
            parts.Add(TextPart(output.Text!));
        }

        if (output.ToolCalls != null)
        {
            for (var index = 0; index < output.ToolCalls.Count; index++)
            {
                var call = output.ToolCalls[index];
                parts.Add(new JsonObject
                {
                    ["type"] = "tool_call",
                    ["id"] = call.ToolCallId,
                    ["name"] = call.ToolName,
                    ["arguments"] = call.Input?.DeepClone(),
                });
            }
        }

        if (output.ToolResults != null)
        {
            for (var index = 0; index < output.ToolResults.Count; index++)
            {
                var toolResult = output.ToolResults[index];
                parts.Add(new JsonObject
                {
                    ["type"] = "tool_call_response",
                    ["id"] = toolResult.ToolCallId,
                    ["response"] = toolResult.Output?.DeepClone(),
                });
            }
        }

        if (output.Files != null)
        {
            for (var index = 0; index < output.Files.Count; index++)
            {
                var file = output.Files[index];
                parts.Add(Blob(file.MediaType, file.Base64));
            }
        }

        var message = Message("assistant", parts);
        message["finish_reason"] = MapFinishReason(output.FinishReason);
        return new JsonArray { message };
    }

    /// <summary>Formats object output as a single assistant text part.</summary>
    public static JsonArray FormatObjectOutputMessages(string objectText, string finishReason)
    {
        var message = Message("assistant", new JsonArray { TextPart(objectText ?? string.Empty) });
        message["finish_reason"] = MapFinishReason(finishReason);
        return new JsonArray { message };
    }

    private static void AppendModelMessages(JsonArray result, JsonArray messages)
    {
        foreach (var messageNode in messages)
        {
            if (messageNode is not JsonObject message)
            {
                continue;
            }

            var role = Role(message);
            if (role == "system")
            {
                result.Add(Message(role, new JsonArray { TextPart(ContentText(message)) }));
                continue;
            }

            if (message["content"] is JsonValue contentValue && contentValue.TryGetValue<string>(out var text))
            {
                result.Add(Message(role, new JsonArray { TextPart(text) }));
                continue;
            }

            var parts = new JsonArray();
            if (message["content"] is JsonArray content)
            {
                foreach (var part in content)
                {
                    if (part is JsonObject obj)
                    {
                        parts.Add(role == "tool" ? ConvertToolResultPart(obj) : ConvertPart(obj));
                    }
                }
            }

            result.Add(Message(role, parts));
        }
    }

    private static JsonObject ConvertPart(JsonObject part)
    {
        var type = part["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var typeText) ? typeText : string.Empty;
        switch (type)
        {
            case "text":
                return TextPart(part["text"] is JsonValue text && text.TryGetValue<string>(out var body) ? body : string.Empty);
            case "reasoning":
                return new JsonObject
                {
                    ["type"] = "reasoning",
                    ["content"] = part["text"] is JsonValue reasoning && reasoning.TryGetValue<string>(out var thought) ? thought : string.Empty,
                };
            case "tool-call":
                return new JsonObject
                {
                    ["type"] = "tool_call",
                    ["id"] = part["toolCallId"]?.DeepClone() ?? JsonNull(),
                    ["name"] = part["toolName"]?.DeepClone(),
                    ["arguments"] = part["input"]?.DeepClone(),
                };
            case "tool-result":
                return new JsonObject
                {
                    ["type"] = "tool_call_response",
                    ["id"] = part["toolCallId"]?.DeepClone() ?? JsonNull(),
                    ["response"] = ToolResponse(part["output"]),
                };
            case "file":
                return ConvertFile(part);
            default:
                return new JsonObject { ["type"] = type };
        }
    }

    private static JsonObject ConvertToolResultPart(JsonObject part)
    {
        if ((part["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var type) ? type : null) == "tool-result")
        {
            return new JsonObject
            {
                ["type"] = "tool_call_response",
                ["id"] = part["toolCallId"]?.DeepClone() ?? JsonNull(),
                ["response"] = ToolResponse(part["output"]),
            };
        }

        return ConvertPart(part);
    }

    private static JsonNode? ToolResponse(JsonNode? output)
    {
        if (output is not JsonObject obj)
        {
            return output?.DeepClone();
        }

        var type = obj["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var typeText) ? typeText : null;
        if (type == "text" || type == "error-text" || type == "json" || type == "error-json")
        {
            return obj["value"]?.DeepClone();
        }

        if (type == "execution-denied")
        {
            return new JsonObject
            {
                ["denied"] = true,
                ["reason"] = obj["reason"]?.DeepClone(),
            };
        }

        return obj.DeepClone();
    }

    private static JsonObject ConvertFile(JsonObject part)
    {
        var mediaType = part["mediaType"] is JsonValue media && media.TryGetValue<string>(out var mediaText) ? mediaText : null;
        var data = part["data"];
        if (data is JsonObject descriptor)
        {
            var kind = descriptor["type"] is JsonValue kindValue && kindValue.TryGetValue<string>(out var kindText) ? kindText : null;
            if (kind == "url")
            {
                return UriPart(mediaType, descriptor["url"] is JsonValue url && url.TryGetValue<string>(out var href) ? href : string.Empty);
            }

            if (kind == "data")
            {
                var payload = descriptor["data"] is JsonValue payloadValue && payloadValue.TryGetValue<string>(out var encoded) ? encoded : string.Empty;
                return Blob(mediaType, payload);
            }

            if (kind == "text")
            {
                var payload = descriptor["text"] is JsonValue textValue && textValue.TryGetValue<string>(out var text) ? text : string.Empty;
                return Blob(mediaType, payload);
            }
        }

        if (data is JsonValue direct && direct.TryGetValue<string>(out var raw))
        {
            if (raw.StartsWith("http://", StringComparison.Ordinal) || raw.StartsWith("https://", StringComparison.Ordinal))
            {
                return UriPart(mediaType, raw);
            }

            return Blob(mediaType, raw);
        }

        return Blob(mediaType, string.Empty);
    }

    private static JsonObject Blob(string? mediaType, string content)
    {
        return new JsonObject
        {
            ["type"] = "blob",
            ["modality"] = Modality(mediaType),
            ["mime_type"] = mediaType,
            ["content"] = content,
        };
    }

    private static JsonObject UriPart(string? mediaType, string uri)
    {
        return new JsonObject
        {
            ["type"] = "uri",
            ["modality"] = Modality(mediaType),
            ["mime_type"] = mediaType,
            ["uri"] = uri,
        };
    }

    private static string Modality(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return "image";
        }

        if (mediaType!.StartsWith("image/", StringComparison.Ordinal))
        {
            return "image";
        }

        if (mediaType.StartsWith("video/", StringComparison.Ordinal))
        {
            return "video";
        }

        if (mediaType.StartsWith("audio/", StringComparison.Ordinal))
        {
            return "audio";
        }

        return "image";
    }

    private static string MapFinishReason(string reason)
    {
        if (reason != null && FinishReasons.TryGetValue(reason, out var mapped))
        {
            return mapped;
        }

        return reason ?? string.Empty;
    }

    private static JsonNode JsonNull()
    {
        return JsonNode.Parse("null")!;
    }

    private static JsonObject Message(string role, JsonArray parts)
    {
        return new JsonObject
        {
            ["role"] = role,
            ["parts"] = parts,
        };
    }

    private static JsonObject TextPart(string content)
    {
        return new JsonObject
        {
            ["type"] = "text",
            ["content"] = content,
        };
    }

    private static JsonObject Instruction(string content)
    {
        return new JsonObject
        {
            ["type"] = "text",
            ["content"] = content,
        };
    }

    private static string Role(JsonObject message)
    {
        return message["role"] is JsonValue value && value.TryGetValue<string>(out var role) ? role : string.Empty;
    }

    private static string ContentText(JsonNode? node)
    {
        if (node is JsonObject obj && obj["content"] is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        if (node is JsonValue direct && direct.TryGetValue<string>(out var body))
        {
            return body;
        }

        return string.Empty;
    }
}
