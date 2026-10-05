// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cartesia;

/// <summary>Cartesia provider.</summary>
public sealed class CartesiaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "cartesia";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.cartesia.ai";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public CartesiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Cartesia does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new CartesiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new CartesiaProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "CARTESIA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.Headers["Cartesia-Version"] = "2026-03-01";
        options.UserAgent = ProviderExchange.UserAgent("cartesia");
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new Speech(this, modelId);

    /// <summary>Synthesizes speech. <paramref name="headers"/> override the provider headers.</summary>
    public Task<SpeechResult> GenerateSpeechAsync(string modelId, SpeechCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return new Speech(this, modelId).GenerateAsync(options, headers, cancellationToken);
    }

    private sealed class Speech : ISpeechModel
    {
        private readonly CartesiaProvider _provider;
        public Speech(CartesiaProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "cartesia.speech";
        public string ModelId { get; }
        public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            return GenerateAsync(options, null, cancellationToken);
        }

        public async Task<SpeechResult> GenerateAsync(SpeechCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(options.Voice))
            {
                throw new AiSdkException("Cartesia speech requires a voice id.");
            }

            var body = new JsonObject
            {
                ["model_id"] = ModelId,
                ["transcript"] = options.Text,
                ["voice"] = new JsonObject { ["mode"] = "id", ["id"] = options.Voice },
                ["output_format"] = new JsonObject { ["container"] = "mp3", ["sample_rate"] = 44100, ["bit_rate"] = 128000 },
            };
            var merged = ProviderExchange.Merge(_provider.CreateHeaders(), headers);
            var response = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/tts/bytes"), ProviderExchange.Json(body.ToJsonString()), merged, cancellationToken).ConfigureAwait(false);
            return new SpeechResult(response.Bytes, "audio/mpeg");
        }
    }

}

/// <summary>Registers Cartesia.</summary>
public static class CartesiaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddCartesia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(CartesiaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new CartesiaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(CartesiaProvider.ProviderId), options);
        });
        return services;
    }
}
