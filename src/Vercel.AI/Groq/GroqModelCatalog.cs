// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Groq;

/// <summary>Groq model catalog. Maps to <c>groq-model-catalog.ts</c>.</summary>
internal static class GroqModelCatalog
{
    // Preserve the provider page's model set. Maps to `documentedLanguageModels`.
    private static readonly HashSet<string> DocumentedLanguageModels = new(StringComparer.Ordinal)
    {
        "gemma2-9b-it",
        "llama-3.1-8b-instant",
        "llama-3.3-70b-versatile",
        "meta-llama/llama-guard-4-12b",
        "deepseek-r1-distill-llama-70b",
        "meta-llama/llama-4-maverick-17b-128e-instruct",
        "meta-llama/llama-4-scout-17b-16e-instruct",
        "meta-llama/llama-prompt-guard-2-22m",
        "meta-llama/llama-prompt-guard-2-86m",
        "moonshotai/kimi-k2-instruct-0905",
        "qwen/qwen3.6-27b",
        "llama-guard-3-8b",
        "llama3-70b-8192",
        "llama3-8b-8192",
        "mixtral-8x7b-32768",
        "qwen-qwq-32b",
        "qwen-2.5-32b",
        "deepseek-r1-distill-qwen-32b",
        "openai/gpt-oss-20b",
        "openai/gpt-oss-120b",
    };

    /// <summary>
    /// Whether the model supports the Groq browser search tool. Null is <c>'unknown'</c>: an undeclared model keeps
    /// forwarding the tool. Maps to <c>capabilities.language.tools.browserSearch</c> of <c>getGroqLanguageModelEntry</c>.
    /// </summary>
    public static bool? SupportsBrowserSearch(string modelId)
    {
        return DocumentedLanguageModels.Contains(modelId)
            ? modelId.StartsWith("openai/gpt-oss", StringComparison.Ordinal)
            : null;
    }
}
