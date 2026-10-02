// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini settings.</summary>
public class GoogleOptions
{
    /// <summary>API origin.</summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Environment variable.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "GOOGLE_GENERATIVE_AI_API_KEY";

    /// <summary>When set, requests use <c>Authorization: Bearer</c> instead of <c>x-goog-api-key</c>.</summary>
    public bool UseBearerToken { get; set; }

    /// <summary>Extra headers merged into every request.</summary>
    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>User-Agent suffix. Defaults to <c>ai-sdk/google</c>.</summary>
    public string UserAgent { get; set; } = "ai-sdk/google";

    /// <summary>Id factory used for tool calls that the response does not identify.</summary>
    public Func<string>? GenerateId { get; set; }

    /// <summary>Embedding provider options read by <see cref="GoogleEmbeddingModel"/>.</summary>
    public JsonElement? EmbeddingProviderOptions { get; set; }
}

/// <summary>Google Gemini provider.</summary>
public class GoogleProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "google";

    /// <summary>Creates a provider.</summary>
    public GoogleProvider(HttpClient httpClient, GoogleOptions? options = null)
        : this(httpClient, options, "google.generative-ai")
    {
    }

    /// <summary>Creates a provider with an explicit model provider id.</summary>
    protected GoogleProvider(HttpClient httpClient, GoogleOptions? options, string modelProvider)
        : base(ProviderName)
    {
        Options = options ?? new GoogleOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
        ModelProvider = modelProvider ?? "google.generative-ai";
    }

    /// <summary>Options.</summary>
    public GoogleOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Provider id recorded on models, such as <c>google.generative-ai</c> or <c>google.vertex.chat</c>.</summary>
    public string ModelProvider { get; }

    /// <summary>Clock used for response timestamps.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <summary>Creates a provider.</summary>
    public static GoogleProvider Create(GoogleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GoogleProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new GoogleLanguageModel(this, modelId);
    }

    /// <summary>Language model that posts to the Interactions API.</summary>
    public virtual ILanguageModel Interactions(string modelId)
    {
        return new GoogleInteractionsModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new GoogleEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return new GoogleImageModel(this, modelId);
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return new GoogleSpeechModel(this, modelId);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new GoogleTranscriptionModel(this, modelId);
    }

    /// <inheritdoc />
    public override IRealtimeModel RealtimeModel(string modelId)
    {
        return new GoogleRealtimeModel(this, modelId);
    }

    /// <summary>Builds request headers. An API key is required.</summary>
    internal Dictionary<string, string?> Headers()
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        if (!ContainsKey(headers, "user-agent") && !string.IsNullOrEmpty(Options.UserAgent))
        {
            headers["user-agent"] = Options.UserAgent;
        }

        var key = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable);
        if (Options.UseBearerToken)
        {
            if (!ContainsKey(headers, "Authorization"))
            {
                headers["Authorization"] = "Bearer " + key;
            }
        }
        else if (!ContainsKey(headers, "x-goog-api-key"))
        {
            headers["x-goog-api-key"] = key;
        }

        return headers;
    }

    private static bool ContainsKey(Dictionary<string, string?> headers, string name)
    {
        foreach (var key in headers.Keys)
        {
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>Realtime model. Event mapping is local; opening the Live socket is not available on this runtime.</summary>
public sealed class GoogleRealtimeModel : IRealtimeModel
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates a realtime model.</summary>
    public GoogleRealtimeModel(GoogleProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _provider.ModelProvider; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        var key = _provider.Options.UseBearerToken ? null : _provider.Options.ApiKey;
        return GoogleSpeechTranslation.WebSocketUrl(_provider.Options.BaseUrl, key);
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException("Gemini Live WebSocket sessions are not opened by this port. Map events with GoogleRealtime instead.");
    }
}

/// <summary>Registers Google and Vertex.</summary>
public static class GoogleServiceCollectionExtensions
{
    /// <summary>Adds <see cref="GoogleProvider"/>.</summary>
    public static IServiceCollection AddGoogle(this IServiceCollection services, Action<GoogleOptions>? configure = null)
    {
        services.AddHttpClient(GoogleProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new GoogleOptions();
            configure?.Invoke(options);
            return new GoogleProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(GoogleProvider.ProviderName), options);
        });
        return services;
    }

    /// <summary>Adds <see cref="GoogleVertexProvider"/>.</summary>
    public static IServiceCollection AddGoogleVertex(this IServiceCollection services, Action<VertexOptions>? configure = null)
    {
        services.AddHttpClient("google-vertex");
        services.AddSingleton(sp =>
        {
            var options = new VertexOptions();
            configure?.Invoke(options);
            return new GoogleVertexProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient("google-vertex"), options);
        });
        return services;
    }
}
