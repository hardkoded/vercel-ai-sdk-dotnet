# Cerebras

`CerebrasProvider` in the `Vercel.AI.Cerebras` namespace creates the Cerebras models that this port implements. The API reference lists every option: [CerebrasProvider](/api/Vercel.AI.Cerebras/CerebrasProvider/).

Browse the upstream catalog of Cerebras models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `gpt-oss-120b` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Cerebras;

var model = CerebrasProvider.Create().LanguageModel("gpt-oss-120b");
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
