# TypeSafe

`TypeSafeAiProvider` in the `Vercel.AI.TypeSafeAi` namespace creates the TypeSafe models that this port implements. The API reference lists every option: [TypeSafeAiProvider](/api/Vercel.AI.TypeSafeAi/TypeSafeAiProvider/).

Browse the upstream catalog of TypeSafe models and their pricing on the [Vercel AI Gateway models page](https://vercel.com/ai-gateway/models).

## Models

| Kind | Factory method | Model ids in this port's tests and docs |
| --- | --- | --- |
| Decision | `DecisionModel` | `jev-latest`, `jev-future` |

A model id in the table is one that the tests or docs use. It is not the complete list of ids the provider accepts. Pass any id that the provider documents.

## Example

Decision model:

```csharp
using Vercel.AI.TypeSafeAi;

var model = TypeSafeAiProvider.Create().DecisionModel("jev-latest");
var result = await Decide.DecideAsync(
    new DecideRequest
    {
        Model = model,
        State = "I was charged twice for my subscription. Please refund the duplicate.",
        Questions = new Dictionary<string, DecisionQuestion>
        {
            ["category"] = new(
                "choice",
                "Which team should handle this request?",
                new DecisionChoiceCriteria(
                    new Dictionary<string, DecisionInput?>
                    {
                        ["billing"] = "Charges, invoices, and refunds",
                        ["technical"] = "Bugs and outages",
                    }
                )
            ),
        },
    }
);

foreach (var answer in result.Answers)
{
    Console.WriteLine($"{answer.Key}: {answer.Value.Choice}");
}
```

See the [Providers](providers.md) guide for authentication and provider options.
