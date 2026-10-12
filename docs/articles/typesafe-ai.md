# TypeSafe

`Vercel.AI.TypeSafeAi` ports `@ai-sdk/typesafe-ai`. It gives `Decide.DecideAsync` a TypeSafe decision model. The capability is experimental, like `Decide` itself.

```csharp
var provider = TypeSafeAiProvider.Create(new TypeSafeAiOptions { ApiKey = "..." });
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = provider.DecisionModel("jev-latest"),
    State = new Dictionary<string, object?> { ["message"] = "I was charged twice." },
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["requestsRefund"] = new DecisionQuestion("boolean", "Is the customer requesting a refund?"),
    },
});
```

## Provider

`TypeSafeAiProvider.Default` reads `TYPESAFE_AI_API_KEY` when a request is made, not when the provider is created. `TypeSafeAiProvider.Create(options, handler)` takes these `TypeSafeAiOptions`:

| Option | Meaning |
| --- | --- |
| `ApiKey` | API key. Defaults to `TYPESAFE_AI_API_KEY`. |
| `BaseUrl` | Defaults to `https://api.typesafe.ai/v1`. A trailing slash is removed. |
| `Headers` | Sent with every request. Headers on the call win over them. |

`DecisionModel(id)` accepts any model id, including ids this library does not know yet. The provider id is `typesafe.decision`. `EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)` and returns a `TypeSafeAiEvaluationModel`. `TypeSafeAiEvaluationModel`, `TypeSafeAiEvaluationModelConfig`, and `EvaluationProviderId` are deprecated aliases too. `LanguageModel`, `EmbeddingModel`, and `ImageModel` throw `NoSuchModelError`.

## Decision model

`TypeSafeAiDecisionModel` posts the model id, the state, and every question to `{BaseUrl}/systemone` in one request. It supports `choice`, `score`, and `boolean` questions. A `boolean` question is sent as TypeSafe's `noul` type, and the `noul` answer comes back as a `boolean` answer with a `Probability`.

- Scores and probabilities are returned unchanged. `Rounding` declares two decimals for both.
- Per-answer `confidence` values are in `ProviderMetadata` under `typesafe.confidence`, keyed by question id.
- `Usage` holds `input_tokens` and `output_tokens`. A missing or null `usage` leaves them null.
- `Response.ModelId` is the model id the API reported, or the requested id when the API sent none.
- The state goes as a string, a JSON object, or a list, like the Gateway model. A file part throws `UnsupportedFunctionalityException`.
- A `choice` question accepts at most 255 options and a `score` question at most 10 levels. A larger question throws `InvalidArgumentError` before any request.
- Every `typesafe` provider option is reported as an `unsupported` warning (`providerOptions.typesafe.<name>`) and is not sent.

## Errors

A failed response throws `ApiCallError` with the status code and `IsRetryable` (408, 409, 429, and 5xx). The message is the first of `message`, `error` (a string or `error.message`), `detail`, and `error_type` that the body carries, then `TypeSafe request failed`. A body that is not JSON uses the status text. A successful response that does not match the TypeSafe schema also throws `ApiCallError`. This model makes one HTTP call and does not retry by itself.

## Workflow boundaries

This port has no workflow runtime. `TypeSafeAiDecisionModel.Serialize()` returns the model id and the JSON-serializable config (`provider`, `baseURL`, resolved `headers`) as `SerializedModelOptions`. `TypeSafeAiDecisionModel.Deserialize(options, httpClient)` restores it. When no headers were serialized, the restored model reads `TYPESAFE_AI_API_KEY` at call time. The `HttpClient` is never serialized.
