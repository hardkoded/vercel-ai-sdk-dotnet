---
name: vercel-ai-dotnet
description: Use the community .NET port of the Vercel AI SDK. Covers GenerateTextAsync, StreamTextAsync, tools, structured output, providers, and the ASP.NET Core UI message stream.
---

# Vercel AI SDK for .NET

Unofficial port of vercel/ai. Apache-2.0. Not an official Vercel product. Parity target is Language Model specification V4.

## Map JavaScript names to C#

- `generateText` → `GenerateTextAsync`
- `streamText` → `StreamTextAsync` (returns `StreamTextResult`)
- `AbortSignal` → `CancellationToken`
- `tool()` → `Tool.Function(name, description, jsonSchema, execute)`
- `stopWhen: isStepCount(n)` → `StopWhen.IsStepCount(n)`
- `"openai/gpt-4.1-mini"` → `GenerateTextOptions.ModelId` (Gateway) or a provider’s `LanguageModel`

## Generate text

```csharp
var model = OpenAIProvider.Create().LanguageModel("gpt-4.1-mini");
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "Summarize this incident in one sentence.",
    Tools = new[]
    {
        Tool.Function("lookup", "Lookup a host", "{\"type\":\"object\"}", (args, ct) => Task.FromResult("{\"ok\":true}")),
    },
    StopWhen = StopWhen.IsStepCount(4),
    Output = OutputSpec.Object("{\"type\":\"object\"}", "report"),
});
```

`result.Text` is the last step. `result.Output` is the parsed JSON object when `Output` is set. The default stop condition is one model step; tools from that step still execute.

## Stream

`StreamTextResult.TextStream` yields text deltas. `Stream` yields every part. In ASP.NET Core, `result.ToUIMessageStreamResult()` writes `text/event-stream` for a JavaScript `useChat` client. Pass `keepAliveMs` to write `: stream-open` immediately and `: keep-alive` comments while the next part is still pending. Do not add a React or Razor chat component for that protocol. The client gets a failed tool call's error text unchanged, the same text the model received. Pass `redactToolErrors: true` to replace it with the `onError` result (`An error occurred.` by default). `redactToolErrors` defaults to `true` when you pass `onError`. A stream `error` chunk stays masked. Source parts are not sent unless you pass `sendSources: true`. `ToUIMessageStream` also takes `GenerateMessageId` for the response message id. To write the same stream to a `ServerResponse` instead, call `result.PipeUIMessageStreamToResponseAsync(response)` (upstream `pipeUIMessageStreamToResponse`). To build a response from a stream of UI message chunks, call `UIMessageStreamResponses.CreateUIMessageStreamResponse(ReadableStream<JsonObject>.From(chunks), new UIMessageStreamResponseInit { ConsumeSseStream = s => ConsumeStream.ConsumeAsync(s) })` (upstream `createUIMessageStreamResponse` with `consumeSseStream`). The consumer gets a tee of the SSE stream, so a client disconnect reports an abort instead of a cancellation.

## Decide and extract

`Decide.DecideAsync` answers typed questions (choice, score, boolean, box) about one shared state. `Extract.ExtractAsync` is the other direction: it pulls typed values out of text, JSON, images, or PDFs.

```csharp
var result = await Extract.ExtractAsync(new ExtractRequest
{
    Model = OpenAIProvider.Create().ExtractionModel("gpt-6-luna"),
    State = "Invoice INV-2041, total 1,250.00 USD, due March 3.",
    ReferenceDate = "2027-01-15",
    Fields = new Dictionary<string, ExtractionField>
    {
        ["number"] = new ExtractionField("text", "The invoice number."),
        ["total"] = new ExtractionField("number") { Unit = "USD", Decimals = 2 },
        ["due"] = new ExtractionField("date"),
    },
});
var total = result.Answers["total"].Number; // null when the state has no total
```

Extraction models also resolve by id: `ProviderRegistry.ExtractionModel("provider:model")`, `CustomProvider.AddExtractionModel(alias, model | "id")` and `ExtractRequest.ModelId` (through `DecisionModels.DefaultProvider`, which must implement `IExtractionProvider`; Gateway has none). Unknown ids throw `NoSuchModelError` with `ModelType` `extractionModel`. See `docs/articles/providers.md`.

`ScaleDownProvider.ExtractionModel("extract-1")` extracts `text` fields and `list` fields of `text` items as verbatim spans, with confidence and sources. It accepts `providerOptions.scaledown.threshold` and rejects other field types with `ExtractionUnsupportedFieldTypeError`. See `docs/articles/scaledown.md`.

`GatewayProvider.Create().ExtractionModel("scaledown/extract-1")` (or `ExtractRequest.ModelId = "scaledown/extract-1"`, which resolves through the Gateway unless `DecisionModels.DefaultProvider` is set) extracts through the AI Gateway. Which field types work depends on the model. An unsupported field throws `ExtractionUnsupportedFieldTypeError` after the request. Send files as bytes or base64.

