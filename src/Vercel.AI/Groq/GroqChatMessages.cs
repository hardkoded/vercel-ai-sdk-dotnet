// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>How a Groq file part is supplied.</summary>
public enum GroqFileKind
{
    /// <summary>Inline bytes.</summary>
    Data,

    /// <summary>Remote URL.</summary>
    Url,

    /// <summary>Provider file reference. Groq chat does not accept these.</summary>
    Reference,

    /// <summary>Text file. Groq chat does not accept these.</summary>
    Text,
}

/// <summary>One user content part in a Groq prompt.</summary>
public abstract class GroqUserPart
{
    private GroqUserPart()
    {
    }

    /// <summary>Creates a text part.</summary>
    public static GroqUserPart Text(string text)
    {
        return new TextPart(text ?? string.Empty);
    }

    /// <summary>Creates a file part.</summary>
    public static GroqUserPart File(string mediaType, GroqFileKind kind, byte[]? data, string? url)
    {
        return new FilePart(mediaType ?? string.Empty, kind, data, url);
    }

    private sealed class TextPart : GroqUserPart
    {
        public TextPart(string text)
        {
            Text = text;
        }

        public new string Text { get; }
    }

    private sealed class FilePart : GroqUserPart
    {
        public FilePart(string mediaType, GroqFileKind kind, byte[]? data, string? url)
        {
            MediaType = mediaType;
            Kind = kind;
            Data = data;
            Url = url;
        }

        public string MediaType { get; }

        public GroqFileKind Kind { get; }

        public byte[]? Data { get; }

        public string? Url { get; }
    }

    internal bool IsText(out string text)
    {
        if (this is TextPart part)
        {
            text = part.Text;
            return true;
        }

        text = string.Empty;
        return false;
    }

    internal bool IsFile(out string mediaType, out GroqFileKind kind, out byte[]? data, out string? url)
    {
        if (this is FilePart part)
        {
            mediaType = part.MediaType;
            kind = part.Kind;
            data = part.Data;
            url = part.Url;
            return true;
        }

        mediaType = string.Empty;
        kind = GroqFileKind.Data;
        data = null;
        url = null;
        return false;
    }
}

