// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.AspNetCore;

/// <summary>File bytes referenced by a UI stream part.</summary>
public sealed class UIGeneratedFile
{
    /// <summary>Creates a file.</summary>
    public UIGeneratedFile(string base64, string mediaType)
    {
        Base64 = base64 ?? string.Empty;
        MediaType = mediaType ?? string.Empty;
    }

    /// <summary>Base64 payload.</summary>
    public string Base64 { get; }

    /// <summary>Media type.</summary>
    public string MediaType { get; }
}

/// <summary>A tool passed to <see cref="UIMessageChunkConverter.ToUIMessageChunk"/>.</summary>
public sealed class UIMessageTool
{
    /// <summary>Creates a tool. <paramref name="dynamic"/> is true for a dynamic tool.</summary>
    public UIMessageTool(bool dynamic)
    {
        Dynamic = dynamic;
    }

    /// <summary>True when the tool type is dynamic.</summary>
    public bool Dynamic { get; }
}

/// <summary>One <c>streamText</c> part that can become a UI message chunk.</summary>
public sealed class UITextStreamPart
{
    /// <summary>Creates a part of <paramref name="type"/>.</summary>
    public UITextStreamPart(string type)
    {
        Type = type ?? string.Empty;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }

    /// <summary>Text or tool-call id.</summary>
    public string? Id { get; set; }

    /// <summary>Text or reasoning delta.</summary>
    public string? Text { get; set; }

    /// <summary>Provider metadata.</summary>
    public JsonNode? ProviderMetadata { get; set; }

    /// <summary>Generated file.</summary>
    public UIGeneratedFile? File { get; set; }

    /// <summary>Source type, <c>url</c> or <c>document</c>.</summary>
    public string? SourceType { get; set; }

    /// <summary>Source URL.</summary>
    public string? Url { get; set; }

    /// <summary>Source or tool title.</summary>
    public string? Title { get; set; }

    /// <summary>Document media type.</summary>
    public string? MediaType { get; set; }

    /// <summary>Document file name.</summary>
    public string? Filename { get; set; }

    /// <summary>Custom part kind.</summary>
    public string? Kind { get; set; }

    /// <summary>Tool call id.</summary>
    public string? ToolCallId { get; set; }

    /// <summary>Tool name.</summary>
    public string? ToolName { get; set; }

    /// <summary>Whether the provider executed the tool.</summary>
    public bool? ProviderExecuted { get; set; }

    /// <summary>Tool metadata.</summary>
    public JsonNode? ToolMetadata { get; set; }

    /// <summary>Whether the tool is dynamic when it is not in the tool set.</summary>
    public bool? Dynamic { get; set; }

    /// <summary>Tool input.</summary>
    public JsonNode? Input { get; set; }

    /// <summary>Original schema input before a transform.</summary>
    public JsonNode? InputSchemaInput { get; set; }

    /// <summary>True when <see cref="InputSchemaInput"/> was set, including null.</summary>
    public bool HasInputSchemaInput { get; set; }

    /// <summary>True when tool input failed validation.</summary>
    public bool Invalid { get; set; }

    /// <summary>Error passed to <see cref="ToUIMessageChunkOptions.OnError"/>.</summary>
    public object? Error { get; set; }

    /// <summary>Finish reason.</summary>
    public string? FinishReason { get; set; }

    /// <summary>Approval or abort reason.</summary>
    public string? Reason { get; set; }

    /// <summary>True when approval was requested automatically.</summary>
    public bool? IsAutomatic { get; set; }

    /// <summary>Approval signature.</summary>
    public string? Signature { get; set; }

    /// <summary>Approval id.</summary>
    public string? ApprovalId { get; set; }

    /// <summary>Tool call attached to an approval part.</summary>
    public UITextStreamPart? ToolCall { get; set; }

    /// <summary>Tool output.</summary>
    public JsonNode? Output { get; set; }

    /// <summary>True when <see cref="Output"/> was set, including null.</summary>
    public bool HasOutput { get; set; }

