# Together AI

`TogetherAIProvider` in the `Vercel.AI.TogetherAI` namespace creates the Together AI models that this port implements. The API reference lists every option: [TogetherAIProvider](/api/Vercel.AI.TogetherAI/TogetherAIProvider/).

Browse the upstream catalog of Together AI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `CreateChatModel` | `together-chat-model` |
| Reranking | `RerankingModel` | `rerank-model`, `Salesforce/Llama-Rank-v1` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.TogetherAI;

var model = TogetherAIProvider.Create().CreateChatModel("together-chat-model");
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
