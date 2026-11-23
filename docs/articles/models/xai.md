# xAI

`XaiProvider` in the `Vercel.AI.Xai` namespace creates the xAI models that this port implements. The API reference lists every option: [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/).

Browse the upstream catalog of xAI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel`, `ResponsesModel` | `grok-4.3`, `grok-4-fast-non-reasoning` |
| Transcription | `TranscriptionModel` | `grok-voice-transcribe-2.0` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.Xai;

var model = XaiProvider.Create().ResponsesModel("grok-4.3");
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
