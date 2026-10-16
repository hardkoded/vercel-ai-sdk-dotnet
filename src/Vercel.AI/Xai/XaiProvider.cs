// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Xai;

/// <summary>
/// xAI provider at <c>https://api.x.ai/v1</c>. <see cref="LanguageModel(string)"/> returns the Responses API model, as upstream does.
/// </summary>
public sealed class XaiProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "xai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.x.ai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "XAI_API_KEY";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public XaiProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a Responses API model, like <see cref="ResponsesModel(string)"/>. The provider id is <c>xai.responses</c>.</summary>
    public override ILanguageModel LanguageModel(string modelId)
    {
        return ResponsesModel(modelId);
    }

    /// <summary>Creates a Responses API model. The provider id is <c>xai.responses</c>.</summary>
    public XaiResponsesLanguageModel ResponsesModel(string modelId)
    {
        return new XaiResponsesLanguageModel(this, modelId);
    }

#pragma warning disable CS0809 // The base member stays current for the other OpenAI-compatible providers.
    /// <summary>Creates a Chat Completions model. Upstream xAI has no chat model; use <see cref="LanguageModel(string)"/> or <see cref="ResponsesModel(string)"/>.</summary>
    [Obsolete("xAI language models use the Responses API, as upstream does. Use LanguageModel or ResponsesModel. The Chat Completions model will be removed.")]
    public override OpenAICompatibleLanguageModel CreateChatModel(string modelId)
    {
        return base.CreateChatModel(modelId);
    }
#pragma warning restore CS0809

    /// <summary>xAI file store.</summary>
    public XaiFiles Files()
    {
        return new XaiFiles(_httpClient, Options.BaseUrl, () => CreateHeaders());
    }

    /// <inheritdoc />
    public override IFileStore FileStore()
    {
        return Files();
    }

    /// <inheritdoc />
    public override IRealtimeModel RealtimeModel(string modelId)
    {
        return new XaiRealtimeModel(modelId, _httpClient, Options.BaseUrl, () => CreateHeaders());
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new XaiProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new XaiProvider(client, options);
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

        if (options.ApiKeyEnvironmentVariable == "OPENAI_API_KEY")
        {
            options.ApiKeyEnvironmentVariable = ApiKeyVariable;
        }

        options.SupportsEmbeddings = false;
        options.SupportsImages = true;

        return options;
    }
}

/// <summary>Registers <see cref="XaiProvider"/>.</summary>
public static class XaiServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddXai(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(XaiProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new XaiProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(XaiProvider.ProviderId), options);
        });
        return services;
    }
}
