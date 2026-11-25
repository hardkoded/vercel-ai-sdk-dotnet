// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.OpenResponses;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.QuiverAI;

/// <summary>QuiverAI provider.</summary>
public sealed class QuiverAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "quiverai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.quiver.ai/v1";

    private readonly OpenResponsesOptions _responsesOptions;
    private readonly OpenResponsesProvider _responsesProvider;

    /// <summary>Creates a provider.</summary>
    public QuiverAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _responsesOptions = CreateResponsesOptions(
            Options.BaseUrl,
            Options.ApiKey,
            Options.Headers
        );
        _responsesProvider = new OpenResponsesProvider(httpClient, _responsesOptions);
        ApplyErrorHandling(_responsesProvider);
    }

    /// <summary>QuiverAI caller-executed tools, such as custom tools. Maps to <c>provider.tools</c>.</summary>
    public OpenResponsesTools Tools => _responsesProvider.Tools;

    /// <summary>Creates a language model for the QuiverAI Responses API. The provider id is <c>quiverai.responses</c>.</summary>
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new QuiverAILanguageModel(
            _responsesProvider.LanguageModel(modelId),
            _responsesOptions
        );
    }

    /// <summary>Creates a provider.</summary>
    public static new QuiverAIProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new QuiverAIProvider(client, options);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new QuiverAIImageModel(this, modelId);

    /// <summary>Throws when neither the option nor <c>QUIVERAI_API_KEY</c> holds a key.</summary>
    protected override async Task<string?> ResolveApiKeyAsync(CancellationToken cancellationToken)
    {
        return await base.ResolveApiKeyAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new AiSdkException(
                "QuiverAI API key is missing. Pass it using the 'apiKey' parameter or the QUIVERAI_API_KEY environment variable."
            );
    }

    /// <summary>The Open Responses settings of the QuiverAI language model. Maps to <c>responsesConfig</c>.</summary>
    internal static OpenResponsesOptions CreateResponsesOptions(
        string? baseUrl,
        Resolvable<string>? apiKey,
        IReadOnlyDictionary<string, string>? headers
    )
    {
        return new OpenResponsesOptions
        {
            Name = ProviderId,
            BaseUrl = string.IsNullOrEmpty(baseUrl)
                ? Environment.GetEnvironmentVariable("QUIVERAI_BASE_URL")?.TrimEnd('/')
                    ?? DefaultBaseUrl
                : baseUrl!.TrimEnd('/'),
            ApiKey = apiKey,
            ApiKeyEnvironmentVariable = "QUIVERAI_API_KEY",
            Headers = headers is null
                ? null
                : Resolvable<IReadOnlyDictionary<string, string>>.From(headers),
            GetResponseErrorMetadata = QuiverAIError.GetQuiverAIResponseErrorMetadata,
            StrictResponseInput = true,
            CustomToolId = "quiverai.custom",
            StructuredOutputs = QuiverAIModelCatalog.LanguageStructuredOutputs,
            UserAgentSuffix = "ai-sdk-quiverai/" + AiSdkVersion.Version,
        };
    }

    /// <summary>Applies <c>quiveraiFailedResponseHandler</c> to the Open Responses transport.</summary>
    internal static void ApplyErrorHandling(OpenResponsesProvider provider)
    {
        provider.Http.ErrorToMessage = QuiverAIError.ErrorToMessage;
        provider.Http.IsRetryableStatus = QuiverAIError.IsRetryable;
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl =
            string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1"
                ? Environment.GetEnvironmentVariable("QUIVERAI_BASE_URL")?.TrimEnd('/')
                    ?? DefaultBaseUrl
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
    public static IServiceCollection AddQuiverAI(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(QuiverAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new QuiverAIProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(QuiverAIProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
