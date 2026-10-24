# Compatibility

Upstream parity target: [vercel/ai](https://github.com/vercel/ai) commit `0c2a9de5de76570cc2ad0281382aa90cbd199d43` (`feat: add maxOutputTokens and reasoning to decide (#23590)`), recorded on 2026-10-20.

This port targets AI SDK 7 / Language Model specification **V4**. Names are C# versions of the JavaScript API: `generateText` is `GenerateTextAsync`, `streamText` is `StreamTextAsync` (it returns `StreamTextResult`, not a task), `AbortSignal` is `CancellationToken`, and option properties are PascalCase.

The default `StopWhen` for `GenerateTextAsync` is one step. Tools requested by that step still run. `Agent` stops after 10 steps unless you set another condition.

## Package map

| JavaScript | .NET |
| --- | --- |
| `ai` | `Vercel.AI` |
| `@ai-sdk/provider` | `Vercel.AI.Provider` |
| `@ai-sdk/provider-utils` | `Vercel.AI.ProviderUtils` |
| `@ai-sdk/gateway` | `Vercel.AI.Gateway` |
| `@ai-sdk/openai-compatible` | `Vercel.AI.OpenAICompatible` |
| `@ai-sdk/openai` | `Vercel.AI.OpenAI` |
| `@ai-sdk/anthropic` | `Vercel.AI.Anthropic` (includes Anthropic on AWS) |
| `@ai-sdk/google` and `@ai-sdk/google-vertex` | `Vercel.AI.Google` |
| `@ai-sdk/amazon-bedrock` | `Vercel.AI.AmazonBedrock` |
| `@ai-sdk/azure` | `Vercel.AI.Azure` |
| `@ai-sdk/cohere` | `Vercel.AI.Cohere` |
| `@ai-sdk/mistral` | `Vercel.AI.Mistral` |
| OpenAI-compatible providers (Groq, DeepSeek, xAI, Together, and the others under `packages/`) | `Vercel.AI.<Provider>` |
| Speech, transcription, image, video, and Voyage | one `Vercel.AI.<Provider>` package each |
| `ai` UI message stream used by `useChat` | `Vercel.AI.AspNetCore` (`ToUIMessageStreamResult`) |
| OpenTelemetry | `Vercel.AI.OpenTelemetry` |
| MCP tools | `Vercel.AI.Mcp` |

`@ai-sdk/typesafe-ai` is `Vercel.AI.TypeSafeAi`. It provides the experimental TypeSafe decision model. See [TypeSafe](docs/articles/typesafe-ai.md). The [TypeSafe.AI.Sdk](https://github.com/hardkoded/typesafe-sdk-dotnet) package remains available as a standalone client.

## Not ported

These JavaScript packages are Node or browser products, not the model SDK:

- `react`, `vue`, `svelte`, `angular`, `rsc` — replaced by the ASP.NET Core UI message stream, not by those frameworks
- `valibot` — JSON Schema plus `System.Text.Json`
- `langchain`, `llamaindex`
- `devtools`, `codemod`, `test-server`, `tui`, `harness*`, `workflow*`, `sandbox-*`, `policy-opa`, `code-mode`

## UI message stream

`ToUIMessageStreamResult` emits `text/event-stream` with header `x-vercel-ai-ui-message-stream: v1` and these chunks: `start`, `start-step`, `text-start`, `text-delta`, `text-end`, `reasoning-start`, `reasoning-delta`, `reasoning-end`, `tool-input-available`, `tool-output-available`, `tool-output-error`, `source-url`, `finish-step`, `finish`, `error`, and `data: [DONE]`. The `text-end` chunk carries `citations` when the step's text has them.

Optional `keepAliveMs` writes `: stream-open` before the start event and `: keep-alive` while the next part is still pending. Null sends no comment lines.

Not emitted yet, because the core stream does not surface them:

- `tool-input-start` and `tool-input-delta` (tool arguments are sent once, in `tool-input-available`)
- `abort`
- `message-metadata`
- custom `data-*` parts
- `source-document`
- `file` (generated files are not a `StreamTextResult` part)

## Provider notes

- Gateway language, embedding, and image calls use `https://ai-gateway.vercel.sh/v4/ai`, the V4 specification headers, `ai-gateway-protocol-version: 0.0.1`, and `ai-gateway-auth-method: api-key`. They are not only the OpenAI-compatible `/v1/chat/completions` route.
- OpenResponses defaults to `https://ai-gateway.vercel.sh/v1/responses`.
- Azure OpenAI uses the `/openai/v1` URLs with API version `v1` and the `api-key` header. `UseDeploymentBasedUrls` switches to the deployments URLs.
- Amazon Bedrock uses the Converse API and Signature Version 4. Streaming calls `/converse-stream` and decodes the AWS event stream.
- Amazon Bedrock Anthropic and Mantle delegate to the Anthropic Messages and OpenAI models. The Mantle Responses model does not turn off `supportsWebSearchSourcesInclude`, and the Bedrock Anthropic provider has no `credentialProvider` or `tools` member.
- Cohere chat streaming is generate-then-delta.
- OpenAI Realtime WebSocket headers are implemented on `net10.0`. `netstandard2.0` throws `PlatformNotSupportedException`.
- MiniMax uses the Anthropic-compatible `/anthropic/v1` base through the shared Anthropic adapter, as the JavaScript package does. The MiniMax video models are not ported. The typed `MiniMaxLanguageModelOptions`, `MiniMaxCacheControlOptions`, `MiniMaxImagePartOptions`, and `MiniMaxVideoPartOptions` types are not ported either; pass the options as JSON under `minimax`.
- Media providers speak that provider’s public HTTP API for one modality. Their inherited chat method is the OpenAI-compatible client and is not the supported entry point.
- Perplexity language generation uses the Agent API at `{base}/v1/agent`. Embeddings stay on the OpenAI-compatible embeddings route. Legacy Sonar model ids are not aliased. Sonar PDF input, video input, and image or video results have no Agent API equivalent.