    /// <summary>True when the tool output is preliminary.</summary>
    public bool? Preliminary { get; set; }

    /// <summary>Approval decision.</summary>
    public bool? Approved { get; set; }

    /// <summary>Tool input delta.</summary>
    public string? Delta { get; set; }
}

/// <summary>Options for <see cref="UIMessageChunkConverter.ToUIMessageChunk"/>.</summary>
public sealed class ToUIMessageChunkOptions
{
    /// <summary>Known tools. A dynamic tool forces <c>dynamic: true</c> on its chunks.</summary>
    public IReadOnlyDictionary<string, UIMessageTool>? Tools { get; set; }

    /// <summary>When false, reasoning parts are dropped. The default is true.</summary>
    public bool SendReasoning { get; set; } = true;

    /// <summary>When false, source parts are dropped. The default is false.</summary>
    public bool SendSources { get; set; }

    /// <summary>When false, the start part is dropped. The default is true.</summary>
    public bool SendStart { get; set; } = true;

    /// <summary>When false, the finish part is dropped. The default is true.</summary>
    public bool SendFinish { get; set; } = true;

    /// <summary>Maps an error to client-visible text. The default is a generic message.</summary>
    public Func<object?, string>? OnError { get; set; }

    /// <summary>Metadata copied onto start and finish chunks.</summary>
    public JsonNode? MessageMetadata { get; set; }

    /// <summary>Message id copied onto the start chunk.</summary>
    public string? ResponseMessageId { get; set; }
}

/// <summary>Converts one <c>streamText</c> part into a UI message chunk.</summary>
public static class UIMessageChunkConverter
{
    /// <summary>
    /// Converts <paramref name="part"/> into a UI message chunk.
    /// Returns null for parts that do not produce a chunk.
    /// </summary>
    public static JsonObject? ToUIMessageChunk(UITextStreamPart part, ToUIMessageChunkOptions? options = null)
    {
        if (part is null)
        {
            throw new ArgumentNullException(nameof(part));
        }

        options ??= new ToUIMessageChunkOptions();
        var onError = options.OnError ?? DefaultOnError;
        switch (part.Type)
        {
            case "text-start":
            case "text-end":
                return WithMetadata(new JsonObject { ["type"] = part.Type, ["id"] = part.Id }, part);
            case "text-delta":
                return WithMetadata(new JsonObject { ["type"] = "text-delta", ["id"] = part.Id, ["delta"] = part.Text }, part);
            case "reasoning-start":
            case "reasoning-end":
                if (!options.SendReasoning)
                {
                    return null;
                }

                return WithMetadata(new JsonObject { ["type"] = part.Type, ["id"] = part.Id }, part);
            case "reasoning-delta":
                if (!options.SendReasoning)
                {
                    return null;
                }

                return WithMetadata(new JsonObject { ["type"] = "reasoning-delta", ["id"] = part.Id, ["delta"] = part.Text }, part);
            case "file":
                return FileChunk(part);
            case "reasoning-file":
                if (!options.SendReasoning)
                {
                    return null;
                }

                return FileChunk(part);
            case "source":
                return SourceChunk(part, options.SendSources);
            case "custom":
                return WithMetadata(new JsonObject { ["type"] = "custom", ["kind"] = part.Kind }, part);
            case "tool-input-start":
                return ToolStart(part, options);
            case "tool-input-delta":
                return new JsonObject
                {
                    ["type"] = "tool-input-delta",
                    ["toolCallId"] = part.Id,
                    ["inputTextDelta"] = part.Delta,
                };
            case "tool-call":
                return ToolCall(part, options, onError);
            case "tool-approval-request":
                return ApprovalRequest(part);
            case "tool-approval-response":
                return ApprovalResponse(part);
            case "tool-result":
                return ToolResult(part, options);
            case "tool-error":
                return ToolError(part, options, onError);
            case "tool-output-denied":
                return new JsonObject { ["type"] = "tool-output-denied", ["toolCallId"] = part.ToolCallId };
            case "error":
                return new JsonObject { ["type"] = "error", ["errorText"] = onError(part.Error) };
            case "start-step":
                return new JsonObject { ["type"] = "start-step" };
            case "finish-step":
                return new JsonObject { ["type"] = "finish-step" };
            case "start":
                if (!options.SendStart)
                {
                    return null;
                }

                var start = new JsonObject { ["type"] = "start" };
                Copy(start, "messageMetadata", options.MessageMetadata);
                if (options.ResponseMessageId != null)
                {
                    start["messageId"] = options.ResponseMessageId;
                }

                return start;
            case "finish":
                if (!options.SendFinish)
                {
                    return null;
                }

                var finish = new JsonObject { ["type"] = "finish", ["finishReason"] = part.FinishReason };
                Copy(finish, "messageMetadata", options.MessageMetadata);
                return finish;
            case "abort":
                var abort = new JsonObject { ["type"] = "abort" };
                if (part.Reason != null)
                {
                    abort["reason"] = part.Reason;
                }

                return abort;
            case "tool-input-end":
            case "raw":
                return null;
            default:
                throw new InvalidOperationException("Unknown chunk type: " + part.Type);
        }
    }

