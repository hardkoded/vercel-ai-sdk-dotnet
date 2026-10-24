// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Azure;

/// <summary>Azure OpenAI settings.</summary>
public sealed class AzureOpenAIOptions
{
    /// <summary>Resource host, for example <c>https://my-resource.openai.azure.com</c>.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Resource name used when <see cref="BaseUrl"/> is empty.
    /// Must be a single DNS label (letters, digits, and hyphens).
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>Explicit key. Falls back to <c>AZURE_API_KEY</c>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>API version query parameter. Defaults to <c>v1</c>, also when set to null or empty.</summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Uses the legacy <c>/openai/deployments/{deploymentId}{path}</c> routes instead of <c>/openai/v1{path}</c>.
    /// </summary>
    public bool UseDeploymentBasedUrls { get; set; }

    /// <summary>Microsoft Entra token provider. Each call receives a fresh bearer token.</summary>
    public Func<string>? TokenProvider { get; set; }

    /// <summary>Headers copied onto every request.</summary>
    public Dictionary<string, string> Headers { get; } = new();
}

/// <summary>Settings for an Azure OpenAI language model.</summary>
public sealed class AzureOpenAILanguageModelSettings
{
    /// <summary>
    /// Underlying model ID used for reported identity and capability detection.
    /// Defaults to the deployment ID. Requests always use the deployment ID.
    /// </summary>
    public string? ModelId { get; set; }
}

/// <summary>
/// Azure OpenAI provider. Calls use the <c>/openai/v1</c> routes and the <c>api-key</c> header, or the
/// deployments routes when <see cref="AzureOpenAIOptions.UseDeploymentBasedUrls"/> is set.
/// </summary>
public sealed class AzureOpenAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "azure";

    // $ can succeed before a trailing newline, so the match must cover the whole value.
    private static readonly Regex HostnamePart = new(
        "^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>Creates a provider.</summary>
    public AzureOpenAIProvider(HttpClient httpClient, AzureOpenAIOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static AzureOpenAIProvider Create(AzureOpenAIOptions? options = null, HttpMessageHandler? handler = null)
    {
        options ??= new AzureOpenAIOptions();
        HttpMessageHandler? transport = handler;
        if (options.TokenProvider != null && string.IsNullOrEmpty(options.ApiKey))
        {
            transport = new AzureEntraTokenHandler(options.TokenProvider, handler ?? new HttpClientHandler());
        }

        var client = transport is null ? new HttpClient() : new HttpClient(transport, disposeHandler: false);
        return new AzureOpenAIProvider(client, options);
    }

    /// <summary>
    /// Responses API model, like <see cref="ResponsesModel(string)"/>. The optional model ID in
    /// <paramref name="settings"/> is used for reported identity and capability detection.
    /// </summary>
    public OpenAI.OpenAIResponsesLanguageModel LanguageModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
    {
        return new OpenAI.OpenAIResponsesLanguageModel(this, deploymentId, settings);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return ResponsesModel(modelId);
    }

    /// <summary>
    /// Responses API model. The provider id is <c>azure.responses</c>.
    /// Options are read from the <c>azure</c> provider options and fall back to <c>openai</c>.
    /// </summary>
    public OpenAI.OpenAIResponsesLanguageModel ResponsesModel(string modelId)
    {
        return new OpenAI.OpenAIResponsesLanguageModel(this, modelId);
    }

    /// <summary>
    /// Responses API model for a deployment. The optional model ID is used for reported identity and
    /// capability detection; requests always use the deployment ID.
    /// </summary>
    public OpenAI.OpenAIResponsesLanguageModel ResponsesModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
    {
        return new OpenAI.OpenAIResponsesLanguageModel(this, deploymentId, settings);
    }

    /// <summary>
    /// Chat model for a deployment. The optional model ID is used for reported identity and capability
    /// detection; requests always use the deployment ID.
    /// </summary>
    public OpenAI.OpenAIChatLanguageModel ChatModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
    {
        return new OpenAI.OpenAIChatLanguageModel(this, deploymentId, settings);
    }

    /// <summary>
    /// Completion model for a deployment. The optional model ID is used for reported identity;
    /// requests always use the deployment ID.
    /// </summary>
    public OpenAICompatibleCompletionLanguageModel CompletionModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
    {
        return new OpenAICompatibleCompletionLanguageModel(
            this,
            settings?.ModelId ?? deploymentId,
            null,
            deploymentId);
    }

    private static OpenAICompatibleOptions Prepare(AzureOpenAIOptions? options)
    {
        options ??= new AzureOpenAIOptions();
        if (!string.IsNullOrEmpty(options.ApiKey) && options.TokenProvider != null)
        {
            throw new ArgumentException("Both apiKey and tokenProvider were provided. Please use only one authentication method.");
        }

        var baseUrl = ResolveBaseUrl(options);
        var prepared = new OpenAICompatibleOptions
        {
            ProviderName = ProviderId,
            BaseUrl = baseUrl,
            ApiKey = options.TokenProvider == null ? options.ApiKey : "azure-ad",
            ApiKeyEnvironmentVariable = "AZURE_API_KEY",
            ApiKeyStyle = options.TokenProvider == null ? ApiKeyStyle.ApiKeyHeader : ApiKeyStyle.Bearer,
            ApiKeyHeaderName = "api-key",
            AzureApiVersion = string.IsNullOrEmpty(options.ApiVersion) ? "v1" : options.ApiVersion,
            AzureUseDeploymentBasedUrls = options.UseDeploymentBasedUrls,
            SupportsEmbeddings = true,
            SupportsImages = false,
        };
        foreach (var pair in options.Headers)
        {
            prepared.Headers[pair.Key] = pair.Value;
        }

        return prepared;
    }

    private static string ResolveBaseUrl(AzureOpenAIOptions options)
    {
        if (!string.IsNullOrEmpty(options.BaseUrl))
        {
            return options.BaseUrl;
        }

        // ResourceName is concatenated into the host. A value such as
        // user@internal:8080/# would send the request somewhere else.
        var resource = options.ResourceName ?? "resource";
        if (!IsValidHostnamePart(resource))
        {
            throw new ArgumentException(
                "An Azure resource name must be a single DNS label (letters, digits, and hyphens). Custom endpoints belong in BaseUrl.",
                nameof(options.ResourceName));
        }

        return "https://" + resource + ".openai.azure.com";
    }

    private static bool IsValidHostnamePart(string value)
    {
        var match = HostnamePart.Match(value);
        return match.Success && match.Value == value;
    }
}

/// <summary>Replaces the placeholder bearer value with a token from the provider on every request.</summary>
internal sealed class AzureEntraTokenHandler : DelegatingHandler
{
    private readonly Func<string> _tokenProvider;

    /// <summary>Creates a handler that calls <paramref name="tokenProvider"/> for each request.</summary>
    public AzureEntraTokenHandler(Func<string> tokenProvider, HttpMessageHandler inner)
        : base(inner)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove("api-key");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider());
        return base.SendAsync(request, cancellationToken);
    }
}

/// <summary>Registers Azure OpenAI.</summary>
public static class AzureOpenAIServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AzureOpenAIProvider"/>.</summary>
    public static IServiceCollection AddAzureOpenAI(this IServiceCollection services, Action<AzureOpenAIOptions>? configure = null)
    {
        services.AddHttpClient(AzureOpenAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new AzureOpenAIOptions();
            configure?.Invoke(options);
            if (options.TokenProvider != null)
            {
                return AzureOpenAIProvider.Create(options);
            }

            return new AzureOpenAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AzureOpenAIProvider.ProviderId), options);
        });
        return services;
    }
}
