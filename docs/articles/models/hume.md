# Hume

`HumeProvider` in the `Vercel.AI.Hume` namespace creates the Hume models that this port implements. The API reference lists every option: [HumeProvider](/api/Vercel.AI.Hume/HumeProvider/).

Browse the upstream catalog of Hume models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Speech | `SpeechModel` | `default` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Speech model:

```csharp
using Vercel.AI.Hume;

var model = HumeProvider.Create().Speech();
var result = await model.GenerateAsync(
    new HumeSpeechRequest("Hello from the Vercel AI SDK for .NET."),
    CancellationToken.None
);

await File.WriteAllBytesAsync("speech.mp3", result.Audio);
```

See the [Providers](providers.md) guide for authentication and provider options.
