// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>A Speech-API MAI-Transcribe model from the catalog. Maps to the object <c>getMAITranscribeModel</c> returns.</summary>
internal sealed class MAITranscribeModel
{
    internal MAITranscribeModel(string name, bool supportsTimestamps)
    {
        Name = name;
        SupportsTimestamps = supportsTimestamps;
    }

    /// <summary>Model name sent to the Speech API.</summary>
    public string Name { get; }

    /// <summary>False when the model cannot return word or segment timings.</summary>
    public bool SupportsTimestamps { get; }
}

/// <summary>
/// The validated <c>azure</c> provider options of an Azure transcription call. Maps to
/// <c>azureTranscriptionModelOptions</c> and <c>AzureTranscriptionModelOptions</c> in <c>azure-transcription-model-options.ts</c>.
/// </summary>
internal sealed class AzureTranscriptionModelOptions
{
    private AzureTranscriptionModelOptions(string? api, string? language, JsonElement? azure)
    {
        Api = api;
        Language = language;
        Azure = azure;
    }

    /// <summary>
    /// API to use: <c>openai</c>, <c>speech</c> or <c>mai</c>. Defaults to Speech for MAI-Transcribe models, MAI for
    /// MAI-Transcribe-2-Streaming, and OpenAI otherwise.
    /// </summary>
    public string? Api { get; }

    /// <summary>The MAI language hint. It shares its key with the OpenAI <c>language</c> option and keeps the MAI validation.</summary>
    public string? Language { get; }

    /// <summary>The raw validated <c>azure</c> options, for picking the options of each API.</summary>
    public JsonElement? Azure { get; }

    /// <summary>
    /// Validates the <c>azure</c> provider options and rejects unknown keys. Maps to <c>parseProviderOptions</c> with the
    /// <c>azureTranscriptionModelOptions</c> schema.
    /// </summary>
    public static AzureTranscriptionModelOptions Parse(JsonElement? providerOptions)
    {
        var azure = OpenAIJson.Child(providerOptions, "azure");
        if (azure is not { } options)
        {
            return new AzureTranscriptionModelOptions(null, null, null);
        }

        var error = ProviderOptionShape.Validate(options, Shape(), true);
        if (error != null)
        {
            throw new InvalidArgumentException("providerOptions", options.GetRawText(), "invalid azure provider options: " + error);
        }

        return new AzureTranscriptionModelOptions(OpenAIJson.String(options, "api"), OpenAIJson.String(options, "language"), options);
    }

    /// <summary>The Speech-API model for <paramref name="modelId"/>, or null. Maps to <c>getMAITranscribeModel</c>.</summary>
    public static MAITranscribeModel? GetMAITranscribeModel(string modelId)
    {
        var config = AzureModelCatalog.GetAzureModelEntry(modelId);
        return config?.TranscribeName is { } name ? new MAITranscribeModel(name, config.TranscribeSupportsTimestamps) : null;
    }

    /// <summary>True when <paramref name="modelId"/> uses MAI streaming transcription. Maps to <c>isMAITranscribeStreaming</c>.</summary>
    public static bool IsMAITranscribeStreaming(string modelId)
    {
        return AzureModelCatalog.GetAzureModelEntry(modelId)?.MaiStreamingTranscription == true;
    }

    // OpenAI API options for Azure OpenAI deployments are listed first so the shared `language` key keeps the MAI validation.
    private static Dictionary<string, OptionValidator> Shape()
    {
        var shape = new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["api"] = ProviderOptionShape.Enum("openai", "speech", "mai"),
        };
        ProviderOptionShape.Spread(shape, OpenAITranscriptionModelOptions.OpenAITranscriptionModelOptionsShape());
        ProviderOptionShape.Spread(shape, AzureSpeechTranscriptionModelOptions.AzureSpeechTranscriptionModelOptionsShape());
        return ProviderOptionShape.Spread(shape, AzureMaiTranscriptionModelOptions.AzureMaiTranscriptionModelOptionsShape());
    }
}
