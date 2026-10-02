// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Maps Gemini <c>usageMetadata</c> onto language-model usage.</summary>
public static class GoogleUsageConverter
{
    /// <summary>
    /// Adds tool-use prompt tokens to input usage and thought tokens to output usage.
    /// Cached content tokens are reported as cache reads.
    /// </summary>
    public static LanguageModelUsage Convert(JsonElement usage)
    {
        if (usage.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null, raw: usage.ValueKind == JsonValueKind.Undefined ? null : usage.Clone());
        }

        var prompt = Read(usage, "promptTokenCount");
        var candidates = Read(usage, "candidatesTokenCount");
        var toolUse = Read(usage, "toolUsePromptTokenCount");
        var cached = Read(usage, "cachedContentTokenCount");
        var thoughts = Read(usage, "thoughtsTokenCount");
        var input = prompt + toolUse;
        var output = candidates + thoughts;
        int? total = usage.TryGetProperty("totalTokenCount", out var totalElement) && totalElement.ValueKind == JsonValueKind.Number
            ? totalElement.GetInt32()
            : input + output;
        return new LanguageModelUsage(input, output, total, cached, 0, thoughts, usage.Clone());
    }

    private static int Read(JsonElement usage, string name)
    {
        if (!usage.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return 0;
        }

        return value.TryGetInt32(out var number) ? number : 0;
    }
}
