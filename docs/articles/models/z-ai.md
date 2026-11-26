# Z.ai

`ZaiProvider` in the `Vercel.AI.Zai` namespace creates the Z.ai models that this port implements. The API reference lists every option: [ZaiProvider](/api/Vercel.AI.Zai/ZaiProvider/).

Browse the upstream catalog of Z.ai models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `glm-5.3` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Zai;

var model = ZaiProvider.Create().LanguageModel("glm-5.3");
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
