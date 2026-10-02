// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Perplexity;

/// <summary>
/// A Sonar chat file part. Use <see cref="InlineData"/> for an already-encoded string payload,
/// or set <see cref="IsReference"/> when the file is a provider reference.
/// </summary>
public sealed class PerplexityFilePart : UserContentPart
{
    /// <summary>Creates a file part.</summary>
    public PerplexityFilePart(string mediaType, string? url, byte[]? data, string? inlineData, string? fileName, bool isReference = false)
        : base("file")
    {
        MediaType = mediaType ?? string.Empty;
        Url = url;
        Data = data;
        InlineData = inlineData;
        FileName = fileName;
        IsReference = isReference;
    }

    /// <summary>IANA media type, a top-level type, or a wildcard such as <c>image/*</c>.</summary>
    public string MediaType { get; }

    /// <summary>Remote URL.</summary>
    public string? Url { get; }

    /// <summary>Inline bytes. Encoded as base64 when sent.</summary>
    public byte[]? Data { get; }

    /// <summary>String payload sent as-is. Used for already-encoded file data.</summary>
    public string? InlineData { get; }

    /// <summary>Optional file name.</summary>
    public string? FileName { get; }

    /// <summary>True when the part is a provider file reference. Sonar chat rejects those parts.</summary>
    public bool IsReference { get; }
}

/// <summary>Converts prompt messages into Sonar chat-completions messages.</summary>
public static class PerplexityMessages
{
    /// <summary>Converts <paramref name="prompt"/> into the chat <c>messages</c> array.</summary>
    public static JsonArray Convert(IReadOnlyList<ModelMessage> prompt)
    {
        if (prompt is null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        var messages = new JsonArray();
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
                    messages.Add(new JsonObject
                    {
                        ["role"] = "assistant",
                        ["content"] = assistant.Text ?? string.Empty,
                    });
                    break;
                case ToolModelMessage:
                    throw new AiSdkException("Unsupported functionality: Tool messages");
                default:
                    throw new AiSdkException("Unsupported role: " + (message is null ? "null" : message.Role));
            }
        }

