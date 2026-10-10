# Providers

Native wire protocols have their own request mappers:

- Gateway (`AI_GATEWAY_API_KEY`) posts V4 call options to `/language-model`, `/embedding-model`, and `/image-model`. `DecisionModel(id)` posts the state and questions to `/decision-model`.
- OpenAI speaks Chat Completions, the Responses API, embeddings, images, speech, and transcription. `DecisionModel(id)` answers `Evaluate` questions through the Decisions API (`POST /decisions`; provider id `openai.decision`). Its only provider option is `safetyIdentifier` (a string of at most 128 characters). Any other `openai` option is reported as an `unsupported` warning and not sent. A refused question returns a `refusal` answer, and `Evaluate.EvaluateAsync` throws `DecisionRefusalError` for it.
- OpenAI Responses on GPT-6 and later models accept `reasoningEffortUpdate` (`none`, `low`, `medium`, `high`, `xhigh`, or `max`). Set it in the `openai` provider options to prepend a `configuration_update` item to the input. Set it on a `SystemModelMessage` with empty content (through its `providerOptions`) to place the update inside the conversation. Pass that message in the messages list. The `instructions` argument is rebuilt from text only and drops message provider options. Sampling parameters (`temperature`, `topP`) and `logprobs` follow the last update, so a `none` update on GPT-6 Sol or Luna keeps them. Adjacent updates, unsupported efforts, and message updates on other models throw `UnsupportedFunctionalityException`. A request-level update on an unsupported model is dropped with a warning.
- OpenAI Responses sends the `reasoningSummary` provider option (any string, such as `auto`, `concise`, or `detailed`; OpenAI validates it) as `reasoning.summary` on reasoning models, next to `reasoning.effort`.
- Anthropic speaks the Messages API. `AnthropicAwsProvider` points that body at the AWS external endpoint.
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
