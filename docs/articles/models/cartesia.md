# Cartesia

`CartesiaProvider` in the `Vercel.AI.Cartesia` namespace creates the Cartesia models that this port implements. The API reference lists every option: [CartesiaProvider](/api/Vercel.AI.Cartesia/CartesiaProvider/).

Browse the upstream catalog of Cartesia models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Transcription | `TranscriptionModel` | `ink-2` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Transcription model:

```csharp
using Vercel.AI.Cartesia;

var model = (ITranscriptionCaller)CartesiaProvider.Create().TranscriptionModel("ink-2");
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
