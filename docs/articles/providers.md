# Providers

Native wire protocols have their own request mappers:

- Gateway (`AI_GATEWAY_API_KEY`) posts V4 call options to `/language-model`, `/embedding-model`, and `/image-model`. `new GatewayImageModel(provider, modelId, providerName)` sets `Provider` (upstream `GatewayConfig.provider`); it defaults to `gateway`. `DecisionModel(id)` posts the state and questions to `/decision-model`. `GatewayProvider.EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)`, and `GatewayProvider` also implements the deprecated `IEvaluationProvider`. Every Gateway request carries observability headers from `GatewayOptions.O11yHeaders`. When it is null, they come from `VERCEL_DEPLOYMENT_ID` (`ai-o11y-deployment-id`), `VERCEL_ENV` (`ai-o11y-environment`), `VERCEL_REGION` (`ai-o11y-region`), and `VERCEL_PROJECT_ID` (`ai-o11y-project-id`). Empty variables are left out. The `ai-o11y-request-id` header is not sent, because the port has no Vercel request context.
- Google `DecisionModel(id)` answers `Decide` questions with Gemini structured output (provider id `google.decision`). It wraps the Gemini language model in `DecisionLanguageModel`, which sends one request for every Choice, Score, and Boolean question and calls the model with reasoning `none` (the minimum thinking level on Gemini 3 models). `providerOptions.google` passes through, so `thinkingConfig` overrides the default. A finish reason other than `STOP`, invalid JSON, an unknown choice, or a score or probability outside its range throws `InvalidResponseDataException`. File parts must be images.
- OpenAI speaks Chat Completions, the Responses API, embeddings, images, speech, and transcription. `DecisionModel(id)` answers `Decide` questions through the Decisions API (`POST /decisions`; provider id `openai.decision`). `OpenAIProvider.EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)`. Its only provider option is `safetyIdentifier` (a string of at most 128 characters). Any other `openai` option is reported as an `unsupported` warning and not sent. A refused question returns a `refusal` answer, and `Decide.DecideAsync` throws `DecisionRefusalError` for it.
- OpenAI Responses on GPT-6 and later models accept `reasoningEffortUpdate` (`none`, `low`, `medium`, `high`, `xhigh`, or `max`). Set it in the `openai` provider options to prepend a `configuration_update` item to the input. Set it on a `SystemModelMessage` with empty content (through its `providerOptions`) to place the update inside the conversation. Pass that message in the messages list. The `instructions` argument is rebuilt from text only and drops message provider options. Sampling parameters (`temperature`, `topP`) and `logprobs` follow the last update, so a `none` update on GPT-6 Sol or Luna keeps them. Adjacent updates, unsupported efforts, and message updates on other models throw `UnsupportedFunctionalityException`. A request-level update on an unsupported model is dropped with a warning.
- OpenAI Responses adds `web_search_call.action.sources` to `include` when the request has a `web_search` or `web_search_preview` tool, so the visited URLs come back as sources. Set `includeWebSearchSources: false` in the `openai` provider options to turn it off. `OpenAIResponsesLanguageModel.SupportsWebSearchSourcesInclude` (default true) turns it off for every call of a model.
- OpenAI Responses exposes the URLs in `web_search_call.action.sources` as `GeneratedSource` parts, and each retrieved `file_search_call` file once as a `GeneratedDocumentSource` (even when several passages come from one file, or the file is not cited). Inline `url_citation`, `file_citation`, and `container_file_citation` annotations become `GeneratedText.Citations`, with the output range as `StartIndex` and `EndIndex`, and on a streamed `TextEndStreamPart` for each completed message. Repeated ranges and URLs with query strings or fragments stay distinct. The raw annotations stay in the text part's provider metadata as `openai.annotations`. A `file_path` annotation is a generated file, not a citation. When the response has no retrieval data, the annotation's own source is still emitted as a source. In a stream that decision is made when the stream ends. Provider-executed `web_search_call` and `file_search_call` results are reported as tool results named `web_search` and `file_search`.
- OpenAI Responses sends the `reasoningSummary` provider option (any string, such as `auto`, `concise`, or `detailed`; OpenAI validates it) as `reasoning.summary` on reasoning models, next to `reasoning.effort`. When a reasoning effort other than `none` is set and `reasoningSummary` is absent, the summary defaults to `detailed`. Pass `reasoningSummary: null` to send none. Setting `reasoningSummary` on a non-reasoning model drops it with an `unsupported` warning.
- `OpenAITools` builds the OpenAI provider-defined tools: `ImageGeneration(OpenAIImageGenerationToolOptions)`, `WebSearch(OpenAIWebSearchToolOptions)`, `Mcp(OpenAIMcpToolOptions)` (one of `ServerUrl` or `ConnectorId` is required), `LocalShell()`, `ApplyPatch()`, `Computer()`, `FileSearch(OpenAIFileSearchToolOptions)`, `CodeInterpreter(OpenAICodeInterpreterToolOptions)` (`ContainerId`, or `FileIds` for an automatic container), `Shell(OpenAIShellToolOptions)`, `CustomTool(OpenAICustomToolOptions)` (the tool name is `Name`; `GrammarSyntax` is `regex` or `lark`), `ToolSearch(OpenAIToolSearchToolOptions)`, `WebSearchPreview(OpenAIWebSearchPreviewToolOptions)`, and `ProgrammaticToolCalling()`. Each returns a `LanguageModelTool` with the upstream camelCase args. The factories do not validate ranges (upstream validates with zod).
- OpenAI Responses maps provider-defined tools (`LanguageModelTool.ProviderDefined(id, name, args)`) to hosted tools. `args` use the upstream camelCase names. Supported ids are `openai.file_search`, `openai.local_shell`, `openai.shell`, `openai.apply_patch`, `openai.computer`, `openai.web_search`, `openai.web_search_preview`, `openai.code_interpreter`, `openai.image_generation`, `openai.mcp`, `openai.custom`, `openai.programmatic_tool_calling`, and `openai.tool_search`. A `ToolChoice.Tool(name)` that names `code_interpreter`, `file_search`, `image_generation`, `web_search_preview`, `web_search`, `mcp`, `apply_patch`, `computer`, or `programmatic_tool_calling` is sent as that hosted tool, a name that matches an `openai.custom` tool is sent as a `custom` choice, and any other name is a function choice. The provider validates `args` with the upstream schemas and throws `TypeValidationException` for invalid ones. It also reads the OpenAI function tool options in `LanguageModelTool.ProviderOptions` (`{ "openai": { ... } }`): `namespace` (`name` and `description`, grouped into one `namespace` tool), `deferLoading`, `allowedCallers`, `async`, and `outputSchema`. `async` is dropped with a warning on models before GPT-6. A `skillReference` skill of `openai.shell` resolves its `providerReference` for `openai` and defaults `version` to `latest`. OpenAI batches accept function and provider-defined tools. A provider tool other than `openai.code_interpreter`, `openai.custom`, `openai.file_search`, `openai.web_search`, and `openai.web_search_preview` adds an `unsupported` warning, because the batch results cannot convert its output.
- OpenAI Responses loads tools after an ordinary tool result. Set `additionalTools` in the `openai` (or `azure`) provider options of a `ToolModelMessage` (its `providerMetadata`) to a list of function definitions (`type`, `name`, `parameters`, and optional `description` and `strict`; other keys, or a malformed list, throw). The provider keeps the `function_call_output` and sends an `additional_tools` item (`role: "developer"`) right after it, so replaying the same messages keeps the insertion point. An empty list sends no item. Set `Tool.ProviderOptions` (or `LanguageModelTool.ProviderOptions`) to `{ "openai": { "omitFromRequestTools": true } }` to keep a discovered tool active for validation and execution without listing its definition in the request `tools`. Forcing such a tool with `ToolChoice.Tool(name)` throws `UnsupportedFunctionalityException`. Set the `allowedTools` provider option (`{ "toolNames": [...], "mode": "auto" | "required" }`) to send `tool_choice: { type: "allowed_tools" }` and replace the request tool choice. It lists function, hosted, MCP, and custom tools by tool name or by provider tool name. Tools omitted from the request, deferred tools, tools in a namespace, and `tool_search` cannot be listed, so they are dropped with a warning. If no entry is left, the call throws `UnsupportedFunctionalityException`. `OpenAIResponsesLanguageModel.PrepareResponsesTools` takes the same settings in `PrepareResponsesToolsOptions`.
- OpenAI Responses reports each `function_call` as a `GeneratedToolCall` (a `ToolCallStreamPart` when streaming, after `tool-input-start`, `tool-input-delta`, and `tool-input-end` parts). Its `openai` provider metadata carries `itemId`, plus `async`, `namespace`, and `caller` when the output item has them. A `program` caller becomes `{ type: "program", callerId }`. Other callers pass through unchanged. A streamed `tool-input-end` carries `namespace` too. When the model calls an internal `parallel` wrapper (`tool_uses` with `functions.<name>` recipients) and you have not declared a tool named `parallel`, each nested call whose tool you declared becomes its own tool call with the id `{callId}_{index}` and an `openai.parallelToolCall` metadata object (`itemId`, `toolCallId`, `toolName`, `input`, `index`, `count`). If any recipient is undeclared, the wrapper is returned as a plain `parallel` call. A streamed wrapper buffers its input and emits the expanded calls when the item completes.
- OpenAI Responses reports a `program` output item (programmatic tool calling) as a provider-executed `GeneratedToolCall` (`ProviderExecuted = true`; a `ToolCallStreamPart` when streaming, on `response.output_item.done`). The tool name is the name you gave `OpenAITools.ProgrammaticToolCalling()` (or `LanguageModelTool.ProviderDefined("openai.programmatic_tool_calling", name, args)`). Its input is `{"code","fingerprint"}`. A `program_output` item becomes a `GeneratedToolResult` (a `ToolResultStreamPart` when streaming) with `{"result","status"}` (`completed` or `incomplete`). Both carry `openai.itemId` in provider metadata. Function calls the program makes carry `openai.caller` as described above. When you send these parts back, a program call becomes an `item_reference` (with `store`) or a `program` item, its result becomes an `item_reference` or a `program_output` item, and a function call or its result that has an `openai.caller` keeps it as `caller`.
- OpenAI Responses reads a `custom_tool_call` as a `GeneratedToolCall` whose `ArgumentsJson` is the JSON-encoded input string and whose finish reason is `ToolCalls`, and streams it as a `ToolInputStartStreamPart` with `InputEncoding = ToolInputEncoding.Text`, one `ToolInputDeltaStreamPart` per `response.custom_tool_call_input.delta` event (the raw text, not JSON-escaped), a `ToolInputEndStreamPart`, and a `ToolCallStreamPart` with the same `ArgumentsJson`. Its `openai` provider metadata carries `itemId`, plus `async` when present.
- OpenAI Responses also reads these provider options:
  - `reasoningMode` (`standard` or `pro`) is sent as `reasoning.mode`, and `reasoningContext` (`auto`, `current_turn`, or `all_turns`) as `reasoning.context`. Both warn on non-reasoning models. `reasoningMode: "pro"`, `contextManagement`, and `truncation: "auto"` do not allow `reasoningEffortUpdate`.
  - `contextManagement` (a list of `{ "type": "compaction", "compactThreshold": n }`) is sent as `context_management`. `compactionTrigger: true` appends a `compaction_trigger` item as the last input item.
  - `conversation` is sent as `conversation`. It warns when combined with `previousResponseId`. With either `conversation` or `previousResponseId`, assistant reasoning that carries an OpenAI `itemId` is not resent. Build that reasoning with the `AssistantModelMessage` constructor that takes `reasoningProviderOptions`. Without a continuation, a reasoning `itemId` is sent as an `item_reference` (or as a `reasoning` item when `store` is `false`).
  - With `store` enabled, an assistant text part that carries an OpenAI `itemId` (an `AssistantTextPart` with provider options) is sent as an `item_reference`. For a reasoning model, the reference is sent only when a reasoning part comes before the text in the same assistant message, because OpenAI rejects a stored message that is sent without its reasoning item. When the reasoning has been removed (for example by pruning), the text is sent inline instead. With `conversation`, text parts that carry an `itemId` are skipped because the conversation already holds them.
  - In multi-step `GenerateText` and `StreamText`, the provider metadata of reasoning (`GeneratedReasoning.ProviderMetadata`, or the metadata on `ReasoningStartStreamPart` and `ReasoningEndStreamPart`) is carried to the next step as `AssistantModelMessage.ReasoningProviderOptions`. OpenAI Responses reasoning returns `itemId` and `reasoningEncryptedContent` this way, so with `store: false` the encrypted reasoning is sent back.