        return messages;
    }

    private static JsonObject ConvertUser(UserModelMessage user)
    {
        var parts = new List<JsonNode>();
        var multipart = false;
        for (var index = 0; index < user.Content.Count; index++)
        {
            var part = user.Content[index];
            if (part is TextContentPart text)
            {
                parts.Add(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = text.Text,
                });
                continue;
            }

            var file = ReadFile(part);
            var top = TopLevel(file.MediaType);
            if (top == "image" || top == "application")
            {
                multipart = true;
            }

            parts.Add(ConvertFile(file, index));
        }

        if (!multipart)
        {
            var text = string.Empty;
            foreach (var part in parts)
            {
                if (part is JsonObject obj && obj["type"]?.GetValue<string>() == "text")
                {
                    text += obj["text"]?.GetValue<string>() ?? string.Empty;
                }
            }

            return new JsonObject
            {
                ["role"] = "user",
                ["content"] = text,
            };
        }

        var content = new JsonArray();
        foreach (var part in parts)
        {
            content.Add(part);
        }

        return new JsonObject
        {
            ["role"] = "user",
            ["content"] = content,
        };
    }

    private static FileInput ReadFile(UserContentPart part)
    {
        switch (part)
        {
            case PerplexityFilePart file:
                return new FileInput(file.MediaType, file.Url, file.Data, file.InlineData, file.FileName, file.IsReference);
            case FileContentPart file:
                return new FileInput(file.MediaType, file.Url, file.Data, null, file.FileName, false);
            default:
                throw new AiSdkException("Unsupported functionality: user content part");
        }
    }

    private static JsonObject ConvertFile(FileInput file, int index)
    {
        if (file.IsReference)
        {
            throw new AiSdkException("Unsupported functionality: file parts with provider references");
        }

        var top = TopLevel(file.MediaType);
        var url = IsUrl(file);
        if (top == "application")
        {
            var full = ResolveFull(file);
            if (!string.Equals(full, "application/pdf", StringComparison.Ordinal))
            {
                throw new AiSdkException("Unsupported functionality: file part media type " + full);
            }

            var item = new JsonObject
            {
                ["type"] = "file_url",
                ["file_url"] = new JsonObject
                {
                    ["url"] = url ? file.Url! : Payload(file),
                },
            };
            var name = file.FileName;
            if (!url && string.IsNullOrEmpty(name))
            {
                name = "document-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".pdf";
            }

            if (!string.IsNullOrEmpty(name))
            {
                item["file_name"] = name;
            }

            return item;
        }

        if (top == "image")
        {
            if (url)
            {
                return new JsonObject
                {
                    ["type"] = "image_url",
                    ["image_url"] = new JsonObject { ["url"] = file.Url! },
                };
            }

            var full = ResolveFull(file);
            return new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JsonObject
                {
                    ["url"] = "data:" + full + ";base64," + Payload(file),
                },
            };
        }

        throw new AiSdkException("Unsupported functionality: file part media type " + file.MediaType);
    }

    private static bool IsUrl(FileInput file)
    {
        return !string.IsNullOrEmpty(file.Url) && file.Data is null && file.InlineData is null;
    }

    private static string Payload(FileInput file)
    {
        if (file.InlineData != null)
        {
            return file.InlineData;
        }

        if (file.Data != null)
        {
            return System.Convert.ToBase64String(file.Data);
        }

        throw new AiSdkException("Unsupported functionality: file part media type " + file.MediaType);
    }

    private static string ResolveFull(FileInput file)
    {
        if (IsFullMediaType(file.MediaType))
        {
            return file.MediaType;
        }

        if (file.Data != null || file.InlineData != null)
        {
            var detected = Detect(TopLevel(file.MediaType), file.Data, file.InlineData);
            if (detected != null)
            {
                return detected;
            }

            throw new AiSdkException("Unsupported functionality: file of media type \"" + file.MediaType + "\" must specify subtype since it could not be auto-detected.");
        }

        throw new AiSdkException("Unsupported functionality: file of media type \"" + file.MediaType + "\" must specify subtype since it is not passed as inline bytes.");
    }

    private static string? Detect(string top, byte[]? bytes, string? inline)
    {
        byte[] data;
        if (bytes != null)
        {
            data = bytes;
        }
        else if (!string.IsNullOrEmpty(inline))
        {
            try
            {
                data = System.Convert.FromBase64String(inline!);
            }
            catch (FormatException)
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        if (top == "image")
        {
            if (Starts(data, new byte[] { 0x89, 0x50, 0x4e, 0x47 }))
            {
                return "image/png";
            }

            if (Starts(data, new byte[] { 0xff, 0xd8 }))
            {
                return "image/jpeg";
            }

            if (Starts(data, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 })
                || Starts(data, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }))
            {
                return "image/gif";
            }

            return null;
        }

        if (top == "application" && Starts(data, new byte[] { 0x25, 0x50, 0x44, 0x46 }))
        {
            return "application/pdf";
        }

        return null;
    }

    private static bool Starts(byte[] data, byte[] prefix)
    {
        if (data.Length < prefix.Length)
        {
            return false;
        }

        for (var index = 0; index < prefix.Length; index++)
        {
            if (data[index] != prefix[index])
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFullMediaType(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        if (slash < 0)
        {
            return false;
        }

        var subtype = mediaType.Substring(slash + 1);
        return subtype.Length > 0 && subtype != "*";
    }

    private static string TopLevel(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    private readonly struct FileInput
    {
        public FileInput(string mediaType, string? url, byte[]? data, string? inlineData, string? fileName, bool isReference)
        {
            MediaType = mediaType ?? string.Empty;
            Url = url;
            Data = data;
            InlineData = inlineData;
            FileName = fileName;
            IsReference = isReference;
        }

        public string MediaType { get; }

        public string? Url { get; }

        public byte[]? Data { get; }

        public string? InlineData { get; }

        public string? FileName { get; }

        public bool IsReference { get; }
    }
}
