// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>Finish-reason checks for the tool loop. Maps to <c>isToolExecutionAllowedFinishReason</c>.</summary>
public static class ToolExecution
{
    /// <summary>Returns true when client tools may run for <paramref name="finishReason"/>.</summary>
    public static bool IsToolExecutionAllowedFinishReason(FinishReason finishReason)
    {
        return finishReason == FinishReason.Stop || finishReason == FinishReason.ToolCalls;
    }
}
