# GMI Cloud

`GmiCloudProvider` in the `Vercel.AI.GmiCloud` namespace creates the GMI Cloud models that this port implements. The API reference lists every option: [GmiCloudProvider](/api/Vercel.AI.GmiCloud/GmiCloudProvider/).

Browse the upstream catalog of GMI Cloud models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Language | `LanguageModel` | `deepseek-ai/DeepSeek-V4-Flash-0731` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Language model:

```csharp
using Vercel.AI.GmiCloud;

var model = GmiCloudProvider.Create().LanguageModel("deepseek-ai/DeepSeek-V4-Flash-0731");
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
