# Providers

Native wire protocols have their own request mappers:

- Gateway (`AI_GATEWAY_API_KEY`) posts V4 call options to `/language-model`, `/embedding-model`, and `/image-model`. `DecisionModel(id)` posts the state and questions to `/decision-model`.
- Google `DecisionModel(id)` answers `Decide` questions with Gemini structured output (provider id `google.decision`). It wraps the Gemini language model in `DecisionLanguageModel`, which sends one request for every Choice, Score, and Boolean question and calls the model with reasoning `none` (the minimum thinking level on Gemini 3 models). `providerOptions.google` passes through, so `thinkingConfig` overrides the default. A finish reason other than `STOP`, invalid JSON, an unknown choice, or a score or probability outside its range throws `InvalidResponseDataException`. File parts must be images.
- OpenAI speaks Chat Completions, the Responses API, embeddings, images, speech, and transcription. `DecisionModel(id)` answers `Decide` questions through the Decisions API (`POST /decisions`; provider id `openai.decision`). Its only provider option is `safetyIdentifier` (a string of at most 128 characters). Any other `openai` option is reported as an `unsupported` warning and not sent. A refused question returns a `refusal` answer, and `Decide.DecideAsync` throws `DecisionRefusalError` for it.
- OpenAI Responses on GPT-6 and later models accept `reasoningEffortUpdate` (`none`, `low`, `medium`, `high`, `xhigh`, or `max`). Set it in the `openai` provider options to prepend a `configuration_update` item to the input. Set it on a `SystemModelMessage` with empty content (through its `providerOptions`) to place the update inside the conversation. Pass that message in the messages list. The `instructions` argument is rebuilt from text only and drops message provider options. Sampling parameters (`temperature`, `topP`) and `logprobs` follow the last update, so a `none` update on GPT-6 Sol or Luna keeps them. Adjacent updates, unsupported efforts, and message updates on other models throw `UnsupportedFunctionalityException`. A request-level update on an unsupported model is dropped with a warning.
- OpenAI Responses exposes the URLs in `web_search_call.action.sources` as `GeneratedSource` parts, and each retrieved `file_search_call` file once as a `GeneratedDocumentSource` (even when several passages come from one file, or the file is not cited). Inline `url_citation`, `file_citation`, and `container_file_citation` annotations become `GeneratedText.Citations`, with the output range as `StartIndex` and `EndIndex`, and on a streamed `TextEndStreamPart` for each completed message. Repeated ranges and URLs with query strings or fragments stay distinct. The raw annotations stay in the text part's provider metadata as `openai.annotations`. A `file_path` annotation is a generated file, not a citation. When the response has no retrieval data, the annotation's own source is still emitted as a source. In a stream that decision is made when the stream ends. Provider-executed `web_search_call` and `file_search_call` results are reported as tool results named `web_search` and `file_search`.
- OpenAI Responses sends the `reasoningSummary` provider option (any string, such as `auto`, `concise`, or `detailed`; OpenAI validates it) as `reasoning.summary` on reasoning models, next to `reasoning.effort`. When a reasoning effort other than `none` is set and `reasoningSummary` is absent, the summary defaults to `detailed`. Pass `reasoningSummary: null` to send none. Setting `reasoningSummary` on a non-reasoning model drops it with an `unsupported` warning.
- OpenAI Responses also reads these provider options:
  - `reasoningMode` (`standard` or `pro`) is sent as `reasoning.mode`, and `reasoningContext` (`auto`, `current_turn`, or `all_turns`) as `reasoning.context`. Both warn on non-reasoning models. `reasoningMode: "pro"`, `contextManagement`, and `truncation: "auto"` do not allow `reasoningEffortUpdate`.
  - `contextManagement` (a list of `{ "type": "compaction", "compactThreshold": n }`) is sent as `context_management`. `compactionTrigger: true` appends a `compaction_trigger` item as the last input item.
  - `conversation` is sent as `conversation`. It warns when combined with `previousResponseId`. With either `conversation` or `previousResponseId`, assistant reasoning that carries an OpenAI `itemId` is not resent. Build that reasoning with the `AssistantModelMessage` constructor that takes `reasoningProviderOptions`. Without a continuation, a reasoning `itemId` is sent as an `item_reference` (or as a `reasoning` item when `store` is `false`).
  - In multi-step `GenerateText` and `StreamText`, the provider metadata of reasoning (`GeneratedReasoning.ProviderMetadata`, or the metadata on `ReasoningStartStreamPart` and `ReasoningEndStreamPart`) is carried to the next step as `AssistantModelMessage.ReasoningProviderOptions`. OpenAI Responses reasoning returns `itemId` and `reasoningEncryptedContent` this way, so with `store: false` the encrypted reasoning is sent back.
