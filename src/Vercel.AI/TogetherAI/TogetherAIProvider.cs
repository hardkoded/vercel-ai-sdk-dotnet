// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.TogetherAI;

/// <summary>TogetherAI provider. OpenAI Chat Completions compatible at <c>https://api.together.xyz/v1</c>.</summary>
public sealed class TogetherAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "togetherai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.together.xyz/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "TOGETHER_API_KEY";

    /// <summary>Deprecated environment variable. Used only when <see cref="ApiKeyVariable"/> is unset.</summary>
    public const string DeprecatedApiKeyVariable = "TOGETHER_AI_API_KEY";

    /// <summary>Warning emitted when the deprecated environment variable supplies the key.</summary>
    public const string DeprecatedApiKeyMessage = "TOGETHER_AI_API_KEY is deprecated and will be removed in a future release. Please use TOGETHER_API_KEY instead.";

    private static readonly List<string> KeyWarnings = new();

    /// <summary>Warnings recorded while resolving the API key.</summary>
    public static IReadOnlyList<string> DeprecationWarnings => KeyWarnings;

    /// <summary>Clears <see cref="DeprecationWarnings"/>.</summary>
    public static void ClearDeprecationWarnings()
    {
        KeyWarnings.Clear();
    }

    /// <summary>True only for <c>deepseek-ai/DeepSeek-V4-Flash-0731</c>.</summary>
    public static bool SupportsStructuredOutputs(string modelId)
    {
        return string.Equals(modelId, "deepseek-ai/DeepSeek-V4-Flash-0731", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId)
    {
        return new TogetherRerankingModel(this, modelId);
    }

    /// <summary>Creates a provider.</summary>
    public TogetherAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new TogetherAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new TogetherAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        if (string.IsNullOrEmpty(options.ProviderName) || options.ProviderName == "openai-compatible")
        {
            options.ProviderName = ProviderId;
        }

        if (options.BaseUrl == "https://api.openai.com/v1")
        {
            options.BaseUrl = DefaultBaseUrl;
        }

        if (options.ApiKeyEnvironmentVariable == "OPENAI_API_KEY" || options.ApiKeyEnvironmentVariable == DeprecatedApiKeyVariable)
        {
            options.ApiKeyEnvironmentVariable = ApiKeyVariable;
        }

        options.SupportsEmbeddings = true;
        options.SupportsImages = true;
        options.AdditionalApiKeyEnvironmentVariables = new[] { DeprecatedApiKeyVariable };
        if (string.IsNullOrEmpty(options.ApiKey)
            && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ApiKeyVariable))
            && Environment.GetEnvironmentVariable(DeprecatedApiKeyVariable) != null)
        {
            KeyWarnings.Add(DeprecatedApiKeyMessage);
        }

        return options;
    }
}

/// <summary>Registers <see cref="TogetherAIProvider"/>.</summary>
public static class TogetherAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddTogetherAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(TogetherAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new TogetherAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(TogetherAIProvider.ProviderId), options);
        });
        return services;
    }
}
