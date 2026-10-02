// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.GenerateText;

/// <summary>Token-rate helpers. Maps to <c>calculateTokensPerSecond</c>.</summary>
public static class TokenRates
{
    /// <summary>
    /// Calculates tokens per second. Returns 0 when the count or duration is unknown,
    /// the duration is 0, or the rate is not a finite number.
    /// </summary>
    public static double CalculateTokensPerSecond(double? tokens, double? durationMs)
    {
        var tokenRate = (1000d * (tokens ?? 0d)) / (durationMs ?? 0d);
        return double.IsFinite(tokenRate) ? tokenRate : 0d;
    }
}
