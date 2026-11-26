# Perplexity

`PerplexityProvider` in the `Vercel.AI.Perplexity` namespace creates the Perplexity models that this port implements. The API reference lists every option: [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/).

Browse the upstream catalog of Perplexity models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `low`, `fast`, `sonar-pro`, `openai/gpt-5.1` |
| Embedding | `EmbeddingModel` | `pplx-embed-v1-4b` |
| Decision | `DecisionModel` | `pplx-decider-latest` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Perplexity;

var model = PerplexityProvider.Create().LanguageModel("low");
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
