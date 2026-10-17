// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Converts V4 prompt messages into Converse <c>system</c> and <c>messages</c> arrays.</summary>
public static class AmazonBedrockMessages
{
    /// <summary>
    /// Groups leading system messages, merges adjacent user and tool messages, and emits assistant tool uses.
    /// A system message after another role throws.
    /// </summary>
    public static void Convert(string modelId, IReadOnlyList<ModelMessage> prompt, out JsonArray? system, out JsonArray messages)
    {
        var isMistral = AmazonBedrockToolIds.IsMistralModel(modelId);
        system = null;
        var built = new JsonArray();
        JsonArray? userContent = null;
        JsonArray? assistantContent = null;
        var documentCount = 0;

        void FlushUser()
        {
            if (userContent == null)
            {
                return;
            }

            built.Add(new JsonObject { ["role"] = "user", ["content"] = userContent });
            userContent = null;
        }

        void FlushAssistant()
        {
            if (assistantContent == null)
            {
                return;
            }

            if (HasContentBlock(assistantContent))
            {
                built.Add(new JsonObject { ["role"] = "assistant", ["content"] = assistantContent });
            }

            assistantContent = null;
        }

        foreach (var message in ToolModelMessage.Flatten(prompt))
        {
            if (message is SystemModelMessage systemMessage)
            {
                if (built.Count > 0 || userContent != null || assistantContent != null)
                {
                    throw new AiSdkException("Multiple system messages that are separated by user/assistant messages are not supported.");
                }

                system ??= new JsonArray();
                system.Add(new JsonObject { ["text"] = systemMessage.Content });
                if (CachePoint(systemMessage.ProviderOptions) is { } cachePoint)
                {
                    system.Add(cachePoint);
                }

                continue;
            }

            if (message is UserModelMessage user)
            {
                FlushAssistant();
                userContent ??= new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        userContent.Add(TextBlock(text));
                    }
                    else if (part is FileContentPart file)
                    {
                        var block = FileBlock(file, ref documentCount);
                        var guardImage = file.Data != null
                            && block.ContainsKey("image")
                            && GuardContent(file.ProviderOptions) is true;
                        userContent.Add(guardImage ? new JsonObject { ["guardContent"] = block } : block);
                    }

                    if (CachePoint(part is TextContentPart t ? t.ProviderOptions : (part as FileContentPart)?.ProviderOptions) is { } partCachePoint)
                    {
                        userContent.Add(partCachePoint);
                    }
                }

                if (CachePoint(user.ProviderOptions) is { } userContentCachePoint)
                {
                    userContent.Add(userContentCachePoint);
                }

                continue;
            }

            if (message is ToolModelMessage tool)
            {
                FlushAssistant();
                userContent ??= new JsonArray();
                userContent.Add(new JsonObject
                {
                    ["toolResult"] = new JsonObject
                    {
                        ["toolUseId"] = AmazonBedrockToolIds.Normalize(tool.ToolCallId, isMistral),
                        ["content"] = new JsonArray { new JsonObject { ["text"] = tool.OutputJson } },
                    },
                });
                continue;
            }

