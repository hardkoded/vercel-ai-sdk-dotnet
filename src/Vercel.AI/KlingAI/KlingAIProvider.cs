// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.KlingAI;

/// <summary>KlingAI provider.</summary>
public sealed class KlingAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "klingai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api-singapore.klingai.com";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public KlingAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("KlingAI does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new KlingAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new KlingAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "KLINGAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        return options;
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) => new Video(this, modelId);

    /// <summary>Starts a text-to-video job. <paramref name="headers"/> override the provider headers.</summary>
    public Task<VideoResult> GenerateVideoAsync(string modelId, VideoCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return new Video(this, modelId).GenerateAsync(options, headers, cancellationToken);
    }

    private sealed class Video : IVideoModel
    {
        private readonly KlingAIProvider _provider;
        public Video(KlingAIProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "klingai";
        public string ModelId { get; }
        public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
        {
            return GenerateAsync(options, null, cancellationToken);
        }

        public async Task<VideoResult> GenerateAsync(VideoCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            var body = new JsonObject { ["model_name"] = ModelId, ["prompt"] = options.Prompt };
            var merged = ProviderExchange.Merge(_provider.CreateHeaders(), headers);
            var response = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/v1/videos/text2video"), ProviderExchange.Json(body.ToJsonString()), merged, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new VideoResult(url, null, "video/mp4");
        }
    }

}

/// <summary>Registers KlingAI.</summary>
public static class KlingAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddKlingAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(KlingAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new KlingAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(KlingAIProvider.ProviderId), options);
        });
        return services;
    }
}
