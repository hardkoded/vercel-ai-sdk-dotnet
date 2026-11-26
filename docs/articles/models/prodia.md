# Prodia

`ProdiaProvider` in the `Vercel.AI.Prodia` namespace creates the Prodia models that this port implements. The API reference lists every option: [ProdiaProvider](/api/Vercel.AI.Prodia/ProdiaProvider/).

Browse the upstream catalog of Prodia models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Image | `ImageModel` | `inference.flux.schnell.txt2img.v2` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Image model:

```csharp
using Vercel.AI.Prodia;

var model = (IImageCaller)ProdiaProvider.Create().ImageModel("inference.flux.schnell.txt2img.v2");
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest { Model = model, Text = "A red apple sticker" }
);

await File.WriteAllBytesAsync("image.png", result.Image.Data!);
```

See the [Providers](providers.md) guide for authentication and provider options.