- Azure OpenAI Responses: `AzureOpenAIProvider.ResponsesModel(id)` posts to `/openai/v1/responses` (provider id `azure.responses`). Provider options and message options are read from `azure` first and fall back to `openai` only when the `azure` key is absent. An empty `azure` object therefore hides the `openai` options.
- TypeSafe (`TYPESAFE_AI_API_KEY`) posts the state and questions to `/systemone` through `TypeSafeAiProvider.DecisionModel(id)` (provider id `typesafe.decision`). `providerOptions.typesafe.longContext` raises the context window from 64k input and 32k output tokens to 262k. PNG and JPEG file parts are sent as `multipart/form-data` images; other files throw `UnsupportedFunctionalityException`. See [TypeSafe](typesafe-ai.md).
- Anthropic speaks the Messages API. `AnthropicAwsProvider` points that body at the AWS external endpoint. `DecisionModel(id)` answers `Decide` questions by wrapping the Messages language model in `DecisionLanguageModel` (provider id `anthropic.decision`). `AnthropicProvider` implements `IDecisionProvider`, and the deprecated `IEvaluationProvider.EvaluationModel(id)` resolves to the same model. `AnthropicProvider.EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)`. `AnthropicProvider.RubricModel(id)` returns the rubric model.
  - `GeneratedSource` parts come from `web_search_tool_result` blocks, one per search result. The citations on a text block are attached to its `GeneratedText.Citations` (a `Citation` for each of `web_search_result_location`, `search_result_location`, `page_location`, `char_location`, and `content_block_location`), including on a streamed `TextEndStreamPart`. They are not repeated as sources. A `web_search_result_location` citation only becomes a `GeneratedSource` when the response has no search results. The raw citations stay in the text part's provider metadata as `anthropic.citations`. Page, character, and block locations refer to the source document, not to the generated answer. A document citation uses the `text/plain` media type, and the title falls back to `Document {index}`, because the port does not track the prompt's documents.
  - `compaction` in the `anthropic` provider options (`type` and optional `instructions`) asks Anthropic to summarize the conversation on demand. It is sent as `compaction` with the `compact-2026-09-04` beta header. Combining it with `contextManagement` throws `InvalidArgumentException` for `providerOptions`. A `compaction` response block becomes a text part whose `anthropic` metadata holds `type: "compaction"` and its `signature`. Replay it as an assistant text part with that same `anthropic` provider metadata, and the signed `compaction` block (with `signature`) goes back to the API.
  - `claude-sonnet-5-5` always thinks and rejects thinking token budgets and forced tool use. Its lowest thinking setting is `thinking: { type: "between_tools" }`: no upfront thinking, only short progress notes between tool calls. `thinking: { type: "disabled" }` and `reasoning: "none"` are sent as `between_tools` (the first with a warning). `between_tools` is accepted only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning. A `required` or named tool choice falls back to `auto`, and `structuredOutputMode: "jsonTool"` falls back to native `output_config.format`, each with a warning.
  - `claude-haiku-5-5` supports every effort level, including `xhigh`. It uses adaptive thinking and rejects thinking token budgets: `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning, so use `effort` to control how much it thinks. Unlike `claude-sonnet-5-5`, thinking can be disabled, but only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. Structured output uses native `output_config.format`.
  - With manual thinking (`thinking: { type: "enabled", budgetTokens }`), a `required` or named tool choice falls back to `auto` with a warning. A named tool choice sends only the selected tool. This also applies when the top-level `Reasoning` option enables manual thinking, including when structured output uses the internal `json` response tool. Tool use is no longer guaranteed: instruct the model to use the tool in your prompt and verify that a tool call was made.
- Google speaks Gemini `generateContent`. `GoogleVertexProvider` changes the base URL and sends a bearer token. `GoogleVertexAnthropicProvider` reads `providerOptions.vertex.labels`, a string-to-string map for billing labels, and sends it as the base64 JSON `x-vertex-ai-labels` header for generate and stream (never in the body). Invalid labels throw `InvalidArgumentError` before any request, and an explicit `x-vertex-ai-labels` header, from the call or `GoogleVertexAnthropicOptions.Headers`, wins.
- A Gemini stream that carries an `error` object mid-stream (`code`, `message`, `status`, `details`) emits an `ErrorStreamPart` whose `Error` is a `ProviderStreamError` (`Type` is the `status`, `Code` and `StatusCode` the `code`, `Data` the whole error object) and finishes with `FinishReason.Error`. `IsRetryable` is true for codes 408, 409, 429, and 500 and above. Text already streamed is still closed and its usage is reported. `GoogleVertexProvider` language models do the same.
- Google `responseModalities` (a `google` provider option) lists the response modalities: `TEXT`, `IMAGE`, and `AUDIO`. Support is model-dependent. When it is absent or empty, the model returns only text. Non-text `inlineData` in the response, such as `audio/wav`, comes back as a `GeneratedFile` with its media type.
  - Long decode (Gemini 4 models such as `gemini-4-argon`): `MaxOutputTokens` is a cumulative budget across requests, not a target length. A request that reaches the per-request cap while budget remains finishes with `CONTINUATION`, which maps to `FinishReason.Length` and keeps the raw reason. `DoGenerateAsync` and `DoStreamAsync` then send the same request again with a top-level `continuationToken` and combine the results into one response. Set `MaxOutputTokens` above the per-request cap to enable this. Without it the call ends after one request. The stream keeps one text block across requests. Allow HTTP read timeouts of at least 15 to 20 minutes, because a complex request can run for several minutes.
  - `LanguageModelUsage` is the sum of all requests, which matches billing. Each continued request re-sends the prompt plus the output so far, so do not use the summed input tokens for context window checks. When a response spanned more than one request, `ProviderMetadata` `google.initialPromptTokenCount` holds the prompt token count of the first request.
  - A response that used up its `MaxOutputTokens` budget finishes with `FinishReason.Length` and exposes `ProviderMetadata` `google.continuationToken`. Pass it back as `providerOptions.google.continuationToken` with a higher `MaxOutputTokens` and the original prompt to resume. A stream exposes the token only when it ended with `MAX_TOKENS` or `CONTINUATION`. If a continuation request fails, the stream emits an `ErrorStreamPart` and finishes with `Length`, the raw reason `CONTINUATION`, and the last token. `GoogleVertexProvider` language models do the same.
  - `GoogleProvider.Interactions(id)` with `providerOptions.google.background: true` runs the model call on the server, and `DoGenerateAsync` sends `generation_config.max_output_tokens` from `MaxOutputTokens` and polls `GET /interactions/{id}` until the status is terminal. The server runs every request, so there is no client-side continuation. Set `MaxOutputTokens` above 65,536 for long decode. Without `background: true`, an interaction generates at most 65,536 tokens.
- Azure OpenAI uses `api-key` and `/openai/deployments/{model}/chat/completions`.
- Amazon Bedrock signs a Converse request with Signature Version 4. It signs the caller's headers too, and sends a header with a non-ASCII value without signing it. Provider options come from `amazonBedrock`, then `bedrock`. `reasoningConfig` (`type`, `budgetTokens`, `maxReasoningEffort` of `low`, `medium`, `high`, `xhigh`, or `max`, and `display`) becomes `additionalModelRequestFields.thinking` and `output_config.effort` on Anthropic models, `reasoning_effort` or `reasoning.effort` on OpenAI models, and a `reasoningConfig` field on other models such as Nova. The top-level `Reasoning` option fills in `reasoningConfig`, and fields you set win. `serviceTier` (`reserved`, `priority`, `default`, or `flex`) selects the inference service tier and is sent as `serviceTier.type`. `Temperature` is clamped to the 0 to 1 range with a warning. Models that reject sampling settings, such as Claude Opus 4.7, drop `Temperature`, `TopP`, and `TopK` with a warning. OpenAI models drop `StopSequences`, and OpenAI models other than gpt-oss also drop `Temperature` and `TopP`, each with a warning. Structured output (`JsonSchema`) on an Anthropic model follows `structuredOutputMode` (`outputFormat`, `jsonTool`, or `auto`, read from `amazonBedrock`, then `anthropic`). In `auto`, models that support it send a sanitized `additionalModelRequestFields.output_config.format`, and `jsonTool` removes a manually set `format`. Otherwise a forced `json` tool carries it, and the tool input comes back as the response text with `providerMetadata.bedrock.isJsonResponseFromTool` set. Models that reject forced tool use, and models that reject `strict` tools when you also pass tools, get a JSON instruction in the system message instead, and the first JSON object in the reply becomes the text. `DoStreamAsync` calls `/converse-stream` and decodes the AWS event stream (`application/vnd.amazon.eventstream`) as it arrives, so text, reasoning, and tool input are emitted as they are generated. A frame that fails its checksum, or bytes left over when the stream ends, throw. A modeled exception frame (`internalServerException`, `modelStreamErrorException`, `serviceUnavailableException`, `throttlingException`, `validationException`) becomes an `ErrorStreamPart` whose `Error` carries `StatusCode` (500, 424, 503, 429, 400) and `IsRetryable`, and the stream finishes with `FinishReason.Error`. Streamed reasoning text carries its signature on a `ReasoningDeltaStreamPart.ProviderMetadata`, and redacted reasoning content is attached once on the `ReasoningEndStreamPart`. A `TextContentPart` or an image `FileContentPart` with bytes sends a `guardContent` block when its `ProviderOptions` hold `guardContent: true` under `amazonBedrock` or `bedrock`. A text part can also set `guardContentQualifiers` (`grounding_source`, `query`, or `guard_content`). A `cachePoint` in `ProviderOptions` (for example `{ "type": "default", "ttl": "1h" }`, with `ttl` of `5m` or `1h`) adds a `cachePoint` block right after the content it belongs to. It works on `SystemModelMessage`, `UserModelMessage`, `AssistantModelMessage`, `TextContentPart`, `FileContentPart`, `ToolModelMessage` (set `ProviderOptions` on each tool result, and `MessageProviderOptions` to add one after all results of a grouped message), and the parts of an assistant message, reasoning parts included. An assistant message left with only cache points is dropped. A provider-executed `AssistantToolResultPart` ends the assistant message in front of it and is sent as a user `toolResult` message, so a result that ends an assistant message joins the next user message. Assistant reasoning is replayed only when its `ProviderOptions` hold a `signature`, `redactedContent`, or `redactedData` under `amazonBedrock` or `bedrock`. Reasoning without one of them is left out, and the text of signed reasoning is sent untrimmed. An empty or whitespace text part is left out unless the message has a reasoning part. The last assistant message of a prompt has its trailing whitespace trimmed, because Bedrock rejects it in a pre-filled response. Every provider option that is not one of `reasoningConfig`, `additionalModelRequestFields`, `serviceTier`, `structuredOutputMode`, or `requestMetadata` (for example `guardrailConfig`) goes into the request as is. Anthropic models also ask Bedrock for `/delta/stop_sequence` so `providerMetadata.bedrock.stopSequence` is set. `AmazonBedrockOptions.Headers` and `LanguageModelCallOptions.Headers` are sent on Converse, Converse stream, embedding, and rerank requests before signing. Call headers win over provider headers, and every request carries the `ai-sdk-amazon-bedrock` user agent. `AmazonBedrockFetch` and `AmazonBedrockSigV4Fetch` apply the same signing and the `ai-sdk-amazon-bedrock` user agent to any other Bedrock call. `DoStreamAsync` returns a `LanguageModelStreamResult`: `Stream` holds the parts, `Request.Body` holds the JSON request body, and `Response.Headers` holds the response headers. `DoGenerateAsync` returns the headers on `LanguageModelGenerateResult.ResponseHeaders`.
- Bedrock Runtime and Agent Runtime origins resolve in this order: `AmazonBedrockOptions.BaseUrl` (or `AmazonBedrockAnthropicOptions.BaseUrl`) and `AgentRuntimeBaseUrl` for rerank, then `AWS_ENDPOINT_URL_BEDROCK_RUNTIME` or `AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME`, then `AWS_ENDPOINT_URL`, then `https://{service}.{region}.{suffix}`. The region is only validated when it builds the host. `AmazonBedrockProvider` Converse, embedding, and rerank models and `AmazonBedrockAnthropicProvider` all follow it.
- `AmazonBedrockAnthropicProvider` runs the Anthropic Messages model through Bedrock `InvokeModel` (`/model/{id}/invoke`, or `/invoke-with-response-stream` for streaming), so tools, files, images, reasoning, structured output, and `anthropic` provider options all work as they do on `AnthropicProvider`. It signs the request with Signature Version 4, or sends `Authorization: Bearer` when `ApiKey` or `AWS_BEARER_TOKEN_BEDROCK` is set. Before sending it drops `model` and `stream`, adds `anthropic_version: bedrock-2023-05-31`, keeps only the `type` and `name` of `tool_choice`, renames the thinking `block_binding.prefix_mismatch_behavior` field to `mismatch_behavior`, upgrades the `bash_20241022`, `text_editor_20241022`, and `computer_20241022` tools to the versions Bedrock supports, turns a tool's `eager_input_streaming` into the `fine-grained-tool-streaming-2025-05-14` beta, and moves the betas into `anthropic_beta`. Bedrock does not accept URL sources, so images and files are downloaded and sent as base64. Bedrock errors become Anthropic error bodies, and the AWS event stream becomes server-sent events (`AmazonBedrockAnthropicFetch.Create` wraps any `HttpMessageHandler` with that conversion). `AmazonBedrockAnthropicOptions.Headers` are sent with every call. `CredentialProvider` supplies SigV4 keys for each request instead of `AccessKeyId`, `SecretAccessKey`, and `SessionToken`; the region still comes from `Region`. `AmazonBedrockAnthropicProvider.Tools` lists the Anthropic provider tool ids, and `EmbeddingModel` and `ImageModel` throw `NoSuchModelError`.
- `BedrockMantleProvider` runs the OpenAI Chat Completions model (`LanguageModel`, `Chat`) or the OpenAI Responses model (`Responses`) against the Mantle endpoint, with the request signed for the `bedrock-mantle` service (or sent with a bearer token). Tools, files, images, reasoning, structured output, streaming, and `openai` provider options work as they do on `OpenAIProvider`. `BedrockMantleOptions.Headers` are sent with every call. The Mantle Responses model never adds `web_search_call.action.sources` to `include`. `CredentialProvider` supplies SigV4 keys for each request instead of `AccessKeyId`, `SecretAccessKey`, and `SessionToken`. `EmbeddingModel` and `ImageModel` throw `NoSuchModelError`.
- Cohere speaks chat, embed, and rerank.
- OpenResponses posts to `{base}/responses`. The default base is the Gateway Open Responses route. `OpenResponsesOptions.Name` (default `open-responses`) is the key for provider options and provider metadata, and the model's provider id is `{Name}.responses`. System messages become `instructions`. Text and tool calls stream as they arrive, and reasoning streams from `reasoning_summary_text` and `reasoning_text` deltas. Reasoning, text, and tool-call parts carry their `itemId` in provider metadata (`GeneratedText.ProviderMetadata` and streamed `text-end` parts also carry `url_citation` annotations), and reasoning keeps its summary, content, and encrypted content. Each reasoning content part becomes its own `GeneratedReasoning`. `AssistantModelMessage.Parts` keeps the text, reasoning, and tool-call parts in order, each with its own provider options (`AssistantTextPart`, `AssistantReasoningPart`, `AssistantToolCallPart`), so a later turn replays the items in the original order with their ids and annotations. `GenerateText` builds these parts from the model output. `StrictResponseInput` serializes assistant history using the strict OpenAI Responses input schemas: assistant text without an item id is sent as an easy input message, and text with an item id as a complete output message.
- OpenResponses warns and ignores `StopSequences`, `TopK`, and `Seed`. `OpenResponsesOptions.Headers` are sent with every request. It sends function tools. A provider-defined tool (`LanguageModelTool.ProviderDefined(id, name, args)`) is omitted with an `unsupported` warning (`provider-defined tool <id>`) unless an extension registers it. A `ToolChoice.Tool(name)` that selects an omitted provider tool is omitted too.
- OpenResponses stream `response.failed` and `error` events become `ErrorStreamPart`s whose `Error` is a `ProviderStreamError` with the event type, the error `code` and `message`, and the whole event as `Data`. `OpenResponsesOptions.GetResponseErrorMetadata` reads optional `StatusCode` and `IsRetryable` values from a response `error` object, including endpoint-specific fields. It applies to generation failures and to streaming `response.failed` and `error` events. The standard Open Responses error fields are `code` and `message`, so endpoint-specific status fields need this callback. Without it, a generation failure has status 400. When only a status code is supplied, retryability comes from that status.

