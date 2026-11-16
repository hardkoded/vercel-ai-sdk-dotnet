# ScaleDown

`Vercel.AI.ScaleDown` ports `@ai-sdk/scaledown`. It gives `Decide.DecideAsync` a decision model for ScaleDown's [Decisions API](https://docs.scaledown.ai/api-reference/decisions), and `Extract.ExtractAsync` an extraction model for its [Extract API](https://docs.scaledown.ai/api-reference/extract). Both capabilities are experimental, like `Decide` and `Extract` themselves.

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
| `ApiKeyProvider` | Resolves the API key on every request, as an async `apiKey` does upstream. Takes precedence over `ApiKey`. Models built with it cannot be serialized. |
| `BaseUrl` | Defaults to `https://api.scaledown.xyz`. A trailing slash is removed. |
| `Headers` | Sent with every request. Headers on the call win over them. |

`AddScaleDown(configure)` registers the provider with dependency injection. `DecisionModel(id)` and `ExtractionModel(id)` accept any model id. `LanguageModel`, `EmbeddingModel`, and `ImageModel` throw `NoSuchModelError`. The provider id of the decision model is `scaledown.decision` and the provider id of the extraction model is `scaledown.extraction`.

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

## Extraction model

```csharp
var result = await Extract.ExtractAsync(new ExtractRequest
{
    Model = ScaleDownProvider.Default.ExtractionModel("extract-1"),
    State = "Invoice INV-2041 from Acme Corp. Contacts: Ana, Bo.",
    Fields = new Dictionary<string, ExtractionField>
    {
        ["invoiceNumber"] = new("text", DecisionInput.From("The invoice number")),
        ["vendor"] = new("text", DecisionInput.From("Company that issued the invoice")),
        ["contacts"] = new("list", DecisionInput.From("Contact names")) { Items = "text" },
    },
    ProviderOptions = JsonDocument.Parse("""{"scaledown":{"threshold":0.5}}""").RootElement.Clone(),
});
```

`ScaleDownExtractionModel` calls `POST {BaseUrl}/v1/chat/completions` with a JSON schema for the fields. `extract` selects the latest model and `extract-1` pins version 1. Both support `text` fields and `list` fields of `text` items, with the same text, JSON, and single-document inputs as the decision model. For document-only input, the model sends the non-empty user message that ScaleDown requires (`Extract the requested fields from the document.`).

- A `text` field returns the most confident verbatim match, or a null `Text` when there is none.
- A `list` field returns up to 30 distinct matches in source order, or null `TextItems` when there are none. Each entry of `Items` keeps its confidence and source when ScaleDown reports them.
- A text source converts ScaleDown's UTF-8 byte offsets to UTF-16 offsets in the original state part. JSON parts have no source location. A document match cites the file when its location can be established. The OCR text is in `result.ProviderMetadata` under `scaledown.ocrText`.
- ScaleDown extracts spans and does not normalize values. `number`, `date`, `time`, `datetime`, `duration`, and lists of numbers or dates throw `ExtractionUnsupportedFieldTypeError` before the request. Use `text` fields to read amounts or dates verbatim.
- `providerOptions.scaledown.threshold` sets the minimum confidence from 0 to 1 (default 0). `ScaleDownExtractionModelOptions` describes it. A value outside that range throws `InvalidArgumentException`.
- `ReferenceDate`, `Reasoning`, and `MaxOutputTokens` are not supported. Each is reported as an `unsupported` warning and not sent. ScaleDown does not return `PNull` or guesses, so the model leaves them unset.

`ScaleDownExtractionModel.Serialize()` and `ScaleDownExtractionModel.Deserialize(options)` carry the model across a workflow boundary. Errors are the same as for the decision model: a failed response throws `ApiCallError` with the `detail` text, and content that is not valid extraction JSON throws `InvalidResponseDataException`.
