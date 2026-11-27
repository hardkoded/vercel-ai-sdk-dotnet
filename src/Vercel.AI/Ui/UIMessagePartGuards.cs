// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Ui;

/// <summary>Type guards and name helpers for UI message parts. Maps to the part helpers of <c>ui-messages.ts</c>.</summary>
public static class UIMessageParts
{
    /// <summary>Checks if a part is a data part. Maps to <c>isDataUIPart</c>.</summary>
    public static bool IsDataUIPart(UIMessagePart part) => part is DataUIPart;

    /// <summary>Checks if a part is a text part. Maps to <c>isTextUIPart</c>.</summary>
    public static bool IsTextUIPart(UIMessagePart part) => part is TextUIPart;

    /// <summary>Checks if a part is a custom part. Maps to <c>isCustomContentUIPart</c>.</summary>
    public static bool IsCustomContentUIPart(UIMessagePart part) => part is CustomContentUIPart;

    /// <summary>Checks if a part is a file part. Maps to <c>isFileUIPart</c>.</summary>
    public static bool IsFileUIPart(UIMessagePart part) => part is FileUIPart;

    /// <summary>Checks if a part is a reasoning file part. Maps to <c>isReasoningFileUIPart</c>.</summary>
    public static bool IsReasoningFileUIPart(UIMessagePart part) => part is ReasoningFileUIPart;

    /// <summary>Checks if a part is a reasoning part. Maps to <c>isReasoningUIPart</c>.</summary>
    public static bool IsReasoningUIPart(UIMessagePart part) => part is ReasoningUIPart;

    /// <summary>Checks if a part is a static tool part. Maps to <c>isStaticToolUIPart</c>.</summary>
    public static bool IsStaticToolUIPart(UIMessagePart part) =>
        part is ToolUIPart { IsDynamic: false };

    /// <summary>Checks if a part is a dynamic tool part. Maps to <c>isDynamicToolUIPart</c>.</summary>
    public static bool IsDynamicToolUIPart(UIMessagePart part) =>
        part is ToolUIPart { IsDynamic: true };

    /// <summary>Checks if a part is a static or dynamic tool part. Maps to <c>isToolUIPart</c>.</summary>
    public static bool IsToolUIPart(UIMessagePart part) => part is ToolUIPart;

    /// <summary>Checks if a part is a tool part in the <c>output-error</c> state. Maps to <c>isToolOutputErrorUIPart</c>.</summary>
    public static bool IsToolOutputErrorUIPart(UIMessagePart part) =>
        part is ToolUIPart { State: ToolUIPartState.OutputError };

    /// <summary>Returns the name of a static tool, the part type after <c>tool-</c>. Maps to <c>getStaticToolName</c>.</summary>
    public static string GetStaticToolName(ToolUIPart part)
    {
        if (part is null)
        {
            throw new ArgumentNullException(nameof(part));
        }

        var type = part.Type;
        var dash = type.IndexOf('-');
        return dash < 0 ? string.Empty : type.Substring(dash + 1);
    }

    /// <summary>Returns the name of a static or dynamic tool. Maps to <c>getToolName</c>.</summary>
    public static string GetToolName(ToolUIPart part)
    {
        if (part is null)
        {
            throw new ArgumentNullException(nameof(part));
        }

        return part.IsDynamic ? part.ToolName : GetStaticToolName(part);
    }
}
