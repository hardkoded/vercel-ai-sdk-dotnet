# Open Responses

`OpenResponsesProvider` in the `Vercel.AI.OpenResponses` namespace creates the Open Responses models that this port implements. The API reference lists every option: [OpenResponsesProvider](/api/Vercel.AI.OpenResponses/OpenResponsesProvider/).

Browse the upstream catalog of Open Responses models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `openai/gpt-6-astra` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.OpenResponses;

var model = OpenResponsesProvider.Create().LanguageModel("openai/gpt-6-astra");
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
