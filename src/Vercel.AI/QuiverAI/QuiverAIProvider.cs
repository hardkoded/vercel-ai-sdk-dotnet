// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.QuiverAI;

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

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("QuiverAI does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new QuiverAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new QuiverAIProvider(client, options);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new QuiverAIImageModel(this, modelId);

    /// <summary>Throws when neither the option nor <c>QUIVERAI_API_KEY</c> holds a key.</summary>
    protected override string? ResolveApiKey()
    {
        return base.ResolveApiKey()
            ?? throw new AiSdkException("QuiverAI API key is missing. Pass it using the 'apiKey' parameter or the QUIVERAI_API_KEY environment variable.");
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1"
            ? Environment.GetEnvironmentVariable("QUIVERAI_BASE_URL")?.TrimEnd('/') ?? DefaultBaseUrl
            : options.BaseUrl.TrimEnd('/');
        options.ApiKeyEnvironmentVariable = "QUIVERAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("quiverai");
        return options;
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
