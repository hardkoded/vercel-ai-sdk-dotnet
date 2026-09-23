// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.Fal;

/// <summary>Fal provider.</summary>
public sealed class FalProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "fal";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://fal.run";

    /// <summary>Creates a provider.</summary>
    public FalProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new FalProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new FalProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "FAL_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Key;
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new Image(this, modelId);

    private sealed class Image : IImageModel
    {
        private readonly FalProvider _provider;
        public Image(FalProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "fal";
        public string ModelId { get; }
        public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {
            var path = "/fal-ai/flux/dev".Replace("{model}", ModelId);
            var body = new JsonObject { ["prompt"] = options.Prompt, ["model"] = ModelId };
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new ImageGenerationResult(new[] { new GeneratedImage("image/png", null, url) });
        }
    }

}

/// <summary>Registers Fal.</summary>
public static class FalServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddFal(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(FalProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new FalProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(FalProvider.ProviderId), options);
        });
        return services;
    }
}
