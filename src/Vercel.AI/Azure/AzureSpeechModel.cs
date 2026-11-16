// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>
/// Azure speech model. Resolves the API for every call: <c>providerOptions.azure.api</c> wins, and otherwise
/// MAI-Voice models use the Azure Speech API and every other model uses the OpenAI speech API. Maps to
/// <c>AzureSpeechModel</c> in <c>azure-openai-provider.ts</c>.
/// </summary>
public sealed class AzureSpeechModel : ISpeechModel
{
    private readonly AzureOpenAIOptions _config;
    private readonly OpenAISpeechModel _openai;
    private readonly AzureSpeechSpeechModel _speech;

    internal AzureSpeechModel(
        string modelId,
        AzureOpenAIOptions config,
        OpenAISpeechModel openai,
        AzureSpeechSpeechModel speech
    )
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _openai = openai ?? throw new ArgumentNullException(nameof(openai));
        _speech = speech ?? throw new ArgumentNullException(nameof(speech));
    }

    /// <inheritdoc />
    public string Provider => "azure.speech";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>
    /// Serializes the model id and the JSON-serializable provider settings for a workflow boundary. Maps to
    /// <c>[WORKFLOW_SERIALIZE]</c>. The API key and the token provider are not part of the serialized config. Pass the key
    /// to <see cref="Deserialize"/>.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        return AzureOpenAIProvider.SerializeModel(ModelId, _config);
    }

    /// <summary>Restores a model from <see cref="Serialize"/>, resolving the API for each call again. Maps to <c>[WORKFLOW_DESERIALIZE]</c>.</summary>
    /// <param name="serialized">The serialized model id and config.</param>
    /// <param name="apiKey">API key. Defaults to the <c>AZURE_API_KEY</c> environment variable.</param>
    /// <param name="handler">HTTP handler. Pass one from tests.</param>
    public static AzureSpeechModel Deserialize(
        SerializedModelOptions serialized,
        string? apiKey = null,
        HttpMessageHandler? handler = null
    )
    {
        return AzureOpenAIProvider
            .DeserializeProvider(serialized, apiKey, handler)
            .Speech(serialized.ModelId);
    }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(
        SpeechCallOptions options,
        CancellationToken cancellationToken
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var result = await GenerateAsync(
                new OpenAISpeechCall(options.Text) { Voice = options.Voice },
                cancellationToken
            )
            .ConfigureAwait(false);
        return new SpeechResult(result.Audio, result.MediaType);
    }

    /// <summary>
    /// Synthesizes speech with the OpenAI or the Azure Speech API and returns the audio, warnings and response metadata.
    /// Maps to <c>doGenerate</c>.
    /// </summary>
    /// <exception cref="Util.InvalidArgumentError">An <c>azure</c> provider option is unknown or out of range.</exception>
    public async Task<OpenAISpeechResult> GenerateAsync(
        OpenAISpeechCall call,
        CancellationToken cancellationToken
    )
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var parsed = AzureSpeechModelOptions.Parse(call.ProviderOptions);
        var speechOptions = new AzureSpeechSpeechModelOptions
        {
            Style = parsed?.Style,
            StyleDegree = parsed?.StyleDegree,
        };
        var openaiOptions = AzureProviderOptions.PickAzureOptions(
            call.ProviderOptions,
            OpenAISpeechModelOptions.Shape()
        );
        var api = parsed?.Api
            ?? (AzureSpeechModelOptions.GetMAIVoiceModel(ModelId) != null ? "speech" : "openai");
        if (api == "speech")
        {
            var result = await _speech
                .GenerateAsync(call, speechOptions, cancellationToken)
                .ConfigureAwait(false);
            var warnings = new List<OpenAICallWarning>(result.Warnings);
            warnings.AddRange(
                AzureProviderOptions.UnsupportedOptionWarnings(
                    openaiOptions,
                    "This option requires the OpenAI speech API."
                )
            );
            return result.WithWarnings(warnings);
        }

        var openaiResult = await _openai
            .GenerateAsync(
                call.WithProviderOptions(
                    AzureProviderOptions.WithAzureOptionsForOpenAI(
                        call.ProviderOptions,
                        openaiOptions
                    )
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        var openaiWarnings = new List<OpenAICallWarning>(openaiResult.Warnings);
        openaiWarnings.AddRange(
            AzureProviderOptions.UnsupportedOptionWarnings(
                SpeechOnlyOptions(speechOptions),
                "This option requires the Azure Speech API."
            )
        );
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
