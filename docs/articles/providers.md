# Providers

Native wire protocols have their own request mappers:

- Gateway (`AI_GATEWAY_API_KEY`) posts V4 call options to `/language-model`, `/embedding-model`, and `/image-model`. `DecisionModel(id)` posts the state and questions to `/decision-model`.
- OpenAI speaks Chat Completions, the Responses API, embeddings, images, speech, and transcription. `DecisionModel(id)` answers `Evaluate` questions through the Decisions API (`POST /decisions`; provider id `openai.decision`). Its only provider option is `safetyIdentifier` (a string of at most 128 characters). Any other `openai` option is reported as an `unsupported` warning and not sent. A refused question returns a `refusal` answer, and `Evaluate.EvaluateAsync` throws `DecisionRefusalError` for it.
- OpenAI Responses on GPT-6 and later models accept `reasoningEffortUpdate` (`none`, `low`, `medium`, `high`, `xhigh`, or `max`). Set it in the `openai` provider options to prepend a `configuration_update` item to the input. Set it on a `SystemModelMessage` with empty content (through its `providerOptions`) to place the update inside the conversation. Pass that message in the messages list. The `instructions` argument is rebuilt from text only and drops message provider options. Sampling parameters (`temperature`, `topP`) and `logprobs` follow the last update, so a `none` update on GPT-6 Sol or Luna keeps them. Adjacent updates, unsupported efforts, and message updates on other models throw `UnsupportedFunctionalityException`. A request-level update on an unsupported model is dropped with a warning.
- OpenAI Responses exposes the URLs in `web_search_call.action.sources` as `GeneratedSource` parts, and each retrieved `file_search_call` file once as a `GeneratedDocumentSource` (even when several passages come from one file, or the file is not cited). Inline `url_citation`, `file_citation`, and `container_file_citation` annotations become `GeneratedText.Citations`, with the output range as `StartIndex` and `EndIndex`, and on a streamed `TextEndStreamPart` for each completed message. Repeated ranges and URLs with query strings or fragments stay distinct. The raw annotations stay in the text part's provider metadata as `openai.annotations`. A `file_path` annotation is a generated file, not a citation. When the response has no retrieval data, the annotation's own source is still emitted as a source. In a stream that decision is made when the stream ends. Provider-executed `web_search_call` and `file_search_call` results are reported as tool results named `web_search` and `file_search`.
- OpenAI Responses sends the `reasoningSummary` provider option (any string, such as `auto`, `concise`, or `detailed`; OpenAI validates it) as `reasoning.summary` on reasoning models, next to `reasoning.effort`. When a reasoning effort other than `none` is set and `reasoningSummary` is absent, the summary defaults to `detailed`. Pass `reasoningSummary: null` to send none. Setting `reasoningSummary` on a non-reasoning model drops it with an `unsupported` warning.
- OpenAI Responses also reads these provider options:
  - `reasoningMode` (`standard` or `pro`) is sent as `reasoning.mode`, and `reasoningContext` (`auto`, `current_turn`, or `all_turns`) as `reasoning.context`. Both warn on non-reasoning models. `reasoningMode: "pro"`, `contextManagement`, and `truncation: "auto"` do not allow `reasoningEffortUpdate`.
  - `contextManagement` (a list of `{ "type": "compaction", "compactThreshold": n }`) is sent as `context_management`. `compactionTrigger: true` appends a `compaction_trigger` item as the last input item.
  - `conversation` is sent as `conversation`. It warns when combined with `previousResponseId`. With either `conversation` or `previousResponseId`, assistant reasoning that carries an OpenAI `itemId` is not resent. Build that reasoning with the `AssistantModelMessage` constructor that takes `reasoningProviderOptions`. Without a continuation, a reasoning `itemId` is sent as an `item_reference` (or as a `reasoning` item when `store` is `false`).
