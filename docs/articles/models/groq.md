# Groq

`GroqProvider` in the `Vercel.AI.Groq` namespace creates the Groq models that this port implements. The API reference lists every option: [GroqProvider](/api/Vercel.AI.Groq/GroqProvider/).

Browse the upstream catalog of Groq models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `gemma2-9b-it`, `llama-3.3-70b-versatile`, `qwen/qwen3-32b` |
| Transcription | `TranscriptionModel` | `whisper-large-v3-turbo`, `whisper-large-v3` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Groq;

var model = GroqProvider.Create().LanguageModel("gemma2-9b-it");
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