### OpenResponses extensions (experimental)

Open Responses implementations can add namespaced tools, items, and streaming events. Register an `OpenResponsesExtension` in `OpenResponsesOptions.Extensions` to encode and decode an implementation's wire formats. This API is experimental and may change.

The provider-tool id uses dot notation (`acme.document_search`). Open Responses wire types use colon notation (`acme:document_search`). The namespace in `Id` must match the namespace in every registered wire type.

```csharp
var extension = new OpenResponsesExtension
{
    Id = "acme.document_search",
    ToolType = "acme:document_search",
    ItemTypes = new[] { "acme:document_search_receipt" },
    EncodeTool = context => new JsonObject
    {
        ["name"] = context.Name,
        ["index"] = context.Args.GetProperty("index").GetString(),
    },
    DecodeItem = context => new GeneratedContent[]
    {
        new GeneratedToolCall((string)context.Item["call_id"]!, (string)context.Item["name"]!, context.Item["query"]!.ToJsonString())
        {
            ProviderExecuted = true,
        },
        new GeneratedToolResult((string)context.Item["call_id"]!, (string)context.Item["name"]!, context.Item["result"]!.ToJsonString()),
    },
};

var acme = OpenResponsesProvider.Create(new OpenResponsesOptions
{
    Name = "acme",
    BaseUrl = "https://api.acme.example/v1",
    Extensions = new[] { extension },
});
```

