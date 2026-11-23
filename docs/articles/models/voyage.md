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

var model = VoyageProvider.Create().EmbeddingModel("voyage-3.5");

Console.WriteLine(model.ModelId);
```

This model does not plug into the shared `Generate*` helpers. Call the model through the members the API reference lists for its type.

See the [Providers](providers.md) guide for authentication and provider options.
