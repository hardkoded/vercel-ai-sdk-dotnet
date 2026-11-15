// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>Azure MAI transcription provider options. Maps to <c>azure-mai-transcription-model-options.ts</c>.</summary>
internal static class AzureMaiTranscriptionModelOptions
{
    /// <summary>Shape of the MAI options. Maps to <c>azureMaiTranscriptionModelOptionsShape</c>.</summary>
    public static Dictionary<string, OptionValidator> AzureMaiTranscriptionModelOptionsShape()
    {
        return new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["language"] = ProviderOptionShape.String(minLength: 1),
        };
    }
}

/// <summary>Options of MAI streaming transcription. Maps to <c>AzureTranscriptionModelMaiOptions</c>.</summary>
internal sealed class AzureTranscriptionModelMaiOptions
{
    /// <summary>Language hint for streaming transcription, for example <c>en</c>. Omit for automatic language detection.</summary>
    public string? Language { get; init; }
}
