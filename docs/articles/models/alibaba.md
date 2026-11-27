# Alibaba

`AlibabaProvider` in the `Vercel.AI.Alibaba` namespace creates the Alibaba models that this port implements. The API reference lists every option: [AlibabaProvider](/api/Vercel.AI.Alibaba/AlibabaProvider/).

Browse the upstream catalog of Alibaba models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `qwen-plus` |
| Embedding | `EmbeddingModel` | `text-embedding-v4` |
| Video | `VideoModel`, `Video` | `wan2.6-t2v`, `wan2.6-i2v`, `wan2.6-r2v-flash`, `wan2.7-r2v`, `wan3.0-video` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Alibaba;

var model = AlibabaProvider.Create().LanguageModel("qwen-plus");
var result = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = model,
        Prompt = "Invent a new holiday and describe its traditions.",
    }
);

Console.WriteLine(result.Text);
```

Video models are described in [Alibaba video models](providers.md#alibaba-video-models).

See the [Providers](providers.md) guide for authentication and provider options.
