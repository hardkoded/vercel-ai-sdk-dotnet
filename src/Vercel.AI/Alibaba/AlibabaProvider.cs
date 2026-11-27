// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Alibaba;

/// <summary>
/// Alibaba provider settings. Adds the native DashScope embedding origin to <see cref="OpenAICompatibleOptions"/>.
/// Maps to <c>AlibabaProviderSettings</c>.
/// </summary>
/// <remarks>
/// <see cref="OpenAICompatibleOptions.IncludeUsage"/> defaults to <c>true</c> here, so streaming responses include token usage
/// in the final chunk. Set it to <c>false</c> on these settings to leave <c>stream_options</c> out. Plain
/// <see cref="OpenAICompatibleOptions"/> cannot tell an unset value from <c>false</c>, so the provider turns usage on for them.
/// </remarks>
public sealed class AlibabaProviderSettings : OpenAICompatibleOptions
{
    /// <summary>Creates settings with <see cref="OpenAICompatibleOptions.IncludeUsage"/> set to <c>true</c>.</summary>
    public AlibabaProviderSettings()
    {
        IncludeUsage = true;
    }

    /// <summary>
    /// Use a different URL prefix for embedding API calls. The embedding API uses the DashScope native endpoint
    /// (not the OpenAI-compatible endpoint). The default prefix is <c>https://dashscope-intl.aliyuncs.com/api/v1</c>.
    /// </summary>
    public string? EmbeddingBaseUrl { get; set; }
}

/// <summary>Alibaba provider. OpenAI Chat Completions compatible at <c>https://dashscope-intl.aliyuncs.com/compatible-mode/v1</c>.</summary>
public sealed class AlibabaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "alibaba";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "ALIBABA_API_KEY";

    /// <summary>Default native DashScope embedding origin.</summary>
    public const string DefaultEmbeddingBaseUrl = "https://dashscope-intl.aliyuncs.com/api/v1";

    private readonly HttpClient _httpClient;
    private readonly string _embeddingBaseUrl;

    /// <summary>Creates a provider. Pass <see cref="AlibabaProviderSettings"/> to set the embedding origin or turn streaming usage off.</summary>
    public AlibabaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _embeddingBaseUrl = ProviderValues.WithoutTrailingSlash(
            (options as AlibabaProviderSettings)?.EmbeddingBaseUrl
        ) ?? DefaultEmbeddingBaseUrl;
    }

    /// <summary>Creates a chat model for text generation. Maps to <c>chatModel</c>.</summary>
    public OpenAICompatibleLanguageModel ChatModel(string modelId)
    {
        return CreateChatModel(modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return Embedding(modelId);
    }

    /// <summary>Creates a model for text embeddings. Maps to <c>embedding</c>.</summary>
    public AlibabaEmbeddingModel Embedding(string modelId)
    {
        return new AlibabaEmbeddingModel(
            modelId,
            new AlibabaConfig(
                "alibaba.embedding",
                _embeddingBaseUrl,
                cancellationToken => CreateHeadersAsync(cancellationToken),
                _httpClient
            )
        );
    }

    /// <summary>Creates a model for video generation. The provider id is <c>alibaba.video</c>.</summary>
    public override IVideoModel VideoModel(string modelId)
    {
        return Video(modelId);
    }

    /// <summary>Creates a model for video generation. Same as <see cref="VideoModel(string)"/>.</summary>
    public IVideoModel Video(string modelId)
    {
        // The video API uses the DashScope native endpoint, not the OpenAI-compatible one.
        return new AlibabaVideoModel(
            modelId,
            new AlibabaVideoModelConfig(
                "alibaba.video",
                ProviderValues.WithoutTrailingSlash(Options.VideoBaseUrl)
                    ?? "https://dashscope-intl.aliyuncs.com",
                async ct => await CreateHeadersAsync(ct).ConfigureAwait(false),
                _httpClient
            )
        );
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new AlibabaProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new AlibabaProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new AlibabaProviderSettings();
        if (options is not AlibabaProviderSettings)
        {
            options.IncludeUsage = true;
        }

        options.SupportedUrls ??= new Dictionary<string, string>
        {
            ["image/*"] = "^https?://.*\\z",
        };

        if (
            string.IsNullOrEmpty(options.ProviderName)
            || options.ProviderName == "openai-compatible"
        )
        {
            options.ProviderName = ProviderId;
        }

        if (options.BaseUrl == "https://api.openai.com/v1")
        {
            options.BaseUrl = DefaultBaseUrl;
        }

        if (options.ApiKeyEnvironmentVariable == "OPENAI_API_KEY")
        {
            options.ApiKeyEnvironmentVariable = ApiKeyVariable;
        }

        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.InterruptedWithoutFinish = true;
        if (
            options.AdditionalApiKeyEnvironmentVariables == null
            || options.AdditionalApiKeyEnvironmentVariables.Length == 0
        )
        {
            options.AdditionalApiKeyEnvironmentVariables = new[] { "DASHSCOPE_API_KEY" };
        }

        options.TransformChatRequest = AlibabaChatRequest.Transform;
        options.PreparesOwnTools = true;
        options.ConvertUsage = static usage => ConvertAlibabaUsage.Convert(usage);
        options.TextBlockId = "0";
        options.StreamToolInput = true;
        options.ReasoningBlockId ??= static () => JsonValues.GenerateId(string.Empty);

        options.ResolveReasoningEffort = (_, _, _, _) => null;
        options.UserAgent = ProviderExchange.UserAgent("alibaba");
        return options;
    }
}

/// <summary>Registers <see cref="AlibabaProvider"/>.</summary>
public static class AlibabaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddAlibaba(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(AlibabaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new AlibabaProviderSettings();
            configure?.Invoke(options);
            return new AlibabaProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(AlibabaProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
