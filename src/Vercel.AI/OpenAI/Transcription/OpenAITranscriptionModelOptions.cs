// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI transcription provider options. Maps to <c>openai-transcription-model-options.ts</c>.</summary>
internal static class OpenAITranscriptionModelOptions
{
    /// <summary>
    /// Shape of the OpenAI transcription model options. Routers that also call the OpenAI transcription API, such as
    /// Azure, accept these options under their own providerOptions key. Maps to <c>openAITranscriptionModelOptionsShape</c>.
    /// </summary>
    public static Dictionary<string, OptionValidator> OpenAITranscriptionModelOptionsShape()
    {
        var strings = ProviderOptionShape.ArrayOf(ProviderOptionShape.String());
        return new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["include"] = strings,
            ["language"] = ProviderOptionShape.String(),
            ["languages"] = strings,
            ["keywords"] = strings,
            ["prompt"] = ProviderOptionShape.String(),
            ["temperature"] = ProviderOptionShape.Number(min: 0, max: 1),
            ["timestampGranularities"] = ProviderOptionShape.ArrayOf(ProviderOptionShape.Enum("word", "segment")),
            ["responseFormat"] = ProviderOptionShape.Enum("json", "verbose_json", "diarized_json"),
            ["chunkingStrategy"] = ProviderOptionShape.Union(
                ProviderOptionShape.Literal("auto"),
                ProviderOptionShape.ObjectOf(
                    new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                    {
                        ["type"] = ProviderOptionShape.Literal("server_vad"),
                        ["threshold"] = ProviderOptionShape.Number(min: 0, max: 1),
                        ["prefixPaddingMs"] = ProviderOptionShape.Number(min: 0, integer: true),
                        ["silenceDurationMs"] = ProviderOptionShape.Number(min: 0, integer: true),
                    },
                    false,
                    "type")),
            ["streaming"] = ProviderOptionShape.ObjectOf(
                new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
                {
                    ["delay"] = ProviderOptionShape.Enum("minimal", "low", "medium", "high", "xhigh"),
                    ["include"] = strings,
                }),
        };
    }
}
