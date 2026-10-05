// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Deepgram;

/// <summary>Deepgram provider.</summary>
public sealed class DeepgramProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "deepgram";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.deepgram.com";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public DeepgramProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Deepgram does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new DeepgramProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new DeepgramProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "DEEPGRAM_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Token;
        options.UserAgent = ProviderExchange.UserAgent("deepgram");
        return options;
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new Transcription(this, modelId);

    /// <summary>Posts raw audio to <c>/v1/listen?model=</c>.</summary>
    public Task<TranscriptionResult> TranscribeAsync(string modelId, AudioInput audio, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return new Transcription(this, modelId).TranscribeAsync(audio, headers, cancellationToken);
    }

    private sealed class Transcription : ITranscriptionModel
    {
        private readonly DeepgramProvider _provider;
        public Transcription(DeepgramProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "deepgram.transcription";
        public string ModelId { get; }
        public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            return TranscribeAsync(audio, null, cancellationToken);
        }

        public async Task<TranscriptionResult> TranscribeAsync(AudioInput audio, IDictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            var content = new ByteArrayContent(audio.Data);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrEmpty(audio.MediaType) ? "application/octet-stream" : audio.MediaType);
            var merged = ProviderExchange.Merge(_provider.CreateHeaders(), headers);
            var uri = ApiKeys.Combine(_provider.Options.BaseUrl, "/v1/listen");
            var withQuery = new Uri(uri.AbsoluteUri + "?model=" + Uri.EscapeDataString(ModelId));
            var response = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, withQuery, content, merged, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
            var text = string.Empty;
            if (document.RootElement.TryGetProperty("results", out var results)
                && results.TryGetProperty("channels", out var channels)
                && channels.ValueKind == JsonValueKind.Array
                && channels.GetArrayLength() > 0
                && channels[0].TryGetProperty("alternatives", out var alternatives)
                && alternatives.ValueKind == JsonValueKind.Array
                && alternatives.GetArrayLength() > 0
                && alternatives[0].TryGetProperty("transcript", out var transcript)
                && transcript.ValueKind == JsonValueKind.String)
            {
                text = transcript.GetString() ?? string.Empty;
            }
            else if (document.RootElement.TryGetProperty("text", out var plain) && plain.ValueKind == JsonValueKind.String)
            {
                text = plain.GetString() ?? string.Empty;
            }

            return new TranscriptionResult(text);
        }
    }

}

/// <summary>Registers Deepgram.</summary>
public static class DeepgramServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddDeepgram(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(DeepgramProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new DeepgramProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(DeepgramProvider.ProviderId), options);
        });
        return services;
    }
}
