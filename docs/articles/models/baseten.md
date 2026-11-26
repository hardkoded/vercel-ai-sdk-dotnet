# Baseten

`BasetenProvider` in the `Vercel.AI.Baseten` namespace creates the Baseten models that this port implements. The API reference lists every option: [BasetenProvider](/api/Vercel.AI.Baseten/BasetenProvider/).

Browse the upstream catalog of Baseten models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `CreateChatModel`, `LanguageModel` | `deepseek-ai/DeepSeek-V3-0324`, `chat`, `llama` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Baseten;

var model = BasetenProvider.Create().LanguageModel("deepseek-ai/DeepSeek-V3-0324");
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
