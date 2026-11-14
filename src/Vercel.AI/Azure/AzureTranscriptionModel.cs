// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>
/// Azure transcription model. It resolves the API for each call, because providerOptions also reach this model through
/// the gateway: <c>openai</c> for Azure OpenAI deployments, <c>speech</c> for the MAI-Transcribe Speech API and <c>mai</c>
/// for MAI streaming transcription. Maps to <c>AzureTranscriptionModel</c> in <c>azure-openai-provider.ts</c>.
/// </summary>
public sealed class AzureTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private static readonly string[] OpenAIOnlyExclusions = { "language" };

    private readonly AzureOpenAIOptions _config;
    private readonly OpenAITranscriptionModel _openai;
    private readonly AzureSpeechTranscriptionModel _speech;
    private readonly AzureMaiTranscriptionModel _mai;

    internal AzureTranscriptionModel(string modelId, AzureOpenAIOptions config, OpenAITranscriptionModel openai, AzureSpeechTranscriptionModel speech, AzureMaiTranscriptionModel mai)
    {
        ModelId = modelId;
        _config = config;
        _openai = openai;
        _speech = speech;
        _mai = mai;
    }

    /// <inheritdoc />
    public string Provider => "azure.transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => true;

    /// <summary>
    /// Serializes the model id and the JSON-serializable provider settings for a workflow boundary. Maps to
    /// <c>[WORKFLOW_SERIALIZE]</c>. The API key and the token provider are not part of the serialized config. Pass the key
    /// to <see cref="Deserialize"/>.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var config = new Dictionary<string, object?>();
        void Add(string name, object? value)
        {
            if (value != null)
            {
                config[name] = value;
            }
        }

        Add("baseURL", _config.BaseUrl);
        Add("resourceName", _config.ResourceName);
        Add("apiVersion", _config.ApiVersion);
        Add("useDeploymentBasedUrls", _config.UseDeploymentBasedUrls);
        Add("speechBaseURL", _config.SpeechBaseUrl);
        Add("maiBaseURL", _config.MaiBaseUrl);
        Add("headers", _config.Headers.ToDictionary(pair => pair.Key, pair => (object?)pair.Value));
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>Restores a model from <see cref="Serialize"/>, resolving the API for each call again. Maps to <c>[WORKFLOW_DESERIALIZE]</c>.</summary>
    /// <param name="serialized">The serialized model id and config.</param>
    /// <param name="apiKey">API key. Defaults to the <c>AZURE_API_KEY</c> environment variable.</param>
    /// <param name="handler">HTTP handler. Pass one from tests.</param>
    public static AzureTranscriptionModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
    {
        if (serialized is null)
        {
            throw new ArgumentNullException(nameof(serialized));
        }

        var options = new AzureOpenAIOptions { ApiKey = apiKey };
        string? Text(string name) => serialized.Config.TryGetValue(name, out var value) ? value as string : null;
        options.BaseUrl = Text("baseURL");
        options.ResourceName = Text("resourceName");
        options.SpeechBaseUrl = Text("speechBaseURL");
        options.MaiBaseUrl = Text("maiBaseURL");
        if (Text("apiVersion") is { } apiVersion)
        {
            options.ApiVersion = apiVersion;
        }

        if (serialized.Config.TryGetValue("useDeploymentBasedUrls", out var deployment) && deployment is bool useDeploymentBasedUrls)
        {
            options.UseDeploymentBasedUrls = useDeploymentBasedUrls;
        }

        if (serialized.Config.TryGetValue("headers", out var headers) && headers is System.Collections.IDictionary map)
        {
            foreach (System.Collections.DictionaryEntry entry in map)
            {
                if (entry.Value is string value)
                {
                    options.Headers[(string)entry.Key] = value;
                }
            }
        }

        return AzureOpenAIProvider.Create(options, handler).Transcription(serialized.ModelId);
    }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var call = new TranscriptionModelCall(
            audio.Data,
            string.IsNullOrEmpty(audio.MediaType) ? "audio/wav" : audio.MediaType,
            OpenAIJson.Parse("{}"),
            new Dictionary<string, string>(),
            cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty);
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var options = GetOptions(call.ProviderOptions);
        if (options.Api == "mai")
        {
            return await _mai.DoGenerateAsync().ConfigureAwait(false);
        }

        if (options.Api == "speech")
        {
            var result = await _speech.DoGenerateAsync(call, AzureTranscriptionModelSpeechOptions.From(options.SpeechOptions), cancellationToken).ConfigureAwait(false);
            return With(
                result,
                result.Warnings
                    .Concat(MaiOnlyWarnings(options.MaiOptions))
                    .Concat(OpenAITranscriptionOnlyWarnings(options.OpenAIOptions)));
        }

        var openai = await _openai.TranscribeAsync(
            new OpenAITranscriptionCall(new AudioInput(call.Audio, call.MediaType, null))
            {
                ProviderOptions = AzureProviderOptions.WithAzureOptionsForOpenAI(call.ProviderOptions, options.OpenAIOptions),
                Headers = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
            },
            cancellationToken).ConfigureAwait(false);
        return new TranscriptionModelResult(
            openai.Text,
            openai.Segments,
            openai.Language,
            openai.DurationInSeconds,
            openai.Warnings.Select(ToWarning).Concat(SpeechOnlyWarnings(options.SpeechOptions)).ToList(),
            openai.ProviderMetadata,
            new ProviderResponse(openai.Response.Headers, OpenAIJson.Parse(openai.Response.Body), timestamp: openai.Response.Timestamp.UtcDateTime, modelId: openai.Response.ModelId));
    }

    /// <inheritdoc />
    public async Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var options = GetOptions(call.ProviderOptions);
        if (options.Api == "speech")
        {
            throw new UnsupportedFunctionalityException(
                "streaming transcription with the Azure Speech API",
                "'streaming transcription with the Azure Speech API' functionality not supported.");
        }

        if (options.Api == "mai")
        {
            return await _mai.DoStreamAsync(
                call,
                options.MaiOptions,
                SpeechOnlyWarnings(options.SpeechOptions).Concat(OpenAITranscriptionOnlyWarnings(options.OpenAIOptions)).ToList()).ConfigureAwait(false);
        }

        // The OpenAI transcription model of this port has no streaming transcription yet, so the call is not supported.
        return null;
    }

    private static TranscriptionModelResult With(TranscriptionModelResult result, IEnumerable<OperationWarning> warnings)
    {
        return new TranscriptionModelResult(
            result.Text,
            result.Segments,
            result.Language,
            result.DurationInSeconds,
            warnings.ToList(),
            result.ProviderMetadata,
            result.Response,
            result.Words);
    }

    private static OperationWarning ToWarning(OpenAICallWarning warning)
    {
        return new OperationWarning(warning.Type, warning.Message, warning.Feature, warning.Details);
    }

    private static IEnumerable<OperationWarning> OpenAITranscriptionOnlyWarnings(JsonObject options)
    {
        // `language` is shared with MAI and reported by `MaiOnlyWarnings`.
        var openaiOnlyOptions = new JsonObject();
        foreach (var pair in options)
        {
            if (Array.IndexOf(OpenAIOnlyExclusions, pair.Key) < 0)
            {
                openaiOnlyOptions[pair.Key] = pair.Value?.DeepClone();
            }
        }

        return AzureProviderOptions.UnsupportedOptionWarnings(openaiOnlyOptions, "This option requires the OpenAI transcription API.").Select(ToWarning);
    }

    private static IEnumerable<OperationWarning> SpeechOnlyWarnings(JsonObject options)
    {
        return AzureProviderOptions.UnsupportedOptionWarnings(options, "This option requires the Azure Speech API.").Select(ToWarning);
    }

    private static IEnumerable<OperationWarning> MaiOnlyWarnings(AzureTranscriptionModelMaiOptions options)
    {
        var picked = new JsonObject();
        if (options.Language != null)
        {
            picked["language"] = options.Language;
        }

        return AzureProviderOptions.UnsupportedOptionWarnings(picked, "This option requires MAI streaming transcription.").Select(ToWarning);
    }

    private Options GetOptions(JsonElement providerOptions)
    {
        var parsed = AzureTranscriptionModelOptions.Parse(providerOptions);
        var api = parsed.Api
            ?? (AzureTranscriptionModelOptions.IsMAITranscribeStreaming(ModelId)
                ? "mai"
                : AzureTranscriptionModelOptions.GetMAITranscribeModel(ModelId) != null ? "speech" : "openai");
        return new Options(
            api,
            AzureProviderOptions.PickKeys(parsed.Azure, AzureSpeechTranscriptionModelOptions.AzureSpeechTranscriptionModelOptionsShape().Keys),
            new AzureTranscriptionModelMaiOptions { Language = parsed.Language },
            AzureProviderOptions.PickAzureOptions(providerOptions, OpenAITranscriptionModelOptions.OpenAITranscriptionModelOptionsShape().Keys));
    }

    private sealed record Options(string Api, JsonObject SpeechOptions, AzureTranscriptionModelMaiOptions MaiOptions, JsonObject OpenAIOptions);
}
