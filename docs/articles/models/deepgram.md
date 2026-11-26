# Deepgram

`DeepgramProvider` in the `Vercel.AI.Deepgram` namespace creates the Deepgram models that this port implements. The API reference lists every option: [DeepgramProvider](/api/Vercel.AI.Deepgram/DeepgramProvider/).

Browse the upstream catalog of Deepgram models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Speech | `SpeechModel` | `aura-2` |
| Transcription | `TranscriptionModel` | `nova-3`, `flux-general-en` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Transcription model:

```csharp
using Vercel.AI.Deepgram;

var model = (ITranscriptionCaller)DeepgramProvider.Create().TranscriptionModel("nova-3");
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
