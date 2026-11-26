# Vercel AI Gateway

`GatewayProvider` in the `Vercel.AI.Gateway` namespace creates the Vercel AI Gateway models that this port implements. The API reference lists every option: [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/).

Browse the upstream catalog of Vercel AI Gateway models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `openai/gpt-6-astra` |
| Embedding | `EmbeddingModel` | `openai/text-embedding-3-small` |
| Image | `ImageModel` | `openai/gpt-image-1` |
| Video | `VideoModel` | `google/veo-2.0-generate-001` |
| Reranking | `Reranking`, `RerankingModel` | `cohere/rerank-v3.5` |
| Decision | `DecisionModel` | `typesafe-ai/jev` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Gateway;

var model = GatewayProvider.Create().LanguageModel("openai/gpt-6-astra");
var result = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = model,
        Prompt = "Invent a new holiday and describe its traditions.",
    }
);

Console.WriteLine(result.Text);
```

See the [Providers](providers.md) guide for authentication and provider options.
