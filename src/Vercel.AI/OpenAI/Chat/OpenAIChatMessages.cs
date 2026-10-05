// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>One prompt message converted into an OpenAI chat message.</summary>
public sealed class OpenAIChatPromptMessage
{
    /// <summary>Creates a message.</summary>
    public OpenAIChatPromptMessage(string role)
    {
        Role = role;
    }

    /// <summary><c>system</c>, <c>user</c>, <c>assistant</c>, or <c>tool</c>.</summary>
    public string Role { get; }

    /// <summary>System text, or assistant text when there are no parts.</summary>
    public string? Text { get; set; }

    /// <summary>User or assistant parts.</summary>
    public IReadOnlyList<OpenAIChatPromptPart>? Parts { get; set; }

    /// <summary>Assistant tool calls.</summary>
    public IReadOnlyList<GeneratedToolCall>? ToolCalls { get; set; }

    /// <summary>Tool result id.</summary>
    public string? ToolCallId { get; set; }

    /// <summary>Tool result text.</summary>
    public string? ToolOutput { get; set; }

    /// <summary>Provider options for this message. The <c>openai</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }
}

/// <summary>One user or assistant content part.</summary>
public sealed class OpenAIChatPromptPart
{
    /// <summary>Creates a part. <paramref name="type"/> is <c>text</c> or <c>file</c>.</summary>
    public OpenAIChatPromptPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }

    /// <summary>Text, when <see cref="Type"/> is <c>text</c>.</summary>
    public string? Text { get; set; }

    /// <summary>IANA media type for a file part.</summary>
    public string? MediaType { get; set; }

    /// <summary>Remote URL for a file part.</summary>
    public string? Url { get; set; }

    /// <summary>Inline file bytes.</summary>
    public byte[]? Data { get; set; }

    /// <summary>Base64 payload used as-is when <see cref="Data"/> is null.</summary>
    public string? Base64 { get; set; }

    /// <summary>OpenAI file id.</summary>
    public string? FileId { get; set; }

    /// <summary>File name for PDF data.</summary>
    public string? FileName { get; set; }

    /// <summary>Provider options for this part.</summary>
    public JsonElement? ProviderOptions { get; set; }
}

/// <summary>Chat Completions messages and the warnings produced while converting them.</summary>
public sealed class OpenAIChatMessageResult
{
    internal OpenAIChatMessageResult(JsonArray messages, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Messages = messages;
        Warnings = warnings;
    }

    /// <summary>OpenAI <c>messages</c> array.</summary>
    public JsonArray Messages { get; }

    /// <summary>Warnings produced while converting the prompt.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>Converts prompts into OpenAI Chat Completions messages.</summary>
public static class OpenAIChatMessages
{
    /// <summary>Converts SDK messages. <paramref name="systemMessageMode"/> is <c>system</c>, <c>developer</c>, or <c>remove</c>.</summary>
    public static OpenAIChatMessageResult Convert(IReadOnlyList<ModelMessage> prompt, string systemMessageMode = "system")
    {
        var messages = new List<OpenAIChatPromptMessage>();
        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    messages.Add(new OpenAIChatPromptMessage("system") { Text = system.Content });
                    break;
                case UserModelMessage user:
                    messages.Add(new OpenAIChatPromptMessage("user") { Parts = Parts(user) });
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(new OpenAIChatPromptMessage("assistant")
                    {
                        Text = assistant.Text,
                        ToolCalls = assistant.ToolCalls,
                    });
                    break;
                case ToolModelMessage tool:
                    messages.Add(new OpenAIChatPromptMessage("tool")
                    {
                        ToolCallId = tool.ToolCallId,
                        ToolOutput = tool.OutputJson,
                        ProviderOptions = tool.ProviderMetadata,
                    });
                    break;
                default:
                    throw new AiSdkException("Unsupported message role '" + message.Role + "'.");
            }
        }

