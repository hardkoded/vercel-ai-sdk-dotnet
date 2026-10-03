// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;

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

    /// <summary>API version query parameter.</summary>
    public string ApiVersion { get; set; } = "2024-10-21";

    /// <summary>Microsoft Entra token provider. Each call receives a fresh bearer token.</summary>
    public Func<string>? TokenProvider { get; set; }

    /// <summary>Headers copied onto every request.</summary>
    public Dictionary<string, string> Headers { get; } = new();
}

/// <summary>Azure OpenAI provider. Chat calls use the deployments route and the <c>api-key</c> header.</summary>
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
            AzureApiVersion = options.ApiVersion,
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
