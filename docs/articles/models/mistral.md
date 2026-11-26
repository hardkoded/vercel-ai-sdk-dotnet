# Mistral

`MistralProvider` in the `Vercel.AI.Mistral` namespace creates the Mistral models that this port implements. The API reference lists every option: [MistralProvider](/api/Vercel.AI.Mistral/MistralProvider/).

Browse the upstream catalog of Mistral models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `mistral-small-latest` |
| Embedding | `EmbeddingModel` | `mistral-embed`, `codestral-embed` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Mistral;

var model = MistralProvider.Create().LanguageModel("mistral-small-latest");
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
