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

`TypeSafeAiProvider.Default` reads `TYPESAFE_AI_API_KEY` when a request is made, not when the provider is created. It reads `TYPESAFE_AI_BASE_URL` once, when it is first created. `TypeSafeAiProvider.Create(options, handler)` takes these `TypeSafeAiOptions`:

| Option | Meaning |
| --- | --- |
| `ApiKey` | API key. Defaults to `TYPESAFE_AI_API_KEY`. |
| `BaseUrl` | Defaults to `TYPESAFE_AI_BASE_URL`, then `https://api.typesafe.ai/v1`. A trailing slash is removed. An assigned value wins over the variable. |
| `Headers` | Sent with every request. Headers on the call win over them. |

`DecisionModel(id)` accepts any model id, including ids this library does not know yet. The provider id is `typesafe.decision`. `EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)` and returns a `TypeSafeAiEvaluationModel`. `TypeSafeAiEvaluationModel`, `TypeSafeAiEvaluationModelConfig`, and `EvaluationProviderId` are deprecated aliases too. `LanguageModel`, `EmbeddingModel`, and `ImageModel` throw `NoSuchModelError`.

## Decision model

`TypeSafeAiDecisionModel` posts the model id, the state, and every question to `{BaseUrl}/systemone` in one request. It supports `choice`, `score`, `boolean`, and `box` questions. A `boolean` question is sent as TypeSafe's `noul` type, and the `noul` answer comes back as a `boolean` answer with a `Probability`.