Both take URL files in `State` only with `Download = Download.CreateDownload(fetch: ...)`. Without it a URL file throws `DownloadError`.

`GenerateTextOptions.ExperimentalDownload` (also on `StreamTextOptions`) routes prompt file URLs and generated file URLs through a download function, for example `Download.CreateDownload(fetch: ...)`. Prompt URLs that `ILanguageModel.SupportedUrls` matches for the step's model are passed through (`IsUrlSupportedByModel` is true and the function returns `null`); all others are downloaded and inlined. A `null` result for a generated file URL throws `DownloadException`. Each request carries the call's `AbortSignal`. Every `ILanguageModel` implements `SupportedUrls` (media type pattern to URL regular expression); return `UrlSupport.None` when the model reads no URL itself. Streamed files arrive as `FilePart` and `StreamTextResult.Files`. `Download.CreateDownload` returns a `CreatedDownload` with `InvokeAsync(batch)` and `InvokeAsync(single)`; it converts to the `Download` option of transcribe and generateVideo. A provider created with a handler (`OpenAIProvider.Create`, `OpenAICompatibleProvider.Create`) also uses its `HttpClient` for untrusted downloads unless `DownloadHttpClient` is set.

## Providers

Each package exposes `CreateXxx()` / `AddXxx()` and reads the same environment variable as the JavaScript provider. Gateway uses `AI_GATEWAY_API_KEY` and `https://ai-gateway.vercel.sh/v4/ai`. OpenAI-compatible providers share `Vercel.AI.OpenAICompatible`.

OpenAI transcription (`OpenAIProvider.TranscriptionModel(id)`, cast to `ITranscriptionCaller`) streams `gpt-transcribe`, `gpt-live-transcribe` and `gpt-realtime-whisper` over the Realtime WebSocket with `StreamTranscribe`; set `OpenAIOptions.WebSocket` for a custom socket. `providerOptions.openai` takes `include`, `languages` and `keywords` for REST calls. See `docs/articles/providers.md`.

xAI transcription (`XaiProvider.TranscriptionModel(id)` or `TranscriptionModel()` for the default model, cast to `ITranscriptionCaller`) posts to `/stt` and streams over its WebSocket with `StreamTranscribe`. It returns `Words` (also in `Segments`) and takes `providerOptions.xai` (`XaiTranscriptionModelOptions`); set `XaiOptions.WebSocket` for a header-capable socket. Gateway transcription (`GatewayProvider.TranscriptionModel(id)`) posts to `/transcription-model` and returns `Words` when the Gateway sends them. Gateway streaming (`GatewayProvider.ExperimentalTranscription(id)`, set `GatewayOptions.WebSocket` for a custom socket) streams over the `/transcription-model` WebSocket with `StreamTranscribe`, and `ExperimentalTranscriptionGetTokenAsync(model)` mints a server-side `vcst_` client secret for a browser; `ExperimentalSpeechGetTokenAsync(model)` mints one for streaming speech. See `docs/articles/providers.md`.

Mistral web search (`MistralProvider.Conversation(id)`) calls the Conversations API (`/conversations`). Pass `MistralTools.WebSearch()` or `MistralTools.WebSearchPremium()` in `Tools`; Mistral runs them, so each comes back as a provider-executed `GeneratedToolCall` and `GeneratedToolResult`, and citations arrive as `GeneratedSource`. The model keeps no history, so send the earlier messages with each call. `SupportedUrls` is `application/pdf` over `https`. See `docs/articles/providers.md`.

Azure transcription (`AzureOpenAIProvider.Transcription(id)`) picks the OpenAI, Speech (MAI-Transcribe) or MAI streaming API for each call. Set `providerOptions.azure.api` to override it, and see `docs/articles/providers.md` for the options.

Azure-hosted DeepSeek chat models (`AzureOpenAIProvider.DeepSeek(deploymentName)`, provider id `azure.deepseek`) map top-level `Reasoning` to `reasoning_effort` and stream `reasoning_content` as reasoning parts. Provider options and metadata use the `azure` key; see `docs/articles/providers.md`.

Google Vertex (`GoogleVertexProvider`) downloads HTTP(S) file URLs in tool results as inline data: `VertexOptions.ToolResultDownloads.MaxBytes` (default 7 MiB) limits each file, and `VertexOptions.DownloadFetch` is the transport, which never carries the Vertex credentials. `GoogleVertexMaasProvider.ImageModel(id)` downloads image URLs on another origin without credentials; `GoogleVertexPartnerOptions.DownloadHttpClient` sets that client. See `docs/articles/secure-url-fetching.md`.

Register the client with `services.AddAiSdk()`. Add `services.AddAiSdkOpenTelemetry()` to record spans on the `Vercel.AI` activity source.

## Tests

Mock `HttpMessageHandler`. Use `TestLanguageModel` from `Vercel.AI.Testing` when the test does not care about HTTP. Skip live tests unless the provider key is set.