Set `ProviderExecuted` on each decoded tool call or `ToolInputStartStreamPart`. It can vary by item or event. An extension can define:

- `EncodeTool`: encodes a provider tool from `OpenResponsesToolContext` (`Name`, `Args`). Returning `null` omits the tool with an `unsupported` warning. A tool choice that selects the omitted tool is omitted too.
- `EncodeToolChoice`: encodes a specific tool choice. The default is an object with only the `type`.
- `DecodeItem`: maps a completed item (`OpenResponsesDecodeItemContext.Item`, plus `Mode` of `Generate` or `Stream`) to content parts.
- `EncodeInputItem`: encodes a tool call or tool result in history (`AssistantToolCallPart` or `AssistantToolResultPart`) when the original wire item is unavailable. Every returned item needs `id`, `status`, and a registered namespaced `type`.
- `DecodeEvent`: maps a streaming event to stream parts. `OpenResponsesDecodeEventContext.State` lasts for one response stream.

Tool, item, and event capabilities are independent. Capability fields must be provided in pairs.

Each decoded item adds a `GeneratedCustom` part of kind `open-responses.extension-replay` (a `CustomStreamPart` when streaming). It stores the original item in provider metadata, and the other decoded parts reference it by `itemId`. Pass the result back with `AssistantModelMessage.FromGeneratedContent(result.Content)` to replay the original item once, opaque fields included. This also preserves items that decode only to source parts.

The model validates only the extension discriminator fields. Validate the other item and event fields in the codec. The original item is stored in provider metadata and can appear in persisted messages, telemetry, logs, or UI payloads.

Extension delegates cannot cross a workflow boundary. `OpenResponsesLanguageModel.Serialize()` throws a `SerializationError` when extensions are registered. Create the provider inside the workflow step.
- Perplexity `DecisionModel(id)` answers `Decide` questions through the Decisions API (`POST {base}/v1/decisions`; provider id `perplexity.decision`). `PerplexityProvider` implements `IDecisionProvider`. Any model id is sent unchanged (for example `pplx-decider-latest`, `pplx-decider-flash-latest`, `pplx-decider-v1.2`, or `perplexity/pplx-decider-latest`). A `boolean` question is sent as Perplexity's native `noul` type, and the `noul` value comes back as the answer's `Probability`. Choice and Score answers keep the API's probability distributions without rounding or normalizing, so no `DecisionRounding` is declared. A `box` question throws `DecisionUnsupportedQuestionTypeException`. State parts map to the native `state` field: a single text, object, or array part is sent as is, and anything else is sent as an array. PNG, JPEG, and WebP files become base64 `image_url` data URLs, at most eight per request, counting images already nested in a json part. A file with another media type, or a URL file, throws `UnsupportedFunctionalityException`. The model rejects, before any request, more than 128 questions, an empty question name, more than 255 choice options, and a score with no level, more than 10 levels, or a null level. A single score level returns score `0` with probability `1`. Choice and Score confidence is in `ProviderMetadata` at `perplexity.confidence`, and Score legends at `perplexity.legend`, both keyed by question id. The model has no provider options, so every entry in `providerOptions.perplexity` is reported as an `unsupported` warning and not sent. The response `Id` is the `x-request-id` header. A failed request throws an `ApiException` that carries the response headers in `ResponseHeaders`, so a caller can read `retry-after`. `PerplexityDecisionModel.Serialize()` returns a `SerializedModelOptions` with the model id, `baseURL`, and the resolved request headers, including the `Authorization` header. `PerplexityDecisionModel.Deserialize(options, httpClient)` restores the model. The HTTP client is not serialized. Runnable examples are in `examples/DecidePerplexityBasic`, `examples/DecidePerplexityCriteriaOnly`, and `examples/DecidePerplexitySingleLevelScore`.
- Perplexity language generation posts to `{base}/v1/agent` (the Agent API). Preset ids are `fast`, `low`, `medium`, `high`, and `xhigh`. Any other id is sent as an Agent API model id. Legacy Sonar model ids are not aliased, and Sonar provider options are not translated. Embeddings stay on the OpenAI-compatible embeddings route. Sonar PDF input, video input, and image or video results have no Agent API equivalent.

Set `LanguageModelCallOptions.JsonResponse` to ask for JSON without a schema (upstream `responseFormat: { type: 'json' }`). OpenAI, OpenAI-compatible, Open Responses, Cohere, and xAI providers send `json_object`, Google sends `responseMimeType: application/json`, and the Gateway sends `json`. Anthropic ignores it and warns that the JSON response format requires a schema. Amazon Bedrock and Hugging Face also ignore it, as upstream does. `JsonSchema` wins when both are set.

The portable `Reasoning` option on `GenerateTextOptions` and `LanguageModelCallOptions` accepts `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, and `max`. Budget-based reasoning uses 95 percent of the output budget for `max`. Providers with a lower ceiling coerce `max` and return a `compatibility` warning:

- Anthropic, DeepSeek, Moonshot, and OpenAI models that list `max` send it unchanged, with no warning.
- Google (Gemini 3), Groq, and Fireworks send `high`. Fireworks also sends `low` for `minimal` and `high` for `xhigh`. Each change adds a warning. A `reasoningEffort` provider option wins over `Reasoning` and adds no warning.
- Perplexity sends `xhigh`.
- Mistral sends `reasoning_effort` `high` for every level except `none`, and returns a `compatibility` warning when it changes the level. Models without reasoning effort (such as `mistral-large-latest`) get no `reasoning_effort` and an `unsupported` warning. A `reasoningEffort` provider option wins.
- OpenResponses sends `reasoning.effort` (`minimal` becomes `low`; `xhigh` and `max` are sent unchanged). The provider options `reasoningEffort` and `reasoningSummary`, under the provider `Name`, set `reasoning.effort` and `reasoning.summary`.
- xAI Responses (`XaiProvider.LanguageModel` and `XaiProvider.ResponsesModel`) sends `reasoning.effort` (`minimal` becomes `low`). `xhigh` and `max` are sent as `xhigh` on `grok-4.6` and as `high` on other models, with a `compatibility` warning when the level changes. The `grok-4.20` reasoning and non-reasoning models reject the parameter: they get no `reasoning` object and an `unsupported` warning. A `reasoningEffort` provider option wins and adds no warning.

Moonshot native structured outputs are enabled for every model id, including Kimi, Moonshot V1, and custom or future ids. A request with a JSON schema sends `response_format.type = json_schema` with `strict: true`. A JSON request with no schema uses `json_object`.

Groq rejects `response_format` together with tools, and Gemini before Gemini 3 rejects JSON response formatting together with function tools. When a call has a JSON schema and at least one function tool, these providers send an internal function tool whose arguments are the structured output, and turn that tool call back into text. The tool is named `json`, or `json_1`, `json_2`, and so on when a caller tool already uses the name. Its description is the output description (`OutputSpec.Object(schema, name, description)`, or `LanguageModelCallOptions.JsonSchemaDescription`), or `Respond with a JSON object.`. Tool choice `Tool("name")` is kept, `None` selects the response tool, and any other choice becomes `Required`. Groq sets `parallel_tool_calls` to false and skips the `structuredOutputs` warning on this path. Gemini 3 keeps native `responseMimeType` and `responseJsonSchema` with automatic or omitted tool choice, and uses the response tool only for `Required` or a named tool. Conversational text is dropped, calls to your own tools stay tool calls, and a response that has only the response tool finishes with `Stop`. Without function tools, both providers keep native structured output. `JsonResponseTools.ConvertJsonResponseToolStream` applies the same rewrite to a stream of `LanguageModelStreamPart`.

OpenAI-compatible providers (Alibaba, Groq, DeepSeek, Mistral, Together, and the other chat wrappers) are thin wrappers over `Vercel.AI.OpenAICompatible`. They set the base URL, the provider id, and the environment variable. Perplexity embeddings use that client. Perplexity language generation uses the Agent API described above.

An OpenAI-compatible image model (`ImageModel(id)`) reads image generation and editing responses that hold base64 data (`b64_json`), image URLs (`url`), or a mix of both, and keeps the images in order. A `url` image is downloaded, so `GeneratedImage.Data` and `GeneratedImage.Base64` are filled. When an image has both fields, base64 wins and nothing is downloaded. A data item with neither field throws `InvalidResponseDataException`. The download request carries the provider headers only when the URL is on the provider origin. Any other host is checked first (private addresses and non-http(s) schemes throw `DownloadException`), redirects are checked hop by hop, and no provider header is sent. Set `OpenAICompatibleOptions.DownloadHttpClient` to control the client used for those hosts.

Speech, transcription, image, video, and Voyage each have a package that calls that provider’s public HTTP API. Use `SpeechModel`, `TranscriptionModel`, `ImageModel`, `VideoModel`, or `EmbeddingModel` rather than `LanguageModel`. Every provider (`ProviderBase.SpecificationVersion`) and every video model (`IVideoModel.SpecificationVersion`) reports `v4`.

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

## Kling AI

The Kling AI provider (`Vercel.AI.KlingAI`) supports Kling AI video generation models: text-to-video, image-to-video, reference-to-video (multi-image), motion control, and multi-shot video generation. It supports video generation only.

### Provider instance

```csharp
using Vercel.AI.KlingAI;

