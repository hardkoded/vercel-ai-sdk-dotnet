// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

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
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AzureOpenAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(AzureOpenAIOptions? options)
    {
        options ??= new AzureOpenAIOptions();
        var baseUrl = ResolveBaseUrl(options);
        return new OpenAICompatibleOptions
        {
            ProviderName = ProviderId,
            BaseUrl = baseUrl,
            ApiKey = options.ApiKey,
            ApiKeyEnvironmentVariable = "AZURE_API_KEY",
            ApiKeyStyle = ApiKeyStyle.ApiKeyHeader,
            ApiKeyHeaderName = "api-key",
            AzureApiVersion = options.ApiVersion,
            SupportsEmbeddings = true,
            SupportsImages = false,
        };
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
            return new AzureOpenAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AzureOpenAIProvider.ProviderId), options);
        });
        return services;
    }
}