- Azure OpenAI Responses: `AzureOpenAIProvider.ResponsesModel(id)` posts to `/openai/v1/responses` (provider id `azure.responses`). Provider options and message options are read from `azure` first and fall back to `openai` only when the `azure` key is absent. An empty `azure` object therefore hides the `openai` options.
- Anthropic speaks the Messages API. `AnthropicAwsProvider` points that body at the AWS external endpoint.
  - `GeneratedSource` parts come from `web_search_tool_result` blocks, one per search result. The citations on a text block are attached to its `GeneratedText.Citations` (a `Citation` for each of `web_search_result_location`, `search_result_location`, `page_location`, `char_location`, and `content_block_location`), including on a streamed `TextEndStreamPart`. They are not repeated as sources. A `web_search_result_location` citation only becomes a `GeneratedSource` when the response has no search results. The raw citations stay in the text part's provider metadata as `anthropic.citations`. Page, character, and block locations refer to the source document, not to the generated answer. A document citation uses the `text/plain` media type, and the title falls back to `Document {index}`, because the port does not track the prompt's documents.
  - `claude-sonnet-5-5` always thinks and rejects thinking token budgets and forced tool use. Its lowest thinking setting is `thinking: { type: "between_tools" }`: no upfront thinking, only short progress notes between tool calls. `thinking: { type: "disabled" }` and `reasoning: "none"` are sent as `between_tools` (the first with a warning). `between_tools` is accepted only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning. A `required` or named tool choice falls back to `auto`, and `structuredOutputMode: "jsonTool"` falls back to native `output_config.format`, each with a warning.
  - `claude-haiku-5-5` supports every effort level, including `xhigh`. It uses adaptive thinking and rejects thinking token budgets: `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning, so use `effort` to control how much it thinks. Unlike `claude-sonnet-5-5`, thinking can be disabled, but only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. Structured output uses native `output_config.format`.
- Google speaks Gemini `generateContent`. `GoogleVertexProvider` changes the base URL and sends a bearer token.
- Azure OpenAI uses `api-key` and `/openai/deployments/{model}/chat/completions`.
- Amazon Bedrock signs a Converse request with Signature Version 4. It signs the caller's headers too, and sends a header with a non-ASCII value without signing it. Structured output (`JsonSchema`) goes through a forced `json` tool, and the tool input comes back as the response text with `providerMetadata.bedrock.isJsonResponseFromTool` set. `AmazonBedrockFetch` and `AmazonBedrockSigV4Fetch` apply the same signing and the `ai-sdk-amazon-bedrock` user agent to any other Bedrock call.
- Cohere speaks chat, embed, and rerank.
- OpenResponses posts to `{base}/responses`. The default base is the Gateway Open Responses route.
- Perplexity language generation posts to `{base}/v1/agent` (the Agent API). Preset ids are `fast`, `low`, `medium`, `high`, and `xhigh`. Any other id is sent as an Agent API model id. Legacy Sonar model ids are not aliased, and Sonar provider options are not translated. Embeddings stay on the OpenAI-compatible embeddings route. Sonar PDF input, video input, and image or video results have no Agent API equivalent.

The portable `Reasoning` option on `GenerateTextOptions` and `LanguageModelCallOptions` accepts `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, and `max`. Budget-based reasoning uses 95 percent of the output budget for `max`. Providers with a lower ceiling coerce `max` and return a `compatibility` warning:

- Anthropic, DeepSeek, Moonshot, and OpenAI models that list `max` send it unchanged, with no warning.
- Google (Gemini 3), Groq, and Fireworks send `high`. Fireworks also sends `low` for `minimal` and `high` for `xhigh`. Each change adds a warning. A `reasoningEffort` provider option wins over `Reasoning` and adds no warning.
- Perplexity sends `xhigh`.

OpenAI-compatible providers (Alibaba, Groq, DeepSeek, Mistral, xAI, Together, and the other chat wrappers) are thin wrappers over `Vercel.AI.OpenAICompatible`. They set the base URL, the provider id, and the environment variable. Perplexity embeddings use that client. Perplexity language generation uses the Agent API described above.

Speech, transcription, image, video, and Voyage each have a package that calls that provider’s public HTTP API. Use `SpeechModel`, `TranscriptionModel`, `ImageModel`, `VideoModel`, or `EmbeddingModel` rather than `LanguageModel`.

## Decision state

`EvaluateRequest.State` is a string, a JSON object, or a list of parts. A raw JSON array is not state. Wrap it in an object or in a `json` part. Each part is a dictionary: `{ type: "text", text }`, `{ type: "json", value }`, or `{ type: "file", mediaType, data, filename? }`. `Evaluate.EvaluateAsync` normalizes the state with `DecisionState.PrepareDecisionStateAsync` before the model call. A string becomes one text part, an object becomes one json part, and a file part gets a full media type. A URL file is downloaded. A failed download throws `DownloadError` before any model call. `EvaluationModelCall.State` and `EvaluateModelEvent.State` hold the normalized parts. `EvaluateEvent.State` keeps the public value. `OpenAIDecisionModel` sends text and json parts as `input_text` and inline PNG, JPEG, WebP, or GIF files as `input_image`. It throws `UnsupportedFunctionalityException` for other files. `GatewayDecisionModel` keeps its legacy state format: one text part or one json object is sent as a string or object, and anything else as an array of values. It throws `UnsupportedFunctionalityException` for any file part.

