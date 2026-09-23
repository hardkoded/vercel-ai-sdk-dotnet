// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.QuiverAI;

/// <summary>QuiverAI provider.</summary>
public sealed class QuiverAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "quiverai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.quiver.ai/v1";

    /// <summary>Creates a provider.</summary>
    public QuiverAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new QuiverAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new QuiverAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "QUIVERAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new Image(this, modelId);

    private sealed class Image : IImageModel
    {
        private readonly QuiverAIProvider _provider;
        public Image(QuiverAIProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "quiverai";
        public string ModelId { get; }
        public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {
            var path = "/images/generations".Replace("{model}", ModelId);
            var body = new JsonObject { ["prompt"] = options.Prompt, ["model"] = ModelId };
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new ImageGenerationResult(new[] { new GeneratedImage("image/png", null, url) });
        }
    }

}

/// <summary>Registers QuiverAI.</summary>
public static class QuiverAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddQuiverAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(QuiverAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new QuiverAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(QuiverAIProvider.ProviderId), options);
        });
        return services;
    }
}
