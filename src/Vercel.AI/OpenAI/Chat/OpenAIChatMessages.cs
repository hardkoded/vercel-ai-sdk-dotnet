// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Result of <see cref="OpenAIChatMessages.ConvertToOpenAIChatMessages"/>.</summary>
public sealed class OpenAIChatMessageConversion
{
    /// <summary>Creates a conversion result.</summary>
    public OpenAIChatMessageConversion(JsonArray messages, IReadOnlyList<CallWarning> warnings)
    {
        Messages = messages ?? new JsonArray();
        Warnings = warnings ?? Array.Empty<CallWarning>();
    }

    /// <summary>OpenAI chat messages.</summary>
    public JsonArray Messages { get; }

    /// <summary>Warnings produced while converting the prompt.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>One prompt message passed to <see cref="OpenAIChatMessages.ConvertToOpenAIChatMessages"/>.</summary>
public abstract class OpenAIChatPromptMessage
{
    /// <summary>Creates a message.</summary>
    protected OpenAIChatPromptMessage(JsonElement? providerOptions)
    {
        ProviderOptions = providerOptions;
    }

    /// <summary>Provider options object, including an <c>openai</c> bag.</summary>
    public JsonElement? ProviderOptions { get; }
}

/// <summary>A system prompt message.</summary>
public sealed class OpenAISystemChatMessage : OpenAIChatPromptMessage
{
    /// <summary>Creates a system message.</summary>
    public OpenAISystemChatMessage(string content, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        Content = content ?? string.Empty;
    }

    /// <summary>Instruction text.</summary>
    public string Content { get; }
}

/// <summary>A user prompt message.</summary>
public sealed class OpenAIUserChatMessage : OpenAIChatPromptMessage
{
    /// <summary>Creates a user message.</summary>
    public OpenAIUserChatMessage(IReadOnlyList<OpenAIChatUserPart> content, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        Content = content ?? Array.Empty<OpenAIChatUserPart>();
    }

    /// <summary>User parts.</summary>
    public IReadOnlyList<OpenAIChatUserPart> Content { get; }
}

/// <summary>An assistant prompt message.</summary>
public sealed class OpenAIAssistantChatMessage : OpenAIChatPromptMessage
{
    /// <summary>Creates an assistant message.</summary>
    public OpenAIAssistantChatMessage(IReadOnlyList<OpenAIAssistantChatPart> content, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        Content = content ?? Array.Empty<OpenAIAssistantChatPart>();
    }

    /// <summary>Assistant parts.</summary>
    public IReadOnlyList<OpenAIAssistantChatPart> Content { get; }
}

/// <summary>A tool result message.</summary>
public sealed class OpenAIToolChatMessage : OpenAIChatPromptMessage
{
    /// <summary>Creates a tool message.</summary>
    public OpenAIToolChatMessage(IReadOnlyList<OpenAIToolResultChatPart> content, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        Content = content ?? Array.Empty<OpenAIToolResultChatPart>();
    }

    /// <summary>Tool results in this turn.</summary>
    public IReadOnlyList<OpenAIToolResultChatPart> Content { get; }
}

/// <summary>A user content part.</summary>
public abstract class OpenAIChatUserPart
{
    /// <summary>Creates a part.</summary>
    protected OpenAIChatUserPart(JsonElement? providerOptions)
    {
        ProviderOptions = providerOptions;
    }

