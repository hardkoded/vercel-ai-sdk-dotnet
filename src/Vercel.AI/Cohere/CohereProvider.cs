// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cohere;

/// <summary>Cohere settings. The default origin is <c>https://api.cohere.com/v2</c>.</summary>
public sealed class CohereOptions
{
    /// <summary>API origin.</summary>
    public string BaseUrl { get; set; } = "https://api.cohere.com/v2";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Headers added to every request.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Supplies citation ids. A null value uses <c>id-0</c>, <c>id-1</c>, and so on.</summary>
    public Func<string>? GenerateId { get; set; }
}

/// <summary>Cohere provider for chat, embeddings, and rerank.</summary>
public sealed class CohereProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "cohere";

    /// <summary>User-Agent product sent when the caller does not set one.</summary>
    public const string UserAgent = "ai-sdk/cohere/0.0.0-test";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public CohereProvider(HttpClient httpClient, CohereOptions? options = null)
        : base(ProviderName)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? new CohereOptions();
        Http = new ProviderHttp(_httpClient);
    }

    /// <summary>Options.</summary>
    public CohereOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static CohereProvider Create(CohereOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new CohereProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId) => new CohereLanguageModel(this, modelId);

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId) => new CohereEmbeddingModel(this, modelId);

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId) => new CohereRerankingModel(this, modelId);

    /// <summary>Merges provider headers, caller headers, and the bearer token.</summary>
    public Dictionary<string, string?> CreateHeaders(IReadOnlyDictionary<string, string?>? requestHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        Copy(Options.Headers, headers);
        headers["Authorization"] = "Bearer " + ApiKeys.Require(Options.ApiKey, "COHERE_API_KEY");
        Copy(requestHeaders, headers);
        if (!headers.TryGetValue("User-Agent", out var userAgent) || string.IsNullOrEmpty(userAgent))
        {
            headers["User-Agent"] = UserAgent;
        }

        return headers;
    }

    internal async Task<ProviderTextResponse> PostJsonAsync(Uri uri, string json, IReadOnlyDictionary<string, string?>? requestHeaders, CancellationToken cancellationToken)
    {
        return await Http.SendJsonStringAsync(HttpMethod.Post, uri, json, CreateHeaders(requestHeaders), cancellationToken).ConfigureAwait(false);
    }

    internal async Task<HttpResponseMessage> SendStreamAsync(Uri uri, string json, IReadOnlyDictionary<string, string?>? requestHeaders, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        Apply(request, CreateHeaders(requestHeaders));
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
        if (source == null)
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
}

/// <summary>Registers Cohere.</summary>
public static class CohereServiceCollectionExtensions
{
    /// <summary>Adds <see cref="CohereProvider"/>.</summary>
    public static IServiceCollection AddCohere(this IServiceCollection services, Action<CohereOptions>? configure = null)
    {
        services.AddHttpClient(CohereProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new CohereOptions();
            configure?.Invoke(options);
            return new CohereProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(CohereProvider.ProviderName), options);
        });
        return services;
    }
}
