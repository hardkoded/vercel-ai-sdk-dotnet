# OpenAI

`OpenAIProvider` in the `Vercel.AI.OpenAI` namespace creates the OpenAI models that this port implements. The API reference lists every option: [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/).

Browse the upstream catalog of OpenAI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `ChatModel`, `LanguageModel`, `ResponsesModel` | `gpt-3.5-turbo`, `gpt-4o`, `gpt-5-mini`, `gpt-5.6`, `gpt-5.3-codex`, `gpt-5.2` |
| Embedding | `EmbeddingModel` | `text-embedding-3-small`, `text-embedding-3-large` |
| Image | `ImageModel` | `gpt-image-99` |
| Decision | `DecisionModel` | `gpt-6-luna`, `future-decisions-model` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.OpenAI;

var model = OpenAIProvider.Create().ResponsesModel("gpt-3.5-turbo");
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

## Decision model images

A decision state image part takes the file part provider option `openai.imageDetail` (`low`, `high`, `auto`, or `original`). The value is sent as `detail` on the `input_image` item. Without the option no `detail` is sent.

## Structured output schemas

OpenAI structured output does not accept `propertyNames` or regex lookaround in `pattern`, so both are removed before the schema is sent, with a `compatibility` warning. A `propertyNames` that is a boolean or has no explicit `type: "string"` is rejected with an `AiSdkException`. A singleton local reference `allOf` (`{ "allOf": [{ "$ref": "#/definitions/person" }] }`, which Zod 4 emits for recursive schemas) becomes a direct `$ref`. At the schema root, the referenced definition is expanded in place, because OpenAI requires an object at the root.
