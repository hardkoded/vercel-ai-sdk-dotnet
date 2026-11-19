// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Liquid;

/// <summary>Liquid provider. It supports experimental decision models.</summary>
public sealed class LiquidProvider : OpenAICompatibleProvider, Operations.IDecisionProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "liquid";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.liquid.ai";

    internal readonly HttpClient _httpClient;

    /// <summary>Creates a provider. The API key is read when a model is used, not here.</summary>
    public LiquidProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new LiquidProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new LiquidProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException(
            "Provider 'liquid' does not support language model '" + modelId + "'."
        );
    }

    /// <summary>Creates an experimental decision model using the Liquid Decisions API. The provider id is <c>liquid.decision</c>. Maps to <c>decisionModel</c>.</summary>
    public DecisionLiquidModel DecisionModel(string modelId) =>
        new DecisionLiquidModel(this, modelId);

    Operations.IDecisionCaller? Operations.IDecisionProvider.DecisionModel(string modelId) =>
        DecisionModel(modelId);

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl =
            string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1"
                ? DefaultBaseUrl
                : options.BaseUrl.TrimEnd('/');
        options.ApiKeyEnvironmentVariable = "LIQUID_API_KEY";
        options.RequireApiKey = true;
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.UserAgent = ProviderExchange.UserAgent("liquid");
        return options;
    }
}

/// <summary>Registers Liquid.</summary>
public static class LiquidServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddLiquid(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(LiquidProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new LiquidProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(LiquidProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
