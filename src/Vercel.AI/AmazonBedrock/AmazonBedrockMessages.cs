// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>System blocks and Converse messages produced from a prompt.</summary>
public sealed class AmazonBedrockMessageConversion
{
    /// <summary>Creates a conversion result.</summary>
    public AmazonBedrockMessageConversion(JsonArray system, JsonArray messages)
    {
        System = system ?? new JsonArray();
        Messages = messages ?? new JsonArray();
    }

    /// <summary>System content blocks.</summary>
    public JsonArray System { get; }

    /// <summary>Converse messages.</summary>
    public JsonArray Messages { get; }

    /// <summary>Returns <c>{ system, messages }</c>.</summary>
    public JsonObject ToJson()
    {
        return new JsonObject
        {
            ["system"] = System.DeepClone(),
            ["messages"] = Messages.DeepClone(),
        };
    }
}

/// <summary>Converts SDK prompts into Amazon Bedrock Converse messages.</summary>
public static class AmazonBedrockMessages
{
    private static readonly Dictionary<string, string> ImageFormats = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["image/jpeg"] = "jpeg",
        ["image/png"] = "png",
        ["image/gif"] = "gif",
        ["image/webp"] = "webp",
    };

    private static readonly Dictionary<string, string> DocumentFormats = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["application/pdf"] = "pdf",
        ["text/csv"] = "csv",
        ["application/msword"] = "doc",
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = "docx",
        ["application/vnd.ms-excel"] = "xls",
        ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = "xlsx",
        ["text/html"] = "html",
        ["text/plain"] = "txt",
        ["text/markdown"] = "md",
    };

    private static readonly Dictionary<string, string> VideoFormats = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["video/x-matroska"] = "mkv",
        ["video/quicktime"] = "mov",
        ["video/mp4"] = "mp4",
        ["video/webm"] = "webm",
        ["video/x-flv"] = "flv",
        ["video/mpeg"] = "mpeg",
        ["video/mpg"] = "mpg",
        ["video/wmv"] = "wmv",
        ["video/x-ms-wmv"] = "wmv",
        ["video/3gpp"] = "three_gp",
    };

    /// <summary>Converts <paramref name="prompt"/>.</summary>
    public static AmazonBedrockMessageConversion Convert(IReadOnlyList<AmazonBedrockPromptMessage> prompt, bool isMistral = false)
    {
        prompt = prompt ?? Array.Empty<AmazonBedrockPromptMessage>();
        var blocks = Group(prompt);
        var system = new JsonArray();
        var messages = new JsonArray();
        var documentCounter = 0;

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var isLastBlock = i == blocks.Count - 1;
            if (block.Role == "system")
            {
                if (messages.Count > 0)
                {
                    throw new AmazonBedrockUnsupportedException("Multiple system messages that are separated by user/assistant messages");
                }

                foreach (var message in block.Messages)
                {
                    system.Add(new JsonObject { ["text"] = message.SystemText ?? string.Empty });
                    AddCachePoint(system, message.ProviderOptions);
                }

                continue;
            }

            if (block.Role == "user")
            {
                var content = new JsonArray();
                foreach (var message in block.Messages)
                {
                    if (message.Role == "user")
                    {
                        foreach (var part in message.Parts)
                        {
                            AppendUserPart(content, part, ref documentCounter);
                            AddCachePoint(content, part.ProviderOptions);
                        }
                    }
                    else if (message.Role == "tool")
                    {
                        foreach (var part in message.Parts)
                        {
                            if (part.Type == "tool-approval-response")
                            {
                                continue;
                            }

                            if (part is AmazonBedrockToolResultPart result)
                            {
                                content.Add(ToolResultBlock(result, isMistral, ref documentCounter));
                                AddCachePoint(content, part.ProviderOptions);
                            }
                        }
                    }

                    AddCachePoint(content, message.ProviderOptions);
                }

                AppendToUserMessage(messages, content);
                continue;
            }

            ConvertAssistantBlock(block, isLastBlock, isMistral, messages, ref documentCounter);
        }

        return new AmazonBedrockMessageConversion(system, messages);
    }

    /// <summary>Maps SDK messages onto the Bedrock prompt model.</summary>
    public static IReadOnlyList<AmazonBedrockPromptMessage> FromModelMessages(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new List<AmazonBedrockPromptMessage>();
        if (prompt == null)
        {
            return messages;
        }

        foreach (var message in prompt)
        {
            if (message is SystemModelMessage system)
            {
                messages.Add(AmazonBedrockPromptMessage.System(system.Content));
            }
            else if (message is UserModelMessage user)
            {
                var parts = new List<AmazonBedrockPromptPart>();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        parts.Add(new AmazonBedrockTextPart(text.Text));
                    }
                    else if (part is FileContentPart file)
                    {
                        var filePart = new AmazonBedrockFilePart(file.MediaType)
                        {
                            FileName = file.FileName,
                            Bytes = file.Data,
                            Url = file.Url,
                        };
                        parts.Add(filePart);
                    }
                }

                messages.Add(AmazonBedrockPromptMessage.User(parts));
            }
            else if (message is AssistantModelMessage assistant)
            {
                var parts = new List<AmazonBedrockPromptPart>();
                if (assistant.Reasoning != null)
                {
                    parts.Add(new AmazonBedrockReasoningPart(assistant.Reasoning));
                }

                if (assistant.Text != null)
                {
                    parts.Add(new AmazonBedrockTextPart(assistant.Text));
                }

                foreach (var call in assistant.ToolCalls)
                {
                    parts.Add(new AmazonBedrockToolCallPart(call.ToolCallId, call.ToolName, ParseToolInput(call.ArgumentsJson)));
                }

                messages.Add(AmazonBedrockPromptMessage.Assistant(parts));
            }
            else if (message is ToolModelMessage tool)
            {
                var result = new AmazonBedrockToolResultPart(tool.ToolCallId, tool.ToolName)
                {
                    OutputType = tool.IsError ? "error-text" : "text",
                    OutputValue = JsonValue.Create(tool.OutputJson ?? string.Empty),
                    ProviderOptions = tool.ProviderMetadata,
                };
                messages.Add(AmazonBedrockPromptMessage.Tool(new AmazonBedrockPromptPart[] { result }, tool.ProviderMetadata));
            }
        }

        return messages;
    }

    private static void ConvertAssistantBlock(PromptBlock block, bool isLastBlock, bool isMistral, JsonArray messages, ref int documentCounter)
    {
        var assistantContent = new JsonArray();
        var toolResultContent = new JsonArray();
        for (var j = 0; j < block.Messages.Count; j++)
        {
            var message = block.Messages[j];
            var isLastMessage = j == block.Messages.Count - 1;
            var hasReasoning = false;
            foreach (var part in message.Parts)
            {
                if (part.Type == "reasoning")
                {
                    hasReasoning = true;
                    break;
                }
            }

            for (var k = 0; k < message.Parts.Count; k++)
            {
                var part = message.Parts[k];
                var isLastContentPart = k == message.Parts.Count - 1;
                if (part.Type != "tool-result")
                {
                    FlushToolResults(messages, ref toolResultContent);
                }

                switch (part)
                {
                    case AmazonBedrockTextPart text:
                        if (text.Text.Trim().Length == 0 && !hasReasoning)
                        {
                            break;
                        }

                        var value = isLastBlock && isLastMessage && isLastContentPart ? text.Text.Trim() : text.Text;
                        assistantContent.Add(new JsonObject { ["text"] = value });
                        break;
                    case AmazonBedrockReasoningPart reasoning:
                        AppendReasoning(assistantContent, reasoning);
                        break;
                    case AmazonBedrockToolCallPart call:
                        assistantContent.Add(new JsonObject
                        {
                            ["toolUse"] = new JsonObject
                            {
                                ["toolUseId"] = AmazonBedrockToolCallId.Normalize(call.ToolCallId, isMistral),
                                ["name"] = SanitizeToolName(call.ToolName),
                                ["input"] = ToBedrockToolInput(call.Input),
                            },
                        });
                        break;
                    case AmazonBedrockToolResultPart result:
                        FlushAssistant(messages, ref assistantContent);
                        toolResultContent.Add(ToolResultBlock(result, isMistral, ref documentCounter));
                        break;
                }

                AddCachePoint(part.Type == "tool-result" ? toolResultContent : assistantContent, part.ProviderOptions);
            }

            var last = message.Parts.Count == 0 ? null : message.Parts[message.Parts.Count - 1];
            AddCachePoint(last != null && last.Type == "tool-result" ? toolResultContent : assistantContent, message.ProviderOptions);
        }

        FlushToolResults(messages, ref toolResultContent);
        FlushAssistant(messages, ref assistantContent);
    }

    private static void AppendUserPart(JsonArray content, AmazonBedrockPromptPart part, ref int documentCounter)
    {
        switch (part)
        {
            case AmazonBedrockTextPart text:
                var textOptions = ReadProviderObject(part.ProviderOptions, "guardContent", "guardContentQualifiers");
                if (textOptions.GuardContent)
                {
                    var guardText = new JsonObject { ["text"] = text.Text };
                    if (textOptions.Qualifiers != null)
                    {
                        guardText["qualifiers"] = textOptions.Qualifiers;
                    }

                    content.Add(new JsonObject
                    {
                        ["guardContent"] = new JsonObject { ["text"] = guardText },
                    });
                }
                else
                {
                    content.Add(new JsonObject { ["text"] = text.Text });
                }

                break;
            case AmazonBedrockFilePart file:
                content.Add(ConvertFile(file, ref documentCounter));
                break;
            default:
                throw new AmazonBedrockUnsupportedException("user content part type: " + part.Type);
        }
    }

    private static JsonNode ConvertFile(AmazonBedrockFilePart file, ref int documentCounter)
    {
        if (file.IsReference)
        {
            throw new AmazonBedrockUnsupportedException("file parts with provider references");
        }

        if (file.Text != null)
        {
            var mediaType = IsFullMediaType(file.MediaType) ? file.MediaType : "text/plain";
            return DocumentBlock(mediaType, DocumentName(file.FileName, ref documentCounter), System.Convert.ToBase64String(Encoding.UTF8.GetBytes(file.Text)), file.ProviderOptions);
        }

        if (!string.IsNullOrEmpty(file.Url))
        {
            if (file.Url!.IndexOf("s3:", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new AmazonBedrockUnsupportedException("File URL data");
            }

            var mediaType = ResolveMediaType(file);
            var top = TopLevel(mediaType);
            var source = new JsonObject
            {
                ["s3Location"] = new JsonObject { ["uri"] = file.Url },
            };
            if (top == "image")
            {
                return new JsonObject
                {
                    ["image"] = new JsonObject
                    {
                        ["format"] = ImageFormat(mediaType),
                        ["source"] = source,
                    },
                };
            }

            if (top == "video")
            {
                return new JsonObject
                {
                    ["video"] = new JsonObject
                    {
                        ["format"] = VideoFormat(mediaType),
                        ["source"] = source,
                    },
                };
            }

            throw new AmazonBedrockUnsupportedException("File URL data");
        }

        if (file.Bytes == null && file.Base64 == null)
        {
            throw new AmazonBedrockUnsupportedException("File URL data");
        }

        var resolved = ResolveMediaType(file);
        var encoded = file.Bytes != null ? System.Convert.ToBase64String(file.Bytes) : file.Base64;
        var level = TopLevel(resolved);
        if (level == "image")
        {
            var image = new JsonObject
            {
                ["image"] = new JsonObject
                {
                    ["format"] = ImageFormat(resolved),
                    ["source"] = new JsonObject { ["bytes"] = encoded },
                },
            };
            var guard = ReadProviderObject(file.ProviderOptions, "guardContent", null);
            if (guard.GuardContent)
            {
                return new JsonObject { ["guardContent"] = image };
            }

            return image;
        }

        if (level == "video")
        {
            return new JsonObject
            {
                ["video"] = new JsonObject
                {
                    ["format"] = VideoFormat(resolved),
                    ["source"] = new JsonObject { ["bytes"] = encoded },
                },
            };
        }

        return DocumentBlock(resolved, DocumentName(file.FileName, ref documentCounter), encoded ?? string.Empty, file.ProviderOptions);
    }

    private static JsonObject DocumentBlock(string mediaType, string name, string base64, JsonElement? providerOptions)
    {
        var document = new JsonObject
        {
            ["format"] = DocumentFormat(mediaType),
            ["name"] = name,
            ["source"] = new JsonObject { ["bytes"] = base64 },
        };
        if (CitationsEnabled(providerOptions))
        {
            document["citations"] = new JsonObject { ["enabled"] = true };
        }

        return new JsonObject { ["document"] = document };
    }

    private static JsonObject ToolResultBlock(AmazonBedrockToolResultPart result, bool isMistral, ref int documentCounter)
    {
        return new JsonObject
        {
            ["toolResult"] = new JsonObject
            {
                ["toolUseId"] = AmazonBedrockToolCallId.Normalize(result.ToolCallId, isMistral),
                ["content"] = ConvertToolResultOutput(result, ref documentCounter),
            },
        };
    }

    private static JsonArray ConvertToolResultOutput(AmazonBedrockToolResultPart result, ref int documentCounter)
    {
        var content = new JsonArray();
        switch (result.OutputType)
        {
            case "content":
                foreach (var part in result.Content ?? Array.Empty<AmazonBedrockPromptPart>())
                {
                    if (part is AmazonBedrockTextPart text)
                    {
                        content.Add(new JsonObject { ["text"] = text.Text });
                    }
                    else if (part is AmazonBedrockFilePart file)
                    {
                        if (file.IsReference || (string.IsNullOrEmpty(file.Url) && file.Bytes == null && file.Base64 == null && file.Text == null))
                        {
                            throw new AmazonBedrockUnsupportedException("tool result file data of type \"" + (file.IsReference ? "reference" : "url") + "\"");
                        }

                        if (!string.IsNullOrEmpty(file.Url) && file.Url!.IndexOf("s3:", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw new AmazonBedrockUnsupportedException("tool result file data of type \"url\"");
                        }

                        var node = ConvertFile(file, ref documentCounter);
                        if (node is JsonObject obj && obj["document"] != null && file.Bytes == null && file.Text == null && string.IsNullOrEmpty(file.Url) && file.Base64 == null)
                        {
                            throw new AmazonBedrockUnsupportedException("tool result file data of type \"url\"");
                        }

                        content.Add(node);
                    }
                    else
                    {
                        throw new AmazonBedrockUnsupportedException("unsupported tool content part type: " + part.Type);
                    }
                }

                break;
            case "text":
            case "error-text":
                content.Add(new JsonObject { ["text"] = result.OutputValue == null ? string.Empty : JsonScalar(result.OutputValue) });
                break;
            case "execution-denied":
                content.Add(new JsonObject { ["text"] = result.DenialReason ?? "Tool call execution denied." });
                break;
            default:
                content.Add(new JsonObject { ["text"] = result.OutputValue == null ? "null" : result.OutputValue.ToJsonString() });
                break;
        }

        return content;
    }

    private static string JsonScalar(JsonNode node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text ?? string.Empty;
        }

        return node.ToJsonString();
    }

    private static void AppendReasoning(JsonArray content, AmazonBedrockReasoningPart reasoning)
    {
        var metadata = ReadReasoningMetadata(reasoning.ProviderOptions);
        if (metadata.Signature != null)
        {
            content.Add(new JsonObject
            {
                ["reasoningContent"] = new JsonObject
                {
                    ["reasoningText"] = new JsonObject
                    {
                        ["text"] = reasoning.Text,
                        ["signature"] = metadata.Signature,
                    },
                },
            });
        }
        else if (metadata.RedactedContent != null)
        {
            content.Add(new JsonObject
            {
                ["reasoningContent"] = new JsonObject
                {
                    ["redactedContent"] = metadata.RedactedContent,
                },
            });
        }
        else if (metadata.RedactedData != null)
        {
            content.Add(new JsonObject
            {
                ["reasoningContent"] = new JsonObject
                {
                    ["redactedReasoning"] = new JsonObject { ["data"] = metadata.RedactedData },
                },
            });
        }
    }

    private static void FlushAssistant(JsonArray messages, ref JsonArray assistantContent)
    {
        var hasReal = false;
        foreach (var block in assistantContent)
        {
            if (block is JsonObject obj && obj["cachePoint"] == null)
            {
                hasReal = true;
                break;
            }
        }

        if (hasReal)
        {
            messages.Add(new JsonObject
            {
                ["role"] = "assistant",
                ["content"] = assistantContent,
            });
        }

        assistantContent = new JsonArray();
    }

    private static void FlushToolResults(JsonArray messages, ref JsonArray toolResultContent)
    {
        if (toolResultContent.Count > 0)
        {
            AppendToUserMessage(messages, toolResultContent);
            toolResultContent = new JsonArray();
        }
    }

    private static void AppendToUserMessage(JsonArray messages, JsonArray content)
    {
        if (messages.Count > 0 && messages[messages.Count - 1] is JsonObject last && last["role"]?.GetValue<string>() == "user" && last["content"] is JsonArray lastContent && ContainsToolResult(lastContent))
        {
            foreach (var block in content)
            {
                lastContent.Add(block == null ? null : block.DeepClone());
            }

            return;
        }

        messages.Add(new JsonObject
        {
            ["role"] = "user",
            ["content"] = content,
        });
    }

    private static bool ContainsToolResult(JsonArray content)
    {
        foreach (var block in content)
        {
            if (block is JsonObject obj && obj["toolResult"] != null)
            {
                return true;
            }
        }

        return false;
    }

    private static JsonNode ToBedrockToolInput(JsonNode? input)
    {
        if (input is JsonObject obj)
        {
            return obj.DeepClone();
        }

        return new JsonObject
        {
            ["rawInvalidInput"] = input == null ? JsonNode.Parse("null") : input.DeepClone(),
        };
    }

    private static JsonNode? ParseToolInput(string argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(argumentsJson);
        }
        catch (JsonException)
        {
            return JsonValue.Create(argumentsJson);
        }
    }

    private static string SanitizeToolName(string toolName)
    {
        if (string.IsNullOrEmpty(toolName))
        {
            return "_";
        }

        var sanitized = Regex.Replace(toolName, "[^a-zA-Z0-9_-]", string.Empty);
        return sanitized.Length == 0 ? "_" : sanitized;
    }

    private static string DocumentName(string? filename, ref int documentCounter)
    {
        var sanitized = string.IsNullOrEmpty(filename) ? string.Empty : SanitizeDocumentName(filename!);
        if (sanitized.Length == 0)
        {
            documentCounter++;
            return "document-" + documentCounter.ToString();
        }

        return sanitized;
    }

    private static string SanitizeDocumentName(string filename)
    {
        var stripped = StripFileExtension(filename);
        stripped = Regex.Replace(stripped, "\\s+", " ");
        stripped = Regex.Replace(stripped, "[^a-zA-Z0-9 ()\\[\\]-]", string.Empty);
        stripped = stripped.Trim();
        if (stripped.Length > 200)
        {
            stripped = stripped.Substring(0, 200).Trim();
        }

        return stripped;
    }

    private static string StripFileExtension(string filename)
    {
        var dot = filename.IndexOf('.');
        return dot < 0 ? filename : filename.Substring(0, dot);
    }

    private static string ResolveMediaType(AmazonBedrockFilePart file)
    {
        if (IsFullMediaType(file.MediaType))
        {
            return file.MediaType;
        }

        var detected = DetectMediaType(file.Bytes ?? DecodeBase64OrUtf8(file.Base64 ?? string.Empty));
        if (detected == null)
        {
            throw new AmazonBedrockUnsupportedException("file mime type: " + file.MediaType, "Unsupported file mime type: " + file.MediaType);
        }

        var top = TopLevel(file.MediaType);
        if (top == "image" || top == "video" || top == "application" || string.IsNullOrEmpty(top))
        {
            if (top.Length > 0 && detected.IndexOf(top + "/", StringComparison.Ordinal) != 0 && top != "application")
            {
                throw new AmazonBedrockUnsupportedException("file mime type: " + file.MediaType);
            }

            return detected;
        }

        return detected;
    }

    private static string? DetectMediaType(byte[] bytes)
    {
        if (StartsWith(bytes, new byte[] { 0x89, 0x50, 0x4E, 0x47 }))
        {
            return "image/png";
        }

        if (StartsWith(bytes, new byte[] { 0xFF, 0xD8, 0xFF }))
        {
            return "image/jpeg";
        }

        if (StartsWith(bytes, Encoding.ASCII.GetBytes("GIF8")))
        {
            return "image/gif";
        }

        if (bytes.Length >= 12 && StartsWith(bytes, Encoding.ASCII.GetBytes("RIFF")) && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P')
        {
            return "image/webp";
        }

        if (StartsWith(bytes, Encoding.ASCII.GetBytes("%PDF")))
        {
            return "application/pdf";
        }

        if (bytes.Length >= 8 && bytes[4] == (byte)'f' && bytes[5] == (byte)'t' && bytes[6] == (byte)'y' && bytes[7] == (byte)'p')
        {
            return "video/mp4";
        }

        return null;
    }

    private static bool StartsWith(byte[] bytes, byte[] prefix)
    {
        if (bytes.Length < prefix.Length)
        {
            return false;
        }

        for (var i = 0; i < prefix.Length; i++)
        {
            if (bytes[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }

    private static byte[] DecodeBase64OrUtf8(string value)
    {
        try
        {
            return System.Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return Encoding.UTF8.GetBytes(value);
        }
    }

    private static string ImageFormat(string mediaType)
    {
        if (ImageFormats.TryGetValue(mediaType, out var format))
        {
            return format;
        }

        throw new AmazonBedrockUnsupportedException(
            "image mime type: " + mediaType,
            "Unsupported image mime type: " + mediaType + ", expected one of: image/jpeg, image/png, image/gif, image/webp");
    }

    private static string DocumentFormat(string mediaType)
    {
        if (DocumentFormats.TryGetValue(mediaType, out var format))
        {
            return format;
        }

        throw new AmazonBedrockUnsupportedException(
            "file mime type: " + mediaType,
            "Unsupported file mime type: " + mediaType + ", expected one of: application/pdf, text/csv, application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, text/html, text/plain, text/markdown");
    }

    private static string VideoFormat(string mediaType)
    {
        if (VideoFormats.TryGetValue(mediaType, out var format))
        {
            return format;
        }

        throw new AmazonBedrockUnsupportedException(
            "video mime type: " + mediaType,
            "Unsupported video mime type: " + mediaType + ", expected one of: video/x-matroska, video/quicktime, video/mp4, video/webm, video/x-flv, video/mpeg, video/mpg, video/wmv, video/x-ms-wmv, video/3gpp");
    }

    private static bool IsFullMediaType(string mediaType)
    {
        return mediaType != null && mediaType.IndexOf('/') >= 0;
    }

    private static string TopLevel(string mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return string.Empty;
        }

        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    private static bool CitationsEnabled(JsonElement? providerOptions)
    {
        var options = ReadNamedObject(providerOptions);
        if (options == null)
        {
            return false;
        }

        if (!options.Value.TryGetProperty("citations", out var citations) || citations.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return citations.TryGetProperty("enabled", out var enabled) && enabled.ValueKind == JsonValueKind.True;
    }

    private static void AddCachePoint(JsonArray content, JsonElement? providerOptions)
    {
        var options = ReadNamedObject(providerOptions);
        if (options == null || !options.Value.TryGetProperty("cachePoint", out var cachePoint) || cachePoint.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        content.Add(new JsonObject
        {
            ["cachePoint"] = JsonNode.Parse(cachePoint.GetRawText()),
        });
    }

    private static GuardRead ReadProviderObject(JsonElement? providerOptions, string guardName, string? qualifierName)
    {
        var options = ReadNamedObject(providerOptions);
        var read = new GuardRead();
        if (options == null)
        {
            return read;
        }

        if (options.Value.TryGetProperty(guardName, out var guard) && guard.ValueKind == JsonValueKind.True)
        {
            read.GuardContent = true;
        }

        if (qualifierName != null && options.Value.TryGetProperty(qualifierName, out var qualifiers) && qualifiers.ValueKind == JsonValueKind.Array)
        {
            read.Qualifiers = (JsonArray)JsonNode.Parse(qualifiers.GetRawText())!;
        }

        return read;
    }

    private static ReasoningRead ReadReasoningMetadata(JsonElement? providerOptions)
    {
        var options = ReadNamedObject(providerOptions);
        var read = new ReasoningRead();
        if (options == null)
        {
            return read;
        }

        if (options.Value.TryGetProperty("signature", out var signature) && signature.ValueKind == JsonValueKind.String)
        {
            read.Signature = signature.GetString();
        }

        if (options.Value.TryGetProperty("redactedData", out var data) && data.ValueKind == JsonValueKind.String)
        {
            read.RedactedData = data.GetString();
        }

        if (options.Value.TryGetProperty("redactedContent", out var content) && content.ValueKind == JsonValueKind.String)
        {
            read.RedactedContent = content.GetString();
        }

        return read;
    }

    private static JsonElement? ReadNamedObject(JsonElement? providerOptions)
    {
        if (providerOptions == null || providerOptions.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var root = providerOptions.Value;
        if (root.TryGetProperty("amazonBedrock", out var primary) && primary.ValueKind == JsonValueKind.Object)
        {
            return primary;
        }

        if (root.TryGetProperty("bedrock", out var legacy) && legacy.ValueKind == JsonValueKind.Object)
        {
            return legacy;
        }

        return null;
    }

    private static List<PromptBlock> Group(IReadOnlyList<AmazonBedrockPromptMessage> prompt)
    {
        var blocks = new List<PromptBlock>();
        PromptBlock? current = null;
        foreach (var message in prompt)
        {
            var role = message.Role == "tool" ? "user" : message.Role;
            if (role != "system" && role != "user" && role != "assistant")
            {
                throw new InvalidOperationException("Unsupported role: " + message.Role);
            }

            if (current == null || current.Role != role)
            {
                current = new PromptBlock(role);
                blocks.Add(current);
            }

            current.Messages.Add(message);
        }

        return blocks;
    }

    private sealed class PromptBlock
    {
        public PromptBlock(string role)
        {
            Role = role;
        }

        public string Role { get; }

        public List<AmazonBedrockPromptMessage> Messages { get; } = new List<AmazonBedrockPromptMessage>();
    }

    private struct GuardRead
    {
        public bool GuardContent;

        public JsonArray? Qualifiers;
    }

    private struct ReasoningRead
    {
        public string? Signature;

        public string? RedactedData;

        public string? RedactedContent;
    }
}
