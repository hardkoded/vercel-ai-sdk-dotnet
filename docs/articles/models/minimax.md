# MiniMax

`MiniMaxProvider` in the `Vercel.AI.MiniMax` namespace creates the MiniMax models that this port implements. The API reference lists every option: [MiniMaxProvider](/api/Vercel.AI.MiniMax/MiniMaxProvider/).

Browse the upstream catalog of MiniMax models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `Chat`, `LanguageModel` | `minimax-m3.1-flash`, `minimax-m3`, `MiniMax-M3.1-Flash`, `minimax-m2.1` |
| Image | `Image`, `ImageModel` | `MiniMax-H3-Image`, `MiniMax-H3-Image-Highspeed` |
| Video | `Video`, `VideoModel` | `MiniMax-H3` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.MiniMax;

var model = MiniMaxProvider.Create().Chat("minimax-m3.1-flash");
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