- Azure OpenAI Responses: `AzureOpenAIProvider.ResponsesModel(id)` posts to `/openai/v1/responses` (provider id `azure.responses`). Provider options and message options are read from `azure` first and fall back to `openai` only when the `azure` key is absent. An empty `azure` object therefore hides the `openai` options.
- TypeSafe (`TYPESAFE_AI_API_KEY`) posts the state and questions to `/systemone` through `TypeSafeAiProvider.DecisionModel(id)` (provider id `typesafe.decision`). See [TypeSafe](typesafe-ai.md).
- Anthropic speaks the Messages API. `AnthropicAwsProvider` points that body at the AWS external endpoint.
  - `GeneratedSource` parts come from `web_search_tool_result` blocks, one per search result. The citations on a text block are attached to its `GeneratedText.Citations` (a `Citation` for each of `web_search_result_location`, `search_result_location`, `page_location`, `char_location`, and `content_block_location`), including on a streamed `TextEndStreamPart`. They are not repeated as sources. A `web_search_result_location` citation only becomes a `GeneratedSource` when the response has no search results. The raw citations stay in the text part's provider metadata as `anthropic.citations`. Page, character, and block locations refer to the source document, not to the generated answer. A document citation uses the `text/plain` media type, and the title falls back to `Document {index}`, because the port does not track the prompt's documents.
  - `claude-sonnet-5-5` always thinks and rejects thinking token budgets and forced tool use. Its lowest thinking setting is `thinking: { type: "between_tools" }`: no upfront thinking, only short progress notes between tool calls. `thinking: { type: "disabled" }` and `reasoning: "none"` are sent as `between_tools` (the first with a warning). `between_tools` is accepted only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning. A `required` or named tool choice falls back to `auto`, and `structuredOutputMode: "jsonTool"` falls back to native `output_config.format`, each with a warning.
  - `claude-haiku-5-5` supports every effort level, including `xhigh`. It uses adaptive thinking and rejects thinking token budgets: `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning, so use `effort` to control how much it thinks. Unlike `claude-sonnet-5-5`, thinking can be disabled, but only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. Structured output uses native `output_config.format`.
- Google speaks Gemini `generateContent`. `GoogleVertexProvider` changes the base URL and sends a bearer token.
- Azure OpenAI uses `api-key` and `/openai/deployments/{model}/chat/completions`.
- Amazon Bedrock signs a Converse request with Signature Version 4. It signs the caller's headers too, and sends a header with a non-ASCII value without signing it. Provider options come from `amazonBedrock`, then `bedrock`. `reasoningConfig` (`type`, `budgetTokens`, `maxReasoningEffort` of `low`, `medium`, `high`, `xhigh`, or `max`, and `display`) becomes `additionalModelRequestFields.thinking` and `output_config.effort` on Anthropic models, `reasoning_effort` or `reasoning.effort` on OpenAI models, and a `reasoningConfig` field on other models such as Nova. The top-level `Reasoning` option fills in `reasoningConfig`, and fields you set win. Structured output (`JsonSchema`) on an Anthropic model follows `structuredOutputMode` (`outputFormat`, `jsonTool`, or `auto`, read from `amazonBedrock`, then `anthropic`). In `auto`, models that support it send a sanitized `additionalModelRequestFields.output_config.format`, and `jsonTool` removes a manually set `format`. Otherwise a forced `json` tool carries it, and the tool input comes back as the response text with `providerMetadata.bedrock.isJsonResponseFromTool` set. Models that reject forced tool use, and models that reject `strict` tools when you also pass tools, get a JSON instruction in the system message instead, and the first JSON object in the reply becomes the text. `AmazonBedrockFetch` and `AmazonBedrockSigV4Fetch` apply the same signing and the `ai-sdk-amazon-bedrock` user agent to any other Bedrock call.
- Cohere speaks chat, embed, and rerank.
- OpenResponses posts to `{base}/responses`. The default base is the Gateway Open Responses route.
- Perplexity language generation posts to `{base}/v1/agent` (the Agent API). Preset ids are `fast`, `low`, `medium`, `high`, and `xhigh`. Any other id is sent as an Agent API model id. Legacy Sonar model ids are not aliased, and Sonar provider options are not translated. Embeddings stay on the OpenAI-compatible embeddings route. Sonar PDF input, video input, and image or video results have no Agent API equivalent.

The portable `Reasoning` option on `GenerateTextOptions` and `LanguageModelCallOptions` accepts `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, and `max`. Budget-based reasoning uses 95 percent of the output budget for `max`. Providers with a lower ceiling coerce `max` and return a `compatibility` warning:

