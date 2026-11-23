# Tencent Cloud

`TencentProvider` in the `Vercel.AI.Tencent` namespace creates the Tencent Cloud models that this port implements. The API reference lists every option: [TencentProvider](/api/Vercel.AI.Tencent/TencentProvider/).

Browse the upstream catalog of Tencent Cloud models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `Chat`, `LanguageModel` | `hy4`, `hy3` |
| Video | `Video`, `VideoModel` | `vod/VS/2.0`, `kling-video-v3`, `not-a-model` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Tencent;

var model = TencentProvider.Create().LanguageModel("hy4");
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
