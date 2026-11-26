# ByteDance

`ByteDanceProvider` in the `Vercel.AI.ByteDance` namespace creates the ByteDance models that this port implements. The API reference lists every option: [ByteDanceProvider](/api/Vercel.AI.ByteDance/ByteDanceProvider/).

Browse the upstream catalog of ByteDance models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Image | `ImageModel` | `seedream-5-0-260128`, `seedream-4-5-251128` |
| Video | `VideoModel` | `seedance-1-0-pro-250528`, `seedance-1-5-pro-251215` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Image model:

```csharp
using Vercel.AI.ByteDance;

var model = (IImageCaller)ByteDanceProvider.Create().ImageModel("seedream-5-0-260128");
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest { Model = model, Text = "A red apple sticker" }
);

await File.WriteAllBytesAsync("image.png", result.Image.Data!);
```

See the [Providers](providers.md) guide for authentication and provider options.