- Anthropic, DeepSeek, Moonshot, and OpenAI models that list `max` send it unchanged, with no warning.
- Google (Gemini 3), Groq, and Fireworks send `high`. Fireworks also sends `low` for `minimal` and `high` for `xhigh`. Each change adds a warning. A `reasoningEffort` provider option wins over `Reasoning` and adds no warning.
- Perplexity sends `xhigh`.
- Mistral sends `reasoning_effort` `high` for every level except `none`, and returns a `compatibility` warning when it changes the level. Models without reasoning effort (such as `mistral-large-latest`) get no `reasoning_effort` and an `unsupported` warning. A `reasoningEffort` provider option wins.
- OpenResponses sends `reasoning.effort` (`minimal` becomes `low`; `xhigh` and `max` are sent unchanged). The `open-responses` provider options `reasoningEffort` and `reasoningSummary` set `reasoning.effort` and `reasoning.summary`.
- xAI Responses (`XaiProvider.ResponsesModel`) sends `reasoning.effort` (`minimal` becomes `low`). `xhigh` and `max` are sent as `xhigh` on `grok-4.6` and as `high` on other models, with a `compatibility` warning when the level changes. The `grok-4.20` reasoning and non-reasoning models reject the parameter: they get no `reasoning` object and an `unsupported` warning. A `reasoningEffort` provider option wins and adds no warning.

Moonshot native structured outputs are enabled for every model id, including Kimi, Moonshot V1, and custom or future ids. A request with a JSON schema sends `response_format.type = json_schema` with `strict: true`. A JSON request with no schema uses `json_object`.

Groq rejects `response_format` together with tools, and Gemini before Gemini 3 rejects JSON response formatting together with function tools. When a call has a JSON schema and at least one function tool, these providers send an internal function tool whose arguments are the structured output, and turn that tool call back into text. The tool is named `json`, or `json_1`, `json_2`, and so on when a caller tool already uses the name. Its description is the output description (`OutputSpec.Object(schema, name, description)`, or `LanguageModelCallOptions.JsonSchemaDescription`), or `Respond with a JSON object.`. Tool choice `Tool("name")` is kept, `None` selects the response tool, and any other choice becomes `Required`. Groq sets `parallel_tool_calls` to false and skips the `structuredOutputs` warning on this path. Gemini 3 keeps native `responseMimeType` and `responseJsonSchema` with automatic or omitted tool choice, and uses the response tool only for `Required` or a named tool. Conversational text is dropped, calls to your own tools stay tool calls, and a response that has only the response tool finishes with `Stop`. Without function tools, both providers keep native structured output. `JsonResponseTools.ConvertJsonResponseToolStream` applies the same rewrite to a stream of `LanguageModelStreamPart`.

OpenAI-compatible providers (Alibaba, Groq, DeepSeek, Mistral, xAI, Together, and the other chat wrappers) are thin wrappers over `Vercel.AI.OpenAICompatible`. They set the base URL, the provider id, and the environment variable. Perplexity embeddings use that client. Perplexity language generation uses the Agent API described above.

Speech, transcription, image, video, and Voyage each have a package that calls that provider’s public HTTP API. Use `SpeechModel`, `TranscriptionModel`, `ImageModel`, `VideoModel`, or `EmbeddingModel` rather than `LanguageModel`.

## HeyGen

The HeyGen provider (`Vercel.AI.HeyGen`) supports `heygen-video-1`, a video generation model with text-to-video, image-to-video, and reference-to-video modes. Generated clips include audio. It supports video generation only. HeyGen's avatar, voice, translation, and video editing APIs are not included.

### Provider instance

