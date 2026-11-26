# Replicate

`ReplicateProvider` in the `Vercel.AI.Replicate` namespace creates the Replicate models that this port implements. The API reference lists every option: [ReplicateProvider](/api/Vercel.AI.Replicate/ReplicateProvider/).

Browse the upstream catalog of Replicate models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Image | `ImageModel` | `black-forest-labs/flux-schnell` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Image model:

```csharp
using Vercel.AI.Replicate;

var model = (IImageCaller)ReplicateProvider.Create().ImageModel("black-forest-labs/flux-schnell");
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest { Model = model, Text = "A red apple sticker" }
);

await File.WriteAllBytesAsync("image.png", result.Image.Data!);
```

See the [Providers](providers.md) guide for authentication and provider options.