var klingai = KlingAIProvider.Create(new KlingAIOptions { ApiKey = Environment.GetEnvironmentVariable("KLINGAI_API_KEY") });
```

These settings are optional:

- **ApiKey** _string_: The Kling AI API key, sent as a bearer token. Defaults to the `KLINGAI_API_KEY` environment variable. Create one in the [Kling AI developer console](https://kling.ai/dev/api-key).
- **AccessKey** _string_: The legacy access key. Defaults to the `KLINGAI_ACCESS_KEY` environment variable. Prefer `ApiKey`.
- **SecretKey** _string_: The legacy secret key. Defaults to the `KLINGAI_SECRET_KEY` environment variable. Prefer `ApiKey`.
- **BaseUrl** _string_: The API URL prefix. Defaults to `https://api-singapore.klingai.com`.
- **Headers** _IDictionary&lt;string, string&gt;_: Additional request headers.

`AddKlingAI` registers the provider with dependency injection. `LanguageModel`, `EmbeddingModel`, and `ImageModel` throw a `NoSuchModelError`.

### Authentication

Kling AI supports two authentication schemes:

- **API key** (recommended): a single key sent as a bearer token. Set `KLINGAI_API_KEY` or pass `ApiKey`.
- **Access key and secret key** (legacy): a key pair the provider uses to sign a short-lived JWT (HS256, valid for 30 minutes) for each request. Set `KLINGAI_ACCESS_KEY` and `KLINGAI_SECRET_KEY`, or pass `AccessKey` and `SecretKey`.

Credentials are resolved in this order, and explicit settings win over environment variables:

1. `ApiKey`
2. `AccessKey` and `SecretKey`
3. The `KLINGAI_API_KEY` environment variable
4. The `KLINGAI_ACCESS_KEY` and `KLINGAI_SECRET_KEY` environment variables

The credentials are read when a request is sent, so a missing key fails then and not when the provider is created. `KlingAIAuth.ResolveAuthToken` and `KlingAIAuth.GenerateAuthToken` expose the same logic.

### Video models

Create a model with `klingai.VideoModel("kling-v3.0-t2v")` or its alias `klingai.Video("kling-v3.0-t2v")`. `KlingAIVideoModel` implements `IVideoCaller`, so pass it as the `Model` of `GenerateVideoRequest`. The suffix of the model id selects the mode: `-t2v`, `-i2v`, or `-motion-control`. Any other model id throws a `NoSuchModelError` when the video starts. Not every option works with every model version and mode. See the [Kling AI capability map](https://app.klingai.com/global/dev/document-api/apiReference/model/skillsMap).

The model creates a task, then polls it until it finishes. The result is the video URL, and the bytes are not downloaded.

#### Text to video

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)klingai.VideoModel("kling-v3.0-t2v"),
    Prompt = new VideoPrompt("A chicken flying into the sunset in the style of 90s anime."),
    AspectRatio = "16:9",
    Duration = 5,
    ProviderOptions = OperationJson.Parse("""{ "klingai": { "mode": "std" } }"""),
});
```

#### Image to video

Pass an image in the prompt to use it as the start frame. Set `imageTail` for start and end frame control, or pass a `last_frame` in `FrameImages`.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)klingai.VideoModel("kling-v3.0-i2v"),
    Prompt = new VideoPrompt("The cat slowly turns its head and blinks", "https://example.com/start-frame.png"),
    Duration = 5,
    ProviderOptions = OperationJson.Parse("""{ "klingai": { "mode": "pro", "imageTail": "https://example.com/end-frame.png" } }"""),
});
```

#### Multi-shot video

Kling v3.0 can split a video into up to 6 storyboard shots, each with its own prompt and duration:

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)klingai.VideoModel("kling-v3.0-t2v"),
    Prompt = new VideoPrompt(""),
    AspectRatio = "16:9",
    Duration = 10,
    ProviderOptions = OperationJson.Parse("""
        {
          "klingai": {
            "mode": "pro",
            "multiShot": true,
            "shotType": "customize",
            "multiPrompt": [
              { "index": 1, "prompt": "A sunrise over a calm ocean, warm golden light.", "duration": "4" },
              { "index": 2, "prompt": "A flock of seagulls take flight from the beach.", "duration": "3" },
              { "index": 3, "prompt": "Waves crash against rocky cliffs at sunset.", "duration": "3" }
            ],
            "sound": "on"
          }
        }
        """),
});
```

Multi-shot also works with image-to-video.

#### Reference to video

Pass images in `InputReferences` on an image-to-video model to combine them. The request goes to the multi-image endpoint. Video references are ignored with a warning, and a model that is not image-to-video ignores all references with a warning.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)klingai.VideoModel("kling-v1.6-i2v"),
    Prompt = new VideoPrompt("The two characters meet and walk together through a sunny park"),
    InputReferences = new[]
    {
        new VideoReference("https://example.com/character-1.png", "image/png"),
        new VideoReference("https://example.com/character-2.png", "image/png"),
    },
    AspectRatio = "16:9",
    Duration = 5,
});
```

#### Motion control

