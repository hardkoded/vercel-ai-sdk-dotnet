# Parallel

`Vercel.AI.Parallel` ports `@ai-sdk/parallel`. The [Parallel](https://parallel.ai) provider gives `Decide.DecideAsync` a web-grounded decision model. It retrieves evidence from the live web and answers boolean, choice, and score questions against that shared evidence. Decisions are experimental, like `Decide` itself.

## Setup

Set the `PARALLEL_API_KEY` environment variable, or create a provider:

```csharp
var provider = ParallelProvider.Create(new ParallelProviderSettings { ApiKey = "your-api-key" });
```

`ParallelProvider.Default` reads `PARALLEL_API_KEY` when a request is made, not when the provider is created. It maps to `parallel`. `ParallelProvider.Create(settings, handler)` takes these `ParallelProviderSettings`:

| Setting | Meaning |
| --- | --- |
| `ApiKey` | API key, sent as `x-api-key`. Defaults to `PARALLEL_API_KEY`. |
| `BaseUrl` | Defaults to `https://api.parallel.ai/v1`. A trailing slash is removed. |
| `Headers` | Sent with every request. Headers on the call win over them. |

`AddParallel(configure)` registers the provider with dependency injection. `LanguageModel`, `EmbeddingModel`, and `ImageModel` throw `NoSuchModelError`, and so does `DecisionModel("")`. The provider id of the decision model is `parallel.decision`.

## Decision models

```csharp
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = ParallelProvider.Default.DecisionModel("fast"),
    State = DecisionState.FromJson(
        JsonDocument.Parse("""{"company":"Parallel","website":"https://parallel.ai"}""").RootElement),
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["hasSearch"] = new("boolean", "Does this company offer a web search API?"),
        ["availability"] = new(
            "choice",
            "Is its search API publicly available?",
            new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?>
            {
                ["available"] = "Public documentation establishes availability.",
                ["unavailable"] = "Public documentation establishes unavailability.",
                ["unknown"] = "The evidence is insufficient.",
            })),
    },
});
```

`DecisionParallelModel` sends the mode, the state, and every question to `POST {BaseUrl}/decisions` in one request. `fast` (`ParallelDecisionModelId.Fast`) is currently the only documented mode. The mode argument is required. Other mode strings are forwarded to Parallel for validation, so newly released modes work without a library update. It is sent as `mode`, not `model`.

- Parallel accepts 1 to 100 related questions, 2 to 255 choice options, and 2 to 10 score levels. Question names may contain only ASCII letters, numbers, underscores, and hyphens. Anything else throws `InvalidArgumentError` before the request.
- A `boolean` question is sent as Parallel's `noul` type, and the answer comes back as a `boolean` answer whose `Probability` is the estimated probability of truth, not confidence in the answer.
- Scores are zero-based probability-weighted means and can be fractional.
- A state with one part is sent as that part's value, and a state with several parts as an array of values.
- File input throws `UnsupportedFunctionalityException`, and `box` questions throw `DecisionUnsupportedQuestionTypeError`, both before the request.
- A response must hold exactly one answer for every question, otherwise `InvalidResponseDataException` is thrown. A response that does not fit the schema throws `ApiCallError` with the message `Invalid JSON response`.
- A refusal answer is preserved as an answer of type `refusal`. `Decide` applies the standard refusal handling.
- `MaxOutputTokens`, `Reasoning`, and every option under `providerOptions.parallel` other than `includeExcerpts` are not supported. Each is reported as an `unsupported` warning and not sent.
- A failed response throws `ApiCallError` with the `error.message` of the response and its headers, such as `retry-after`.

### Citations and provider metadata

`result.ProviderMetadata` holds `parallel`, shaped like `ParallelDecisionProviderMetadata`:

- `citations`: source objects with `url`, an optional or null `title`, and `excerpts`.
- `decisionId`: the provider's decision identifier. It is also `result.Response.Id`.
- `mode`: the execution mode.
- `confidence`: native confidence, keyed by question id when supplied. Boolean and refused questions have none.
- `legend`: score level descriptions, keyed by question id.

Citations apply to the whole request. Parallel provides no per-answer source mapping. Empty citations and empty excerpt arrays are preserved.

Evidence excerpts are included by default. To omit them from the response, set `providerOptions.parallel.includeExcerpts` to `false` (`ParallelDecisionModelOptions.IncludeExcerpts`). This keeps URLs and available titles, and does not change the evidence used to make decisions. A value that is not a boolean throws `InvalidArgumentError` before the request.

The provider supports synchronous decision calls. It does not implement language generation or streaming.

`DecisionParallelModel.Serialize()` and `DecisionParallelModel.Deserialize(options)` carry the model across a workflow boundary. A model restored without headers reads `PARALLEL_API_KEY` at call time.

### Capabilities and options

`ParallelModelCatalog.Entries` maps every known decision model id (`DecisionModelIds`, currently `fast`) to its capabilities, and `GetParallelDecisionModelEntry(modelId)` accepts any id. An unknown id gets unknown support for `includeExcerpts`. `ParallelProviderOptions.Decision` lists the accepted options. The declarations are metadata; they do not validate requests.

| Model | Boolean | Choice | Score | Box | Image input | Reasoning | Max output tokens |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `fast` | yes | yes | yes | no | no | no | no |

| Operation | Option | Description |
| --- | --- | --- |
| decision | `includeExcerpts` | Include evidence excerpts in citations. Defaults to true. |
