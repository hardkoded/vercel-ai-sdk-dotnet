# DeepInfra

`DeepInfraProvider` in the `Vercel.AI.DeepInfra` namespace creates the DeepInfra models that this port implements. The API reference lists every option: [DeepInfraProvider](/api/Vercel.AI.DeepInfra/DeepInfraProvider/).

Browse the upstream catalog of DeepInfra models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `google/gemma-2-9b-it`, `mistralai/Mixtral-8x7B-Instruct-v0.1`, `deepinfra-chat-model` |
| Image | `ImageModel` | `deepinfra-image-model` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.DeepInfra;

var model = DeepInfraProvider.Create().LanguageModel("google/gemma-2-9b-it");
var result = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = model,
        Prompt = "Invent a new holiday and describe its traditions.",
    }
);

Console.WriteLine(result.Text);
```

See the [Providers](providers.md) guide for authentication and provider options.
