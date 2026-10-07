// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Replicate;

/// <summary>Replicate provider.</summary>
public sealed class ReplicateProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "replicate";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.replicate.com/v1";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public ReplicateProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Replicate does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new ReplicateProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new ReplicateProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : ProviderValues.ValidateBaseUrl(options.BaseUrl) ?? DefaultBaseUrl;
        options.ApiKeyEnvironmentVariable = "REPLICATE_API_TOKEN";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("replicate");
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new ReplicateImageModel(this, modelId);

    /// <summary>Sends a request with the provider headers. <paramref name="headers"/> override them.</summary>
    internal Task<ProviderExchangeResult> SendAsync(HttpMethod method, Uri uri, HttpContent? content, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return ProviderExchange.SendAsync(_httpClient, method, uri, content, ProviderExchange.Merge(CreateHeaders(), headers), cancellationToken);
    }

    /// <summary>
    /// Gets a URL taken from a response body. A URL off the provider origin must pass download validation.
    /// <paramref name="headers"/> and the provider headers go only to the provider origin; null sends no headers.
    /// </summary>
    internal Task<ProviderExchangeResult> GetResponseUrlAsync(string url, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        var sameOrigin = ProviderValues.IsSameOrigin(url, Options.BaseUrl);
        if (!sameOrigin)
        {
            DownloadUrls.ValidateDownloadUrl(url);
        }

        var outgoing = sameOrigin && headers != null ? ProviderExchange.Merge(CreateHeaders(), headers) : null;
        return ProviderExchange.SendAsync(_httpClient, HttpMethod.Get, new Uri(url), null, outgoing, cancellationToken);
    }
}

/// <summary>Registers Replicate.</summary>
public static class ReplicateServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddReplicate(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(ReplicateProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ReplicateProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ReplicateProvider.ProviderId), options);
        });
        return services;
    }
}
