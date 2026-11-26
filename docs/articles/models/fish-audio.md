# Fish Audio

`FishAudioProvider` in the `Vercel.AI.FishAudio` namespace creates the Fish Audio models that this port implements. The API reference lists every option: [FishAudioProvider](/api/Vercel.AI.FishAudio/FishAudioProvider/).

Browse the upstream catalog of Fish Audio models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Speech | `Speech`, `SpeechModel` | `s1`, `s2-pro` |
| Transcription | `TranscriptionModel` | `transcribe-1` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Speech model:

```csharp
using Vercel.AI.FishAudio;

var model = (ISpeechCaller)FishAudioProvider.Create().Speech("s1");
var result = await GenerateSpeech.GenerateSpeechAsync(
    new GenerateSpeechRequest { Model = model, Text = "Hello from the Vercel AI SDK for .NET." }
);

await File.WriteAllBytesAsync("speech.mp3", result.Audio.Data);
```

See the [Providers](providers.md) guide for authentication and provider options.
