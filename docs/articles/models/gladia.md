# Gladia

`GladiaProvider` in the `Vercel.AI.Gladia` namespace creates the Gladia models that this port implements. The API reference lists every option: [GladiaProvider](/api/Vercel.AI.Gladia/GladiaProvider/).

Browse the upstream catalog of Gladia models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Transcription | `TranscriptionModel` | `solaria-3` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Transcription model:

```csharp
using Vercel.AI.Gladia;

var model = (GladiaProvider.GladiaTranscriptionModel)GladiaProvider.Create().TranscriptionModel("solaria-3");
var result = await model.TranscribeAsync(
    new AudioInput(await File.ReadAllBytesAsync("audio.mp3"), "audio/mpeg", null),
    new GladiaTranscriptionRequest(),
    CancellationToken.None
);

Console.WriteLine(result.Text);
```

See the [Providers](providers.md) guide for authentication and provider options.
