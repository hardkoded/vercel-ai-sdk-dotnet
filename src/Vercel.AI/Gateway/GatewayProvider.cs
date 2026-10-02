// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>AI Gateway settings. The default base URL is <c>https://ai-gateway.vercel.sh/v4/ai</c>.</summary>
public sealed class GatewayOptions
{
    /// <summary>Gateway origin for the V4 routes.</summary>
    public string BaseUrl { get; set; } = "https://ai-gateway.vercel.sh/v4/ai";

    /// <summary>Explicit key. Falls back to <c>AI_GATEWAY_API_KEY</c>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Value of <c>ai-gateway-auth-method</c>. <c>api-key</c> and <c>oidc</c> select the authentication error hint.
    /// </summary>
    public string AuthMethod { get; set; } = "api-key";

    /// <summary>Optional observability headers, usually named <c>ai-o11y-*</c>, sent on every call.</summary>
    public IReadOnlyDictionary<string, string>? ObservabilityHeaders { get; set; }
}

/// <summary>
/// Vercel AI Gateway provider. Language, embedding, and image calls use the Gateway V4 routes
/// (<c>/language-model</c>, <c>/embedding-model</c>, <c>/image-model</c>).
/// </summary>
public sealed class GatewayProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "gateway";

    /// <summary>Environment variable for the Gateway key.</summary>
    public const string ApiKeyEnvironmentVariable = "AI_GATEWAY_API_KEY";

    /// <summary>Creates a Gateway provider.</summary>
    public GatewayProvider(GatewayOptions? options, HttpClient httpClient)
        : base(ProviderId)
    {
        Options = options ?? new GatewayOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)), new RetryPolicy { MaxRetries = 0 });
    }

    /// <summary>Options.</summary>
    public GatewayOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static GatewayProvider Create(GatewayOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        client.Timeout = Timeout.InfiniteTimeSpan;
        return new GatewayProvider(options, client);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new GatewayLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new GatewayEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return new GatewayImageModel(this, modelId);
    }

    internal Dictionary<string, string?> Headers(
        string specificationHeader,
        string specificationValue,
        string modelHeader,
        string modelId,
        bool? streaming,
        IReadOnlyDictionary<string, string?>? callHeaders = null,
        IReadOnlyDictionary<string, string>? observabilityHeaders = null)
    {
        var key = ApiKeys.Require(Options.ApiKey, ApiKeyEnvironmentVariable);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = "Bearer " + key,
            [GatewayHeaders.ProtocolVersion] = "0.0.1",
            [GatewayHeaders.AuthMethod] = string.IsNullOrEmpty(Options.AuthMethod) ? "api-key" : Options.AuthMethod,
        };
        CopyHeaders(headers, callHeaders);
        headers[specificationHeader] = specificationValue;
        headers[modelHeader] = modelId;
        if (streaming is { } value)
        {
            headers["ai-language-model-streaming"] = value ? "true" : "false";
        }

        if (observabilityHeaders != null)
        {
            foreach (var pair in observabilityHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    /// <summary>Posts JSON. HTTP failures become <see cref="GatewayError"/> values.</summary>
    internal async Task<JsonDocument> PostAsync(Uri uri, string json, Dictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        try
        {
            return await Http.SendJsonAsync(HttpMethod.Post, uri, json, headers, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not JsonException)
        {
            throw Normalize(exception, headers);
        }
    }

    /// <summary>Maps a transport failure onto a Gateway error. An existing <see cref="GatewayError"/> is returned unchanged.</summary>
    internal static Exception Normalize(Exception exception, IReadOnlyDictionary<string, string?>? headers)
    {
        if (exception is GatewayError)
        {
            return exception;
        }

        return GatewayErrors.AsGatewayError(exception, GatewayErrors.ParseAuthMethod(headers));
    }

    private static void CopyHeaders(Dictionary<string, string?> target, IReadOnlyDictionary<string, string?>? source)
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

    internal Uri Route(string path)
    {
        return ApiKeys.Combine(Options.BaseUrl, path);
    }
}

/// <summary>Gateway language model. The request body is the V4 call options.</summary>
public sealed class GatewayLanguageModel : ILanguageModel
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway language model.</summary>
    public GatewayLanguageModel(GatewayProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => GatewayProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var headers = RequestHeaders(options, false);
        ProviderTextResponse response;
        try
        {
            response = await _provider.Http.SendJsonStringAsync(
                HttpMethod.Post,
                _provider.Route("language-model"),
                V4Json.CallOptions(options),
                headers,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not JsonException)
        {
            throw GatewayProvider.Normalize(exception, headers);
        }

        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        return V4Json.ParseGenerate(document.RootElement, response.Body, response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var headers = RequestHeaders(options, true);
        var stream = _provider.Http.SendSseAsync(
            _provider.Route("language-model"),
            V4Json.CallOptions(options),
            headers,
            cancellationToken);
        var enumerator = stream.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                string data;
                bool moved;
                try
                {
                    moved = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    data = moved ? enumerator.Current : string.Empty;
                }
                catch (Exception exception) when (exception is not JsonException)
                {
                    throw GatewayProvider.Normalize(exception, headers);
                }

                if (!moved)
                {
                    yield break;
                }

                LanguageModelStreamPart? part;
                try
                {
                    part = V4Json.ParseStreamPart(data);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (part == null || (part is RawStreamPart && !options.IncludeRawChunks))
                {
                    continue;
                }

                yield return part;
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    private Dictionary<string, string?> RequestHeaders(LanguageModelCallOptions options, bool streaming)
    {
        return _provider.Headers(
            "ai-language-model-specification-version",
            "4",
            "ai-language-model-id",
            ModelId,
            streaming,
            options.Headers,
            _provider.Options.ObservabilityHeaders);
    }
}

/// <summary>Gateway embedding model.</summary>
public sealed class GatewayEmbeddingModel : IEmbeddingModel
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway embedding model.</summary>
    public GatewayEmbeddingModel(GatewayProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => GatewayProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        var input = new JsonArray();
        foreach (var value in values)
        {
            input.Add(value);
        }

        var body = new JsonObject { ["values"] = input };
        var headers = _provider.Headers("ai-embedding-model-specification-version", "4", "ai-model-id", ModelId, null);
        using var document = await _provider.PostAsync(
            _provider.Route("embedding-model"),
            body.ToJsonString(),
            headers,
            cancellationToken).ConfigureAwait(false);
        var vectors = new List<float[]>();
        foreach (var embedding in document.RootElement.GetProperty("embeddings").EnumerateArray())
        {
            var vector = new float[embedding.GetArrayLength()];
            var index = 0;
            foreach (var number in embedding.EnumerateArray())
            {
                vector[index++] = number.GetSingle();
            }

            vectors.Add(vector);
        }

        int? tokens = null;
        if (document.RootElement.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("tokens", out var tokenCount))
        {
            tokens = tokenCount.GetInt32();
        }

        return new EmbeddingResult(vectors, tokens);
    }
}

/// <summary>Gateway image model.</summary>
public sealed class GatewayImageModel : IImageModel
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway image model.</summary>
    public GatewayImageModel(GatewayProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => GatewayProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["prompt"] = options.Prompt,
            ["n"] = options.Count,
            ["size"] = options.Size,
            ["aspectRatio"] = options.AspectRatio,
        };
        var headers = _provider.Headers("ai-image-model-specification-version", "4", "ai-model-id", ModelId, null);
        using var document = await _provider.PostAsync(
            _provider.Route("image-model"),
            body.ToJsonString(),
            headers,
            cancellationToken).ConfigureAwait(false);
        var images = new List<GeneratedImage>();
        if (document.RootElement.TryGetProperty("images", out var array))
        {
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    images.Add(new GeneratedImage("image/png", Convert.FromBase64String(item.GetString() ?? string.Empty), null));
                    continue;
                }

                string? url = item.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;
                byte[]? data = null;
                if (item.TryGetProperty("b64", out var b64) || item.TryGetProperty("base64", out b64))
                {
                    data = Convert.FromBase64String(b64.GetString() ?? string.Empty);
                }

                var mediaType = item.TryGetProperty("mediaType", out var media) ? media.GetString() ?? "image/png" : "image/png";
                images.Add(new GeneratedImage(mediaType, data, url));
            }
        }

        return new ImageGenerationResult(images);
    }
}

/// <summary>Registers the Gateway provider.</summary>
public static class GatewayServiceCollectionExtensions
{
    /// <summary>Registers <see cref="GatewayProvider"/>.</summary>
    public static IServiceCollection AddGateway(this IServiceCollection services, Action<GatewayOptions>? configure = null)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddHttpClient(GatewayProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new GatewayOptions();
            configure?.Invoke(options);
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(GatewayProvider.ProviderId);
            return new GatewayProvider(options, http);
        });
        return services;
    }
}
