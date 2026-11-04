// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Ui;

/// <summary>Maps to <c>warn-if-ui-message-has-deprecated-raw-input.ts</c>.</summary>
internal static class UIMessageRawInputWarning
{
    private static readonly DeprecatedWarning RawInputDeprecationWarning = new(
        "rawInput in output-error UI message parts",
        "Use the \"input\" field instead. The \"rawInput\" field will be removed in the next major version.");

    /// <summary>Logs a deprecation warning when an <c>output-error</c> tool part carries a <c>rawInput</c>. Maps to <c>warnIfUIMessageHasDeprecatedRawInput</c>.</summary>
    internal static void WarnIfUIMessageHasDeprecatedRawInput(IReadOnlyList<UIMessage> messages)
    {
        foreach (var message in messages)
        {
            foreach (var part in message.Parts)
            {
                if (part is ToolUIPart { State: ToolUIPartState.OutputError, RawInput: not null })
                {
                    LogWarnings.Log(new LogWarningsOptions(new ModelWarning[] { RawInputDeprecationWarning }));
                    return;
                }
            }
        }
    }
}
