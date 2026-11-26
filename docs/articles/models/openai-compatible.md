# OpenAI-compatible

`OpenAICompatibleProvider` in the `Vercel.AI.OpenAICompatible` namespace creates the OpenAI-compatible models that this port implements. The API reference lists every option: [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/).

Browse the upstream catalog of OpenAI-compatible models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `CreateChatModel`, `LanguageModel` | `chat-model`, `chat`, `chat-2` |
| Embedding | `EmbeddingModel` | `text-embedding-3-large`, `embedding-model` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.OpenAICompatible;

var model = OpenAICompatibleProvider.Create(new OpenAICompatibleOptions { ProviderName = "example", BaseUrl = "https://api.example.com/v1" }).LanguageModel("chat-model");
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