Transfer the motion of a reference video to a character image. `videoUrl`, `characterOrientation`, and `mode` are required, and a call without them throws an `AiSdkException`.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)klingai.VideoModel("kling-v3.0-motion-control"),
    Prompt = new VideoPrompt("The character performs a smooth dance move", "https://example.com/character.png"),
    ProviderOptions = OperationJson.Parse("""
        {
          "klingai": {
            "videoUrl": "https://example.com/reference-motion.mp4",
            "characterOrientation": "image",
            "mode": "std",
            "elementList": [{ "element_id": 829836802793406551 }]
          }
        }
        """),
});
```

#### Caller-managed callbacks

Use `StartVideo.StartVideoAsync` with `WebhookUrl` to register your own callback receiver. The URL is sent as `callback_url` and wins over `providerOptions.klingai.callback_url`. Without `WebhookUrl`, a raw `callback_url` option is passed through unchanged.

Kling AI sends progress notifications (`submitted` and `processing`) as well as terminal ones (`succeed` and `failed`). Your receiver must filter the progress notifications and match each callback to its task. Call `GetVideoStatus.GetVideoStatusAsync` with the operation from `StartVideoAsync` to read the result after a terminal notification. The model does not handle webhooks itself (`CanWebhook` is false), so `GenerateVideo` with a webhook polls instead.

#### Provider options

Set the options under the `klingai` key of `ProviderOptions`. An option of the wrong type or with an unknown enum value throws an `InvalidArgumentException`. Keys that are not listed are sent to the API as is.

Common options:

- **mode** _"std" | "pro"_: `std` is cost-effective. `pro` has higher quality and takes longer.
- **pollIntervalMs** _number_ and **pollTimeoutMs** _number_: Accepted and not sent to the API. Pass `PollIntervalMs` and `PollTimeoutMs` to `GenerateVideoRequest` instead.
- **watermarkEnabled** _boolean_: Also generate a watermarked result.

Text-to-video and image-to-video options:

- **negativePrompt** _string_: What to avoid (max 2500 characters).
- **sound** _"on" | "off"_: Generate audio. V2.6 and later, and needs `mode: "pro"`. A `GenerateAudio` value on the request wins.
- **cfgScale** _number_: How closely to follow the prompt, in [0, 1]. V2.x models do not support it.
- **cameraControl** _object_: A `type` preset (`simple`, `down_back`, `forward_up`, `right_turn_forward`, `left_turn_forward`) and an optional `config` with `horizontal`, `vertical`, `pan`, `tilt`, `roll`, and `zoom`.
- **multiShot** _boolean_, **shotType** _"customize" | "intelligence"_, **multiPrompt** _array of `{ index, prompt, duration }`_: Multi-shot generation (v3.0+). `shotType` is required with `multiShot`. `multiPrompt` is required for `customize`.
- **voiceList** _array of `{ voice_id }`_: Voice references (v3.0+, up to 2), used as `<<<voice_1>>>` in the prompt. It needs `sound: "on"` and cannot be combined with `elementList` on image-to-video.

Image-to-video only:

- **imageTail** _string_: The end frame, as a URL or raw base64. Most models need `mode: "pro"`.
- **staticMask** _string_ and **dynamicMasks** _array of `{ mask, trajectories }`_: Motion brush.

Image-to-video and motion control:

- **elementList** _array of `{ element_id }`_: Reference elements (v3.0+). Up to 3 for image-to-video and 1 for motion control.

Motion control only:

- **videoUrl** _string_ (required): The reference motion video (.mp4 or .mov, up to 100 MB, 3 to 30 seconds).
- **characterOrientation** _"image" | "video"_ (required): `image` follows the image (reference video up to 10 s). `video` follows the video (up to 30 s).
- **keepOriginalSound** _"yes" | "no"_: Keep the reference video's sound. Defaults to `yes` on the API.

#### Warnings

The model reports `Unsupported` warnings for `Resolution`, `Seed`, `Fps`, and `N` greater than 1 on every mode. Other warnings depend on the mode: `AspectRatio` on image-to-video and motion control, `Duration` on motion control, an image on text-to-video, and video files used as images or references.

Status polls send the credentials to the configured Kling AI origin only. A redirect to another origin is checked with the download URL rules before it is followed, and a private address throws a `DownloadError`.

#### Provider metadata

`providerMetadata.klingai` has the `taskId` and a `videos` array. Each entry has the `id` and `url`, and the `watermarkUrl` and `duration` when Kling AI reports them.

#### Video model ids

Text to video: `kling-v3.0-t2v`, `kling-v2.6-t2v`, `kling-v2.5-turbo-t2v`, `kling-v2.1-master-t2v`, `kling-v2-master-t2v`, `kling-v1.6-t2v`, `kling-v1-t2v`.

Image to video: `kling-v3.0-i2v`, `kling-v2.6-i2v`, `kling-v2.5-turbo-i2v`, `kling-v2.1-master-i2v`, `kling-v2.1-i2v`, `kling-v2-master-i2v`, `kling-v1.6-i2v`, `kling-v1.5-i2v`, `kling-v1-i2v`.

Motion control: `kling-v3.0-motion-control`, `kling-v2.6-motion-control`.

Any other model id with one of the three suffixes is also accepted.

## Black Forest Labs

The Black Forest Labs provider (`Vercel.AI.BlackForestLabs`) generates images with `ImageModel(id)` (provider id `black-forest-labs.image`). It reads `BFL_API_KEY` and sends it as `x-key`. `LanguageModel` throws `AiSdkException`.

The model submits the request, polls `polling_url` (with `?id=` added when missing) until the task is `Ready`, then downloads the sample. `BlackForestLabsProvider.Create(options, handler, pollIntervalMillis, pollTimeoutMillis)` sets the poll interval (default 500 ms) and the overall poll timeout (default 60 s) for every image model. The `pollIntervalMillis` and `pollTimeoutMillis` provider options override them for one call. A timeout throws `AiSdkException` ("Black Forest Labs generation timed out."). A task that ends as `Error` or `Failed`, or is `Request Moderated`, `Content Moderated`, or `Task not found`, throws `AiSdkException`. An HTTP error throws `ApiException` with the `detail` (or `message`) of the body; an HTTP 503 whose body carries a terminal `status` or `state` is not retryable.

Credentials go to the polling URL only when it is on the provider origin or an https host under `bfl.ai` (for example `api.us1.bfl.ai`). The download sends no credentials, except that a custom base URL outside `bfl.ai` receives the provider and call headers when the sample is on the same origin. Other hosts are checked first (private addresses and non-http(s) schemes throw `DownloadException`).

`BlackForestLabsImageModel.SupportsFileInputs` and `SupportsMaskInputs` report what the model id accepts (`null` when unknown). The model id picks the API: ids such as `flux-pro-1.1`, `flux-kontext-pro`, `flux-pro-1.0-fill`, `flux-1.*`, and `flux-2-*` use the legacy API. `flux-3-image` uses the FLUX 3 API. Any other id (for example `flux-3-image-klein`) uses the latest API.

- Legacy: `AspectRatio` wins over `Size`. A `Size` adds an `unsupported` `size` warning, and its width and height are sent unless the `width` and `height` provider options are set. Files are sent as `input_image`, `input_image_2`, and so on (`image` for `flux-pro-1.0-fill`), and the mask as `mask`.
- FLUX 3 and latest: files are sent in `images` (URLs as is, bytes as base64), at most 10 (more throws `AiSdkException`). A mask throws `UnsupportedFunctionalityException`. The aspect ratio comes from `AspectRatio` or from `Size`; `7:3` and `3:7` are sent as `21:9` and `9:21`, and a ratio the endpoint does not take is left out with an `unsupported` `aspectRatio` warning. FLUX 3 adds an `unsupported` `seed` warning for a `Seed`. The latest API sends `Seed`, and its `Size` becomes `width` and `height` (the `width` and `height` provider options win); with dimensions set, no aspect ratio or resolution is sent.
- Provider options are read from `blackForestLabs` and validated before the request (`InvalidArgumentException`). An option the model does not support is dropped with an `unsupported` `blackForestLabs.<name>` warning. Ranges: `safetyTolerance` 0 to 6 (legacy) or 0 to 7, `width` and `height` 256 to 1920 (legacy) or 64 to 6256 together with at most 16,822,272 pixels (latest), `steps` 1 to 16 (latest), and `resolution` one of `768sq`, `1k`, `1.5k`, `2k`, `4k`.

`GenerateImage` returns `providerMetadata.blackForestLabs.images[0]` with `seed`, `start_time`, `end_time`, `duration`, `cost`, `inputMegapixels`, and `outputMegapixels` when the API sent them, and the response `Timestamp`, `ModelId`, and `Headers` of the download.

## xAI Responses

`XaiProvider.LanguageModel(modelId)` creates a language model for the xAI Responses API (`POST {base}/responses`), as upstream does. `XaiProvider.ResponsesModel(modelId)` creates the same model with its concrete type. The provider id is `xai.responses`. Upstream has no xAI chat model, so `XaiProvider.CreateChatModel(modelId)`, the Chat Completions model, is obsolete and will be removed.

```csharp
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Xai;

var xai = XaiProvider.Create(new OpenAICompatibleOptions { ApiKey = Environment.GetEnvironmentVariable("XAI_API_KEY") });
var model = xai.LanguageModel("grok-4.3");
```

The model supports text, image, and file input, streaming, function tools, xAI provider-defined tools, `JsonSchema` structured output (with `JsonSchemaName` and `JsonSchemaDescription`), `JsonResponse`, reasoning, and URL citations as sources. Reasoning returns as `XaiReasoning` content, which carries the `xai.itemId` and `xai.reasoningEncryptedContent` metadata. Streams send the same metadata on `reasoning-start` and `reasoning-end`, and the `itemId` on each `reasoning-delta`. To continue a conversation, pass that metadata back on the assistant message (`ReasoningProviderOptions`). This round trip works only for messages you build by hand. `GenerateText` and `StreamText` do not yet carry the metadata into the next step ([#344](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/344)). A request sends `topK` as `top_k` and warns about `FrequencyPenalty`, `PresencePenalty`, and `StopSequences`. A file part with a URL becomes `input_image` for images and `input_file` for other types. Inline bytes are only accepted for images. For other files, upload them with `XaiProvider.Files()` and pass a provider reference.

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

### Provider-defined tools

The Responses API has server-side tools that the model runs on xAI's servers during its reasoning. Pass them in `LanguageModelCallOptions.Tools` with `LanguageModelTool.ProviderDefined(id, name, args)`. `args` is a JSON object with the parameters below. Invalid arguments throw a `TypeValidationException`.

```csharp
var result = await model.DoGenerateAsync(
    new LanguageModelCallOptions
    {
        Prompt = new ModelMessage[] { new UserModelMessage("What are the latest developments in AI?") },
        Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "xai.web_search",
                "web_search",
                JsonDocument.Parse("""{"allowedDomains":["arxiv.org","openai.com"],"enableImageUnderstanding":true}""").RootElement),
        },
    },
    CancellationToken.None);
