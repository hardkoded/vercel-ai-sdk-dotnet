// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.BlackForestLabs;

/// <summary>BlackForestLabs provider.</summary>
public sealed class BlackForestLabsProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "black-forest-labs";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.bfl.ai/v1";

    /// <summary>Creates a provider.</summary>
    public BlackForestLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new BlackForestLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new BlackForestLabsProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "BFL_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "x-key";
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new Image(this, modelId);

    private sealed class Image : IImageModel
    {
        private readonly BlackForestLabsProvider _provider;
        public Image(BlackForestLabsProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "black-forest-labs";
        public string ModelId { get; }
        public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {
            var path = "/flux-pro-1.1".Replace("{model}", ModelId);
            var body = new JsonObject { ["prompt"] = options.Prompt, ["model"] = ModelId };
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new ImageGenerationResult(new[] { new GeneratedImage("image/png", null, url) });
        }
    }

}

/// <summary>Registers BlackForestLabs.</summary>
public static class BlackForestLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddBlackForestLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(BlackForestLabsProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new BlackForestLabsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(BlackForestLabsProvider.ProviderId), options);
        });
        return services;
    }
}
