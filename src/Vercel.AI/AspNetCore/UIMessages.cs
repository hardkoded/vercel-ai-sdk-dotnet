// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AspNetCore;

/// <summary>A UI message part. Only the fields the completion checks read are required.</summary>
public sealed class UIMessagePart
{
    /// <summary>Creates a part of <paramref name="type"/>.</summary>
    public UIMessagePart(string type)
    {
        Type = type ?? string.Empty;
    }

    /// <summary>Part type, such as <c>text</c>, <c>tool-getWeather</c>, or <c>dynamic-tool</c>.</summary>
    public string Type { get; }

    /// <summary>Tool state, such as <c>output-available</c> or <c>approval-responded</c>.</summary>
    public string? State { get; set; }

    /// <summary>True when a tool output is still streaming.</summary>
    public bool? Preliminary { get; set; }

    /// <summary>True when the provider executed the tool.</summary>
    public bool? ProviderExecuted { get; set; }

    /// <summary>Dynamic tool name.</summary>
    public string? ToolName { get; set; }
}

/// <summary>A UI message.</summary>
public sealed class UIMessage
{
    /// <summary>Creates a message.</summary>
    public UIMessage(string id, string role, IReadOnlyList<UIMessagePart>? parts = null)
    {
        Id = id ?? string.Empty;
        Role = role ?? string.Empty;
        Parts = parts ?? Array.Empty<UIMessagePart>();
    }

    /// <summary>Message id.</summary>
    public string Id { get; }

    /// <summary>Message role.</summary>
    public string Role { get; }

    /// <summary>Message parts in order.</summary>
    public IReadOnlyList<UIMessagePart> Parts { get; }
}

/// <summary>UI message helpers for tool parts and auto-continue checks.</summary>
public static class UIMessages
{
    /// <summary>
    /// Returns the message id for the assistant response.
    /// Null <paramref name="originalMessages"/> means the client generates the id.
    /// An assistant message at the end reuses its id. Otherwise <paramref name="responseMessageId"/> is used.
    /// </summary>
    public static string? GetResponseMessageId(IReadOnlyList<UIMessage>? originalMessages, string responseMessageId)
    {
        return Resolve(originalMessages, responseMessageId, null);
    }

    /// <summary>
    /// Returns the message id for the assistant response.
    /// Null <paramref name="originalMessages"/> means the client generates the id.
    /// An assistant message at the end reuses its id. Otherwise <paramref name="responseMessageId"/> is called.
    /// </summary>
    public static string? GetResponseMessageId(IReadOnlyList<UIMessage>? originalMessages, Func<string> responseMessageId)
    {
        if (responseMessageId is null)
        {
            throw new ArgumentNullException(nameof(responseMessageId));
        }

        return Resolve(originalMessages, null, responseMessageId);
    }

    /// <summary>
    /// True when the last message is an assistant message whose last step has at least one
    /// non-provider tool invocation and every one of those invocations has a final output or an error.
    /// </summary>
    public static bool LastAssistantMessageIsCompleteWithToolCalls(IReadOnlyList<UIMessage> messages)
    {
        var tools = LastStepTools(messages, skipProviderExecuted: true);
        if (tools == null || tools.Count == 0)
        {
            return false;
        }

        for (var index = 0; index < tools.Count; index++)
        {
            if (!IsFinalToolOutput(tools[index]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// True when the last message is an assistant message whose last step has at least one
    /// approval response and every tool invocation in that step is in a terminal state.
    /// </summary>
    public static bool LastAssistantMessageIsCompleteWithApprovalResponses(IReadOnlyList<UIMessage> messages)
    {
        var tools = LastStepTools(messages, skipProviderExecuted: false);
        if (tools == null || tools.Count == 0)
        {
            return false;
        }

        var responded = false;
        for (var index = 0; index < tools.Count; index++)
        {
            var part = tools[index];
            if (part.State == "approval-responded")
            {
                responded = true;
            }

            if (!IsFinalToolOutput(part) && part.State != "output-denied" && part.State != "approval-responded")
            {
                return false;
            }
        }

        return responded;
    }

    /// <summary>Returns the tool name after the <c>tool-</c> prefix. Later dashes stay in the name.</summary>
    public static string GetStaticToolName(UIMessagePart part)
    {
        if (part is null)
        {
            throw new ArgumentNullException(nameof(part));
        }

        var pieces = part.Type.Split('-');
        if (pieces.Length <= 1)
        {
            return string.Empty;
        }

        return string.Join("-", pieces, 1, pieces.Length - 1);
    }

    /// <summary>True when <paramref name="part"/> is a custom content part.</summary>
    public static bool IsCustomContentPart(UIMessagePart part)
    {
        return part != null && part.Type == "custom";
    }

    /// <summary>True when <paramref name="part"/> is a <c>data-*</c> part.</summary>
    public static bool IsDataPart(UIMessagePart part)
    {
        return part != null && part.Type.StartsWith("data-", StringComparison.Ordinal);
    }

    /// <summary>True when <paramref name="part"/> is a static or dynamic tool part.</summary>
    public static bool IsToolPart(UIMessagePart part)
    {
        return part != null && (part.Type.StartsWith("tool-", StringComparison.Ordinal) || part.Type == "dynamic-tool");
    }

    /// <summary>True when <paramref name="part"/> is a tool part whose state is <c>output-error</c>.</summary>
    public static bool IsToolOutputErrorPart(UIMessagePart part)
    {
        return IsToolPart(part) && part.State == "output-error";
    }

    private static string? Resolve(IReadOnlyList<UIMessage>? originalMessages, string? responseMessageId, Func<string>? generate)
    {
        if (originalMessages == null)
        {
            return null;
        }

        if (originalMessages.Count > 0)
        {
            var last = originalMessages[originalMessages.Count - 1];
            if (last.Role == "assistant")
            {
                return last.Id;
            }
        }

        if (generate != null)
        {
            return generate();
        }

        return responseMessageId;
    }

    private static List<UIMessagePart>? LastStepTools(IReadOnlyList<UIMessage> messages, bool skipProviderExecuted)
    {
        if (messages == null || messages.Count == 0)
        {
            return null;
        }

        var message = messages[messages.Count - 1];
        if (message.Role != "assistant")
        {
            return null;
        }

        var lastStep = -1;
        for (var index = 0; index < message.Parts.Count; index++)
        {
            if (message.Parts[index].Type == "step-start")
            {
                lastStep = index;
            }
        }

        var tools = new List<UIMessagePart>();
        for (var index = lastStep + 1; index < message.Parts.Count; index++)
        {
            var part = message.Parts[index];
            if (!IsToolPart(part))
            {
                continue;
            }

            if (skipProviderExecuted && part.ProviderExecuted == true)
            {
                continue;
            }

            tools.Add(part);
        }

        return tools;
    }

    private static bool IsFinalToolOutput(UIMessagePart part)
    {
        return (part.State == "output-available" && part.Preliminary != true) || part.State == "output-error";
    }
}
