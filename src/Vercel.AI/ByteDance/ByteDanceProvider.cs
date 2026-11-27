// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.ByteDance;

/// <summary>ByteDance provider.</summary>
public sealed class ByteDanceProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "bytedance";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://ark.ap-southeast.bytepluses.com/api/v3";

    internal readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public ByteDanceProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "languageModel");
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <summary>Creates a provider.</summary>
    public static new ByteDanceProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        // Redirects are followed by hand so a status poll cannot be bounced to a private address.
        RedirectGuard.DisableAutoRedirect(handler);
        var client = handler is null
            ? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            : new HttpClient(handler, disposeHandler: false);
        if (handler is not null)
        {
            options = options?.Copy() ?? new OpenAICompatibleOptions();
            options.DownloadHttpClient ??= client;
        }

        return new ByteDanceProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl =
            string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1"
                ? DefaultBaseUrl
                : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ARK_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("bytedance");
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) =>
        new ByteDanceImageModel(this, modelId);

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) =>
        new ByteDanceVideoModel(this, modelId);

    /// <summary>Posts an image generation. <paramref name="headers"/> override the provider headers.</summary>
    public Task<ImageGenerationResult> GenerateImageAsync(
        string modelId,
        ImageCallOptions options,
        IDictionary<string, string>? headers,
        CancellationToken cancellationToken
    )
    {
        return new ByteDanceImageModel(this, modelId).GenerateAsync(
            options,
            headers,
            cancellationToken
        );
    }
}

/// <summary>Registers ByteDance.</summary>
public static class ByteDanceServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddByteDance(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        // Redirects are followed by hand so a status poll cannot be bounced to a private address.
        services
            .AddHttpClient(ByteDanceProvider.ProviderId)
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler { AllowAutoRedirect = false }
            );
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ByteDanceProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(ByteDanceProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
