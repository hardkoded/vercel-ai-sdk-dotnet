# Voyage

`VoyageProvider` in the `Vercel.AI.Voyage` namespace creates the Voyage models that this port implements. The API reference lists every option: [VoyageProvider](/api/Vercel.AI.Voyage/VoyageProvider/).

Browse the upstream catalog of Voyage models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Embedding | `EmbeddingModel` | `voyage-3.5` |
| Reranking | `RerankingModel` | `rerank-2.5` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Embedding model:

```csharp
using Vercel.AI.Voyage;

var model = (VoyageProvider.VoyageEmbeddingModel)VoyageProvider.Create().EmbeddingModel("voyage-3.5");
var result = await model.EmbedAsync(
    new VoyageEmbeddingRequest(new[] { "sunny day at the beach" }),
    CancellationToken.None
);

Console.WriteLine(result.Embeddings[0].Length);
```

Reranking model:

```csharp
using Vercel.AI.Voyage;

var model = (VoyageProvider.VoyageRerankingModel)VoyageProvider.Create().RerankingModel("rerank-2.5");
var result = await model.RerankAsync(
    new VoyageRerankRequest
    {
        Query = "What is an agent?",
        TextDocuments = new[] { "An agent calls tools in a loop.", "Bananas are yellow." },
        TopK = 1,
    },
    CancellationToken.None
);

foreach (var ranked in result.Ranking)
{
    Console.WriteLine($"{ranked.Index}: {ranked.Score}");
}
```

See the [Providers](providers.md) guide for authentication and provider options.