    private static string DefaultOnError(object? error)
    {
        return "An error occurred.";
    }

    private static JsonObject FileChunk(UITextStreamPart part)
    {
        var mediaType = part.File?.MediaType ?? string.Empty;
        var chunk = new JsonObject
        {
            ["type"] = part.Type,
            ["mediaType"] = mediaType,
            ["url"] = "data:" + mediaType + ";base64," + (part.File?.Base64 ?? string.Empty),
        };
        return WithMetadata(chunk, part);
    }

    private static JsonObject? SourceChunk(UITextStreamPart part, bool sendSources)
    {
        if (!sendSources)
        {
            return null;
        }

        if (part.SourceType == "url")
        {
            var chunk = new JsonObject
            {
                ["type"] = "source-url",
                ["sourceId"] = part.Id,
                ["url"] = part.Url,
            };
            if (part.Title != null)
            {
                chunk["title"] = part.Title;
            }

            return WithMetadata(chunk, part);
        }

        if (part.SourceType == "document")
        {
            var chunk = new JsonObject
            {
                ["type"] = "source-document",
                ["sourceId"] = part.Id,
                ["mediaType"] = part.MediaType,
                ["title"] = part.Title,
            };
            if (part.Filename != null)
            {
                chunk["filename"] = part.Filename;
            }

            return WithMetadata(chunk, part);
        }

        return null;
    }

    private static JsonObject ToolStart(UITextStreamPart part, ToUIMessageChunkOptions options)
    {
        var chunk = new JsonObject
        {
            ["type"] = "tool-input-start",
            ["toolCallId"] = part.Id,
            ["toolName"] = part.ToolName,
        };
        AddToolFields(chunk, part, options, errorText: null);
        return chunk;
    }

    private static JsonObject ToolCall(UITextStreamPart part, ToUIMessageChunkOptions options, Func<object?, string> onError)
    {
        var chunk = new JsonObject
        {
            ["type"] = part.Invalid ? "tool-input-error" : "tool-input-available",
            ["toolCallId"] = part.ToolCallId,
            ["toolName"] = part.ToolName,
            ["input"] = part.Input?.DeepClone() ?? JsonNull(),
        };
        AddToolFields(chunk, part, options, part.Invalid ? onError(part.Error) : null);
        return chunk;
    }

    private static JsonObject ToolResult(UITextStreamPart part, ToUIMessageChunkOptions options)
    {
        var chunk = new JsonObject
        {
            ["type"] = "tool-output-available",
            ["toolCallId"] = part.ToolCallId,
            ["output"] = part.HasOutput && part.Output != null ? part.Output.DeepClone() : JsonNull(),
        };
        Flag(chunk, "providerExecuted", part.ProviderExecuted);
        Copy(chunk, "providerMetadata", part.ProviderMetadata);
        Copy(chunk, "toolMetadata", part.ToolMetadata);
        Flag(chunk, "preliminary", part.Preliminary);
        Flag(chunk, "dynamic", Dynamic(part, options));
        return chunk;
    }

