// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.Cohere;

/// <summary>Cohere settings. The default origin is <c>https://api.cohere.com/v2</c>.</summary>
public sealed class CohereOptions
{
    /// <summary>API origin.</summary>
    public string BaseUrl { get; set; } = "https://api.cohere.com/v2";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }
}

/// <summary>Cohere provider for chat, embeddings, and rerank.</summary>
public sealed class CohereProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "cohere";

    /// <summary>Creates a provider.</summary>
    public CohereProvider(HttpClient httpClient, CohereOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new CohereOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
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

    internal Dictionary<string, string?> Headers()
    {
        return new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer " + ApiKeys.Require(Options.ApiKey, "COHERE_API_KEY"),
        };
    }
}

/// <summary>Cohere v2 chat model.</summary>
public sealed class CohereLanguageModel : ILanguageModel
{
    private readonly CohereProvider _provider;

    /// <summary>Creates a model.</summary>
    public CohereLanguageModel(CohereProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "chat"), Build(options).ToJsonString(), _provider.Headers(), cancellationToken).ConfigureAwait(false);
        return Parse(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(result.Text))
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private JsonObject Build(LanguageModelCallOptions options)
    {
        var messages = new JsonArray();
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage system)
            {
                messages.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
            }
            else if (message is UserModelMessage user)
            {
                var text = new StringBuilder();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart textPart)
                    {
                        text.Append(textPart.Text);
                    }
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = text.ToString() });
            }
            else if (message is AssistantModelMessage assistant)
            {
                messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = assistant.Text });
            }
        }

        var body = new JsonObject { ["model"] = ModelId, ["messages"] = messages };
        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.MaxOutputTokens is { } max)
        {
            body["max_tokens"] = max;
        }

        return body;
    }

    private static LanguageModelGenerateResult Parse(JsonElement root)
    {
        var text = new StringBuilder();
        if (root.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var partText))
                {
                    text.Append(partText.GetString());
                }
            }
        }
        else if (root.TryGetProperty("text", out var legacy))
        {
            text.Append(legacy.GetString());
        }

        var raw = root.TryGetProperty("finish_reason", out var finish) ? finish.GetString() : "COMPLETE";
        return new LanguageModelGenerateResult(new GeneratedContent[] { new GeneratedText(text.ToString()) }, FinishReasons.Parse(raw), LanguageModelUsage.Empty, raw);
    }
}

/// <summary>Cohere embedding model.</summary>
public sealed class CohereEmbeddingModel : IEmbeddingModel
{
    private readonly CohereProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public CohereEmbeddingModel(CohereProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        var texts = new JsonArray();
        foreach (var value in values)
        {
            texts.Add(value);
        }

        var body = new JsonObject { ["model"] = ModelId, ["texts"] = texts, ["input_type"] = "search_document" };
        using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "embed"), body.ToJsonString(), _provider.Headers(), cancellationToken).ConfigureAwait(false);
        var root = document.RootElement;
        JsonElement vectorsElement;
        if (root.TryGetProperty("embeddings", out var embeddings) && embeddings.ValueKind == JsonValueKind.Object && embeddings.TryGetProperty("float", out var floats))
        {
            vectorsElement = floats;
        }
        else
        {
            vectorsElement = root.GetProperty("embeddings");
        }

        var vectors = new List<float[]>();
        foreach (var embedding in vectorsElement.EnumerateArray())
        {
            var vector = new float[embedding.GetArrayLength()];
            var index = 0;
            foreach (var number in embedding.EnumerateArray())
            {
                vector[index++] = number.GetSingle();
            }

            vectors.Add(vector);
        }

        return new EmbeddingResult(vectors, null);
    }
}

/// <summary>Cohere rerank model.</summary>
public sealed class CohereRerankingModel : IRerankingModel
{
    private readonly CohereProvider _provider;

    /// <summary>Creates a reranking model.</summary>
    public CohereRerankingModel(CohereProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken)
    {
        var docs = new JsonArray();
        foreach (var document in documents)
        {
            docs.Add(document);
        }

        var body = new JsonObject { ["model"] = ModelId, ["query"] = query, ["documents"] = docs };
        if (topN is { } top)
        {
            body["top_n"] = top;
        }

        using var response = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "rerank"), body.ToJsonString(), _provider.Headers(), cancellationToken).ConfigureAwait(false);
        var items = new List<RerankItem>();
        foreach (var item in response.RootElement.GetProperty("results").EnumerateArray())
        {
            var score = item.TryGetProperty("relevance_score", out var relevance) ? relevance.GetDouble() : 0;
            items.Add(new RerankItem(item.GetProperty("index").GetInt32(), score));
        }

        return new RerankResult(items);
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