/// <summary>Converts prompts into Groq chat messages.</summary>
public static class GroqChatMessages
{
    /// <summary>Converts V4 prompt messages.</summary>
    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new JsonArray();
        if (prompt == null)
        {
            return messages;
        }

        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    messages.Add(new JsonObject
                    {
                        ["role"] = "system",
                        ["content"] = system.Content,
                    });
                    break;
                case UserModelMessage user:
                    messages.Add(ConvertUser(user));
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(ConvertAssistant(assistant));
                    break;
                case ToolModelMessage tool:
                    messages.Add(new JsonObject
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = tool.ToolCallId,
                        ["content"] = tool.OutputJson,
                    });
                    break;
            }
        }

        return messages;
    }

    /// <summary>Converts user parts, including file kinds that the V4 file part cannot express.</summary>
    public static JsonObject ConvertUserParts(IReadOnlyList<GroqUserPart> parts)
    {
        if (parts != null && parts.Count == 1 && parts[0].IsText(out var only))
        {
            return new JsonObject
            {
                ["role"] = "user",
                ["content"] = only,
            };
        }

        var content = new JsonArray();
        if (parts != null)
        {
            foreach (var part in parts)
            {
                content.Add(ConvertPart(part));
            }
        }

        return new JsonObject
        {
            ["role"] = "user",
            ["content"] = content,
        };
    }

    /// <summary>Converts an assistant turn that may include reasoning and tool calls.</summary>
    public static JsonObject ConvertAssistantTurn(string? text, string? reasoning, IReadOnlyList<GeneratedToolCall>? toolCalls)
    {
        var message = new JsonObject
        {
            ["role"] = "assistant",
            ["content"] = text ?? string.Empty,
        };
        if (!string.IsNullOrEmpty(reasoning))
        {
            message["reasoning"] = reasoning;
        }

        if (toolCalls != null && toolCalls.Count > 0)
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
                        ["arguments"] = call.ArgumentsJson,
                    },
                });
            }

            message["tool_calls"] = calls;
        }

        return message;
    }

    /// <summary>Converts a tool result. JSON values are serialized; text values are copied.</summary>
    public static JsonObject ConvertToolResult(string toolCallId, string outputType, string? text, JsonNode? json, string? reason)
    {
        string content;
        switch (outputType)
        {
            case "text":
            case "error-text":
                content = text ?? string.Empty;
                break;
            case "execution-denied":
                content = string.IsNullOrEmpty(reason) ? "Tool call execution denied." : reason!;
                break;
            default:
                content = json == null ? "null" : json.ToJsonString();
                break;
        }

        return new JsonObject
        {
            ["role"] = "tool",
            ["tool_call_id"] = toolCallId,
            ["content"] = content,
        };
    }

    private static JsonObject ConvertUser(UserModelMessage user)
    {
        if (user.Content.Count == 1 && user.Content[0] is TextContentPart text)
        {
            return new JsonObject
            {
                ["role"] = "user",
                ["content"] = text.Text,
            };
        }

        var content = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart textPart)
            {
                content.Add(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = textPart.Text,
                });
                continue;
            }

            if (part is FileContentPart file)
            {
                content.Add(ConvertFile(file.MediaType, file.Url != null ? GroqFileKind.Url : GroqFileKind.Data, file.Data, file.Url));
            }
        }

        return new JsonObject
        {
            ["role"] = "user",
            ["content"] = content,
        };
    }

    private static JsonObject ConvertAssistant(AssistantModelMessage assistant)
    {
        return ConvertAssistantTurn(assistant.Text, assistant.Reasoning, assistant.ToolCalls);
    }

    private static JsonNode ConvertPart(GroqUserPart part)
    {
        if (part.IsText(out var text))
        {
            return new JsonObject
            {
                ["type"] = "text",
                ["text"] = text,
            };
        }

        if (part.IsFile(out var mediaType, out var kind, out var data, out var url))
        {
            return ConvertFile(mediaType, kind, data, url);
        }

        throw new UnsupportedFunctionalityException("Non-image file content parts");
    }

    private static JsonObject ConvertFile(string mediaType, GroqFileKind kind, byte[]? data, string? url)
    {
        if (kind == GroqFileKind.Reference)
        {
            throw new UnsupportedFunctionalityException("file parts with provider references");
        }

        if (kind == GroqFileKind.Text)
        {
            throw new UnsupportedFunctionalityException("text file parts");
        }

        if (!IsImage(mediaType))
        {
            throw new UnsupportedFunctionalityException("Non-image file content parts");
        }

        string resolved;
        if (kind == GroqFileKind.Url)
        {
            resolved = url ?? string.Empty;
        }
        else
        {
            var bytes = data ?? Array.Empty<byte>();
            var fullType = ResolveMediaType(mediaType, bytes);
            resolved = "data:" + fullType + ";base64," + System.Convert.ToBase64String(bytes);
        }

        return new JsonObject
        {
            ["type"] = "image_url",
            ["image_url"] = new JsonObject { ["url"] = resolved },
        };
    }

    private static bool IsImage(string mediaType)
    {
        return string.Equals(TopLevel(mediaType), "image", StringComparison.OrdinalIgnoreCase);
    }

    private static string TopLevel(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    private static string ResolveMediaType(string mediaType, byte[] data)
    {
        var wildcard = mediaType.IndexOf('*') >= 0;
        var slash = mediaType.IndexOf('/');
        if (slash > 0 && !wildcard)
        {
            return mediaType;
        }

        if (StartsWith(data, 0x89, 0x50, 0x4E, 0x47))
        {
            return "image/png";
        }

        if (StartsWith(data, 0xFF, 0xD8, 0xFF))
        {
            return "image/jpeg";
        }

        if (data.Length >= 6 && data[0] == (byte)'G' && data[1] == (byte)'I' && data[2] == (byte)'F')
        {
            return "image/gif";
        }

        if (data.Length >= 12
            && data[0] == (byte)'R'
            && data[1] == (byte)'I'
            && data[2] == (byte)'F'
            && data[3] == (byte)'F'
            && data[8] == (byte)'W'
            && data[9] == (byte)'E'
            && data[10] == (byte)'B'
            && data[11] == (byte)'P')
        {
            return "image/webp";
        }

        return "image/png";
    }

    private static bool StartsWith(byte[] data, params byte[] prefix)
    {
        if (data.Length < prefix.Length)
        {
            return false;
        }

        for (var i = 0; i < prefix.Length; i++)
        {
            if (data[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }
}
