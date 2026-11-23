# Fireworks

`FireworksProvider` in the `Vercel.AI.Fireworks` namespace creates the Fireworks models that this port implements. The API reference lists every option: [FireworksProvider](/api/Vercel.AI.Fireworks/FireworksProvider/).

Browse the upstream catalog of Fireworks models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `fireworks-chat-model`, `accounts/fireworks/models/gpt-oss-120b` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Fireworks;

var model = FireworksProvider.Create().LanguageModel("fireworks-chat-model");
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