```csharp
using Vercel.AI.HeyGen;
using Vercel.AI.OpenAICompatible;

var heygen = HeyGenProvider.Create(new OpenAICompatibleOptions { ApiKey = Environment.GetEnvironmentVariable("HEYGEN_API_KEY") });
```

These settings are optional:

- **ApiKey** _string_: The key sent in the `x-api-key` header. Defaults to the `HEYGEN_API_KEY` environment variable. The key is read when a model is used, so a missing key fails then and not when the provider is created.
- **BaseUrl** _string_: The API URL prefix. Defaults to `https://api.heygen.com`.
- **Headers** _IDictionary&lt;string, string&gt;_: Additional request headers.

`AddHeyGen` registers the provider with dependency injection. `LanguageModel`, `EmbeddingModel`, `ImageModel`, `SpeechModel`, and `TranscriptionModel` throw an `AiSdkException`.

### Video models

Create a model with `heygen.VideoModel("heygen-video-1")` or its alias `heygen.Video("heygen-video-1")`. `HeyGenVideoModel` implements `IVideoCaller`, so pass it as the `Model` of `GenerateVideoRequest`.

#### Text to video

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)heygen.VideoModel("heygen-video-1"),
    Prompt = new VideoPrompt("A paper boat floats down a quiet stream. Static camera, soft morning light, the sound of flowing water. No music."),
    Duration = 5,
    AspectRatio = "16:9",
});
```

#### Image to video

Pass an image in the prompt to use it as the first frame. The video follows the image's aspect ratio. A supplied `AspectRatio` is ignored with a warning.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)heygen.VideoModel("heygen-video-1"),
    Prompt = new VideoPrompt("The subject moves slowly. Preserve the composition and natural lighting.", "https://example.com/first-frame.jpg"),
    Duration = 5,
});
```

A single `first_frame` in `FrameImages` is also supported. Last-frame conditioning and multiple first frames are rejected.

#### Reference to video

Use `InputReferences` to condition a new scene on images or videos. Include a `MediaType` for URL references so the provider can select the reference kind.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)heygen.VideoModel("heygen-video-1"),
    Prompt = new VideoPrompt("The product in <Picture 1> sits on a desk beside a sunlit window."),
    InputReferences = new[] { new VideoReference("https://example.com/product.jpg", "image/jpeg") },
    Duration = 5,
    AspectRatio = "16:9",
});
```

References are grouped by media type, preserving order within each group. Address them in the prompt as `<Picture 1>`, `<Video 1>`, and `<Audio 1>`. References in the `heygen` provider options are appended after the standard references of the same kind.

Reference-to-video requires at least one image or video. Audio-only conditioning is not supported. HeyGen accepts up to 9 image references, 3 video references, and 3 audio references, with a combined maximum of 12. Only the first 5 seconds of each reference video are used.

#### Provider options

Set the options under the `heygen` key of `ProviderOptions`:

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)heygen.VideoModel("heygen-video-1"),
    Prompt = new VideoPrompt("The product in <Picture 1> rotates slowly on a table. No music."),
    AspectRatio = "16:9",
    ProviderOptions = OperationJson.Parse("""
        {
          "heygen": {
            "resolution": "1080p",
            "promptEnhancement": "disabled",
            "referenceImages": [{ "type": "asset_id", "assetId": "YOUR_ASSET_ID" }]
          }
        }
        """),
});
```

- **mode** _`text_to_video` | `image_to_video` | `reference_to_video`_: Inferred from the inputs when omitted. Explicit modes must match the supplied inputs. First-frame images cannot be combined with references.
- **resolution** _`480p` | `768p` | `1080p` | `2k`_: Output size class. Defaults to `768p`. Takes precedence over the standard `Resolution`.
- **promptEnhancement** _`turbo` | `quality` | `disabled`_: HeyGen's prompt enhancement setting. The API defaults to `turbo`.
- **image** _asset_: First-frame input, including an existing asset ID. Cannot be combined with a standard image or `FrameImages`.
- **referenceImages** _asset[]_: Additional image references.
- **referenceVideos** _asset[]_: Additional video references.
- **referenceAudio** _asset[]_: Additional audio references.

An asset has one of these shapes:

