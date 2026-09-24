// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Provider that speaks the OpenAI Chat Completions, embeddings, and images APIs.</summary>
public class OpenAICompatibleProvider : ProviderBase
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider. The caller owns <paramref name="httpClient"/>.</summary>
    public OpenAICompatibleProvider(OpenAICompatibleOptions options, HttpClient httpClient)
        : base(options?.ProviderName ?? throw new ArgumentNullException(nameof(options)))
    {
        Options = options;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Http = new ProviderHttp(httpClient);
    }

    /// <summary>Options used to build requests.</summary>
    public OpenAICompatibleOptions Options { get; }

    /// <summary>HTTP helper used by the models.</summary>
    public ProviderHttp Http { get; }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new OpenAICompatibleLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        if (!Options.SupportsEmbeddings)
        {
            return base.EmbeddingModel(modelId);
        }

        return new OpenAICompatibleEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        if (!Options.SupportsImages)
        {
            return base.ImageModel(modelId);
        }

        return new OpenAICompatibleImageModel(this, modelId);
    }

    /// <summary>Creates a provider with an <see cref="HttpClient"/> that uses <paramref name="handler"/>.</summary>
    public static OpenAICompatibleProvider Create(OpenAICompatibleOptions options, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new OpenAICompatibleProvider(options, client);
    }

    /// <summary>Resolves the API key and builds request headers.</summary>
    public Dictionary<string, string?> CreateHeaders()
    {
        var key = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable, Options.AdditionalApiKeyEnvironmentVariables);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        switch (Options.ApiKeyStyle)
        {
            case ApiKeyStyle.Bearer:
                headers["Authorization"] = "Bearer " + key;
                break;
            case ApiKeyStyle.Token:
                headers["Authorization"] = "Token " + key;
                break;
            case ApiKeyStyle.Key:
                headers["Authorization"] = "Key " + key;
                break;
            case ApiKeyStyle.RawAuthorization:
                headers["Authorization"] = key;
                break;
            case ApiKeyStyle.ApiKeyHeader:
            case ApiKeyStyle.CustomHeader:
                headers[Options.ApiKeyHeaderName] = key;
                break;
        }

        return headers;
    }

    /// <summary>Chat Completions URL for a model. Azure deployment routes include the model id.</summary>
    public Uri ChatUri(string modelId)
    {
        if (!string.IsNullOrEmpty(Options.AzureApiVersion))
        {
            return ApiKeys.Combine(
                Options.BaseUrl,
                "openai/deployments/" + Uri.EscapeDataString(modelId) + "/chat/completions?api-version=" + Options.AzureApiVersion);
        }

        return ApiKeys.Combine(Options.BaseUrl, Options.ChatCompletionsPath);
    }

    /// <summary>Embeddings URL.</summary>
    public Uri EmbeddingsUri(string modelId)
    {
        if (!string.IsNullOrEmpty(Options.AzureApiVersion))
        {
            return ApiKeys.Combine(
                Options.BaseUrl,
                "openai/deployments/" + Uri.EscapeDataString(modelId) + "/embeddings?api-version=" + Options.AzureApiVersion);
        }

        return ApiKeys.Combine(Options.BaseUrl, Options.EmbeddingsPath);
    }

    /// <summary>Images URL.</summary>
    public Uri ImagesUri()
    {
        return ApiKeys.Combine(Options.BaseUrl, Options.ImagesPath);
    }
}

/// <summary>OpenAI-compatible embeddings model.</summary>
public sealed class OpenAICompatibleEmbeddingModel : IEmbeddingModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public OpenAICompatibleEmbeddingModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name;

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

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = input,
        };

        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.EmbeddingsUri(ModelId),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        var vectors = new List<float[]>();
        foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
        {
            var embedding = item.GetProperty("embedding");
            var vector = new float[embedding.GetArrayLength()];
            var index = 0;
            foreach (var number in embedding.EnumerateArray())
            {
                vector[index++] = number.GetSingle();
            }

            vectors.Add(vector);
        }

        int? tokens = null;
        if (document.RootElement.TryGetProperty("usage", out var usage) && usage.TryGetProperty("total_tokens", out var total))
        {
            tokens = total.GetInt32();
        }

        return new EmbeddingResult(vectors, tokens);
    }
}

/// <summary>OpenAI-compatible image model.</summary>
public sealed class OpenAICompatibleImageModel : IImageModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates an image model.</summary>
    public OpenAICompatibleImageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["prompt"] = options.Prompt,
            ["n"] = options.Count,
        };
        if (!string.IsNullOrEmpty(options.Size))
        {
            body["size"] = options.Size;
        }

        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.ImagesUri(),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        var images = new List<GeneratedImage>();
        foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
        {
            string? url = item.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;
            byte[]? data = null;
            if (item.TryGetProperty("b64_json", out var b64))
            {
                data = Convert.FromBase64String(b64.GetString() ?? string.Empty);
            }

            images.Add(new GeneratedImage(data != null ? "image/png" : "image/*", data, url));
        }

        return new ImageGenerationResult(images);
    }
}
