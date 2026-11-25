// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Fal;

/// <summary>Fal provider.</summary>
public sealed class FalProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "fal";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://fal.run";

    internal readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public FalProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
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
    public static new FalProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        if (handler is not null)
        {
            options = options?.Copy() ?? new OpenAICompatibleOptions();
            options.DownloadHttpClient ??= client;
        }

        return new FalProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl =
            string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1"
                ? DefaultBaseUrl
                : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "FAL_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Key;
        options.UserAgent = ProviderExchange.UserAgent("fal");
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new FalImageModel(this, modelId);

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new FalSpeechModel(this, modelId);

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) =>
        new FalTranscriptionModel(this, modelId);

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId) => new FalVideoModel(this, modelId);

    /// <summary>Creates a video model. Same as <see cref="VideoModel"/>.</summary>
    public IVideoModel Video(string modelId) => VideoModel(modelId);

    /// <summary>Posts <paramref name="options"/> to <c>{base}/{modelId}</c>. <paramref name="headers"/> override the provider headers.</summary>
    public Task<ImageGenerationResult> GenerateImageAsync(
        string modelId,
        ImageCallOptions options,
        IDictionary<string, string>? headers,
        CancellationToken cancellationToken
    )
    {
        return new FalImageModel(this, modelId).GenerateAsync(options, headers, cancellationToken);
    }

    internal async Task<ProviderExchangeResult> SendAsync(
        HttpMethod method,
        Uri uri,
        HttpContent? content,
        IReadOnlyDictionary<string, string>? headers,
        CancellationToken cancellationToken,
        Func<int, string, ApiException>? failedResponseHandler = null
    )
    {
        return await ProviderExchange
            .SendAsync(
                _httpClient,
                method,
                uri,
                content,
                ProviderExchange.Merge(
                    await CreateHeadersAsync(cancellationToken).ConfigureAwait(false),
                    headers
                ),
                cancellationToken,
                failedResponseHandler
            )
            .ConfigureAwait(false);
    }

    /// <summary>Downloads a generated file. A URL outside <paramref name="trustedOrigin"/> must pass download validation.</summary>
    internal async Task<byte[]> DownloadAsync(
        string url,
        string trustedOrigin,
        CancellationToken cancellationToken
    )
    {
        var response = await ProviderExchange
            .GetResponseUrlAsync(
                _httpClient,
                url,
                trustedOrigin,
                DownloadUrls.ValidateDownloadUrl,
                null,
                cancellationToken,
                ProviderExchange.DownloadClient(Options),
                ProviderExchange.UserAgentOnly(
                    await CreateHeadersAsync(cancellationToken).ConfigureAwait(false)
                )
            )
            .ConfigureAwait(false);
        return response.Bytes;
    }

    internal static JsonElement? FalOptions(JsonElement? providerOptions)
    {
        if (
            providerOptions is { ValueKind: JsonValueKind.Object } options
            && options.TryGetProperty(ProviderId, out var fal)
            && fal.ValueKind == JsonValueKind.Object
        )
        {
            return fal;
        }

        return null;
    }
}

/// <summary>Registers Fal.</summary>
public static class FalServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddFal(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(FalProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new FalProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(FalProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
