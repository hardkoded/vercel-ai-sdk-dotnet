# Compatibility

Upstream parity target: [vercel/ai](https://github.com/vercel/ai) commit `269331916eb51ecd937a55df5fcf44ff8f63eaf8` (`feat(google): support Gemini 3.8 text-to-speech (#21403)`), recorded on 2026-09-23.

This port targets AI SDK 7 / Language Model specification **V4**. Names are C# versions of the JavaScript API: `generateText` is `GenerateTextAsync`, `streamText` is `StreamTextAsync` (it returns `StreamTextResult`, not a task), `AbortSignal` is `CancellationToken`, and option properties are PascalCase.

The default `StopWhen` for `GenerateTextAsync` is one step. Tools requested by that step still run. `Agent` stops after 10 steps unless you set another condition.

## Package map

| JavaScript | .NET |
| --- | --- |
| `ai` | `Vercel.AI.Sdk` |
| `@ai-sdk/provider` | `Vercel.AI.Sdk.Provider` |
| `@ai-sdk/provider-utils` | `Vercel.AI.Sdk.ProviderUtils` |
| `@ai-sdk/gateway` | `Vercel.AI.Sdk.Gateway` |
| `@ai-sdk/openai-compatible` | `Vercel.AI.Sdk.OpenAICompatible` |
| `@ai-sdk/openai` | `Vercel.AI.Sdk.OpenAI` |
| `@ai-sdk/anthropic` | `Vercel.AI.Sdk.Anthropic` (includes Anthropic on AWS) |
| `@ai-sdk/google` and `@ai-sdk/google-vertex` | `Vercel.AI.Sdk.Google` |
| `@ai-sdk/amazon-bedrock` | `Vercel.AI.Sdk.AmazonBedrock` |
| `@ai-sdk/azure` | `Vercel.AI.Sdk.Azure` |
| `@ai-sdk/cohere` | `Vercel.AI.Sdk.Cohere` |
| `@ai-sdk/mistral` | `Vercel.AI.Sdk.Mistral` |
| OpenAI-compatible providers (Groq, DeepSeek, xAI, Together, and the others under `packages/`) | `Vercel.AI.Sdk.<Provider>` |
| Speech, transcription, image, video, and Voyage | one `Vercel.AI.Sdk.<Provider>` package each |
| `ai` UI message stream used by `useChat` | `Vercel.AI.Sdk.AspNetCore` (`ToUIMessageStreamResult`) |
| OpenTelemetry | `Vercel.AI.Sdk.OpenTelemetry` |
| MCP tools | `Vercel.AI.Sdk.Mcp` |

`typesafe-ai` is not re-ported. Pair this library with the existing [TypeSafe.AI.Sdk](https://github.com/hardkoded/typesafe-sdk-dotnet) package when you want that companion.

## Not ported

These JavaScript packages are Node or browser products, not the model SDK:

- `react`, `vue`, `svelte`, `angular`, `rsc` — replaced by the ASP.NET Core UI message stream, not by those frameworks
- `valibot` — JSON Schema plus `System.Text.Json`
- `langchain`, `llamaindex`
- `devtools`, `codemod`, `test-server`, `tui`, `harness*`, `workflow*`, `sandbox-*`, `policy-opa`, `code-mode`

## UI message stream

`ToUIMessageStreamResult` emits `text/event-stream` with header `x-vercel-ai-ui-message-stream: v1` and these chunks: `start`, `start-step`, `text-start`, `text-delta`, `text-end`, `reasoning-start`, `reasoning-delta`, `reasoning-end`, `tool-input-available`, `tool-output-available`, `tool-output-error`, `source-url`, `finish-step`, `finish`, `error`, and `data: [DONE]`.

Not emitted yet, because the core stream does not surface them:

- `tool-input-start` and `tool-input-delta` (tool arguments are sent once, in `tool-input-available`)
- `abort`
- `message-metadata`
- custom `data-*` parts
- `source-document`
- `file` (generated files are not a `StreamTextResult` part)

## Provider notes

- Gateway language, embedding, and image calls use `https://ai-gateway.vercel.sh/v4/ai` and the V4 specification headers. They are not only the OpenAI-compatible `/v1/chat/completions` route.
- OpenResponses defaults to `https://ai-gateway.vercel.sh/v1/responses`.
- Azure OpenAI uses the deployments URL, the `api-key` header, and API version `2024-10-21`.
- Amazon Bedrock uses the Converse API and Signature Version 4. Streaming is generate-then-delta.
- Cohere chat streaming is generate-then-delta.
- OpenAI Realtime WebSocket headers are implemented on `net10.0`. `netstandard2.0` throws `PlatformNotSupportedException`.
- MiniMax uses the OpenAI-compatible `/v1` base. The JavaScript package also has an Anthropic-compatible path.
- Media providers speak that provider’s public HTTP API for one modality. Their inherited chat method is the OpenAI-compatible client and is not the supported entry point.