```

`XaiTools` builds the `Tool` for `GenerateText`, `StreamText`, and `Agent`, like `xai.tools` upstream: `XaiTools.WebSearch(new XaiWebSearchOptions { AllowedDomains = new[] { "arxiv.org" } })`, `XSearch`, `CodeExecution`, `ViewImage`, `ViewXVideo`, `ImageGeneration`, `FileSearch`, and `McpServer`. Each takes an optional tool name as its last argument. It defaults to the wire name, such as `web_search`.

- `xai.web_search`: real-time web search and page browsing. **allowedDomains** _string[]_ (max 5) and **excludedDomains** _string[]_ (max 5) cannot be used together. **enableImageSearch** _bool_ lets the model run an image search when images help the answer. **enableImageUnderstanding** _bool_ lets the model view images it finds, and uses more tokens.
- `xai.x_search`: search X posts. **allowedXHandles** _string[]_ (max 20) and **excludedXHandles** _string[]_ (max 20) cannot be used together. **fromDate** and **toDate** _string_ (`YYYY-MM-DD`). **enableImageUnderstanding** and **enableVideoUnderstanding** _bool_.
- `xai.code_execution`: runs Python code. No parameters.
- `xai.view_image`: views and analyzes images. No parameters.
- `xai.view_x_video`: views and analyzes videos from X posts. No parameters.
- `xai.image_generation`: creates and edits images with Grok Imagine. **action** _string_: `auto` (the default), `generate`, or `edit`. The tool takes no size or format parameters. Ask for an aspect ratio in the prompt.
- `xai.mcp`: connects to a remote MCP server. **serverUrl** _string_ (required), **serverLabel** _string_, **serverDescription** _string_, **allowedTools** _string[]_ (all tools when not set), **headers** _object_ of strings, and **authorization** _string_ (for example `Bearer token123`).
- `xai.file_search`: searches xAI vector stores (collections). **vectorStoreIds** _string[]_ (required) and **maxNumResults** _number_. Add `file_search_call.results` to the `include` provider option to get the matching file content and scores. File search needs a grok-4 family model.

Another provider-defined tool id is left out with an `unsupported` warning (`provider-defined tool <name>`). xAI can force only function tools, so a `ToolChoice.Tool(name)` that selects a provider-defined tool is not sent and adds an `unsupported` warning (`toolChoice for server-side tool "<name>"`).

The result has a `GeneratedToolCall` with `ProviderExecuted` set for each server-side call, named after the tool you declared. xAI reports sub-tools such as `x_keyword_search` or `browse_page`, and they map back to that name. Web search, file search, and image generation calls also return a `GeneratedToolResult`:

- web search: `{ action, sources }`. `action` is `{ type: "search", query, queries }`, `{ type: "openPage", url }`, or `{ type: "findInPage", url, pattern }`. `sources` lists `{ type: "url", url }` when xAI sends them (request them with the `web_search_call.action.sources` include).
- file search: `{ queries, results }`, where each result has `fileId`, `filename`, `score`, and `text`. `results` is null when xAI sends none.
- image generation: `{ result, prompt }`, where `result` is the base64 image. A call without an image gives an error result (`IsError`).

Streams send `tool-input-start`, `tool-input-delta`, `tool-input-end`, and `tool-call` once per server-side call, and a `tool-result` part when the call is done (`ToolResultStreamPart.IsError` marks a failed image generation). A prompt replays only the client-side tool calls: assistant tool calls with `ProviderExecuted` set and assistant tool results are not sent back.

### Not included

Tool results whose output is content with images (`input_image` in `function_call_output`) are not sent, because `ToolModelMessage` has no multipart content ([#327](https://github.com/hardkoded/vercel-ai-sdk-dotnet/issues/327)).

## QuiverAI reference images

The SVG edit provider option `quiverai.referenceImages` takes up to four `{ "url" }` or `{ "base64" }` objects. URL references must use HTTP or HTTPS. Base64 references must decode to PNG, JPEG, WebP, GIF, or SVG data. Use `QuiverAIImageReference.Prepare` to convert binary, data URL, or URL inputs into this shape. It returns a `QuiverAIImageReference` with exactly one of `Url` and `Base64` set, and it serializes to the JSON shape above.

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.QuiverAI;

var options = new JsonObject
{
    ["quiverai"] = new JsonObject
    {
        ["operation"] = "edit",
        ["referenceImages"] = new JsonArray(
            JsonSerializer.SerializeToNode(QuiverAIImageReference.Prepare(File.ReadAllBytes("blue-reference.png"))),
            new JsonObject { ["url"] = "https://example.com/second-reference.png" }),
    },
};
```

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

Anthropic and Google read the same reference. Anthropic sends the `anthropic` entry as a Files API `file_id`. Google sends the `google` entry as the `fileUri`. `AnthropicSkills.UploadSkillAsync` returns its skill id as `ProviderReference["anthropic"]`.

## Anthropic container forwarding

Code execution runs in a container. To keep the same container across steps, return `AnthropicContainer.ForwardFromLastStep` from `PrepareStep`. It searches the steps from the end for `providerMetadata.anthropic.container.id` and returns an update whose `ProviderOptions` set `anthropic.container.id`. It returns null when no step has a container id.

```csharp
var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = anthropic.LanguageModel("claude-sonnet-4-5"),
    StopWhen = StopWhen.IsStepCount(10),
    Tools = tools,
    PrepareStep = context => AnthropicContainer.ForwardFromLastStep(context.Steps),
});
```

`PrepareStepUpdate.ProviderOptions` is deep-merged onto the call's provider options for that step.

`PrepareStepContext` also exposes `Messages`, the prompt plus the responses of the earlier steps, and `Model`, the model the step is about to use. Like upstream, the results of all tool calls in one step are a single `ToolModelMessage`; read them from `ToolModelMessage.Results` (a single-result message lists itself).

## Decision state

`DecideRequest.State` is a `DecisionState`: a string, a JSON object, or a list of parts. A string converts to it implicitly. `DecisionState.FromJson(element)` takes a JSON object, and a raw JSON array is not state. Wrap it in an object or in a `DecisionJsonPart`. A list of parts is an array of `DecisionTextPart`, `DecisionJsonPart`, and `DecisionFilePart` (media type, `FilePartInput` data, optional `FileName` and `ProviderOptions`). `Decide.DecideAsync` normalizes the state with `DecisionState.PrepareDecisionStateAsync` before the model call. A string becomes one text part, an object becomes one json part, and a file part gets a full media type. A URL file is downloaded. A failed download throws `DownloadError` before any model call. `DecisionModelCall.State` and `DecisionModelEvent.State` hold the normalized parts as `DecisionModelTextPart`, `DecisionModelJsonPart`, and `DecisionModelFilePart`. `DecideEvent.State` keeps the public value. `OpenAIDecisionModel` sends text and json parts as `input_text` and inline PNG, JPEG, WebP, or GIF files as `input_image`. It throws `UnsupportedFunctionalityException` for other files. `GatewayDecisionModel` posts `{ stateParts, questions, providerOptions }` to `/decision-model`. `stateParts` holds the ordered text, json, and file parts. File bytes go out as base64, and a file with URL, reference, or text data throws `UnsupportedFunctionalityException` before any request. `providerOptions` is sent whenever the call has it, even when it is empty.

## Decision questions

`new DecisionQuestion(type, instructions, criteria)` takes `DecisionInput?` instructions: a string (implicit), or a JSON object or array through `DecisionInput.FromJson(element)` or `DecisionInput.Parse(json)`. Any other JSON value throws `InvalidArgumentException`. For `choice`, `score`, and `boolean`, the instructions may be null when the provider supports it and the criteria define the judgment. A question without instructions leaves the field out of the request. Set `HasInstructions = true` on a question with null instructions to send an explicit `null`, as upstream does for `instructions: null`. `PerplexityDecisionModel` and `GatewayDecisionModel` honor it. A `boolean` question without instructions must describe at least one of `True` or `False` with a non-null value, otherwise `Decide.DecideAsync` throws `InvalidArgumentException`. A `box` always requires instructions. Criteria are typed by question type. A `choice` takes `DecisionChoiceCriteria` (a nonempty map of option names to `DecisionInput?`), a `score` takes `DecisionScoreCriteria` (at least one level; a provider can require more), and a `boolean` takes an optional `DecisionBooleanCriteria` with `True` and `False`. A side that is never set is left out of the request. A side set to null is sent as null. A `box` takes no criteria. Descriptions may be null in the shared types, but providers can impose stricter limits. `OpenAIDecisionModel` and `GatewayDecisionModel` send a question without instructions as is, and `DecisionLanguageModel` leaves `instructions` out of the rubric.

## Decision language model

`new DecisionLanguageModel(languageModel, provider?)` wraps a V4 `ILanguageModel` so `Decide.DecideAsync` can use it for `choice`, `score`, and `boolean` questions. It does not support `box` questions. The provider id defaults to the wrapped model's provider plus `.decision`. It sends one structured-output request with reasoning set to `none`. Question ids and choice labels are replaced by internal keys (`q0`, `c0`, ...) in the schema and mapped back in the answers. State parts keep their order. Text and image file parts pass through, and a json part is sent as compact JSON text. Any non-image file part throws `UnsupportedFunctionalityException` before the model is called. A wrong finish reason, invalid JSON, a missing or extra answer, an unknown choice, a score outside its rubric, or a probability outside 0 to 1 throws `InvalidResponseDataException`. A choice needs at least one option and a score at least two levels, otherwise `InvalidArgumentException` is thrown. A file part's `providerOptions` are forwarded as the `FileContentPart.ProviderOptions` of the image. `DecisionLanguageModel.Serialize(model)` returns a `DecisionLanguageModelOptions` with the wrapped `Model` and the `Provider` id for a workflow boundary, and `DecisionLanguageModel.Deserialize(options)` restores the model. The wrapped model is not copied, so a workflow runtime must serialize it with its own hooks.

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

### OpenAI batch

Batch support is experimental and the API may change in patch releases.

