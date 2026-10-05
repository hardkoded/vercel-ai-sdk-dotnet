// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.GenerateText;

/// <summary>Token-count helpers. Maps to <c>sumTokenCounts</c>.</summary>
public static class TokenCounts
{
    /// <summary>
    /// Adds token counts. An unknown count is treated as 0 when the other count is known.
    /// Returns null when both counts are unknown.
    /// </summary>
    public static int? SumTokenCounts(int? tokenCount1, int? tokenCount2)
    {
        if (tokenCount1 is null && tokenCount2 is null)
        {
            return null;
        }

        return (tokenCount1 ?? 0) + (tokenCount2 ?? 0);
    }
}
