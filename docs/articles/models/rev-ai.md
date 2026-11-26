# Rev.ai

`RevAIProvider` in the `Vercel.AI.RevAI` namespace creates the Rev.ai models that this port implements. The API reference lists every option: [RevAIProvider](/api/Vercel.AI.RevAI/RevAIProvider/).

Browse the upstream catalog of Rev.ai models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Transcription | `TranscriptionModel` | `machine` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Transcription model:

```csharp
using Vercel.AI.RevAI;

var model = (RevAIProvider.RevAITranscriptionModel)RevAIProvider.Create().TranscriptionModel("machine");
var result = await model.TranscribeAsync(
    new AudioInput(await File.ReadAllBytesAsync("audio.mp3"), "audio/mpeg", null),
    new RevAITranscriptionRequest(),
    CancellationToken.None
);

Console.WriteLine(result.Text);
```

See the [Providers](providers.md) guide for authentication and provider options.