    private static JsonObject ToolError(UITextStreamPart part, ToUIMessageChunkOptions options, Func<object?, string> onError)
    {
        var errorText = part.ProviderExecuted == true ? Stringify(part.Error) : onError(part.Error);
        var chunk = new JsonObject
        {
            ["type"] = "tool-output-error",
            ["toolCallId"] = part.ToolCallId,
            ["errorText"] = errorText,
        };
        Flag(chunk, "providerExecuted", part.ProviderExecuted);
        Copy(chunk, "providerMetadata", part.ProviderMetadata);
        Copy(chunk, "toolMetadata", part.ToolMetadata);
        Flag(chunk, "dynamic", Dynamic(part, options));
        return chunk;
    }

    private static JsonObject ApprovalRequest(UITextStreamPart part)
    {
        var chunk = new JsonObject
        {
            ["type"] = "tool-approval-request",
            ["approvalId"] = part.ApprovalId,
            ["toolCallId"] = part.ToolCall?.ToolCallId,
        };
        if (part.HasInputSchemaInput && part.ToolCall != null && !JsonNode.DeepEquals(part.InputSchemaInput, part.ToolCall.Input))
        {
            chunk["inputSchemaInput"] = part.InputSchemaInput?.DeepClone() ?? JsonNull();
        }

        if (part.Reason != null)
        {
            chunk["reason"] = part.Reason;
        }

        Flag(chunk, "isAutomatic", part.IsAutomatic);
        if (part.Signature != null)
        {
            chunk["signature"] = part.Signature;
        }

        return chunk;
    }

    private static JsonObject ApprovalResponse(UITextStreamPart part)
    {
        var chunk = new JsonObject
        {
            ["type"] = "tool-approval-response",
            ["approvalId"] = part.ApprovalId,
            ["approved"] = part.Approved == true,
        };
        if (part.Reason != null)
        {
            chunk["reason"] = part.Reason;
        }

        Flag(chunk, "providerExecuted", part.ProviderExecuted);
        return chunk;
    }

    private static void AddToolFields(JsonObject chunk, UITextStreamPart part, ToUIMessageChunkOptions options, string? errorText)
    {
        Flag(chunk, "providerExecuted", part.ProviderExecuted);
        Copy(chunk, "providerMetadata", part.ProviderMetadata);
        Copy(chunk, "toolMetadata", part.ToolMetadata);
        Flag(chunk, "dynamic", Dynamic(part, options));
        if (errorText != null)
        {
            chunk["errorText"] = errorText;
        }

        if (part.Title != null)
        {
            chunk["title"] = part.Title;
        }
    }

    private static bool? Dynamic(UITextStreamPart part, ToUIMessageChunkOptions options)
    {
        UIMessageTool? tool = null;
        if (options.Tools != null && part.ToolName != null)
        {
            options.Tools.TryGetValue(part.ToolName, out tool);
        }

        if (tool == null)
        {
            return part.Dynamic;
        }

        return tool.Dynamic ? true : null;
    }

    private static JsonObject WithMetadata(JsonObject chunk, UITextStreamPart part)
    {
        Copy(chunk, "providerMetadata", part.ProviderMetadata);
        return chunk;
    }

    private static void Copy(JsonObject target, string name, JsonNode? value)
    {
        if (value != null)
        {
            target[name] = value.DeepClone();
        }
    }

    private static void Flag(JsonObject target, string name, bool? value)
    {
        if (value is bool flag)
        {
            target[name] = flag;
        }
    }

    private static string Stringify(object? error)
    {
        if (error is string text)
        {
            return text;
        }

        if (error is JsonNode node)
        {
            return node.ToJsonString();
        }

        return "{}";
    }

    private static JsonNode JsonNull()
    {
        return JsonNode.Parse("null")!;
    }
}
