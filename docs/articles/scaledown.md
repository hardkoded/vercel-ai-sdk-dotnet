# ScaleDown

`Vercel.AI.ScaleDown` ports `@ai-sdk/scaledown`. It gives `Decide.DecideAsync` a decision model for ScaleDown's [Decisions API](https://docs.scaledown.ai/api-reference/decisions). The capability is experimental, like `Decide` itself. The extraction model of the package is not ported, because `experimental_extract` is not ported yet (#923).

```csharp
var provider = ScaleDownProvider.Create(new ScaleDownProviderSettings { ApiKey = "..." });
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = provider.DecisionModel("classify-1"),
    State = "The export button crashes in Safari. It works in Chrome.",
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["hasWorkaround"] = new DecisionQuestion("boolean", "Can the user work around this issue?"),
    },
});
```

## Provider

`ScaleDownProvider.Default` reads `SCALEDOWN_API_KEY` when a request is made, not when the provider is created. It maps to `scaledown` and its alias `scaleDown`. `ScaleDownProvider.Create(settings, handler)` takes these `ScaleDownProviderSettings`:

| Setting | Meaning |
| --- | --- |
| `ApiKey` | API key, sent as `x-api-key`. Defaults to `SCALEDOWN_API_KEY`. |
| `BaseUrl` | Defaults to `https://api.scaledown.xyz`. A trailing slash is removed. |
| `Headers` | Sent with every request. Headers on the call win over them. |

`AddScaleDown(configure)` registers the provider with dependency injection. `DecisionModel(id)` accepts any model id. `LanguageModel`, `EmbeddingModel`, and `ImageModel` throw `NoSuchModelError`. The provider id of the model is `scaledown.decision`.

## Decision model

`DecisionScaleDownModel` sends the model id, the state, and every question to `POST {BaseUrl}/v1/scaledown` in one request. `classify` selects the latest version and `classify-1` pins version 1. Other ids are forwarded to ScaleDown for validation. It supports `choice`, `boolean`, and `score` questions. Any other type throws `DecisionUnsupportedQuestionTypeError` before the request.

- A `choice` question returns the chosen option and its full probability distribution. A null option description sends the option name.
- A `boolean` question is sent as ScaleDown's `noul` type, and the `noul` answer comes back as a `boolean` answer with a `Probability`. ScaleDown ignores `noul` criteria, so they are added to the question instructions as `Answer yes when: ...` and `Answer no when: ...` lines.
- A `score` question needs 2 to 10 non-null levels, otherwise `InvalidArgumentError` is thrown before the request. Scores are zero-based and can be fractional.
- Text parts are joined with newlines. JSON state parts and structured instructions or criteria are sent as JSON text.
- A state can hold one JPEG, PNG, TIFF, or PDF file part with inline data. It is sent as a base64 document next to the text. Any other file, or a second one, throws `UnsupportedFunctionalityException`.
- `MaxOutputTokens`, `Reasoning`, and every `providerOptions.scaledown` option are not supported. Each is reported as an `unsupported` warning and not sent.

`result.ProviderMetadata` holds `scaledown.confidence` and `scaledown.legend`, keyed by question id, and `scaledown.cost` (USD) when ScaleDown supplies it. `ScaleDownDecisionProviderMetadata` describes that shape.

`DecisionScaleDownModel.Serialize()` and `DecisionScaleDownModel.Deserialize(options)` carry the model across a workflow boundary. A model restored without headers reads `SCALEDOWN_API_KEY` at call time.

A failed response throws `ApiCallError` with the `detail` text of the response. A response that does not fit the schema throws `ApiCallError` with the message `Invalid JSON response`.
