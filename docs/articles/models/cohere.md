# Cohere

`CohereProvider` in the `Vercel.AI.Cohere` namespace creates the Cohere models that this port implements. The API reference lists every option: [CohereProvider](/api/Vercel.AI.Cohere/CohereProvider/).

Browse the upstream catalog of Cohere models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `command-r-plus` |
| Embedding | `EmbeddingModel` | `embed-english-v3.0`, `embed-v4.0` |
| Reranking | `RerankingModel` | `rerank-english-v3.0` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Cohere;

var model = CohereProvider.Create().LanguageModel("command-r-plus");
var result = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = model,
        Prompt = "Invent a new holiday and describe its traditions.",
    }
);

Console.WriteLine(result.Text);
```

## Embedding options

Pass the `cohere` provider options to `DoEmbedAsync` to set `inputType`, `truncate`, `embeddingType` (`float`, `int8`, `uint8`, `binary`, or `ubinary`), and `outputDimension`. `outputDimension` must be 256, 512, 768, 1024, 1536, or 2048. Any other value throws before the request is sent. The embeddings come back in the requested format without conversion.

## Files and images

Image files in a user message are sent inline as `image_url` parts. Set the `cohere` provider option `detail` on the file part to pass `image_url.detail`. Other files are sent as documents that carry only their text and file name, so the model can cite them. A file part that refers to a provider upload is not supported and throws `UnsupportedFunctionalityException`.

Provider-defined tools are not supported. They are skipped and add an `unsupported` warning.

See the [Providers](providers.md) guide for authentication and provider options.
