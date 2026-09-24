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
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    /// <summary>Options.</summary>
    public GatewayOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static GatewayProvider Create(GatewayOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
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

    internal Dictionary<string, string?> Headers(string specificationHeader, string specificationValue, string modelHeader, string modelId, bool? streaming)
    {
        var key = ApiKeys.Require(Options.ApiKey, ApiKeyEnvironmentVariable);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = "Bearer " + key,
            ["ai-gateway-protocol-version"] = "0.0.1",
            ["ai-gateway-auth-method"] = "api-key",
            [specificationHeader] = specificationValue,
            [modelHeader] = modelId,
        };
        if (streaming is { } value)
        {
            headers["ai-language-model-streaming"] = value ? "true" : "false";
        }

        return headers;
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
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.Route("language-model"),
            V4Json.CallOptions(options),
            _provider.Headers("ai-language-model-specification-version", "4", "ai-language-model-id", ModelId, false),
            cancellationToken).ConfigureAwait(false);
        return V4Json.ParseGenerate(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var data in _provider.Http.SendSseAsync(
            _provider.Route("language-model"),
            V4Json.CallOptions(options),
            _provider.Headers("ai-language-model-specification-version", "4", "ai-language-model-id", ModelId, true),
            cancellationToken).ConfigureAwait(false))
        {
            LanguageModelStreamPart? part;
            try
            {
                part = V4Json.ParseStreamPart(data);
            }
            catch (JsonException)
            {
                continue;
            }

            if (part != null)
            {
                yield return part;
            }
        }
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
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.Route("embedding-model"),
            body.ToJsonString(),
            _provider.Headers("ai-embedding-model-specification-version", "4", "ai-model-id", ModelId, null),
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
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.Route("image-model"),
            body.ToJsonString(),
            _provider.Headers("ai-image-model-specification-version", "4", "ai-model-id", ModelId, null),
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