```json
{ "type": "url", "url": "https://example.com/input.jpg" }
{ "type": "asset_id", "assetId": "YOUR_ASSET_ID" }
{ "type": "base64", "mediaType": "image/jpeg", "data": "BASE64_DATA" }
```

Asset IDs must already exist in the workspace associated with the API key. This provider does not upload or manage HeyGen assets. Standard file inputs (`VideoModelFile.FromFile`) are sent inline as base64. HeyGen permits up to 5 MB per inline image and 16 MB per inline video or audio input. URL inputs must use HTTPS and be directly accessible without redirects. HeyGen permits up to 16 MB per image URL and 32 MB per video or audio URL.

#### Resolution and aspect ratio

Prefer `providerOptions.heygen.resolution` to select a size class. The standard `Resolution` accepts the exact frame sizes below and infers the corresponding aspect ratio when `AspectRatio` is omitted:

| Aspect ratio | `480p`    | `768p`     | `1080p`     | `2k`        |
| ------------ | --------- | ---------- | ----------- | ----------- |
| `21:9`       | `960x416` | `1536x672` | —           | —           |
| `16:9`       | `832x480` | `1344x768` | `1890x1080` | `2688x1536` |
| `4:3`        | `640x480` | `1024x768` | —           | —           |
| `1:1`        | `480x480` | `768x768`  | —           | —           |
| `3:4`        | `480x640` | `768x1024` | —           | —           |
| `9:16`       | `480x832` | `768x1344` | `1080x1890` | `1536x2688` |

Text-to-video defaults to `16:9`. Reference-to-video defaults to `adaptive`, inheriting the first reference image or video's shape. At `1080p` or `2k`, text-to-video and reference-to-video require `16:9` or `9:16`. Image-to-video always inherits the first frame's shape, even when a standard frame size is supplied. Only its size class is used.

#### Other constraints

- Prompts must contain between 1 and 32,000 characters.
- Duration must be an integer from 5 to 15 seconds. The default is 5.
- The seed must not be negative.
- One video is generated per provider call.
- Output is MP4 at 24 fps with generated audio. An unsupported `Fps` or `GenerateAudio = false` produces a warning, as does `N` above 1.
- An invalid option is rejected with an `InvalidArgumentException` before the request.

#### Asynchronous generation

`HeyGenVideoModel` implements `DoStartAsync` and `DoStatusAsync`. The SDK polls for `GenerateVideo`. Webhook completion is not implemented.

HeyGen jobs continue running after submission. Canceling the `CancellationToken` stops the local request and does not cancel the remote generation. The result is the signed video URL, and the bytes are not downloaded. Reading the status of the same operation again refreshes the URL.

Pass an `Idempotency-Key` through the call headers when retrying the same submission. HeyGen retains keys for 24 hours. Concurrent duplicates return an HTTP 409 error.

#### Provider metadata

`providerMetadata.heygen.videos` has one entry per completed video. Each entry contains:

- `videoId`: The HeyGen video identifier.
- `mode`: The generation mode submitted to HeyGen.
- `resolution`: The resolved size class submitted to HeyGen, kept across status calls. This is separate from the actual output dimensions.
- `duration`, `width`, `height`, `aspectRatio`, and `seed`: Values reported by HeyGen on completion, when available.
- `timings.inference`: The inference timing reported by HeyGen, when available.

Submission and in-progress operation metadata exposes `videoId`, `mode`, and `resolution` directly under `providerMetadata.heygen`. Failed or canceled operations also expose `failureCode` when HeyGen reports it.

The HeyGen video endpoint does not document token counts or credit consumption in its generation response. The provider returns the reported duration and dimensions without estimating those quantities.

## xAI Responses

`XaiProvider.ResponsesModel(modelId)` creates a language model for the xAI Responses API (`POST {base}/responses`). The provider id is `xai.responses`. `XaiProvider.LanguageModel(modelId)` still uses Chat Completions.

```csharp
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Xai;

var xai = XaiProvider.Create(new OpenAICompatibleOptions { ApiKey = Environment.GetEnvironmentVariable("XAI_API_KEY") });
var model = xai.ResponsesModel("grok-4.3");
```

