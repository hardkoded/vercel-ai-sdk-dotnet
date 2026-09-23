// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.Voyage;

/// <summary>Voyage provider.</summary>
public sealed class VoyageProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "voyage";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.voyageai.com/v1";

    /// <summary>Creates a provider.</summary>
    public VoyageProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new VoyageProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new VoyageProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "VOYAGE_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        return options;
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId) => new Embedding(this, modelId);

    private sealed class Embedding : IEmbeddingModel
    {
        private readonly VoyageProvider _provider;
        public Embedding(VoyageProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string SpecificationVersion => "V4";
        public string Provider => "voyage";
        public string ModelId { get; }
        public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
        {
            var input = new JsonArray();
            foreach (var value in values) input.Add(value);
            var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/embeddings"), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var vectors = new List<float[]>();
            foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
            {
                var embedding = item.GetProperty("embedding");
                var vector = new float[embedding.GetArrayLength()];
                var index = 0;
                foreach (var number in embedding.EnumerateArray()) vector[index++] = number.GetSingle();
                vectors.Add(vector);
            }
            return new EmbeddingResult(vectors, null);
        }
    }

}

/// <summary>Registers Voyage.</summary>
public static class VoyageServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddVoyage(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(VoyageProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new VoyageProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(VoyageProvider.ProviderId), options);
        });
        return services;
    }
}