The OpenAI provider supports asynchronous text generation through the [Batch API](https://developers.openai.com/api/docs/guides/batch). Pass the provider to `Batch.StartBatchAsync`, `Batch.GetBatchStatusAsync`, `Batch.GetBatchResults`, `Batch.CancelBatchAsync`, and `Batch.ListBatchesAsync` for the complete workflow.

`Batch.GetBatchResults` returns an `IAsyncEnumerable<BatchItemResult>` of terminal results. Read it with `await foreach`. Each result is downloaded and converted only when you ask for it. To read every result into a list, call `Batch.GetAllBatchResultsAsync` instead.

- `DoStartAsync` uploads the requests as a `batch.jsonl` input file and creates a `/v1/responses` batch with a `24h` completion window. Each request body is the Responses request body for its model. OpenAI requires every request in a batch to be a text request for the same model. A different type throws `UnsupportedFunctionalityException` and a different model throws `InvalidArgumentException`, both before any upload. Only function tools are supported in batch requests for now.
- `providerOptions.openai.inputFileExpiresAfter` sets the input file TTL in seconds, from 3600 (1 hour) to 2592000 (30 days). It defaults to 48 hours. The start result's `ProviderMetadata.openai` holds `inputFileId` and, when the upload reports one, `inputFileExpiresAt`.
- The OpenAI Batch API does not support per-batch webhooks. When you set `WebhookUrl`, the start result has an `unsupported` warning and the batch starts without a webhook.
- `Status` is `completed` for `completed`, `failed` for `failed`, `expired`, and `cancelled`, and `pending` for every other status. `RawStatus` holds the OpenAI status. `RequestCounts`, `CreatedAt`, `ExpiresAt`, and the first batch error (`ErrorMessage`, `ErrorCode`) come from the batch object. A batch error without a message reads `OpenAI batch failed.`
- `DoListAsync` sends `Limit` as `limit` and `Cursor` as `after`. `NextCursor` is the page's `last_id` when `has_more` is true.

### OpenAI batch results

`OpenAIBatchApi.DoGetResultsAsync` reads the batch first and throws `InvalidArgumentException` while its status is `pending`. It then returns an `IAsyncEnumerable<BatchItem>`. Reading it downloads the output file and then the error file, and converts each row as it arrives. A row that fails never stops the later rows. A malformed JSON Lines row throws `JsonParseException` from the stream, after the rows before it.

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

`ApiException` now also carries `Url`, `RequestBodyValues`, `ResponseHeaders`, and `IsRetryable` (408, 429, and 500 and above by default) and `ErrorData`, the provider error parsed from the response body (upstream `APICallError.data`). It is a `JsonElement` when `CreateJsonErrorResponseAsync` parsed the body, and `null` otherwise. It is not called `Data` because `Exception.Data` already exists.

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

`EmbeddingResult.Response` holds the raw headers and body of the call, and `EmbeddingResult.Warnings` its warnings. To send per-call headers, `dimensions`, or a `user` to an OpenAI-compatible embedding model, pass an `EmbeddingCallOptions` to `OpenAICompatibleEmbeddingModel.DoEmbedAsync`. The model keeps no per-call state, so concurrent calls do not see each other.

`ImageCallOptions` carries `Seed`, `Files`, `Mask`, `ProviderOptions`, and `Headers` for each call. `ImageGenerationResult` returns `CallWarnings`, `Response` (timestamp, model id, headers), and `Usage`. Image models keep no per-call state, so concurrent calls on one model do not see each other.

Gateway sends `dimensions` next to `values` and keeps `providerOptions` as its own object. It does not merge them.

`IEmbeddingModel.DoEmbedAsync` takes `int? dimensions = null` and `IReadOnlyDictionary<string, string?>? headers = null` after the cancellation token, and `EmbeddingModelCall` carries `Dimensions`. `IRerankingModel.DoRerankAsync` takes the same `headers` parameter. Call headers override the provider headers. `EmbedOptions.Headers` and `RerankOptions.Headers` pass them from `AiClient`. Custom models must add the parameters.

`ImageCallOptions.Files` and `Mask` are `ImageModelFile` values, the same file-or-URL union as upstream's `ImageModelV4File`. Create one with `ImageModelFile.FromFile(bytes, mediaType)` or `ImageModelFile.FromUrl(url)`, and set `ProviderOptions` on it when a provider reads per-file options. `OpenAICompatibleImageFile` is gone. Use `ImageModelFile` instead.

### Decide and the deprecated Evaluate names

`Decide.DecideAsync(DecideRequest)` is `experimental_decide`. It answers typed questions against one shared state. A decision model implements `IDecisionCaller.DoDecideAsync(DecisionModelCall)` and returns a `DecisionModelResult`. Operation ids are `ai.decide` and `ai.decide.doDecide`. Telemetry callbacks on `DecideTelemetry` are `OnDecideStart`, `OnDecideEnd`, `OnDecisionModelCallStart`, and `OnDecisionModelCallEnd`. Lifecycle events are `DecideEvent` and `DecisionModelEvent`. The refused-question and unsupported-type errors are `DecisionRefusalError`, `DecisionUnsupportedQuestionTypeError` (marker `vercel.ai.error.AI_DecisionUnsupportedQuestionTypeError`), and `DecisionUnsupportedQuestionTypeException`.

The old names still compile and are marked `[Obsolete]`: `Evaluate.EvaluateAsync`, `EvaluateRequest`, `EvaluateResult`, `EvaluateEvent`, `EvaluateModelEvent`, `EvaluateTelemetry`, `IEvaluationCaller.DoEvaluateAsync`, `EvaluationQuestion`, `EvaluationAnswer`, `EvaluationRounding`, `EvaluationModelCall`, `EvaluationModelResult`, and the two `EvaluationUnsupportedQuestionType*` types. C# cannot make two names one type, so each alias derives from its decision type, and `Evaluate.EvaluateAsync` forwards to `Decide.DecideAsync`. A model that implements only `IEvaluationCaller` still runs through `Evaluate.EvaluateAsync`. To pass it to `Decide.DecideAsync`, wrap it with `DecisionCaller.FromLegacy(model)`. Both `Decide.DecideAsync` and `DecisionLanguageModel` throw the unsupported-type error as `EvaluationUnsupportedQuestionTypeException`, so a `catch` of the old or the new type matches, and `DecisionUnsupportedQuestionTypeError.IsInstance` recognizes it. Deprecated telemetry callbacks run once and receive the decision operation ids. When both a decision callback and its deprecated alias are set, only the decision callback runs. The rubric API is a different feature. See "Rubric models" below.

#### Decision models in registries and custom providers

A provider exposes decision models by implementing `IDecisionProvider.DecisionModel(id)`. `GatewayProvider` does. `ProviderRegistry.DecisionModel("providerId:modelId")` returns the decision model of a registered provider. Custom separators and colons inside the model id work as they do for other model types. Language model middleware does not wrap decision models. `CustomProvider.AddDecisionModel(alias, model)` registers an alias, and `CustomProvider.DecisionModel(alias)` returns it. An unknown alias goes to the fallback provider's decision factory when it has one. A provider without a decision factory, or one that returns null, throws `NoSuchModelError` with `ModelType` `decisionModel`. An unknown registry provider throws `NoSuchProviderError`. A model whose `SpecificationVersion` is not `v4` throws `UnsupportedModelVersionException`.

The deprecated factory `IEvaluationProvider.EvaluationModel(id)` and the deprecated alias map `CustomProvider.AddEvaluationModel(alias, IEvaluationCaller)` still resolve through `DecisionModel`. A provider that implements both factories is asked through `IDecisionProvider` only. The deprecated alias map is ignored once any alias is added with `AddDecisionModel`. `ProviderRegistry.EvaluationModel(id)` and `CustomProvider.EvaluationModel(id)` are deprecated aliases of `DecisionModel(id)`. They return the same decision model as an `IEvaluationDecisionCaller`, which also has the deprecated `DoEvaluateAsync`. A model that already implements `IEvaluationDecisionCaller` is returned unchanged. Any other model is wrapped. The wrapper's `DoEvaluateAsync` calls the model's own `IEvaluationCaller.DoEvaluateAsync` when it has one, and `DoDecideAsync` otherwise.

#### String model ids and the default provider

`DecideRequest` takes the model as `Model` (an `IDecisionCaller`) or as `ModelId` (a string). `Model` wins when both are set, and `AiSdkException` ("Model or ModelId is required.") is thrown when neither is. A model id is resolved through `DecisionModels.DefaultProvider` (`AI_SDK_DEFAULT_PROVIDER` upstream), which must implement `IDecisionProvider` or the deprecated `IEvaluationProvider`. Otherwise `DecisionModels.Resolve` throws `InvalidOperationException` ("The default provider does not support decision models."). When `DefaultProvider` is null, the id goes through `GatewayProvider` and its `AI_GATEWAY_API_KEY` key. `DecisionModels.GatewayFactory` replaces that provider. A provider that returns no model throws `NoSuchModelError` (`decisionModel`), and errors thrown by the provider pass through. The `v4` check applies whether the model comes from an instance, an alias, a registry, or the default provider. `CustomProvider.AddDecisionModel(alias, "provider-model-id")` registers a string alias. `CustomProvider.DecisionModel(alias)` resolves it through the default provider.

#### Rubric models

`AiClient.EvaluateAsync(EvaluateOptions)` scores a candidate against a rubric and returns a `RubricResult`. Upstream has no such API, so the port names it after the rubric. A model that scores directly implements `IRubricModel` and is set on `EvaluateOptions.RubricModel`. Without one, a language model scores the candidate. `ProviderBase.RubricModel(id)`, `ProviderRegistry.RubricModel("providerId:modelId")`, `CustomProvider.RubricModel(id)`, and `CustomProvider.AddRubricModel(id, model)` return and register rubric models. `AnthropicProvider.RubricModel(id)` returns an `AnthropicEvaluationModel`.

These names replace the old rubric names, because `EvaluationModel` now means the deprecated decision accessor. `IEvaluationModel`, `EvaluationResult`, `EvaluateOptions.EvaluationModel`, and `CustomProvider.AddEvaluationModel(id, IEvaluationModel)` still compile and are marked `[Obsolete]`. `RubricModels.FromLegacy(model)` wraps an `IEvaluationModel` as an `IRubricModel`. `ProviderBase.EvaluationModel` is gone; override `RubricModel` instead. `AnthropicProvider.EvaluationModel(id)` now returns the decision model; call `RubricModel(id)` for the rubric model.