## Decision language model

`new DecisionLanguageModel(languageModel, provider?)` wraps a V4 `ILanguageModel` so `Evaluate.EvaluateAsync` can use it for `choice`, `score`, and `boolean` questions. The provider id defaults to the wrapped model's provider plus `.decision`. It sends one structured-output request with reasoning set to `none`. Question ids and choice labels are replaced by internal keys (`q0`, `c0`, ...) in the schema and mapped back in the answers. State parts keep their order. Text and image file parts pass through, and a json part is sent as compact JSON text. Any non-image file part throws `UnsupportedFunctionalityException` before the model is called. A wrong finish reason, invalid JSON, a missing or extra answer, an unknown choice, a score outside its rubric, or a probability outside 0 to 1 throws `InvalidResponseDataException`. A choice needs at least one option and a score at least two levels, otherwise `InvalidArgumentException` is thrown. The C# file content part has no provider options, so per-file provider options are not forwarded.

## Gateway decision fallbacks

`providerOptions.gateway.models` for a decision request may start with one conditional entry, `{ "model": "...", "when": { ... } }`, followed by plain model ids. `when` is `confidenceBelow`, `probabilityBetween: [min, max]`, or an `any`, `all`, or `atLeast` group of conditions, nested at most 5 levels with 1 to 20 conditions per list. `GatewayDecisionModel` rejects an invalid shape with an `InvalidArgumentException` before any request. Other gateway options pass through unchanged.

## Batch result downloads

`GatewayOptions`, `OpenAIOptions`, `AnthropicOptions`, and `GoogleOptions` each take an optional `BatchResultDownloads` setting. It configures the download of JSON Lines batch results. `provider.ExperimentalBatch()` returns the batch API whose `DoGetResultsAsync` reads them.

- **BatchResultDownloads** _BatchResultDownloads_

  Settings for downloading JSON Lines batch results. For Google, the setting applies to file-based results.

  - **MaxLineBytes** _int?_

    Maximum UTF-8 bytes per row, excluding the LF delimiter. Defaults to 64 MiB (67,108,864 bytes). Must be positive. Oversized rows throw a `DownloadError` and cancel the download.

```csharp
var anthropic = AnthropicProvider.Create(new AnthropicOptions
{
    BatchResultDownloads = new BatchResultDownloads { MaxLineBytes = 16 * 1024 * 1024 },
});
```

`JsonStreams.ReadJsonLinesAsync` takes the same limit as `maxLineBytes`, plus the `url` reported by the `DownloadError`.

## Embedding dimensions

`Embed.EmbedAsync`, `EmbedMany.EmbedManyAsync`, and `AiClient.EmbedAsync` / `EmbedManyAsync` take an optional `Dimensions` setting. It requests an output vector size and must be a positive integer. Any other value throws an `InvalidArgumentException` for parameter `dimensions` before the model runs. `EmbedMany` sends the same value to every chunk and every retry, and the start events and the `OnEmbedStart` telemetry callback carry it. `AiClient` calls the model once and does not retry or raise those events. Provider options are not changed.

A provider option for the same setting wins over `Dimensions`. When both are unset, the field is left out of the request.

| Model | Provider option | Request field |
| --- | --- | --- |
| OpenAI | `openai.dimensions` | `dimensions` |
| OpenAI-compatible | `dimensions` on the compatible options | `dimensions` |
| Cohere | `cohere.outputDimension` | `output_dimension` |
| Google (non-Vertex) | `google.outputDimensionality` | `outputDimensionality` |
| Perplexity | `perplexity.dimensions` | `dimensions` |
| Voyage | `voyage.outputDimension` | `output_dimension` |
| Alibaba | `alibaba.dimension` | `parameters.dimension` |

Gateway sends `dimensions` next to `values` and keeps `providerOptions` as its own object. It does not merge them. Amazon Bedrock, Google Vertex, and Mistral ignore `Dimensions` for now.

`IEmbeddingModel.DoEmbedAsync` takes `int? dimensions = null` after the cancellation token, and `EmbeddingModelCall` carries `Dimensions`. Custom models must add the parameter.
