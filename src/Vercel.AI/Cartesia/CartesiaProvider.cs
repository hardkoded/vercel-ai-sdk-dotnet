// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
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

    /// <summary>Creates a provider.</summary>
    public CartesiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
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
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new Speech(this, modelId);

    private sealed class Speech : ISpeechModel
    {
        private readonly CartesiaProvider _provider;
        public Speech(CartesiaProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "cartesia";
        public string ModelId { get; }
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            var voice = options.Voice ?? "default";
            var path = "/tts/bytes".Replace("{voice}", voice);
            var body = new JsonObject { ["text"] = options.Text, ["model_id"] = ModelId, ["model"] = ModelId };
            var bytes = await _provider.Http.SendBytesAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            return new SpeechResult(bytes, "audio/mpeg");
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
