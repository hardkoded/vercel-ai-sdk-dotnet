# AssemblyAI

`AssemblyAIProvider` in the `Vercel.AI.AssemblyAI` namespace creates the AssemblyAI models that this port implements. The API reference lists every option: [AssemblyAIProvider](/api/Vercel.AI.AssemblyAI/AssemblyAIProvider/).

Browse the upstream catalog of AssemblyAI models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Speech | `Speech` | `assemblyai-tts-1` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Speech model:

```csharp
using Vercel.AI.AssemblyAI;

var model = AssemblyAIProvider.Create().Speech("assemblyai-tts-1");

Console.WriteLine(model.ModelId);
```

This model does not plug into the shared `Generate*` helpers. Call the model through the members the API reference lists for its type.

See the [Providers](providers.md) guide for authentication and provider options.
