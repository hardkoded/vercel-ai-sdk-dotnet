# Azure OpenAI

`AzureOpenAIProvider` in the `Vercel.AI.Azure` namespace creates the Azure OpenAI models that this port implements. The API reference lists every option: [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/).

Browse the upstream catalog of Azure OpenAI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Speech | `SpeechModel` | `mai-voice-2`, `tts-1`, `mai-voice-2-flash` |
| Transcription | `TranscriptionModel` | `mai-transcribe-2`, `mai-transcribe-1.5`, `whisper-1`, `MAI-Transcribe-2-Streaming`, `mai-transcribe-2-streaming` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Transcription model:

```csharp
using Vercel.AI.Azure;

var model = (ITranscriptionCaller)AzureOpenAIProvider.Create().TranscriptionModel("mai-transcribe-2");
var result = await Transcribe.TranscribeAsync(
    new TranscribeRequest
    {
        Model = model,
        Audio = await File.ReadAllBytesAsync("audio.mp3"),
    }
);

Console.WriteLine(result.Text);
```

See the [Providers](providers.md) guide for authentication and provider options.