            if (message is AssistantModelMessage assistant)
            {
                FlushUser();
                assistantContent ??= new JsonArray();
                foreach (var part in assistant.Parts)
                {
                    if (part is AssistantTextPart text)
                    {
                        if (!string.IsNullOrEmpty(text.Text))
                        {
                            assistantContent.Add(new JsonObject { ["text"] = text.Text });
                        }
                    }
                    else if (part is AssistantToolCallPart toolCall)
                    {
                        var call = toolCall.ToolCall;
                        assistantContent.Add(new JsonObject
                        {
                            ["toolUse"] = new JsonObject
                            {
                                ["toolUseId"] = AmazonBedrockToolIds.Normalize(call.ToolCallId, isMistral),
                                ["name"] = SanitizeToolName(call.ToolName),
                                ["input"] = ToolInput(call.ArgumentsJson),
                            },
                        });
                    }

                    if (CachePoint(part.ProviderOptions) is { } partCachePoint)
                    {
                        assistantContent.Add(partCachePoint);
                    }
                }

                if (CachePoint(assistant.ProviderOptions) is { } assistantContentCachePoint)
                {
                    assistantContent.Add(assistantContentCachePoint);
                }
            }
        }

        FlushUser();
        FlushAssistant();
        messages = built;
    }

    private static bool HasContentBlock(JsonArray content)
    {
        foreach (var block in content)
        {
            if (block is JsonObject o && !o.ContainsKey("cachePoint"))
            {
                return true;
            }
        }

        return false;
    }

    private static JsonObject TextBlock(TextContentPart part)
    {
        var options = BedrockOptions(part.ProviderOptions);
        if (GuardContent(options) is not true)
        {
            return new JsonObject { ["text"] = part.Text };
        }

        var text = new JsonObject { ["text"] = part.Text };
        if (options!.Value.TryGetProperty("guardContentQualifiers", out var qualifiers) && qualifiers.ValueKind != JsonValueKind.Null)
        {
            if (qualifiers.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Bedrock guardContentQualifiers must be an array of grounding_source, query, or guard_content.");
            }

            var list = new JsonArray();
            foreach (var qualifier in qualifiers.EnumerateArray())
            {
                var value = qualifier.ValueKind == JsonValueKind.String ? qualifier.GetString() : null;
                if (value is not ("grounding_source" or "query" or "guard_content"))
                {
                    throw new ArgumentException("Bedrock guardContentQualifiers must be an array of grounding_source, query, or guard_content.");
                }

                list.Add(value);
            }

            text["qualifiers"] = list;
        }

        return new JsonObject { ["guardContent"] = new JsonObject { ["text"] = text } };
    }

    /// <summary>Reads the options bag from <c>amazonBedrock</c>, then <c>bedrock</c>.</summary>
    private static JsonElement? BedrockOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        foreach (var key in new[] { "amazonBedrock", "bedrock" })
        {
            if (providerOptions != null && providerOptions.TryGetValue(key, out var bag) && bag.ValueKind == JsonValueKind.Object)
            {
                return bag;
            }
        }

        return null;
    }

    private static bool? GuardContent(IReadOnlyDictionary<string, JsonElement>? providerOptions) => GuardContent(BedrockOptions(providerOptions));

    private static bool? GuardContent(JsonElement? options)
    {
        if (options is not { } bag || !bag.TryGetProperty("guardContent", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new ArgumentException("Bedrock guardContent must be a boolean.");
        }

        return value.GetBoolean();
    }

    /// <summary>Reads <c>cachePoint</c> from <c>amazonBedrock</c>, then <c>bedrock</c>, and wraps it as a content block.</summary>
    private static JsonObject? CachePoint(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        foreach (var key in new[] { "amazonBedrock", "bedrock" })
        {
            if (providerOptions != null
                && providerOptions.TryGetValue(key, out var bag)
                && bag.ValueKind == JsonValueKind.Object
                && bag.TryGetProperty("cachePoint", out var cachePoint)
                && cachePoint.ValueKind == JsonValueKind.Object)
            {
                return new JsonObject { ["cachePoint"] = JsonNode.Parse(cachePoint.GetRawText()) };
            }
        }

        return null;
    }

    private static JsonObject FileBlock(FileContentPart file, ref int documentCount)
    {
        var mediaType = file.MediaType ?? string.Empty;
        var topLevel = TopLevel(mediaType);
        if (string.Equals(topLevel, "image", StringComparison.OrdinalIgnoreCase))
        {
            var source = MediaSource(file, "File URL data");
            return new JsonObject
            {
                ["image"] = new JsonObject
                {
                    ["format"] = ImageFormat(mediaType),
                    ["source"] = source,
                },
            };
        }

        if (string.Equals(topLevel, "video", StringComparison.OrdinalIgnoreCase))
        {
            var source = MediaSource(file, "File URL data");
            return new JsonObject
            {
                ["video"] = new JsonObject
                {
                    ["format"] = VideoFormat(mediaType),
                    ["source"] = source,
                },
            };
        }

        var documentSource = MediaSource(file, "File URL data");
        documentCount++;
        return new JsonObject
        {
            ["document"] = new JsonObject
            {
                ["format"] = DocumentFormat(mediaType),
                ["name"] = DocumentName(file.FileName, documentCount),
                ["source"] = documentSource,
            },
        };
    }

    private static JsonObject MediaSource(FileContentPart file, string functionality)
    {
        if (file.Data != null)
        {
            return new JsonObject { ["bytes"] = System.Convert.ToBase64String(file.Data) };
        }

        if (!string.IsNullOrEmpty(file.Url) && file.Url!.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonObject { ["s3Location"] = new JsonObject { ["uri"] = file.Url } };
        }

        throw new AiSdkException(functionality + " is not supported. Bedrock file parts accept bytes or an s3:// URL.");
    }

    private static string ImageFormat(string mediaType)
    {
        switch (mediaType)
        {
            case "image/jpeg":
                return "jpeg";
            case "image/png":
                return "png";
            case "image/gif":
                return "gif";
            case "image/webp":
                return "webp";
            default:
                throw new AiSdkException("Unsupported image mime type: " + mediaType + ", expected one of: image/jpeg, image/png, image/gif, image/webp");
        }
    }

    private static string VideoFormat(string mediaType)
    {
        switch (mediaType)
        {
            case "video/x-matroska":
                return "mkv";
            case "video/quicktime":
                return "mov";
            case "video/mp4":
                return "mp4";
            case "video/webm":
                return "webm";
            case "video/x-flv":
                return "flv";
            case "video/mpeg":
                return "mpeg";
            case "video/mpg":
                return "mpg";
            case "video/wmv":
            case "video/x-ms-wmv":
                return "wmv";
            case "video/3gpp":
                return "three_gp";
            default:
                throw new AiSdkException("Unsupported video mime type: " + mediaType);
        }
    }

    private static string DocumentFormat(string mediaType)
    {
        switch (mediaType)
        {
            case "application/pdf":
                return "pdf";
            case "text/csv":
                return "csv";
            case "application/msword":
                return "doc";
            case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                return "docx";
            case "application/vnd.ms-excel":
                return "xls";
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                return "xlsx";
            case "text/html":
                return "html";
            case "text/plain":
                return "txt";
            case "text/markdown":
                return "md";
            default:
                throw new AiSdkException("Unsupported document mime type: " + mediaType);
        }
    }

    private static string DocumentName(string? fileName, int documentCount)
    {
        var sanitized = SanitizeDocumentName(fileName);
        return sanitized.Length == 0 ? "document-" + documentCount.ToString() : sanitized;
    }

    private static string SanitizeDocumentName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var name = fileName!;
        var slash = name.LastIndexOfAny(new[] { '/', '\\' });
        if (slash >= 0 && slash < name.Length - 1)
        {
            name = name.Substring(slash + 1);
        }

        var dot = name.LastIndexOf('.');
        if (dot > 0)
        {
            name = name.Substring(0, dot);
        }

        var builder = new StringBuilder(name.Length);
        var pendingSpace = false;
        foreach (var character in name)
        {
            if (character == ' ' || character == '\t' || character == '\n' || character == '\r')
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            var allowed = (character >= 'a' && character <= 'z')
                || (character >= 'A' && character <= 'Z')
                || (character >= '0' && character <= '9')
                || character == '(' || character == ')' || character == '[' || character == ']' || character == '-';
            if (!allowed)
            {
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(character);
            if (builder.Length >= 200)
            {
                break;
            }
        }

        return builder.ToString().Trim();
    }

    private static string SanitizeToolName(string toolName)
    {
        if (string.IsNullOrEmpty(toolName))
        {
            return "_";
        }

        var builder = new StringBuilder(toolName.Length);
        foreach (var character in toolName)
        {
            if ((character >= 'a' && character <= 'z')
                || (character >= 'A' && character <= 'Z')
                || (character >= '0' && character <= '9')
                || character == '_' || character == '-')
            {
                builder.Append(character);
            }
        }

        return builder.Length == 0 ? "_" : builder.ToString();
    }

    private static JsonNode ToolInput(string argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
        {
            return new JsonObject();
        }

        try
        {
            var node = JsonNode.Parse(argumentsJson);
            if (node is JsonObject)
            {
                return node;
            }

            return new JsonObject { ["rawInvalidInput"] = node };
        }
        catch (System.Text.Json.JsonException)
        {
            return new JsonObject { ["rawInvalidInput"] = argumentsJson };
        }
    }

    private static string TopLevel(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }
}
