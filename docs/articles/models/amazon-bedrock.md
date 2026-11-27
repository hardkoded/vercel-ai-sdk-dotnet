# Amazon Bedrock

`AmazonBedrockProvider` in the `Vercel.AI.AmazonBedrock` namespace creates the Amazon Bedrock models that this port implements. The API reference lists every option: [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/).

Browse the upstream catalog of Amazon Bedrock models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `Chat`, `LanguageModel` | `openai.gpt-oss-20b`, `anthropic.claude-3-haiku-20240307-v1:0` |
| Embedding | `EmbeddingModel` | `amazon.nova-2-multimodal-embeddings-v1:0`, `amazon.titan-embed-text-v2:0`, `amazon.titan-embed-text-v1` |
| Image | `Image` | `amazon.titan-image-generator`, `amazon.nova-canvas-v1:0` |
| Reranking | `RerankingModel` | `cohere.rerank-v3-5:0`, `amazon.rerank-v1:0` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.AmazonBedrock;

var model = AmazonBedrockProvider.Create().LanguageModel("openai.gpt-oss-20b");
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

## Provider options

Pass these under the `amazon-bedrock` provider options key.

- `structuredOutputMode` is `outputFormat`, `jsonTool`, or `auto` (the default). Any other value, or a value that is not a string, throws `ArgumentException` before the request is sent.
- `serviceTier` is `reserved`, `priority`, `default`, or `flex`. Any other value throws `ArgumentException`.
- `requestMetadata` is an object whose values are all strings.

## Cache points

`AmazonBedrockApiTypes.CreateAmazonBedrockCachePoint(ttl)` is the C# name of upstream `createAmazonBedrockCachePoint`. It returns the complete `{ "cachePoint": { "type": "default", "ttl": ... } }` block. The `ttl` is `5m` or `1h`. When it is null, the `ttl` field is left out and Bedrock uses its default of five minutes. Pass the `cachePoint` value of the block, not the block itself, as the `cachePoint` provider option.
