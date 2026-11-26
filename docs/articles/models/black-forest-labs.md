# Black Forest Labs

`BlackForestLabsProvider` in the `Vercel.AI.BlackForestLabs` namespace creates the Black Forest Labs models that this port implements. The API reference lists every option: [BlackForestLabsProvider](/api/Vercel.AI.BlackForestLabs/BlackForestLabsProvider/).

Browse the upstream catalog of Black Forest Labs models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Image | `Image`, `ImageModel` | `flux-pro-1.1`, `flux-pro-1.1-ultra` |
| Video | `Video`, `VideoModel` | `flux-3-video` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Image model:

```csharp
using Vercel.AI.BlackForestLabs;

var model = (IImageCaller)BlackForestLabsProvider.Create().ImageModel("flux-pro-1.1");
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest { Model = model, Text = "A red apple sticker" }
);

await File.WriteAllBytesAsync("image.png", result.Image.Data!);
```

See the [Providers](providers.md) guide for authentication and provider options.
