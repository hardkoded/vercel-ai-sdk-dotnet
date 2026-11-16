// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Ui;

/// <summary>Maps to <c>unavailable-tool.ts</c>.</summary>
internal static class UnavailableTool
{
    /// <summary>
    /// Returns a copy of the part with <c>dynamic: false</c>. Preserves the static tool's provenance across persistence
    /// boundaries so model-message conversion never falls back to its unfiltered raw output. Maps to <c>markToolPartAsUnavailable</c>.
    /// </summary>
    internal static ToolUIPart MarkToolPartAsUnavailable(ToolUIPart part)
    {
        return new ToolUIPart(part.ToolName, part.ToolCallId, part.State, isDynamic: true)
        {
            Dynamic = false,
            Title = part.Title,
            ToolMetadata = part.ToolMetadata,
            InputEncoding = part.InputEncoding,
            ProviderExecuted = part.ProviderExecuted,
            Input = part.Input,
            RawInput = part.RawInput,
            Output = part.Output,
            ErrorText = part.ErrorText,
            Preliminary = part.Preliminary,
            CallProviderMetadata = part.CallProviderMetadata,
            ResultProviderMetadata = part.ResultProviderMetadata,
            Approval = part.Approval,
        };
    }

    /// <summary>Maps to <c>isToolPartFromUnavailableTool</c>.</summary>
    internal static bool IsToolPartFromUnavailableTool(ToolUIPart part)
    {
        return part.IsDynamic && part.Dynamic == false;
    }
}
