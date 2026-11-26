# fal

`FalProvider` in the `Vercel.AI.Fal` namespace creates the fal models that this port implements. The API reference lists every option: [FalProvider](/api/Vercel.AI.Fal/FalProvider/).

Browse the upstream catalog of fal models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Image | `ImageModel` | `fal-ai/flux/dev` |
| Speech | `SpeechModel` | `fal-ai/minimax/speech-02-hd` |
| Transcription | `TranscriptionModel` | `wizper` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Image model:

```csharp
using Vercel.AI.Fal;

var model = (IImageCaller)FalProvider.Create().ImageModel("fal-ai/flux/dev");
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest { Model = model, Text = "A red apple sticker" }
);

await File.WriteAllBytesAsync("image.png", result.Image.Data!);
```

See the [Providers](providers.md) guide for authentication and provider options.