- Scores and probabilities are returned unchanged. `Rounding` declares two decimals for both.
- Per-answer `confidence` values are in `ProviderMetadata` under `typesafe.confidence`, keyed by question id.
- `Usage` holds `input_tokens` and `output_tokens`. A missing or null `usage` leaves them null.
- `Response.ModelId` is the model id the API reported, or the requested id when the API sent none.
- The state goes as a string, a JSON object, or a list, like the Gateway model. See [Images](#images) for file parts.
- A `choice` question accepts at most 255 options and a `score` question at most 10 levels. A larger question throws `InvalidArgumentError` before any request.
- `providerOptions.typesafe.longContext` is the only `typesafe` provider option that is sent. Every other one is reported as an `unsupported` warning (`providerOptions.typesafe.<name>`) and is not sent.

## Box questions

A `box` question locates one target in an image and takes no criteria:

```csharp
var questions = new Dictionary<string, DecisionQuestion>
{
    ["target"] = new DecisionQuestion("box", "The cat in the image"),
};
```

Send an image in the state as described in [Images](#images). The answer has type `box` and a `DecisionBox` in `Box` with `Top`, `Bottom`, `Left`, and `Right`. Each edge is a fraction in [0, 1], measured from the top-left corner, with `Left < Right` and `Top < Bottom`. Any other box throws `InvalidResponseDataException`. The request enables the `TypeSafe-Danger-Zone-Images: true` header even when the state has no image. A box needs Jev 1.14 or later. The answer's confidence is in `ProviderMetadata` under `typesafe.confidence`, keyed by question id.

This feature is experimental, and a box is returned even when the target is absent. Ask a `boolean` question alongside it to check visibility. See [TypeSafe's Box documentation](https://docs.typesafe.ai/primitives/box). The OpenAI, Google, and Gateway decision models do not support `box`, so `Decide.DecideAsync` throws `DecisionUnsupportedQuestionTypeException` for it before it downloads files or calls the provider.

## Model ids

`jev-1.13.0`, `jev-1.14.0`, `jev-latest`, and `jev-preview` are sent exactly as you pass them. Any other id is sent too. The default context window is 64k tokens for input and 32k for output.

## Long context

`providerOptions.typesafe.longContext` takes a boolean. `true` sends the `TypeSafe-Danger-Zone-Long-Context: true` header and raises the context window to 262k tokens. `false` or no value sends no header and no warning. Any other value (`"true"`, `1`, `null`) throws `InvalidArgumentError` before any request.

```csharp
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = provider.DecisionModel("jev-latest"),
    State = state,
    Questions = questions,
    ProviderOptions = new Dictionary<string, object?> { ["typesafe"] = new Dictionary<string, object?> { ["longContext"] = true } },
});
```

## Images

A file part with the `image/png` or `image/jpeg` media type and inline data (base64 or bytes) is sent as an image. Any other file part throws `UnsupportedFunctionalityException` before any request: a non-inline data type (functionality `TypeSafe AI decision file input: <mediaType> (<data type>)`) or another media type, such as `application/pdf`, `image/webp`, or `image/gif` (functionality `TypeSafe AI decision image media type: <mediaType>`).

With at least one image the request is `multipart/form-data` and carries the `TypeSafe-Danger-Zone-Images: true` header:

- The `request` part is `application/json` (file name `request.json`) with `{ model, state, assets, questions }`.
- Each image is a part named `image-<state index>`, with the media type as content type and `filename` (or `image-<index>.png` / `image-<index>.jpeg`) as file name.
- `assets` maps each image reference to `{ type: "image", part: "image-<index>" }`.
- `state` is the parts joined with a newline, text as text, JSON as its JSON text, and each image as its reference, unless the state already contains the reference (see below). When the only remaining part is a JSON part, `state` stays that JSON value.

The reference is the asset id. Set it with `providerOptions.typesafe.reference` on the file part, and use it in question instructions and criteria (`Does [photo] show a cat?`). Without one, a new id is generated for every call. A reference that is empty, whitespace, or not a string throws `InvalidArgumentError`, and so does a duplicate (parameter `providerOptions.typesafe.reference`). Text-only requests stay `application/json`, with no `assets` and no image header.

If the state already contains the reference (in a text part, or in a string value of a JSON part, not in an object key), the model sends the state as written, so JSON state stays an object. Otherwise it inserts the reference at the file's position in the state.

TypeSafe sends another copy of the image for every occurrence of its reference in state, instructions, or criteria. To ask several questions about one image, put it in the state once and refer to it by path in backticks (`Does `photo` show a cat?`) rather than repeating the reference in each question.

Set `providerOptions.typesafe.insertReference` to `false` to send an image only where you reference it in a question's instructions or criteria, for example a known picture to compare against. This requires an explicit `reference`, otherwise `InvalidArgumentError` is thrown (parameter `providerOptions.typesafe.reference`). Set it to `true` to insert the reference even when the state already contains it. Any other value throws `InvalidArgumentError` (parameter `providerOptions.typesafe.insertReference`).

```csharp
static Dictionary<string, object?> Image(string path, Dictionary<string, object?> typesafe) => new()
{
    ["type"] = "file",
    ["mediaType"] = "image/jpeg",
    ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = File.ReadAllBytes(path) },
    ["providerOptions"] = new Dictionary<string, object?> { ["typesafe"] = typesafe },
};

var state = new List<object?>
{
    new Dictionary<string, object?> { ["type"] = "json", ["value"] = new Dictionary<string, object?> { ["photo"] = "[found-dog]" } },
    Image("found-dog.jpg", new() { ["reference"] = "[found-dog]" }),
    Image("lost-dog-poster.jpg", new() { ["reference"] = "[poster]", ["insertReference"] = false }),
};
// Question: "Is the dog in `photo` the same dog as in [poster]?"
```

## Errors

A failed response throws `ApiCallError` with the status code and `IsRetryable` (408, 409, 429, and 5xx). The message is the first of `message`, `error` (a string or `error.message`), `detail`, and `error_type` that the body carries, then `TypeSafe request failed`. A body that is not JSON uses the status text. A successful response that does not match the TypeSafe schema also throws `ApiCallError`. This model makes one HTTP call and does not retry by itself.

## Workflow boundaries

This port has no workflow runtime. `TypeSafeAiDecisionModel.Serialize()` returns the model id and the JSON-serializable config (`provider`, `baseURL`, resolved `headers`) as `SerializedModelOptions`. `TypeSafeAiDecisionModel.Deserialize(options, httpClient)` restores it. When no headers were serialized, the restored model reads `TYPESAFE_AI_API_KEY` at call time. The `HttpClient` is never serialized.