        return Convert(messages, systemMessageMode);
    }

    /// <summary>Converts prepared messages. <paramref name="systemMessageMode"/> is <c>system</c>, <c>developer</c>, or <c>remove</c>.</summary>
    public static OpenAIChatMessageResult Convert(IReadOnlyList<OpenAIChatPromptMessage> prompt, string systemMessageMode = "system")
    {
        var messages = new JsonArray();
        var warnings = new List<OpenAICallWarning>();
        foreach (var message in prompt)
        {
            switch (message.Role)
            {
                case "system":
                    ConvertSystem(messages, warnings, message, systemMessageMode);
                    break;
                case "user":
                    messages.Add(ConvertUser(message));
                    break;
                case "assistant":
                    messages.Add(ConvertAssistant(message));
                    break;
                case "tool":
                    messages.Add(ConvertTool(message));
                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + message.Role);
            }
        }

        return new OpenAIChatMessageResult(messages, warnings);
    }

    private static void ConvertSystem(JsonArray messages, List<OpenAICallWarning> warnings, OpenAIChatPromptMessage message, string systemMessageMode)
    {
        switch (systemMessageMode)
        {
            case "remove":
                warnings.Add(new OpenAICallWarning("other", null, null, "system messages are removed for this model"));
                return;
            case "system":
            case "developer":
                var breakpoint = OpenAIJson.PromptCacheBreakpoint(message.ProviderOptions);
                JsonNode content = breakpoint == null
                    ? JsonValue.Create(message.Text ?? string.Empty)!
                    : new JsonArray(TextBlock(message.Text ?? string.Empty, breakpoint));
                messages.Add(new JsonObject { ["role"] = systemMessageMode, ["content"] = content });
                return;
            default:
                throw new AiSdkException("Unsupported system message mode: " + systemMessageMode);
        }
    }

    private static JsonObject ConvertUser(OpenAIChatPromptMessage message)
    {
        var parts = message.Parts ?? Array.Empty<OpenAIChatPromptPart>();
        if (parts.Count == 1 && parts[0].Type == "text" && OpenAIJson.PromptCacheBreakpoint(parts[0].ProviderOptions) == null)
        {
            return new JsonObject { ["role"] = "user", ["content"] = parts[0].Text ?? string.Empty };
        }

        var content = new JsonArray();
        for (var index = 0; index < parts.Count; index++)
        {
            content.Add(ConvertUserPart(parts[index], index));
        }

        return new JsonObject { ["role"] = "user", ["content"] = content };
    }

    private static JsonObject ConvertUserPart(OpenAIChatPromptPart part, int index)
    {
        var breakpoint = OpenAIJson.PromptCacheBreakpoint(part.ProviderOptions);
        if (part.Type == "text")
        {
            return TextBlock(part.Text ?? string.Empty, breakpoint);
        }

        if (!string.IsNullOrEmpty(part.FileId))
        {
            var reference = new JsonObject
            {
                ["type"] = "file",
                ["file"] = new JsonObject { ["file_id"] = part.FileId },
            };
            AddBreakpoint(reference, breakpoint);
            return reference;
        }

        var mediaType = part.MediaType ?? "application/octet-stream";
        var inline = part.Data != null || part.Base64 != null;
        var bytes = part.Data;
        if (bytes == null && part.Base64 != null)
        {
            bytes = System.Convert.FromBase64String(part.Base64);
        }

        var fullMediaType = OpenAIJson.ResolveFullMediaType(mediaType, bytes, inline && part.Url == null);
        var topLevel = OpenAIJson.TopLevel(fullMediaType);
        if (topLevel == "image")
        {
            string url;
            if (!string.IsNullOrEmpty(part.Url))
            {
                url = part.Url!;
            }
            else
            {
                var payload = part.Base64 ?? System.Convert.ToBase64String(bytes ?? Array.Empty<byte>());
                url = "data:" + fullMediaType + ";base64," + payload;
            }

            var image = new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JsonObject { ["url"] = url },
            };
            var detail = OpenAIJson.ImageDetail(part.ProviderOptions);
            if (!string.IsNullOrEmpty(detail))
            {
                ((JsonObject)image["image_url"]!)["detail"] = detail;
            }

            AddBreakpoint(image, breakpoint);
            return image;
        }

        if (topLevel == "audio")
        {
            if (!string.IsNullOrEmpty(part.Url))
            {
                throw new AiSdkException("audio file parts with URLs");
            }

            string? format = fullMediaType switch
            {
                "audio/wav" => "wav",
                "audio/mp3" => "mp3",
                "audio/mpeg" => "mp3",
                _ => null,
            };
            if (format == null)
            {
                throw new AiSdkException("audio content parts with media type " + fullMediaType);
            }

            var audio = new JsonObject
            {
                ["type"] = "input_audio",
                ["input_audio"] = new JsonObject
                {
                    ["data"] = part.Base64 ?? System.Convert.ToBase64String(bytes ?? Array.Empty<byte>()),
                    ["format"] = format,
                },
            };
            AddBreakpoint(audio, breakpoint);
            return audio;
        }

        if (fullMediaType != "application/pdf")
        {
            throw new AiSdkException("file part media type " + fullMediaType);
        }

        if (!string.IsNullOrEmpty(part.Url))
        {
            throw new AiSdkException("PDF file parts with URLs");
        }

        var fileName = string.IsNullOrEmpty(part.FileName) ? "part-" + index.ToString() + ".pdf" : part.FileName;
        var pdf = new JsonObject
        {
            ["type"] = "file",
            ["file"] = new JsonObject
            {
                ["filename"] = fileName,
                ["file_data"] = "data:application/pdf;base64," + (part.Base64 ?? System.Convert.ToBase64String(bytes ?? Array.Empty<byte>())),
            },
        };
        AddBreakpoint(pdf, breakpoint);
        return pdf;
    }

    private static JsonObject ConvertAssistant(OpenAIChatPromptMessage message)
    {
        var toolCalls = message.ToolCalls ?? Array.Empty<GeneratedToolCall>();
        var text = message.Text ?? string.Empty;
        var textParts = new JsonArray();
        var hasBreakpoint = false;
        if (message.Parts != null)
        {
            var builder = new StringBuilder();
            foreach (var part in message.Parts)
            {
                if (part.Type != "text")
                {
                    continue;
                }

                var partText = part.Text ?? string.Empty;
                builder.Append(partText);
                var breakpoint = OpenAIJson.PromptCacheBreakpoint(part.ProviderOptions);
                if (breakpoint != null)
                {
                    hasBreakpoint = true;
                }

                textParts.Add(TextBlock(partText, breakpoint));
            }

            text = builder.ToString();
        }

        var json = new JsonObject { ["role"] = "assistant" };
        if (hasBreakpoint)
        {
            json["content"] = textParts;
        }
        else if (toolCalls.Count > 0)
        {
            json["content"] = text.Length == 0 ? JsonNode.Parse("null") : text;
        }
        else
        {
            json["content"] = text;
        }

        if (toolCalls.Count > 0)
        {
            var calls = new JsonArray();
            foreach (var call in toolCalls)
            {
                calls.Add(new JsonObject
                {
                    ["id"] = call.ToolCallId,
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = call.ToolName,
                        ["arguments"] = ToolArguments(call.ArgumentsJson),
                    },
                });
            }

            json["tool_calls"] = calls;
        }

        return json;
    }

    private static string ToolArguments(string? arguments)
    {
        if (string.IsNullOrEmpty(arguments))
        {
            return "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(arguments);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                return arguments;
            }
        }
        catch (JsonException)
        {
        }

        return "{}";
    }

    private static JsonObject ConvertTool(OpenAIChatPromptMessage message)
    {
        var breakpoint = OpenAIJson.PromptCacheBreakpoint(message.ProviderOptions);
        JsonNode content = breakpoint == null
            ? JsonValue.Create(message.ToolOutput ?? string.Empty)!
            : new JsonArray(TextBlock(message.ToolOutput ?? string.Empty, breakpoint));
        return new JsonObject
        {
            ["role"] = "tool",
            ["tool_call_id"] = message.ToolCallId,
            ["content"] = content,
        };
    }

    private static JsonObject TextBlock(string text, JsonObject? breakpoint)
    {
        var block = new JsonObject { ["type"] = "text", ["text"] = text };
        AddBreakpoint(block, breakpoint);
        return block;
    }

    private static void AddBreakpoint(JsonObject block, JsonObject? breakpoint)
    {
        if (breakpoint != null)
        {
            block["prompt_cache_breakpoint"] = breakpoint;
        }
    }

    private static IReadOnlyList<OpenAIChatPromptPart> Parts(UserModelMessage user)
    {
        var parts = new List<OpenAIChatPromptPart>();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text)
            {
                parts.Add(new OpenAIChatPromptPart("text") { Text = text.Text });
            }
            else if (part is FileContentPart file)
            {
                parts.Add(new OpenAIChatPromptPart("file")
                {
                    MediaType = file.MediaType,
                    Url = file.Url,
                    Data = file.Data,
                    FileName = file.FileName,
                });
            }
        }

        return parts;
    }
}
