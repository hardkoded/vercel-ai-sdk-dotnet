# ElevenLabs

`ElevenLabsProvider` in the `Vercel.AI.ElevenLabs` namespace creates the ElevenLabs models that this port implements. The API reference lists every option: [ElevenLabsProvider](/api/Vercel.AI.ElevenLabs/ElevenLabsProvider/).

Browse the upstream catalog of ElevenLabs models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Speech | `SpeechModel` | `eleven_multilingual_v2` |
| Transcription | `TranscriptionModel` | `scribe_v1` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Speech model:

```csharp
using Vercel.AI.ElevenLabs;

var model = ElevenLabsProvider.Create().SpeechModel("eleven_multilingual_v2");

Console.WriteLine(model.ModelId);
```

This model does not plug into the shared `Generate*` helpers. Call the model through the members the API reference lists for its type.

See the [Providers](providers.md) guide for authentication and provider options.
