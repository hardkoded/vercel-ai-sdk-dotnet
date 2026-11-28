# Google Generative AI

`GoogleProvider` in the `Vercel.AI.Google` namespace creates the Google Generative AI models that this port implements. The API reference lists every option: [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/).

Browse the upstream catalog of Google Generative AI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `gemini-pro`, `gemini-2.5-flash`, `endpoints/1234567890`, `gemini-3-pro-preview`, `meta/llama-3.1-8b-instruct-maas`, `claude-sonnet-4-5@20250929` |
| Embedding | `EmbeddingModel` | `embedding-001`, `text-embedding-005`, `gemini-embedding-2`, `text`, `gemini-embedding-001` |
| Image | `ImageModel` | `gemini-2.5-flash-image`, `imagen-3.0-generate-002` |
| Speech | `SpeechModel` | `chirp-3-hd`, `gemini-2.5-flash-tts`, `gemini-2.5-pro-tts` |
| Transcription | `TranscriptionModel` | `gemini-3.5-transcribe`, `gemini-3.5-transcribe-live`, `chirp_2`, `chirp_3` |
| Decision | `DecisionModel` | `gemini-decision-1` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Google;

var model = GoogleProvider.Create().LanguageModel("gemini-pro");
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

## Multimodal embeddings

Pass `content` in the call's `google` provider options to embed images, audio, video or files together with the text. There is one entry per value. An entry is a list of Gemini parts, such as `inlineData` or `fileData`, or `null` for a text-only value. The parts follow the value's text in the request. The number of entries must match the number of values, or the call throws.
