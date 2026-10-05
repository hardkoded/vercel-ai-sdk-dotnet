// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Luma;

/// <summary>Luma provider.</summary>
public sealed class LumaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "luma";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.lumalabs.ai";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public LumaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Luma does not provide language models.");
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId)
    {
        throw new AiSdkException("Luma does not provide video models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new LumaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new LumaProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "LUMA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("luma");
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new Image(this, modelId);

    /// <summary>Posts an image generation to Dream Machine.</summary>
    public Task<ImageGenerationResult> GenerateImageAsync(string modelId, ImageCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return new Image(this, modelId).GenerateAsync(options, headers, cancellationToken);
    }

    private sealed class Image : IImageModel
    {
        private readonly LumaProvider _provider;
        public Image(LumaProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "luma.image";
        public string ModelId { get; }
        public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {
            return GenerateAsync(options, null, cancellationToken);
        }

        public async Task<ImageGenerationResult> GenerateAsync(ImageCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            var body = new JsonObject { ["prompt"] = options.Prompt, ["model"] = ModelId };
            var merged = ProviderExchange.Merge(_provider.CreateHeaders(), headers);
            var response = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/dream-machine/v1/generations/image"), ProviderExchange.Json(body.ToJsonString()), merged, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new ImageGenerationResult(new[] { new GeneratedImage("image/png", null, url) });
        }
    }

}

/// <summary>Registers Luma.</summary>
public static class LumaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddLuma(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(LumaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new LumaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(LumaProvider.ProviderId), options);
        });
        return services;
    }
}
