# Hugging Face

`HuggingFaceProvider` in the `Vercel.AI.HuggingFace` namespace creates the Hugging Face models that this port implements. The API reference lists every option: [HuggingFaceProvider](/api/Vercel.AI.HuggingFace/HuggingFaceProvider/).

Browse the upstream catalog of Hugging Face models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel`, `ResponsesModel` | `org/model` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.HuggingFace;

var model = HuggingFaceProvider.Create().ResponsesModel("org/model");
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
