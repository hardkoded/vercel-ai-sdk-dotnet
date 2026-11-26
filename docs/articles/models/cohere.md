# Cohere

`CohereProvider` in the `Vercel.AI.Cohere` namespace creates the Cohere models that this port implements. The API reference lists every option: [CohereProvider](/api/Vercel.AI.Cohere/CohereProvider/).

Browse the upstream catalog of Cohere models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `command-r-plus` |
| Embedding | `EmbeddingModel` | `embed-english-v3.0`, `embed-v4.0` |
| Reranking | `RerankingModel` | `rerank-english-v3.0` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Cohere;

var model = CohereProvider.Create().LanguageModel("command-r-plus");
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
