# QuiverAI

`QuiverAIProvider` in the `Vercel.AI.QuiverAI` namespace creates the QuiverAI models that this port implements. The API reference lists every option: [QuiverAIProvider](/api/Vercel.AI.QuiverAI/QuiverAIProvider/).

Browse the upstream catalog of QuiverAI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `arrow-2`, `arrow-2-telos` |
| Image | `ImageModel` | `arrow-1` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.QuiverAI;

var model = QuiverAIProvider.Create().LanguageModel("arrow-2");
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
