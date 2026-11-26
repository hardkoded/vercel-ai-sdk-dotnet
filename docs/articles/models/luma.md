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

var model = LumaProvider.Create().ImageModel("luma-v1");

Console.WriteLine(model.ModelId);
```

This model does not plug into the shared `Generate*` helpers. Call the model through the members the API reference lists for its type.

See the [Providers](providers.md) guide for authentication and provider options.