The model supports text, image, and file input, streaming, function tools, `JsonSchema` structured output, reasoning, and URL citations as sources. Reasoning returns as `XaiReasoning` content, which carries the `xai.itemId` and `xai.reasoningEncryptedContent` metadata. Streams send the same metadata on `reasoning-start` and `reasoning-end`. To continue a conversation, pass that metadata back on the assistant message (`ReasoningProviderOptions`). This round trip works only for messages you build by hand. `GenerateText` and `StreamText` do not yet carry the metadata into the next step ([#344](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/344)). A request sends `topK` as `top_k` and warns about `FrequencyPenalty`, `PresencePenalty`, and `StopSequences`. A file part with a URL becomes `input_image` for images and `input_file` for other types. Inline bytes are only accepted for images. For other files, upload them with `XaiProvider.Files()` and pass a provider reference.

### Provider options

Pass these under the `xai` key of `ProviderOptions`.

- **reasoningEffort** _string_: `none`, `low`, `medium`, `high`, or `xhigh` (`xhigh` is supported by `grok-4.6`). Overrides the portable `Reasoning` option.
- **reasoningSummary** _string_: Deprecated. xAI ignores it, so the model accepts it and does not send it.
- **logprobs** _bool_ and **topLogprobs** _int_ (0 to 8): `topLogprobs` also turns on `logprobs`.
- **minP** _number_ (0 to 1), **maxTurns** _int_, **parallelToolCalls** _bool_, **promptCacheKey** _string_, **safetyIdentifier** _string_, and **user** _string_.
- **serviceTier** _string_: `default` or `priority`. The service tier xAI used comes back in `providerMetadata.xai.serviceTier`.
- **store** _bool_: With `false`, the request also includes `reasoning.encrypted_content` so reasoning can be sent back.
- **previousResponseId** _string_.
- **include** _string[]_: `file_search_call.results`, `web_search_call.action.sources`, `code_interpreter_call.outputs`, `reasoning.encrypted_content`, or `no_inline_citations`.

An image file part accepts the provider option `xai.imageDetail` (`low`, `high`, or `auto`) as `FileContentPart.ProviderOptions`.

`providerMetadata.xai` holds `costInUsdTicks`, `serviceTier`, `promptCacheKey`, and `safetyIdentifier` when the response reports them.

### Not included

The xAI provider-defined tools (web search, X search, code execution, file search, image generation, view image, view X video, and MCP) are not mapped, because the port has no provider-defined tools. Stream error parts carry the message only, without the status code and retry flag that upstream adds. Tool results with image content are not sent.

## Uploaded File References

An OpenAI-compatible chat model accepts a file that you already uploaded to the provider. Build the part with a provider reference: a map from provider name to file id.

```csharp
var part = FileContentPart.FromProviderReference(
    "application/pdf",
    new Dictionary<string, string> { ["my-provider"] = "file-123" },
    "document.pdf");
```

- The reference key must match `OpenAICompatibleOptions.ProviderName`, including hyphens. A camelCase variant such as `myProvider` is never used as the id. A reference without that key throws `NoSuchProviderReferenceError`.
- The request body carries `{ "type": "file", "file": { "file_id": "file-123" } }`. This works for `application/pdf` and `image/png`. Properties from the part's `openaiCompatible` provider options are copied onto that part.
- `DoStreamAsync` sends the same part.
- Inline PDF bytes are unchanged: they are sent as a base64 `file_data` URL.

The provider does not upload the file. Which media types are accepted depends on the destination model.

## Decision state

`DecideRequest.State` is a string, a JSON object, or a list of parts. A raw JSON array is not state. Wrap it in an object or in a `json` part. Each part is a dictionary: `{ type: "text", text }`, `{ type: "json", value }`, or `{ type: "file", mediaType, data, filename?, providerOptions? }`. `Decide.DecideAsync` normalizes the state with `DecisionState.PrepareDecisionStateAsync` before the model call. A string becomes one text part, an object becomes one json part, and a file part gets a full media type. A URL file is downloaded. A failed download throws `DownloadError` before any model call. `DecisionModelCall.State` and `DecisionModelEvent.State` hold the normalized parts. `DecideEvent.State` keeps the public value. `OpenAIDecisionModel` sends text and json parts as `input_text` and inline PNG, JPEG, WebP, or GIF files as `input_image`. It throws `UnsupportedFunctionalityException` for other files. `GatewayDecisionModel` keeps its legacy state format: one text part or one json object is sent as a string or object, and anything else as an array of values. It throws `UnsupportedFunctionalityException` for any file part.

## Decision language model

`new DecisionLanguageModel(languageModel, provider?)` wraps a V4 `ILanguageModel` so `Decide.DecideAsync` can use it for `choice`, `score`, and `boolean` questions. The provider id defaults to the wrapped model's provider plus `.decision`. It sends one structured-output request with reasoning set to `none`. Question ids and choice labels are replaced by internal keys (`q0`, `c0`, ...) in the schema and mapped back in the answers. State parts keep their order. Text and image file parts pass through, and a json part is sent as compact JSON text. Any non-image file part throws `UnsupportedFunctionalityException` before the model is called. A wrong finish reason, invalid JSON, a missing or extra answer, an unknown choice, a score outside its rubric, or a probability outside 0 to 1 throws `InvalidResponseDataException`. A choice needs at least one option and a score at least two levels, otherwise `InvalidArgumentException` is thrown. A file part's `providerOptions` are forwarded as the `FileContentPart.ProviderOptions` of the image.

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

### OpenAI batch results

`OpenAIBatchApi.DoGetResultsAsync` converts each row of the output and error files. A row that fails never stops the later rows.

- A row with `error.code` `batch_cancelled` or `batch_expired` has status `cancelled` or `expired`. Any other row error has status `failed`.
- A row with a non-2xx `status_code` fails with `ErrorMessage`, `ErrorType`, `ErrorCode`, and `ErrorStatusCode` from `response.body.error`. When the body has no error message, the message is `OpenAI batch request failed with status code <n>.`
- A 2xx row whose body does not match the Responses response schema fails with `invalid_response`. This includes an unknown `output` item `type` and an `output` of `null`. A body with no `output` key also fails with `invalid_response`.
- A row that does not match the result line schema (`custom_id` string, `response.status_code` number, `error.code` and `error.message` strings) throws a `TypeValidationException` and stops the download.
- A completed batch with no output or error file throws `InvalidResponseDataException`.
- A Responses `output` converts to `BatchTextGeneration.Content`: `reasoning` (with `itemId` and `reasoningEncryptedContent` provider metadata), `text`, and `tool-call` parts for `function_call` and `custom_tool_call`. `web_search_call`, `file_search_call`, and `code_interpreter_call` become a provider-executed dynamic `tool-call` plus a `tool-result`. A failed or incomplete web search sets `IsError` on its result. Any other output item fails the row with `unsupported_content`.
- `FinishReason`, `RawFinishReason`, `Usage`, `ResponseId`, `Timestamp`, and `ModelId` come from the body. `ProviderMetadata.openai` holds `responseId` and, when present, `logprobs`, `serviceTier`, and `reasoningContext`.

## Response handlers

`ResponseHandlers` in `Vercel.AI.ProviderUtils` holds the closest .NET shape of the upstream response handler factories. Each one takes the `HttpResponseMessage`, the request `url`, and the request body values, and returns the value with the response headers (`ResponseHandlerResult<T>`) or, for the error handlers, an `ApiException`.

| Method | Upstream | Result |
| --- | --- | --- |
| `CreateBinaryResponseAsync` | `createBinaryResponseHandler` | The body bytes. An empty body throws an `ApiException` with "Response body is empty". |
| `CreateBinaryStreamResponseAsync` | `createBinaryStreamResponseHandler` | The body stream. An empty body throws "Empty response body". |
| `CreateEventSourceResponseAsync` | `createEventSourceResponseHandler` | A stream of parsed events. A read failure (`IOException`, `HttpRequestException`) on the successful response throws a retryable `ApiException` "Failed to process successful response" that keeps the status code, URL, headers, and the original error as `InnerException`. |
| `CreateJsonErrorResponseAsync` | `createJsonErrorResponseHandler` | An `ApiException` whose message comes from `errorToMessage`, or the status text when the body does not match the error schema. |
| `CreateStatusCodeErrorResponseAsync` | `createStatusCodeErrorResponseHandler` | An `ApiException` whose message is the status text. |

The binary and error handlers read at most `Download.DefaultMaxBytes`. A larger `Content-Length` throws a `DownloadError` ("exceeded maximum size") before the body is read, and the body is released.

`ApiException` now also carries `Url`, `RequestBodyValues`, `ResponseHeaders`, and `IsRetryable` (408, 429, and 500 and above by default).

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
| Amazon Bedrock Titan | `amazonBedrock.dimensions` | `dimensions` |
| Amazon Bedrock Cohere | `amazonBedrock.outputDimension` | `output_dimension` |
| Amazon Bedrock Nova | `amazonBedrock.embeddingDimension` | `singleEmbeddingParams.embeddingDimension` (1024 when unset) |
| Google Vertex (predict) | `googleVertex.outputDimensionality` | `parameters.outputDimensionality` |
| Google Vertex (embedContent) | `googleVertex.outputDimensionality` | `embedContentConfig.outputDimensionality` |
| Mistral | `mistral.outputDimension` | `output_dimension` |

`AmazonBedrockProvider.EmbeddingModel` creates an `InvokeModel` embedding model for Titan, Cohere (up to 96 values per call), and Nova model ids. The `bedrock` options key is accepted as well as `amazonBedrock`. Cohere token usage comes from the `x-amzn-bedrock-input-token-count` response header. When the header is missing, `Tokens` is null.

Application inference profile ARNs do not identify their underlying model. Pass the model family when you create the embedding model, so the provider can select the correct request format and batch size:

```csharp
var profileArn = "arn:aws:bedrock:us-east-1:123456789012:application-inference-profile/qibm5eutlkcy";
var model = provider.EmbeddingModel(profileArn, new AmazonBedrockEmbeddingModelSettings { ModelFamily = AmazonBedrockEmbeddingModelFamily.Cohere });
```

`AmazonBedrockOptions.Headers` are sent with every embedding and reranking request. `AmazonBedrockEmbeddingModel.Headers` are sent with the next call and override provider headers with the same name. Both are signed with the request.

`OpenAICompatibleEmbeddingModel.LastResponseBody` holds the raw body of the most recent call, next to `LastResponseHeaders`.

Gateway sends `dimensions` next to `values` and keeps `providerOptions` as its own object. It does not merge them.

`IEmbeddingModel.DoEmbedAsync` takes `int? dimensions = null` after the cancellation token, and `EmbeddingModelCall` carries `Dimensions`. Custom models must add the parameter.

### Decide and the deprecated Evaluate names

`Decide.DecideAsync(DecideRequest)` is `experimental_decide`. It answers typed questions against one shared state. A decision model implements `IDecisionCaller.DoDecideAsync(DecisionModelCall)` and returns a `DecisionModelResult`. Operation ids are `ai.decide` and `ai.decide.doDecide`. Telemetry callbacks on `DecideTelemetry` are `OnDecideStart`, `OnDecideEnd`, `OnDecisionModelCallStart`, and `OnDecisionModelCallEnd`. Lifecycle events are `DecideEvent` and `DecisionModelEvent`. The refused-question and unsupported-type errors are `DecisionRefusalError`, `DecisionUnsupportedQuestionTypeError` (marker `vercel.ai.error.AI_DecisionUnsupportedQuestionTypeError`), and `DecisionUnsupportedQuestionTypeException`.

The old names still compile and are marked `[Obsolete]`: `Evaluate.EvaluateAsync`, `EvaluateRequest`, `EvaluateResult`, `EvaluateEvent`, `EvaluateModelEvent`, `EvaluateTelemetry`, `IEvaluationCaller.DoEvaluateAsync`, `EvaluationQuestion`, `EvaluationAnswer`, `EvaluationRounding`, `EvaluationModelCall`, `EvaluationModelResult`, `EvaluationValues`, and the two `EvaluationUnsupportedQuestionType*` types. C# cannot make two names one type, so each alias derives from its decision type, and `Evaluate.EvaluateAsync` forwards to `Decide.DecideAsync`. A model that implements only `IEvaluationCaller` still runs through `Evaluate.EvaluateAsync`. To pass it to `Decide.DecideAsync`, wrap it with `DecisionCaller.FromLegacy(model)`. Both `Decide.DecideAsync` and `DecisionLanguageModel` throw the unsupported-type error as `EvaluationUnsupportedQuestionTypeException`, so a `catch` of the old or the new type matches, and `DecisionUnsupportedQuestionTypeError.IsInstance` recognizes it. Deprecated telemetry callbacks run once and receive the decision operation ids. When both a decision callback and its deprecated alias are set, only the decision callback runs. The rubric API (`AiClient.EvaluateAsync`, `IEvaluationModel`, `EvaluationResult`) is a different feature and keeps its names.