    /// <summary>Provider options for this part.</summary>
    public JsonElement? ProviderOptions { get; }
}

/// <summary>User text.</summary>
public sealed class OpenAIChatTextPart : OpenAIChatUserPart
{
    /// <summary>Creates a text part.</summary>
    public OpenAIChatTextPart(string text, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>User file, image, audio, or PDF.</summary>
public sealed class OpenAIChatFilePart : OpenAIChatUserPart
{
    /// <summary>Creates a file part.</summary>
    public OpenAIChatFilePart(string mediaType, OpenAIChatFileData data, string? fileName = null, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        MediaType = mediaType ?? string.Empty;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        FileName = fileName;
    }

    /// <summary>Media type, which may be a top-level type such as <c>image</c>.</summary>
    public string MediaType { get; }

    /// <summary>File bytes, URL, base64, reference, or text.</summary>
    public OpenAIChatFileData Data { get; }

    /// <summary>Optional file name.</summary>
    public string? FileName { get; }
}

/// <summary>File payload for <see cref="OpenAIChatFilePart"/>.</summary>
public sealed class OpenAIChatFileData
{
    private OpenAIChatFileData(string kind, string? base64, byte[]? bytes, string? url, IReadOnlyDictionary<string, string>? reference, string? text)
    {
        Kind = kind;
        Base64 = base64;
        Bytes = bytes;
        Url = url;
        Reference = reference;
        Text = text;
    }

    /// <summary><c>base64</c>, <c>bytes</c>, <c>url</c>, <c>reference</c>, or <c>text</c>.</summary>
    public string Kind { get; }

    /// <summary>Base64 payload when <see cref="Kind"/> is <c>base64</c>.</summary>
    public string? Base64 { get; }

    /// <summary>Raw bytes when <see cref="Kind"/> is <c>bytes</c>.</summary>
    public byte[]? Bytes { get; }

    /// <summary>URL when <see cref="Kind"/> is <c>url</c>.</summary>
    public string? Url { get; }

    /// <summary>Provider file ids when <see cref="Kind"/> is <c>reference</c>.</summary>
    public IReadOnlyDictionary<string, string>? Reference { get; }

    /// <summary>Text payload when <see cref="Kind"/> is <c>text</c>.</summary>
    public string? Text { get; }

    /// <summary>Base64 that is already encoded and must not be re-encoded.</summary>
    public static OpenAIChatFileData FromBase64(string base64)
    {
        return new OpenAIChatFileData("base64", base64 ?? string.Empty, null, null, null, null);
    }

    /// <summary>Raw bytes that are base64-encoded when sent.</summary>
    public static OpenAIChatFileData FromBytes(byte[] bytes)
    {
        return new OpenAIChatFileData("bytes", null, bytes ?? Array.Empty<byte>(), null, null, null);
    }

    /// <summary>A remote URL.</summary>
    public static OpenAIChatFileData FromUrl(string url)
    {
        return new OpenAIChatFileData("url", null, null, url ?? string.Empty, null, null);
    }

    /// <summary>A provider file reference map.</summary>
    public static OpenAIChatFileData FromReference(IReadOnlyDictionary<string, string> reference)
    {
        return new OpenAIChatFileData("reference", null, null, null, reference ?? new Dictionary<string, string>(), null);
    }

    /// <summary>Text file contents. OpenAI chat rejects these parts.</summary>
    public static OpenAIChatFileData FromText(string text)
    {
        return new OpenAIChatFileData("text", null, null, null, null, text ?? string.Empty);
    }
}

/// <summary>An assistant content part.</summary>
public abstract class OpenAIAssistantChatPart
{
    /// <summary>Creates a part.</summary>
    protected OpenAIAssistantChatPart(JsonElement? providerOptions)
    {
        ProviderOptions = providerOptions;
    }

    /// <summary>Provider options for this part.</summary>
    public JsonElement? ProviderOptions { get; }
}

/// <summary>Assistant text.</summary>
public sealed class OpenAIAssistantTextPart : OpenAIAssistantChatPart
{
    /// <summary>Creates assistant text.</summary>
    public OpenAIAssistantTextPart(string text, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>An assistant tool call.</summary>
public sealed class OpenAIAssistantToolCallPart : OpenAIAssistantChatPart
{
    /// <summary>Creates a tool call. A null <paramref name="input"/> or a non-object becomes <c>{}</c>.</summary>
    public OpenAIAssistantToolCallPart(string toolCallId, string toolName, JsonNode? input, JsonElement? providerOptions = null)
        : base(providerOptions)
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        Input = input;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON object arguments, or a non-object that serializes as <c>{}</c>.</summary>
    public JsonNode? Input { get; }
}

/// <summary>One tool result inside a tool message.</summary>
public sealed class OpenAIToolResultChatPart
{
    /// <summary>Creates a tool result or approval response.</summary>
    public OpenAIToolResultChatPart(string type, string toolCallId, string toolName, OpenAIToolOutput? output, JsonElement? providerOptions = null)
    {
        Type = type ?? "tool-result";
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        Output = output;
        ProviderOptions = providerOptions;
    }

    /// <summary><c>tool-result</c> or <c>tool-approval-response</c>.</summary>
    public string Type { get; }

    /// <summary>Matching tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Tool output. Null for approval responses.</summary>
    public OpenAIToolOutput? Output { get; }

    /// <summary>Provider options on the tool result.</summary>
    public JsonElement? ProviderOptions { get; }
}

/// <summary>Tool output sent back to the model.</summary>
public sealed class OpenAIToolOutput
{
    private OpenAIToolOutput(string type, JsonNode? value, string? text, string? reason, JsonElement? providerOptions, IReadOnlyList<OpenAIChatUserPart>? content)
    {
        Type = type;
        Value = value;
        Text = text;
        Reason = reason;
        ProviderOptions = providerOptions;
        Content = content;
    }

    /// <summary><c>text</c>, <c>error-text</c>, <c>json</c>, <c>error-json</c>, <c>content</c>, or <c>execution-denied</c>.</summary>
    public string Type { get; }

    /// <summary>JSON value for json outputs.</summary>
    public JsonNode? Value { get; }

    /// <summary>Text for text outputs.</summary>
    public string? Text { get; }

    /// <summary>Denial reason.</summary>
    public string? Reason { get; }

    /// <summary>Provider options on the output.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>Content parts for <c>content</c> outputs.</summary>
    public IReadOnlyList<OpenAIChatUserPart>? Content { get; }

    /// <summary>Plain text output.</summary>
    public static OpenAIToolOutput FromText(string value, JsonElement? providerOptions = null)
    {
        return new OpenAIToolOutput("text", null, value ?? string.Empty, null, providerOptions, null);
    }

    /// <summary>Error text output.</summary>
    public static OpenAIToolOutput FromErrorText(string value, JsonElement? providerOptions = null)
    {
        return new OpenAIToolOutput("error-text", null, value ?? string.Empty, null, providerOptions, null);
    }

    /// <summary>JSON output. Null becomes JSON null.</summary>
    public static OpenAIToolOutput FromJson(JsonNode? value, JsonElement? providerOptions = null)
    {
        return new OpenAIToolOutput("json", value, null, null, providerOptions, null);
    }

    /// <summary>Error JSON output.</summary>
    public static OpenAIToolOutput FromErrorJson(JsonNode? value, JsonElement? providerOptions = null)
    {
        return new OpenAIToolOutput("error-json", value, null, null, providerOptions, null);
    }

    /// <summary>Multipart content output.</summary>
    public static OpenAIToolOutput FromContent(IReadOnlyList<OpenAIChatUserPart> content, JsonElement? providerOptions = null)
    {
        return new OpenAIToolOutput("content", null, null, null, providerOptions, content ?? Array.Empty<OpenAIChatUserPart>());
    }

    /// <summary>Denied tool execution.</summary>
    public static OpenAIToolOutput FromExecutionDenied(string? reason, JsonElement? providerOptions = null)
    {
        return new OpenAIToolOutput("execution-denied", null, null, reason, providerOptions, null);
    }
}

/// <summary>Converts a V4 prompt into OpenAI Chat Completions messages.</summary>
public static class OpenAIChatMessages
{
    /// <summary>
    /// Converts <paramref name="prompt"/>. <paramref name="systemMessageMode"/> is <c>system</c>, <c>developer</c>, or <c>remove</c>.
    /// </summary>
    public static OpenAIChatMessageConversion ConvertToOpenAIChatMessages(
        IReadOnlyList<OpenAIChatPromptMessage> prompt,
        string systemMessageMode = "system")
    {
        var messages = new JsonArray();
        var warnings = new List<CallWarning>();
        if (prompt == null)
        {
            return new OpenAIChatMessageConversion(messages, warnings);
        }

        foreach (var message in prompt)
        {
            switch (message)
            {
                case OpenAISystemChatMessage system:
                    ConvertSystem(system, systemMessageMode, messages, warnings);
                    break;
                case OpenAIUserChatMessage user:
                    messages.Add(ConvertUser(user));
                    break;
                case OpenAIAssistantChatMessage assistant:
                    messages.Add(ConvertAssistant(assistant));
                    break;
                case OpenAIToolChatMessage tool:
                    foreach (var part in ConvertTool(tool))
                    {
                        messages.Add(part);
                    }

                    break;
                default:
                    throw new AiSdkException("Unsupported role.");
            }
        }

        return new OpenAIChatMessageConversion(messages, warnings);
    }

    private static void ConvertSystem(OpenAISystemChatMessage system, string systemMessageMode, JsonArray messages, List<CallWarning> warnings)
    {
        switch (systemMessageMode)
        {
            case "system":
            case "developer":
                var breakpoint = PromptCacheBreakpoint(system.ProviderOptions);
                var content = breakpoint == null
                    ? (JsonNode)system.Content
                    : new JsonArray(TextBlock(system.Content, breakpoint));
                messages.Add(new JsonObject
                {
                    ["role"] = systemMessageMode,
                    ["content"] = content,
                });
                break;
            case "remove":
                warnings.Add(new OpenAICallWarning("other", null, null, "system messages are removed for this model"));
                break;
            default:
                throw new AiSdkException("Unsupported system message mode: " + systemMessageMode);
        }
    }

    private static JsonObject ConvertUser(OpenAIUserChatMessage user)
    {
        if (user.Content.Count == 1
            && user.Content[0] is OpenAIChatTextPart only
            && PromptCacheBreakpoint(only.ProviderOptions) == null)
        {
            return new JsonObject
            {
                ["role"] = "user",
                ["content"] = only.Text,
            };
        }

        var parts = new JsonArray();
        for (var index = 0; index < user.Content.Count; index++)
        {
            parts.Add(ConvertUserPart(user.Content[index], index));
        }

        return new JsonObject
        {
            ["role"] = "user",
            ["content"] = parts,
        };
    }

    private static JsonObject ConvertUserPart(OpenAIChatUserPart part, int index)
    {
        switch (part)
        {
            case OpenAIChatTextPart text:
                return TextBlock(text.Text, PromptCacheBreakpoint(text.ProviderOptions));
            case OpenAIChatFilePart file:
                return ConvertFile(file, index);
            default:
                throw new AiSdkException("Unsupported user content part.");
        }
    }

    private static JsonObject ConvertFile(OpenAIChatFilePart file, int index)
    {
        var breakpoint = PromptCacheBreakpoint(file.ProviderOptions);
        switch (file.Data.Kind)
        {
            case "reference":
                var reference = file.Data.Reference ?? new Dictionary<string, string>();
                if (!reference.TryGetValue("openai", out var fileId) || fileId == null)
                {
                    throw new AiSdkException(
                        "No provider reference found for provider 'openai'. Available providers: " + string.Join(", ", reference.Keys));
                }

                return WithBreakpoint(new JsonObject
                {
                    ["type"] = "file",
                    ["file"] = new JsonObject { ["file_id"] = fileId },
                }, breakpoint);
            case "text":
                throw new AiSdkException("text file parts");
            case "url":
            case "base64":
            case "bytes":
                return ConvertUrlOrData(file, index, breakpoint);
            default:
                throw new AiSdkException("Unsupported file data.");
        }
    }

    private static JsonObject ConvertUrlOrData(OpenAIChatFilePart file, int index, JsonNode? breakpoint)
    {
        var topLevel = TopLevelMediaType(file.MediaType);
        if (topLevel == "image")
        {
            var image = new JsonObject
            {
                ["url"] = file.Data.Kind == "url" ? file.Data.Url : DataUrl(file),
            };
            var detail = ImageDetail(file.ProviderOptions);
            if (detail != null)
            {
                image["detail"] = detail;
            }

            return WithBreakpoint(new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = image,
            }, breakpoint);
        }

        if (topLevel == "audio")
        {
            if (file.Data.Kind == "url")
            {
                throw new AiSdkException("audio file parts with URLs");
            }

            var full = ResolveFullMediaType(file);
            string format;
            switch (full)
            {
                case "audio/wav":
                    format = "wav";
                    break;
                case "audio/mp3":
                case "audio/mpeg":
                    format = "mp3";
                    break;
                default:
                    throw new AiSdkException("audio content parts with media type " + full);
            }

            return WithBreakpoint(new JsonObject
            {
                ["type"] = "input_audio",
                ["input_audio"] = new JsonObject
                {
                    ["data"] = Base64Payload(file.Data),
                    ["format"] = format,
                },
            }, breakpoint);
        }

        var mediaType = ResolveFullMediaType(file);
        if (mediaType != "application/pdf")
        {
            throw new AiSdkException("file part media type " + mediaType);
        }

        if (file.Data.Kind == "url")
        {
            throw new AiSdkException("PDF file parts with URLs");
        }

        return WithBreakpoint(new JsonObject
        {
            ["type"] = "file",
            ["file"] = new JsonObject
            {
                ["filename"] = string.IsNullOrEmpty(file.FileName) ? "part-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".pdf" : file.FileName,
                ["file_data"] = "data:application/pdf;base64," + Base64Payload(file.Data),
            },
        }, breakpoint);
    }

    private static JsonObject ConvertAssistant(OpenAIAssistantChatMessage assistant)
    {
        var text = string.Empty;
        var textParts = new JsonArray();
        var hasBreakpoint = false;
        var toolCalls = new JsonArray();
        foreach (var part in assistant.Content)
        {
            switch (part)
            {
                case OpenAIAssistantTextPart textPart:
                    var breakpoint = PromptCacheBreakpoint(textPart.ProviderOptions);
                    text += textPart.Text;
                    textParts.Add(TextBlock(textPart.Text, breakpoint));
                    if (breakpoint != null)
                    {
                        hasBreakpoint = true;
                    }

                    break;
                case OpenAIAssistantToolCallPart call:
                    toolCalls.Add(new JsonObject
                    {
                        ["id"] = call.ToolCallId,
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = call.ToolName,
                            ["arguments"] = SerializeToolCallArguments(call.Input),
                        },
                    });
                    break;
            }
        }

        var message = new JsonObject { ["role"] = "assistant" };
        if (hasBreakpoint)
        {
            message["content"] = textParts;
        }
        else if (toolCalls.Count > 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                // The indexer treats a null node as "remove this property".
                message["content"] = JsonValue.Create((string?)null);
            }
            else
            {
                message["content"] = text;
            }
        }
        else
        {
            message["content"] = text;
        }

        if (toolCalls.Count > 0)
        {
            message["tool_calls"] = toolCalls;
        }

        return message;
    }

    private static List<JsonObject> ConvertTool(OpenAIToolChatMessage tool)
    {
        var messages = new List<JsonObject>();
        foreach (var part in tool.Content)
        {
            if (part.Type == "tool-approval-response")
            {
                continue;
            }

            var output = part.Output ?? OpenAIToolOutput.FromText(string.Empty);
            JsonNode? breakpoint = null;
            if (output.Type == "content" && output.Content != null)
            {
                foreach (var contentPart in output.Content)
                {
                    breakpoint = PromptCacheBreakpoint(contentPart.ProviderOptions);
                    if (breakpoint != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                breakpoint = PromptCacheBreakpoint(output.ProviderOptions);
            }

            breakpoint = breakpoint ?? PromptCacheBreakpoint(part.ProviderOptions);
            string contentValue;
            switch (output.Type)
            {
                case "text":
                case "error-text":
                    contentValue = output.Text ?? string.Empty;
                    break;
                case "execution-denied":
                    contentValue = output.Reason ?? "Tool call execution denied.";
                    break;
                default:
                    contentValue = output.Value == null ? "null" : output.Value.ToJsonString();
                    break;
            }

            var toolMessage = new JsonObject
            {
                ["role"] = "tool",
                ["tool_call_id"] = part.ToolCallId,
            };
            if (breakpoint == null)
            {
                toolMessage["content"] = contentValue;
            }
            else
            {
                toolMessage["content"] = new JsonArray(TextBlock(contentValue, breakpoint));
            }

            messages.Add(toolMessage);
        }

        return messages;
    }

    private static string SerializeToolCallArguments(JsonNode? input)
    {
        return input is JsonObject ? input.ToJsonString() : "{}";
    }

    private static JsonObject TextBlock(string text, JsonNode? breakpoint)
    {
        var block = new JsonObject
        {
            ["type"] = "text",
            ["text"] = text,
        };
        if (breakpoint != null)
        {
            block["prompt_cache_breakpoint"] = breakpoint;
        }

        return block;
    }

    private static JsonObject WithBreakpoint(JsonObject part, JsonNode? breakpoint)
    {
        if (breakpoint != null)
        {
            part["prompt_cache_breakpoint"] = breakpoint;
        }

        return part;
    }

    private static JsonNode? PromptCacheBreakpoint(JsonElement? providerOptions)
    {
        if (!TryOpenAI(providerOptions, out var openai) || !openai.TryGetProperty("promptCacheBreakpoint", out var breakpoint))
        {
            return null;
        }

        return JsonNode.Parse(breakpoint.GetRawText());
    }

    private static string? ImageDetail(JsonElement? providerOptions)
    {
        if (!TryOpenAI(providerOptions, out var openai) || !openai.TryGetProperty("imageDetail", out var detail))
        {
            return null;
        }

        return detail.ValueKind == JsonValueKind.String ? detail.GetString() : null;
    }

    private static bool TryOpenAI(JsonElement? providerOptions, out JsonElement openai)
    {
        openai = default;
        if (providerOptions == null || providerOptions.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return providerOptions.Value.TryGetProperty("openai", out openai) && openai.ValueKind == JsonValueKind.Object;
    }

    private static string DataUrl(OpenAIChatFilePart file)
    {
        return "data:" + ResolveFullMediaType(file) + ";base64," + Base64Payload(file.Data);
    }

    private static string Base64Payload(OpenAIChatFileData data)
    {
        if (data.Kind == "bytes")
        {
            return Convert.ToBase64String(data.Bytes ?? Array.Empty<byte>());
        }

        return data.Base64 ?? string.Empty;
    }

    private static byte[]? InlineBytes(OpenAIChatFileData data)
    {
        if (data.Kind == "bytes")
        {
            return data.Bytes ?? Array.Empty<byte>();
        }

        if (data.Kind == "base64" && !string.IsNullOrEmpty(data.Base64))
        {
            return Convert.FromBase64String(data.Base64!);
        }

        return null;
    }

    private static string ResolveFullMediaType(OpenAIChatFilePart file)
    {
        if (IsFullMediaType(file.MediaType))
        {
            return file.MediaType;
        }

        var inline = InlineBytes(file.Data);
        if (inline != null)
        {
            var detected = DetectMediaType(inline, TopLevelMediaType(file.MediaType));
            if (detected != null)
            {
                return detected;
            }

            throw new AiSdkException("file of media type \"" + file.MediaType + "\" must specify subtype since it could not be auto-detected");
        }

        throw new AiSdkException("file of media type \"" + file.MediaType + "\" must specify subtype since it is not passed as inline bytes");
    }

    private static string? DetectMediaType(byte[] data, string topLevel)
    {
        if (topLevel == "image" && StartsWith(data, 0x89, 0x50, 0x4E, 0x47))
        {
            return "image/png";
        }

        if (topLevel == "image" && StartsWith(data, 0xFF, 0xD8))
        {
            return "image/jpeg";
        }

        if (topLevel == "application" && StartsWith(data, 0x25, 0x50, 0x44, 0x46))
        {
            return "application/pdf";
        }

        return null;
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

    private static string TopLevelMediaType(string mediaType)
    {
        if (mediaType == null)
        {
            return string.Empty;
        }

        var slash = mediaType.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }

    private static bool IsFullMediaType(string mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return false;
        }

        var slash = mediaType.IndexOf('/');
        if (slash < 0 || slash == mediaType.Length - 1)
        {
            return false;
        }

        var subtype = mediaType.Substring(slash + 1);
        return subtype.Length > 0 && subtype != "*";
    }
}
