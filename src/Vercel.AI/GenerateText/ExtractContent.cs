// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>Reads text and reasoning out of generated content. Maps to <c>extractTextContent</c> and <c>extractReasoningContent</c>.</summary>
public static class ContentParts
{
    /// <summary>Joins text parts. Returns null when there are none.</summary>
    public static string? ExtractTextContent(IReadOnlyList<GeneratedContent>? content)
    {
        if (content is null || content.Count == 0)
        {
            return null;
        }

        var parts = new List<string>();
        foreach (var part in content)
        {
            if (part is GeneratedText text)
            {
                parts.Add(text.Text);
            }
        }

        return parts.Count == 0 ? null : string.Concat(parts);
    }

    /// <summary>Joins reasoning parts with newlines. Returns null when there are none.</summary>
    public static string? ExtractReasoningContent(IReadOnlyList<GeneratedContent>? content)
    {
        if (content is null || content.Count == 0)
        {
            return null;
        }

        var parts = new List<string>();
        foreach (var part in content)
        {
            if (part is GeneratedReasoning reasoning)
            {
                parts.Add(reasoning.Text);
            }
        }

        return parts.Count == 0 ? null : string.Join("\n", parts);
    }
}
