# Luma

`LumaProvider` in the `Vercel.AI.Luma` namespace creates the Luma models that this port implements. The API reference lists every option: [LumaProvider](/api/Vercel.AI.Luma/LumaProvider/).

Browse the upstream catalog of Luma models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Image | `ImageModel` | `luma-v1` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Image model:

```csharp
using Vercel.AI.Luma;

var model = (LumaImageModel)LumaProvider.Create().ImageModel("luma-v1");
var result = await model.GenerateAsync(
    new LumaImageRequest("A red apple sticker") { AspectRatio = "16:9" },
    CancellationToken.None
);

await File.WriteAllBytesAsync("image.png", result.Images[0].Data!);
```

See the [Providers](providers.md) guide for authentication and provider options.
