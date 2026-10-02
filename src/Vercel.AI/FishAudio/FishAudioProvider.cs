// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.FishAudio;

/// <summary>FishAudio provider.</summary>
public sealed class FishAudioProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "fish-audio";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.fish.audio";

    /// <summary>Creates a provider.</summary>
    public FishAudioProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Specification version reported by the Fish Audio provider.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Fish Audio does not provide language models.");
    }

    /// <summary>Creates a speech model.</summary>
    public ISpeechModel Speech(string modelId)
    {
        return SpeechModel(modelId);
    }

    /// <summary>Creates the default transcription model <c>transcribe-1</c>.</summary>
    public ITranscriptionModel Transcription()
    {
        return TranscriptionModel("transcribe-1");
    }

    /// <summary>Creates a provider.</summary>
    public static new FishAudioProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new FishAudioProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "FISH_AUDIO_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new FishSpeechModel(this, modelId);

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new FishTranscriptionModel(this, string.IsNullOrEmpty(modelId) ? "transcribe-1" : modelId);

    /// <summary>Fish Audio speech model.</summary>
    public sealed class FishSpeechModel : ISpeechModel
    {
        private readonly FishAudioProvider _provider;

        /// <summary>Creates a speech model.</summary>
        public FishSpeechModel(FishAudioProvider provider, string modelId) { _provider = provider; ModelId = modelId; }

        /// <summary>Specification version.</summary>
        public string SpecificationVersion => "v4";

        /// <inheritdoc />
        public string Provider => "fish-audio.speech";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            var path = "/v1/tts";
            var body = new JsonObject { ["text"] = options.Text, ["format"] = "mp3" };
            if (!string.IsNullOrEmpty(options.Voice))
            {
                body["reference_id"] = options.Voice;
            }
            var bytes = await _provider.Http.SendBytesAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            return new SpeechResult(bytes, "audio/mpeg");
        }
    }

    /// <summary>Fish Audio transcription model.</summary>
    public sealed class FishTranscriptionModel : ITranscriptionModel
    {
        private readonly FishAudioProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public FishTranscriptionModel(FishAudioProvider provider, string modelId) { _provider = provider; ModelId = modelId; }

        /// <summary>Specification version.</summary>
        public string SpecificationVersion => "v4";

        /// <inheritdoc />
        public string Provider => "fish-audio.transcription";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var body = new JsonObject { ["model"] = ModelId, ["audio"] = Convert.ToBase64String(audio.Data) };
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/v1/asr"), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var text = document.RootElement.TryGetProperty("text", out var value) ? value.GetString() ?? string.Empty : string.Empty;
            return new TranscriptionResult(text);
        }
    }

}

/// <summary>Registers FishAudio.</summary>
public static class FishAudioServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddFishAudio(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(FishAudioProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new FishAudioProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(FishAudioProvider.ProviderId), options);
        });
        return services;
    }
}
