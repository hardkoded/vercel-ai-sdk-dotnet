// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

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

    /// <summary>Headers added to every request.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>
    /// Microsoft Entra token provider. When set, requests use <c>Authorization: Bearer</c>
    /// and do not send <c>api-key</c>.
    /// </summary>
    public Func<CancellationToken, Task<string>>? TokenProvider { get; set; }
}

/// <summary>Azure OpenAI provider. Chat calls use the deployments route and the <c>api-key</c> header.</summary>
public sealed class AzureOpenAIProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "azure";

    /// <summary>User-Agent product sent when the caller does not set one.</summary>
    public const string UserAgent = "ai-sdk/azure/0.0.0-test";

    // $ can succeed before a trailing newline, so the match must cover the whole value.
    private static readonly Regex HostnamePart = new(
        "^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public AzureOpenAIProvider(HttpClient httpClient, AzureOpenAIOptions? options = null)
        : base(ProviderId)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? new AzureOpenAIOptions();
        if (!string.IsNullOrEmpty(Options.ApiKey) && Options.TokenProvider != null)
        {
            throw new ArgumentException(
                "Both apiKey and tokenProvider were provided. Please use only one authentication method.");
        }

        BaseUrl = ResolveBaseUrl(Options);
        Http = new ProviderHttp(_httpClient);
    }

    /// <summary>Options.</summary>
    public AzureOpenAIOptions Options { get; }

    /// <summary>Resolved origin.</summary>
    public string BaseUrl { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static AzureOpenAIProvider Create(AzureOpenAIOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AzureOpenAIProvider(client, options);
    }

    /// <summary>Chat Completions model. This is also <see cref="LanguageModel"/>.</summary>
    public ILanguageModel Chat(string modelId)
    {
        return new AzureChatLanguageModel(this, modelId, deepseek: false);
    }

    /// <summary>Responses model.</summary>
    public AzureResponsesLanguageModel Responses(string modelId)
    {
        return new AzureResponsesLanguageModel(this, modelId);
    }

    /// <summary>DeepSeek chat model on Azure.</summary>
    public ILanguageModel Deepseek(string modelId)
    {
        return new AzureChatLanguageModel(this, modelId, deepseek: true);
    }

    /// <summary>Legacy completions model.</summary>
    public ILanguageModel Completion(string modelId)
    {
        return new AzureCompletionLanguageModel(this, modelId);
    }

    /// <summary>Embeddings model.</summary>
    public AzureEmbeddingModel Embedding(string modelId)
    {
        return new AzureEmbeddingModel(this, modelId);
    }

    /// <summary>Image model.</summary>
    public AzureImageModel Image(string modelId)
    {
        return new AzureImageModel(this, modelId);
    }

    /// <summary>Speech model.</summary>
    public ISpeechModel Speech(string modelId)
    {
        return new AzureSpeechModel(this, modelId);
    }

    /// <summary>Transcription model.</summary>
    public ITranscriptionModel Transcription(string modelId)
    {
        return new AzureTranscriptionModel(this, modelId);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return Chat(modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return Embedding(modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return Image(modelId);
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return Speech(modelId);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return Transcription(modelId);
    }

    /// <summary>Foundry project endpoints require an explicit message item type.</summary>
    public bool UseExplicitMessageTypes
    {
        get
        {
            if (string.IsNullOrEmpty(Options.BaseUrl) || !Uri.TryCreate(Options.BaseUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Host.EndsWith(".services.ai.azure.com", StringComparison.OrdinalIgnoreCase)
                && uri.AbsolutePath.StartsWith("/api/projects/", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Deployments URL for one operation, including the API version query.</summary>
    public Uri DeploymentUri(string modelId, string operation)
    {
        var path = "openai/deployments/"
            + Uri.EscapeDataString(modelId)
            + "/"
            + operation.TrimStart('/')
            + "?api-version="
            + Uri.EscapeDataString(Options.ApiVersion ?? string.Empty);
        return ApiKeys.Combine(BaseUrl, path);
    }

    /// <summary>Merges provider headers, caller headers, and authentication.</summary>
    public async Task<Dictionary<string, string?>> CreateHeadersAsync(
        IReadOnlyDictionary<string, string?>? requestHeaders,
        CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        Copy(Options.Headers, headers);
        if (Options.TokenProvider != null)
        {
            var token = await Options.TokenProvider(cancellationToken).ConfigureAwait(false);
            headers["Authorization"] = "Bearer " + token;
        }
        else
        {
            headers["api-key"] = ApiKeys.Require(Options.ApiKey, "AZURE_API_KEY");
        }

        Copy(requestHeaders, headers);
        if (!headers.TryGetValue("User-Agent", out var userAgent) || string.IsNullOrEmpty(userAgent))
        {
            headers["User-Agent"] = UserAgent;
        }

        return headers;
    }

    /// <summary>Posts JSON and returns the body with response headers.</summary>
    internal async Task<ProviderTextResponse> PostJsonAsync(
        Uri uri,
        string json,
        IReadOnlyDictionary<string, string?>? requestHeaders,
        CancellationToken cancellationToken)
    {
        var headers = await CreateHeadersAsync(requestHeaders, cancellationToken).ConfigureAwait(false);
        return await Http.SendJsonStringAsync(HttpMethod.Post, uri, json, headers, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Posts JSON and returns the response after headers are available.</summary>
    internal async Task<HttpResponseMessage> SendStreamAsync(
        Uri uri,
        string json,
        IReadOnlyDictionary<string, string?>? requestHeaders,
        CancellationToken cancellationToken)
    {
        var headers = await CreateHeadersAsync(requestHeaders, cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        Apply(request, headers);
        request.Content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            response.Dispose();
            throw ProviderHttp.MapStatus((int)response.StatusCode, body);
        }

        return response;
    }

    internal static Dictionary<string, string> CopyResponseHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }

    private static void Copy(IReadOnlyDictionary<string, string?>? source, Dictionary<string, string?> target)
    {
        if (source is null)
        {
            return;
        }

        foreach (var pair in source)
        {
            target[pair.Key] = pair.Value;
        }
    }

    private static void Apply(HttpRequestMessage request, IReadOnlyDictionary<string, string?> headers)
    {
        foreach (var pair in headers)
        {
            if (string.IsNullOrEmpty(pair.Value) || pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var value = pair.Value!;
                var space = value.IndexOf(' ');
                if (space > 0)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(value.Substring(0, space), value.Substring(space + 1));
                }
                else
                {
                    request.Headers.TryAddWithoutValidation("Authorization", value);
                }

                continue;
            }

            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
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
