// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
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

    /// <summary>Deprecated environment variable. <see cref="PreferredApiKeyVariable"/> wins when both are set.</summary>
    public const string ApiKeyVariable = "TOGETHER_AI_API_KEY";

    /// <summary>Environment variable for the API key.</summary>
    public const string PreferredApiKeyVariable = "TOGETHER_API_KEY";


    // Chat models verified to honor response_format json_schema. Every other model id gets
    // json_object and the schema is dropped, so probe a model with a schema-only enum value
    // before adding it here.
    private static readonly HashSet<string> s_jsonSchemaModelIds =
    [
        "deepseek-ai/DeepSeek-V4-Flash-0731",
        "deepseek-ai/DeepSeek-V4.1-Flash",
        "deepseek-ai/DeepSeek-V4.1-Flash-1017",
        "openai/gpt-oss-120b",
        "zai-org/GLM-5.2",
        "zai-org/GLM-5.3-Flash",
        "moonshotai/Kimi-K3",
        "MiniMaxAI/MiniMax-M3.1-Flash",
    ];

    /// <summary>Set when the key was read from <see cref="ApiKeyVariable"/>.</summary>
    public string? ApiKeyDeprecationWarning { get; private set; }

    /// <summary>Creates a provider.</summary>
    public TogetherAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Whether <paramref name="modelId"/> should send JSON schema responses.</summary>
    public static bool SupportsStructuredOutputs(string modelId)
    {
        return s_jsonSchemaModelIds.Contains(modelId);
    }

    /// <inheritdoc />
    public override OpenAICompatibleLanguageModel CreateChatModel(string modelId)
    {
        var model = base.CreateChatModel(modelId);
        model.SupportsStructuredOutputs = SupportsStructuredOutputs(modelId);
        return model;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return new TogetherAIImageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId)
    {
        return new TogetherAIRerankingModel(this, modelId);
    }

    /// <inheritdoc />
    protected override string? ResolveApiKey()
    {
        ApiKeyDeprecationWarning = null;
        if (!string.IsNullOrWhiteSpace(Options.ApiKey))
        {
            return Options.ApiKey;
        }

        var preferredName = string.IsNullOrEmpty(Options.ApiKeyEnvironmentVariable)
            ? PreferredApiKeyVariable
            : Options.ApiKeyEnvironmentVariable;
        var preferred = Environment.GetEnvironmentVariable(preferredName);
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            return preferred;
        }

        var legacy = Environment.GetEnvironmentVariable(ApiKeyVariable);
        if (!string.IsNullOrWhiteSpace(legacy))
        {
            ApiKeyDeprecationWarning = "TOGETHER_AI_API_KEY is deprecated and will be removed in a future release. Please use TOGETHER_API_KEY instead.";
            return legacy;
        }

        if (Options.RequireApiKey)
        {
            throw new AiSdkException(
                "API key is required. Pass it explicitly or set the " + preferredName + " environment variable.");
        }

        return null;
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new TogetherAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new TogetherAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        if (string.IsNullOrEmpty(options.ProviderName) || options.ProviderName == "openai-compatible")
        {
            options.ProviderName = ProviderId;
        }

        if (options.BaseUrl == "https://api.openai.com/v1")
        {
            options.BaseUrl = DefaultBaseUrl;
        }

        if (options.ApiKeyEnvironmentVariable == "OPENAI_API_KEY" || options.ApiKeyEnvironmentVariable == ApiKeyVariable)
        {
            options.ApiKeyEnvironmentVariable = PreferredApiKeyVariable;
        }

        options.SupportsEmbeddings = true;
        options.SupportsImages = true;
        options.RequireApiKey = true;
        if (string.IsNullOrEmpty(options.UserAgent))
        {
            options.UserAgent = OpenAICompatibleInfo.UserAgent(ProviderId);
        }
        options.IncludeUsage = true;
        options.AdditionalApiKeyEnvironmentVariables = new[] { ApiKeyVariable };
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
