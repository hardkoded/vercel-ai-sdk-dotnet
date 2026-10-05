// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Shared OpenAI-compatible chat request and response helpers.</summary>
public static class OpenAICompatibleChat
{
    /// <summary>Provider id for a model kind, such as <c>groq.chat</c>.</summary>
    public static string Qualify(string? providerName, string kind)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            return kind ?? string.Empty;
        }

        return providerName + "." + kind;
    }

    /// <summary>First segment of a provider id. <c>anthropic.beta</c> is <c>anthropic</c>.</summary>
    public static string BaseProviderName(string? providerId)
    {
        if (string.IsNullOrEmpty(providerId))
        {
            return string.Empty;
        }

        var dot = providerId!.IndexOf('.');
        return dot < 0 ? providerId : providerId.Substring(0, dot);
    }

    /// <summary>Converts <c>provider-name</c> and <c>provider_name</c> to <c>providerName</c>.</summary>
    public static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var upper = false;
        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            if (ch == '_' || ch == '-')
            {
                upper = true;
                continue;
            }

            if (upper)
            {
                if (ch >= 'a' && ch <= 'z')
                {
                    builder.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    builder.Append(ch);
                }

                upper = false;
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Prefers the camelCase provider-options key when the caller supplied it.
    /// </summary>
    public static string ResolveProviderOptionsKey(string rawName, IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var camel = ToCamelCase(rawName);
        if (!string.Equals(camel, rawName, StringComparison.Ordinal) && HasOptions(providerOptions, camel))
        {
            return camel;
        }

        return rawName ?? string.Empty;
    }

    /// <summary>
    /// Adds a deprecation warning when <paramref name="providerOptions"/> uses a non-camelCase key.
    /// </summary>
    public static void WarnIfDeprecated(string rawName, IReadOnlyDictionary<string, JsonElement>? providerOptions, IList<CallWarning> warnings)
    {
        var camel = ToCamelCase(rawName ?? string.Empty);
        if (!string.Equals(camel, rawName, StringComparison.Ordinal) && HasOptions(providerOptions, rawName ?? string.Empty))
        {
            warnings.Add(new CallWarning(
                "deprecated",
                "providerOptions key '" + rawName + "'. Use '" + camel + "' instead."));
        }
    }

    /// <summary>True when <paramref name="key"/> is present and not JSON null.</summary>
    public static bool HasOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions, string key)
    {
        if (providerOptions == null || string.IsNullOrEmpty(key) || !providerOptions.TryGetValue(key, out var value))
        {
            return false;
        }

        return value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined;
    }

    /// <summary>Maps a Chat Completions usage object. A missing object leaves token counts unset.</summary>
    public static OpenAICompatibleUsage ConvertUsage(JsonElement? usage)
    {
        if (usage is not { } element || element.ValueKind != JsonValueKind.Object)
        {
            return OpenAICompatibleUsage.Missing;
        }

        var prompt = ReadInt(element, "prompt_tokens") ?? 0;
        var completion = ReadInt(element, "completion_tokens") ?? 0;
        var total = ReadInt(element, "total_tokens");
        var cache = ReadNestedInt(element, "prompt_tokens_details", "cached_tokens") ?? 0;
        var reasoning = ReadNestedInt(element, "completion_tokens_details", "reasoning_tokens") ?? 0;
        var modelUsage = new LanguageModelUsage(
            prompt,
            completion,
            total,
            cacheReadTokens: cache,
            reasoningTokens: reasoning,
            raw: element.Clone());
        return new OpenAICompatibleUsage(
            modelUsage,
            ReadNestedInt(element, "completion_tokens_details", "accepted_prediction_tokens"),
            ReadNestedInt(element, "completion_tokens_details", "rejected_prediction_tokens"));
    }

    /// <summary>Maps a Completions usage object. A missing object leaves token counts unset.</summary>
    public static LanguageModelUsage ConvertCompletionUsage(JsonElement? usage)
    {
        if (usage is not { } element || element.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null);
        }

        return new LanguageModelUsage(
            ReadInt(element, "prompt_tokens") ?? 0,
            ReadInt(element, "completion_tokens") ?? 0,
            ReadInt(element, "total_tokens"),
            raw: element.Clone());
    }

    /// <summary>Reads <c>error.message</c> or a string <c>error</c> without appending <c>param</c>.</summary>
    public static string? ReadErrorMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body!);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("error", out var error))
            {
                return null;
            }

            if (error.ValueKind == JsonValueKind.String)
            {
                return error.GetString();
            }

            if (error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    /// <summary>Copies an API exception onto the same status type with a new message.</summary>
    public static ApiException WithMessage(ApiException exception, string message)
    {
        var body = exception.ResponseBody;
        switch (exception.StatusCode)
        {
            case 400:
                return new BadRequestException(message, body);
            case 401:
                return new AuthenticationException(message, body);
            case 403:
                return new PermissionDeniedException(message, body);
            case 404:
                return new NotFoundException(message, body);
            case 422:
                return new UnprocessableEntityException(message, body);
            case 429:
                return new RateLimitException(message, body);
            default:
                if (exception.StatusCode >= 500 && exception.StatusCode <= 599)
                {
                    return new InternalServerException(message, exception.StatusCode, body);
                }

                return new ApiException(message, exception.StatusCode, body);
        }
    }

    /// <summary>Top-level reasoning values other than <c>provider-default</c> are sent as <c>reasoning_effort</c>.</summary>
    public static bool IsCustomReasoning(string? reasoning)
    {
        return reasoning != null && !string.Equals(reasoning, "provider-default", StringComparison.Ordinal);
    }

    /// <summary>Converts prompt messages to Chat Completions messages.</summary>
    public static JsonArray ConvertMessages(IReadOnlyList<ModelMessage> prompt, string metadataKey)
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
                    messages.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
                    break;
                case UserModelMessage user:
                    messages.Add(new JsonObject { ["role"] = "user", ["content"] = MapUserContent(user) });
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(MapAssistant(assistant, metadataKey));
                    break;
                case ToolModelMessage tool:
                    messages.Add(new JsonObject
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = tool.ToolCallId,
                        ["content"] = tool.OutputJson,
                    });
                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + (message == null ? string.Empty : message.Role) + ".");
            }
        }

        return messages;
    }

    /// <summary>Converts text and thinking content. Unknown parts are ignored.</summary>
    public static List<OpenAICompatibleContentPart> ConvertContent(JsonElement content)
    {
        var parts = new List<OpenAICompatibleContentPart>();
        if (content.ValueKind == JsonValueKind.String)
        {
            var text = content.GetString();
            if (!string.IsNullOrEmpty(text))
            {
                parts.Add(new OpenAICompatibleContentPart(false, text!));
            }

            return parts;
        }

        if (content.ValueKind != JsonValueKind.Array)
        {
            return parts;
        }

        foreach (var part in content.EnumerateArray())
        {
            if (part.ValueKind != JsonValueKind.Object || !part.TryGetProperty("type", out var typeElement))
            {
                continue;
            }

            var type = typeElement.GetString();
            if (type == "text" && part.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
            {
                var text = textElement.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    parts.Add(new OpenAICompatibleContentPart(false, text!));
                }
            }
            else if (type == "thinking" && part.TryGetProperty("thinking", out var thinking) && thinking.ValueKind == JsonValueKind.Array)
            {
                var builder = new StringBuilder();
                foreach (var chunk in thinking.EnumerateArray())
                {
                    if (chunk.ValueKind == JsonValueKind.Object
                        && chunk.TryGetProperty("type", out var chunkType)
                        && chunkType.GetString() == "text"
                        && chunk.TryGetProperty("text", out var chunkText)
                        && chunkText.ValueKind == JsonValueKind.String)
                    {
                        builder.Append(chunkText.GetString());
                    }
                }

                if (builder.Length > 0)
                {
                    parts.Add(new OpenAICompatibleContentPart(true, builder.ToString()));
                }
            }
        }

        return parts;
    }

    /// <summary>Builds provider metadata for accepted and rejected prediction tokens.</summary>
    public static JsonElement ProviderMetadata(string metadataKey, int? acceptedPredictionTokens, int? rejectedPredictionTokens)
    {
        var inner = new JsonObject();
        if (acceptedPredictionTokens is { } accepted)
        {
            inner["acceptedPredictionTokens"] = accepted;
        }

        if (rejectedPredictionTokens is { } rejected)
        {
            inner["rejectedPredictionTokens"] = rejected;
        }

        return ToElement(new JsonObject { [metadataKey ?? string.Empty] = inner });
    }

    /// <summary>Builds tool-call metadata that carries a Google thought signature.</summary>
    public static JsonElement ThoughtSignatureMetadata(string metadataKey, string thoughtSignature)
    {
        var inner = new JsonObject { ["thoughtSignature"] = thoughtSignature };
        return ToElement(new JsonObject { [metadataKey ?? string.Empty] = inner });
    }

    /// <summary>Throws when a file part cannot be represented on this API.</summary>
    public static AiSdkException Unsupported(string functionality)
    {
        return new AiSdkException("'" + functionality + "' functionality not supported.");
    }

    private static JsonNode MapUserContent(UserModelMessage user)
    {
        if (user.Content.Count == 1 && user.Content[0] is TextContentPart only)
        {
            return only.Text ?? string.Empty;
        }

        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text)
            {
                parts.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
            }
            else if (part is FileContentPart file)
            {
                parts.Add(MapFile(file));
            }
        }

        return parts;
    }

    private static JsonObject MapFile(FileContentPart file)
    {
        if (file.Url == null && file.Data == null)
        {
            throw Unsupported("file parts with provider references");
        }

        var mediaType = file.MediaType ?? string.Empty;
        var slash = mediaType.IndexOf('/');
        var top = slash < 0 ? mediaType : mediaType.Substring(0, slash);
        if (top == "image")
        {
            return new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JsonObject { ["url"] = FileUrl(file, mediaType) },
            };
        }

        if (top == "video")
        {
            return new JsonObject
            {
                ["type"] = "video_url",
                ["video_url"] = new JsonObject { ["url"] = FileUrl(file, mediaType) },
            };
        }

        if (top == "audio")
        {
            if (file.Url != null && file.Data == null)
            {
                throw Unsupported("audio file parts with URLs");
            }

            var format = AudioFormat(mediaType);
            if (format == null)
            {
                throw Unsupported("audio media type " + mediaType);
            }

            return new JsonObject
            {
                ["type"] = "input_audio",
                ["input_audio"] = new JsonObject
                {
                    ["data"] = Convert.ToBase64String(file.Data ?? Array.Empty<byte>()),
                    ["format"] = format,
                },
            };
        }

        if (top == "application")
        {
            if (file.Url != null && file.Data == null)
            {
                throw Unsupported("PDF file parts with URLs");
            }

            if (!string.Equals(mediaType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw Unsupported("file part media type " + mediaType);
            }

            return new JsonObject
            {
                ["type"] = "file",
                ["file"] = new JsonObject
                {
                    ["filename"] = string.IsNullOrEmpty(file.FileName) ? "document.pdf" : file.FileName,
                    ["file_data"] = "data:application/pdf;base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>()),
                },
            };
        }

        if (top == "text")
        {
            var text = file.Url != null && file.Data == null
                ? file.Url
                : Encoding.UTF8.GetString(file.Data ?? Array.Empty<byte>());
            return new JsonObject { ["type"] = "text", ["text"] = text };
        }

        throw Unsupported("file part media type " + mediaType);
    }

    private static string FileUrl(FileContentPart file, string mediaType)
    {
        if (file.Url != null && file.Data == null)
        {
            return file.Url;
        }

        return "data:" + mediaType + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }

    private static string? AudioFormat(string mediaType)
    {
        if (string.Equals(mediaType, "audio/wav", StringComparison.OrdinalIgnoreCase))
        {
            return "wav";
        }

        if (string.Equals(mediaType, "audio/mp3", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "audio/mpeg", StringComparison.OrdinalIgnoreCase))
        {
            return "mp3";
        }

        return null;
    }

    private static JsonObject MapAssistant(AssistantModelMessage assistant, string metadataKey)
    {
        var text = assistant.Text ?? string.Empty;
        var json = new JsonObject { ["role"] = "assistant" };
        if (assistant.ToolCalls.Count > 0)
        {
            json["content"] = text.Length == 0 ? JsonNull() : JsonValue.Create(text);
            var calls = new JsonArray();
            foreach (var call in assistant.ToolCalls)
            {
                var tool = new JsonObject
                {
                    ["id"] = call.ToolCallId,
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = call.ToolName,
                        ["arguments"] = call.ArgumentsJson ?? string.Empty,
                    },
                };
                var signature = ReadThoughtSignature(call.ProviderMetadata, metadataKey);
                if (!string.IsNullOrEmpty(signature))
                {
                    tool["extra_content"] = new JsonObject
                    {
                        ["google"] = new JsonObject { ["thought_signature"] = signature },
                    };
                }

                calls.Add(tool);
            }

            json["tool_calls"] = calls;
        }
        else
        {
            json["content"] = text;
        }

        if (!string.IsNullOrEmpty(assistant.Reasoning))
        {
            json["reasoning_content"] = assistant.Reasoning;
        }

        return json;
    }

    private static string? ReadThoughtSignature(JsonElement? metadata, string metadataKey)
    {
        if (metadata is not { } element || element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var fromKey = ReadSignature(element, metadataKey);
        if (!string.IsNullOrEmpty(fromKey))
        {
            return fromKey;
        }

        return ReadSignature(element, "google");
    }

    private static string? ReadSignature(JsonElement element, string key)
    {
        if (string.IsNullOrEmpty(key) || !element.TryGetProperty(key, out var bag) || bag.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (bag.TryGetProperty("thoughtSignature", out var signature) && signature.ValueKind == JsonValueKind.String)
        {
            return signature.GetString();
        }

        return null;
    }

    private static JsonNode JsonNull()
    {
        return JsonNode.Parse("null")!;
    }

    private static JsonElement ToElement(JsonObject obj)
    {
        using var document = JsonDocument.Parse(obj.ToJsonString());
        return document.RootElement.Clone();
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
        {
            return null;
        }

        return number;
    }

    private static int? ReadNestedInt(JsonElement element, string parent, string name)
    {
        if (!element.TryGetProperty(parent, out var child) || child.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ReadInt(child, name);
    }
}

/// <summary>Chat usage plus prediction-token counts stored as provider metadata.</summary>
public sealed class OpenAICompatibleUsage
{
    /// <summary>Usage when the provider omitted the object.</summary>
    public static OpenAICompatibleUsage Missing { get; } = new(new LanguageModelUsage(null, null, null), null, null);

    /// <summary>Creates converted usage.</summary>
    public OpenAICompatibleUsage(LanguageModelUsage usage, int? acceptedPredictionTokens, int? rejectedPredictionTokens)
    {
        Usage = usage;
        AcceptedPredictionTokens = acceptedPredictionTokens;
        RejectedPredictionTokens = rejectedPredictionTokens;
    }

    /// <summary>Token counts.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Accepted prediction tokens, when reported.</summary>
    public int? AcceptedPredictionTokens { get; }

    /// <summary>Rejected prediction tokens, when reported.</summary>
    public int? RejectedPredictionTokens { get; }
}

/// <summary>One text or reasoning fragment from a Chat Completions content field.</summary>
public sealed class OpenAICompatibleContentPart
{
    /// <summary>Creates a fragment.</summary>
    public OpenAICompatibleContentPart(bool reasoning, string text)
    {
        Reasoning = reasoning;
        Text = text ?? string.Empty;
    }

    /// <summary>True when the fragment is reasoning.</summary>
    public bool Reasoning { get; }

    /// <summary>Fragment text.</summary>
    public string Text { get; }
}
