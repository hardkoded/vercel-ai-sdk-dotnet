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
## Provider settings

`AlibabaProvider.Create` takes `OpenAICompatibleOptions`. Pass `AlibabaProviderSettings`, which extends it, for the Alibaba settings:

- `BaseUrl`: the URL prefix for chat calls. The default is `https://dashscope-intl.aliyuncs.com/compatible-mode/v1`.
- `EmbeddingBaseUrl`: the URL prefix for embedding calls. The embedding API uses the DashScope native endpoint, not the OpenAI-compatible one. The default is `https://dashscope-intl.aliyuncs.com/api/v1`.
- `ApiKey`: sent in the `Authorization` header. It defaults to the `ALIBABA_API_KEY` environment variable. A callback is called before each request.
- `Headers`: custom headers for every request.
- `IncludeUsage`: includes token usage in the final chunk of a streaming response. It defaults to `true`. Set it to `false` on `AlibabaProviderSettings` to turn it off. Plain `OpenAICompatibleOptions` cannot tell an unset value from `false`, so the provider turns usage on for them.

The upstream `fetch` setting is the `HttpClient` you pass to the `AlibabaProvider` constructor, or the handler you pass to `Create`.

```csharp
var provider = AlibabaProvider.Create(
    new AlibabaProviderSettings
    {
        EmbeddingBaseUrl = "https://dashscope.aliyuncs.com/api/v1",
        IncludeUsage = false,
    }
);
```

`ChatModel(id)` and `LanguageModel(id)` create the same chat model.

## Embedding models

`Embedding(id)` and `EmbeddingModel(id)` create an `AlibabaEmbeddingModel` for text embeddings:

```csharp
var model = AlibabaProvider.Create().Embedding("text-embedding-v4");
```

Alibaba text embedding models take up to 10 values per call. A call with more throws `TooManyEmbeddingValuesForCallException`. `EmbedMany` splits larger batches and sends them one after the other.

Pass these options as JSON under `alibaba` in the provider options:

| Option | Request field | Description |
| --- | --- | --- |
| `textType` | `parameters.text_type` | `query` or `document`. Tells query text from document text for asymmetric retrieval. Defaults to `document`. |
| `dimension` | `parameters.dimension` | The dimension of the output vectors. Defaults to 1024. `text-embedding-v4` also supports 1536 and 2048. It wins over the `Dimensions` of the call. |
| `outputType` | `parameters.output_type` | `dense`, `sparse` or `dense&sparse`. Defaults to `dense`. |

Embeddings are dense number arrays, so `outputType: 'sparse'` throws `UnsupportedFunctionalityException` before the request is sent. Use `dense&sparse` to get the dense vectors as embeddings and the sparse vectors in `providerMetadata.alibaba.sparseEmbeddings`. Each entry has `textIndex` and `sparseEmbedding`, a list of `{ index, value, token }`.

When the response has no `usage`, the token count is left unset. `EmbeddingResult.Response` holds the raw headers and body.

| Model | Default dimensions | Flexible dimensions | Max values per call |
| --- | --- | --- | --- |
| `text-embedding-v4` | 1024 | 64, 128, 256, 512, 768, 1024, 1536, 2048 | 10 |
| `text-embedding-v3` | 1024 | 512, 768, 1024 | 10 |

See the [Providers](providers.md) guide for authentication and provider options.
