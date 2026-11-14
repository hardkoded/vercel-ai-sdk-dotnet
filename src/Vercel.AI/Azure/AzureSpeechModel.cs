// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Azure;

/// <summary>
/// Azure speech model. Resolves the API for every call: <c>providerOptions.azure.api</c> wins, and otherwise
/// MAI-Voice models use the Azure Speech API and every other model uses the OpenAI speech API. Maps to
/// <c>AzureSpeechModel</c> in <c>azure-openai-provider.ts</c>.
/// </summary>
public sealed class AzureSpeechModel : ISpeechModel
{
    private readonly OpenAISpeechModel _openai;
    private readonly AzureSpeechSpeechModel _speech;

    internal AzureSpeechModel(string modelId, OpenAISpeechModel openai, AzureSpeechSpeechModel speech)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _openai = openai ?? throw new ArgumentNullException(nameof(openai));
        _speech = speech ?? throw new ArgumentNullException(nameof(speech));
    }

    /// <inheritdoc />
    public string Provider => "azure.speech";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var result = await GenerateAsync(new OpenAISpeechCall(options.Text) { Voice = options.Voice }, cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, result.MediaType);
    }

    /// <summary>
    /// Synthesizes speech with the OpenAI or the Azure Speech API and returns the audio, warnings and response metadata.
    /// Maps to <c>doGenerate</c>.
    /// </summary>
    /// <exception cref="Util.InvalidArgumentError">An <c>azure</c> provider option is unknown or out of range.</exception>
    public async Task<OpenAISpeechResult> GenerateAsync(OpenAISpeechCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var parsed = AzureSpeechModelOptions.Parse(call.ProviderOptions);
        var speechOptions = new AzureSpeechSpeechModelOptions { Style = parsed?.Style, StyleDegree = parsed?.StyleDegree };
        var openaiOptions = AzureProviderOptions.PickAzureOptions(call.ProviderOptions, OpenAISpeechModelOptions.Shape());
        var api = parsed?.Api ?? (AzureSpeechModelOptions.GetMAIVoiceModel(ModelId) != null ? "speech" : "openai");
        if (api == "speech")
        {
            var result = await _speech.GenerateAsync(call, speechOptions, cancellationToken).ConfigureAwait(false);
            var warnings = new List<OpenAICallWarning>(result.Warnings);
            warnings.AddRange(AzureProviderOptions.UnsupportedOptionWarnings(openaiOptions, "This option requires the OpenAI speech API."));
            return result.WithWarnings(warnings);
        }

        var openaiResult = await _openai.GenerateAsync(
            call.WithProviderOptions(AzureProviderOptions.WithAzureOptionsForOpenAI(call.ProviderOptions, openaiOptions)),
            cancellationToken).ConfigureAwait(false);
        var openaiWarnings = new List<OpenAICallWarning>(openaiResult.Warnings);
        openaiWarnings.AddRange(AzureProviderOptions.UnsupportedOptionWarnings(
            SpeechOnlyOptions(speechOptions),
            "This option requires the Azure Speech API."));
        return openaiResult.WithWarnings(openaiWarnings);
    }

    private static JsonObject SpeechOnlyOptions(AzureSpeechSpeechModelOptions options)
    {
        var picked = new JsonObject();
        if (options.Style != null)
        {
            picked["style"] = options.Style;
        }

        if (options.StyleDegree != null)
        {
            picked["styleDegree"] = options.StyleDegree;
        }

        return picked;
    }
}
