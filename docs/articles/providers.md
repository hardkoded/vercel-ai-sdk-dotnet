# Providers

Native wire protocols have their own request mappers:

- Gateway posts V4 call options to `/language-model`, `/embedding-model`, `/image-model`, and `/video-model`. `GatewayProvider.VideoModel(id)` returns a `GatewayVideoModel` (see [Gateway video model](#gateway-video-model)). `new GatewayImageModel(provider, modelId, providerName)` sets `Provider` (upstream `GatewayConfig.provider`); it defaults to `gateway`. `TranscriptionModel(id)` returns a `GatewayTranscriptionModel` that posts the base64 audio, the media type, and the provider options to `/transcription-model` (see [Gateway transcription](#gateway-transcription)). `DecisionModel(id)` posts the state and questions to `/decision-model`. `GatewayProvider.EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)`, and `GatewayProvider` also implements the deprecated `IEvaluationProvider`. `ExtractionModel(id)` (alias `Extraction(id)`) posts the state and fields to `/extraction-model` (see [Gateway extraction models](#gateway-extraction-models)). Every Gateway language, embedding, image, video, transcription, decision, extraction, and batch request resolves its credential like the realtime calls: `GatewayOptions.ApiKey`, then `AI_GATEWAY_API_KEY`, then the Vercel OIDC token. `GatewayOptions.ApiKey` is a `Resolvable<string>`: pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`. A callback is resolved before each request, and the callback handles its own caching and renewal. A value that resolves to an empty string counts as no key, so the credential falls back to `AI_GATEWAY_API_KEY` and then the OIDC token. `ai-gateway-auth-method` is `api-key` or `oidc` to match, and a failure is a `GatewayAuthenticationError`. Every Gateway language, embedding, image, video, and batch request carries observability headers from `GatewayOptions.O11yHeaders`. When it is null, they come from `VERCEL_DEPLOYMENT_ID` (`ai-o11y-deployment-id`), `VERCEL_ENV` (`ai-o11y-environment`), `VERCEL_REGION` (`ai-o11y-region`), and `VERCEL_PROJECT_ID` (`ai-o11y-project-id`). Empty variables are left out. The `ai-o11y-request-id` header comes from the `x-vercel-id` header of the Vercel request context (upstream `getVercelRequestId`). A .NET host supplies that context with `using var scope = VercelRequestContext.Use(headers)`, for example from ASP.NET Core middleware that copies the request headers (`IHeaderDictionary` holds `StringValues`, so build a `Dictionary<string, string?>` from `context.Request.Headers`) and keeps the scope open while it awaits `next(context)`. Pass `x-vercel-oidc-token` only when a trusted Vercel runtime or ingress sets it and strips client-supplied values, because the header takes precedence over `VERCEL_OIDC_TOKEN`. Disposing the scope revokes the headers; names that differ only by case must carry the same value. `VercelRequestContext.Use(get)` takes a function instead, like the `get` method of upstream's request context: it runs on every lookup, and a null function or a null result reads as no headers. As upstream, an expired request-header token is not replaced by a refreshed `VERCEL_OIDC_TOKEN`, so supply a fresh one. Without a context the header is left out. `GetAvailableModelsAsync`, `GetCreditsAsync`, `GetSpendReportAsync`, and `GetGenerationInfoAsync` send the same auth headers as the realtime calls: the key (or, when no key is set, the Vercel OIDC token) (upstream `getVercelOidcToken` from `@vercel/oidc`: the Gateway reads `VERCEL_OIDC_TOKEN`; when it is empty or expired, it refreshes the token with the Vercel CLI login (`vercel login`) and the project in `.vercel/project.json` (`vercel link`), caches it in the CLI data directory, and sets `VERCEL_OIDC_TOKEN`. The `x-vercel-oidc-token` header of the Vercel request context (`VercelRequestContext.Use`) is read first, as upstream's `getContext` does), `ai-gateway-auth-method`, `x-vercel-ai-gateway-team` from `GatewayOptions.TeamIdOrSlug`, the custom `GatewayOptions.Headers`, and a `user-agent` of `ai-sdk-gateway/<version>`. These four calls do not add the observability headers. A failure is mapped to a Gateway error; without a key or an OIDC token it is a `GatewayAuthenticationError`. `GatewayProvider.GetGenerationInfoAsync(new GatewayGenerationInfoParams { Id = "gen_..." })` (upstream `getGenerationInfo`) calls `GET /v1/generation?id=` on the base URL's origin and returns a `GatewayGenerationInfo` with `TotalCost`, the optional `MarketCost` (cost excluding surcharges), `SurchargeCost`, and `GatewayCost` (amount debited), plus model, provider, latency, and token counts. The three optional costs are null when the Gateway does not report them (absent or null), and a reported `0` is kept as `0`. `GatewayProvider.GetAvailableModelsAsync()` (upstream `getAvailableModels`) calls `GET {BaseUrl}/config` and returns a `GatewayFetchMetadataResponse` of `GatewayLanguageModelEntry` items (`Id`, `Name`, `Description`, `Pricing`, `Specification`, `ModelType`). `Pricing` carries `Input`, `Output`, and the optional `CachedInputTokens` and `CacheCreationInputTokens`. Models with a `ModelType` outside `GatewayLanguageModelEntry.KnownModelTypes` are dropped. The result is cached for `GatewayOptions.MetadataCacheRefreshMillis` (default five minutes), and a failed fetch is not cached. `GatewayProvider.GetCreditsAsync()` (upstream `getCredits`) calls `GET /v1/credits` on the base URL's origin and returns a `GatewayCreditsResponse` with `Balance` and `TotalUsed`. When the request headers carry `x-vercel-ai-gateway-team`, it is sent as the `teamId` query parameter (a value starting with `team_`) or the `slug` query parameter. `GatewayProvider.GetSpendReportAsync(new GatewaySpendReportParams { StartDate = "2026-03-01", EndDate = "2026-03-25" })` (upstream `getSpendReport`) calls `GET /v1/report` and returns a `GatewaySpendReportResponse`. `GroupBy` (`GatewaySpendReportGroupBy`), `DatePart`, `UserId`, `Model`, `Provider`, `CredentialType`, and `Tags` filter and group the rows. Each `GatewaySpendReportRow` has `TotalCost`, the optional `MarketCost`, token counts, and `RequestCount`, which are null when the Gateway does not report them. `GatewayProviderMetadata.From(result.ProviderMetadata)` (upstream `GatewayProviderMetadata`) reads the typed `providerMetadata.gateway` object. Its `AsyncJob` (`GatewayAsyncJobMetadata`) has `JobId`, `Status`, and the optional `WebhookSigningSecret` of a batch started with a webhook URL. The signing secret is sensitive start-response metadata: do not log or forward it. Other fields stay in `AdditionalProperties`.

  ASP.NET Core middleware that supplies the request context (accept `x-vercel-oidc-token` only when your Vercel runtime or ingress sets it and strips client-supplied values):

  ```csharp
  app.Use(async (context, next) =>
  {
      var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
      foreach (var name in new[] { "x-vercel-id", "x-vercel-oidc-token" })
      {
          if (context.Request.Headers.TryGetValue(name, out var values))
          {
              headers[name] = values.ToString(); // StringValues -> string
          }
      }

      using var scope = VercelRequestContext.Use(headers);
      await next(context); // keep the scope open while the request runs
  });
  ```

- Google `DecisionModel(id)` answers `Decide` questions through the Gemini Decisions API (`POST /decisions` on `v1beta`; provider id `google.decision`). Use `gemini-decision-1`: language model ids such as `gemini-3.5-flash-lite` no longer work. Choice and Score answers carry native probability distributions, and a Boolean question maps to Google's `noul` type, whose probability is the answer. `result.ProviderMetadata` has `google.confidence` (every question type, keyed by question id), `google.legend` (Score legends), and `google.usage` (the raw usage, with modality details); usage has input tokens and zero output tokens. A safety-blocked request returns a refusal for every question. State can hold text, JSON, images, audio, video, and PDFs, sent inline or by URL; a file reference or another media type throws `UnsupportedFunctionalityException`. Every question needs instructions, and Choice and Score questions need 2 to 255 options or non-empty levels, or the call throws `InvalidArgumentException` before any request. A `box` question throws `DecisionUnsupportedQuestionTypeException`. `MaxOutputTokens`, `Reasoning`, and every `providerOptions.google` entry are not sent; each becomes an `unsupported` warning. `GoogleProvider.EvaluationModel(id)` is a deprecated alias of `DecisionModel`. A response with a missing, extra, or invalid answer throws `InvalidResponseDataException`.
- OpenAI speaks Chat Completions, the Responses API, embeddings, images, speech, and transcription. `DecisionModel(id)` answers `Decide` questions through the Decisions API (`POST /decisions`; provider id `openai.decision`). `OpenAIProvider.EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)`. Its provider options are `safetyIdentifier` (a string of at most 128 characters) and `promptCacheKey` (a string that partitions prompt cache reuse across requests that share state). Any other `openai` option is reported as an `unsupported` warning and not sent. `OpenAITranscriptionModel` reports `Language` as a two-letter code, whether OpenAI sends the code, a known language name, or a single entry in `languages`. The full `languages` array is kept under `ProviderMetadata.openai.languages`. A refused question returns a `refusal` answer, and `Decide.DecideAsync` throws `DecisionRefusalError` for it.
- AssemblyAI transcription models are `assemblyai-stt-3-7` (the flagship) and `universal-2` (kept for languages `assemblyai-stt-3-7` does not support). `assemblyai-stt-3-7` takes `prompt`, `keytermsPrompt`, `temperature`, and `removeAudioTags`. `universal-3-5-pro` still works but adds a `deprecated` warning that points to `assemblyai-stt-3-7`, and `universal-2` adds an `other` warning that does the same. Every model id is sent in `speech_models`, so removed ids (`best`, `universal-3-pro`, `slam-1`, `nano`) raise no warning and the API reports the accepted models. The legacy `speech_model` request field is gone.
- AssemblyAI speech: `AssemblyAIProvider.SpeechModel("assemblyai-tts-1")` (typed: `Speech(id)`, which returns an `AssemblyAIProvider.AssemblyAISpeechModel`) posts a buffered `POST /v1/speech` to `https://tts.us.assemblyai.com` and returns the audio. `AssemblyAISpeechModel.GenerateAsync(new AssemblyAISpeechRequest(text))` also returns the warnings, `Usage` (billed characters), and `ProviderMetadata.assemblyai` (`requestId`, `model`, `voiceId`, `language`, `encoding`, `sampleRate`, `audioDurationMs`, `billedCharacters`). `OutputFormat` is `mp3` (default), `wav`, `pcm`, `mulaw`, or `alaw`; an unknown value falls back to `mp3` with an `unsupported` warning. `Voice` is sent as `voice_id`. `Language` takes `de`, `en`, `es`, `fr`, `it`, `pt` or the full name; other values, and `auto`, are dropped with an `unsupported` warning. `Speed` other than `1` and `Instructions` are dropped with an `unsupported` warning. The `assemblyai` provider options are typed as `AssemblyAISpeechModelOptions` (`AssemblyAISpeechModelOptions.Parse(providerOptions)` reads and validates them; an unknown value throws `InvalidArgumentException`). `encoding` (`pcm_s16le`, `pcm_mulaw`, `pcm_alaw`, `mp3`), `container` (`raw`, `wav`; needs a `pcm_*` encoding), `sampleRate` (8000, 16000, 22050, 24000, 44100, 48000), and `bitRate` (32000, 48000, 64000, 96000, 128000; MP3 only) override the output format. A `container` on MP3 and a `bitRate` on PCM are dropped with an `unsupported` warning. Streaming speech and the EU endpoints are not supported. The model catalog (`assemblyai-tts-1`, `assemblyai-stt-3-7`, `universal-2`, `universal-3-5-pro`) declares each model's capabilities, per-model provider option support, and the transcription deprecation and replacement warnings; any other model id reports `unknown` support.
- ElevenLabs speech and transcription failures preserve the provider message and parsed body on `ApiException.ErrorData`. Object details include `RequestId` and append a non-null error code to the exception message, validation arrays join each location and message, string details use the string directly, and legacy `error.message` bodies remain supported.
- ElevenLabs text-to-speech accepts `providerOptions.elevenlabs.withTimestamps` (a boolean, `null` is ignored). When `true`, the call goes to `/v1/text-to-speech/{voice}/with-timestamps`, the audio is decoded from `audio_base64`, and `ElevenLabsSpeechResult.ProviderMetadata` (`elevenlabs`) gains `alignment` and `normalizedAlignment` with `characters`, `characterStartTimesSeconds`, and `characterEndTimesSeconds`. A missing or `null` alignment is left out, and the raw JSON stays on `Response.Body`. `ElevenLabsMusicResponse.CreateMusicTimestampsResponseHandler()` reads the `multipart/mixed` body of `/v1/music/detailed` into `ElevenLabsMusicTimestampsResponse` (`Audio`, `WordTimestamps` with `Word`, `StartMs`, `EndMs`). `ElevenLabsSpeechModel` also serves the music (`music_*`, `/v1/music`, or `/v1/music/detailed` with `withTimestamps`), sound-effects (`eleven_text_to_sound*`, `/v1/sound-generation`) and text-to-dialogue (`eleven_v3*`, `eleven_v4*`, `/v1/text-to-dialogue`) models through `GenerateAsync`. Dialogue is used when `providerOptions.elevenlabs.inputs` is set, or for `eleven_v4` with a single input built from the text and voice. `OutputFormat` is normalized the same way for all of them (`wav` becomes `wav_44100`; native formats such as `auto` or `wav_24000` pass through) and `ProviderMetadata.elevenlabs.outputFormat` reports it, and is omitted when the output format is empty. Music and sound effects report `requestedDurationMs` only when a length is requested (`musicLengthMs` or `compositionPlan` for music, `durationSeconds` for sound effects). Detailed music reports `wordTimestamps` (`word`, `startMs`, `endMs`), and dialogue with `withTimestamps` reports `voiceSegments`. Call options a model ignores add `unsupported` warnings. Streaming speech and provider-option schema validation are not ported.
- ElevenLabs transcription accepts `providerOptions.elevenlabs.sourceUrl` (a string, `null` is ignored): the HTTPS URL of a hosted file for ElevenLabs to download and transcribe, instead of uploading `audio`. The request sends the form field `source_url` and no `file`, so pass an empty `AudioInput` for large or already-hosted files. A non-empty `audio` is ignored and adds an `other` warning (`ElevenLabs transcribes providerOptions.elevenlabs.sourceUrl; the audio input was ignored.`).

```csharp
var model = (ElevenLabsProvider.ElevenLabsTranscriptionModel)elevenlabs.TranscriptionModel("scribe_v2");
var result = await model.TranscribeAsync(
    new AudioInput(Array.Empty<byte>(), "audio/mpeg", null),
    new ElevenLabsTranscriptionRequest
    {
        ProviderOptions = JsonDocument.Parse("{\"elevenlabs\":{\"sourceUrl\":\"https://example.com/meeting.mp3\"}}").RootElement.Clone(),
    },
    CancellationToken.None
);
```
- OpenAI Responses on GPT-6 and later models accept `reasoningEffortUpdate` (`none`, `low`, `medium`, `high`, `xhigh`, or `max`). Set it in the `openai` provider options to prepend a `configuration_update` item to the input. Set it on a `SystemModelMessage` with empty content (through its `providerOptions`) to place the update inside the conversation. Pass that message in the messages list. The `instructions` argument is rebuilt from text only and drops message provider options. Sampling parameters (`temperature`, `topP`) and `logprobs` follow the last update, so a `none` update on GPT-6 Sol or Luna keeps them. Adjacent updates, unsupported efforts, and message updates on other models throw `UnsupportedFunctionalityException`. A request-level update on an unsupported model is dropped with a warning.
- OpenAI Responses `textFormat` (provider option) overrides the request-level `text.format` and keeps structured-output parsing and validation unchanged. Use `{"type":"text"}` or `{"type":"json_schema","name":...,"schema":...,"description":...,"strict":...}`. Without it, the format comes from the response format of the call (`json_schema`, or `json_object` when no schema is set). `textVerbosity` is sent as `text.verbosity`.
- `textFormatUpdate` on an empty `SystemModelMessage` (through its `providerOptions`) changes the response format at that position in the history. It emits a `configuration_update` item with `text.format` and accepts the same shape as `textFormat`, typed as `OpenAIResponsesTextFormat` (`OpenAIResponsesTextFormatText` or `OpenAIResponsesTextFormatJsonSchema`; `ToJsonElement()` returns the value for the `textFormat` provider option) (`json_object` is rejected with `InvalidArgumentException`). Keep the original request-level `textFormat` on every call and replay every earlier update in place. Put `reasoningEffortUpdate` and `textFormatUpdate` in the same empty message to change both. Consecutive configuration updates throw `UnsupportedFunctionalityException` (`Adjacent configuration updates`), including a request-level `reasoningEffortUpdate` in front of a leading format update. The updates share the restrictions of `reasoningEffortUpdate`: GPT-6 and later models, no `reasoningMode: pro`, no `contextManagement`, and no `truncation: auto`. Message updates then throw `Message-level textFormatUpdate` errors.
- `promptCacheOptions.comparisonResponseId` (a recent completed response ID from the same organization) requests cache diagnostics. It is sent as `prompt_cache_options.comparison_response_id`. The result is on `ProviderMetadata.openai.promptCacheDiagnostics` for generation, streaming (on the `finish` part), and batch results. It holds `type` (`cache_hit`, `cache_miss`, `comparison_response_not_found`, `unavailable`, or a future value) and, when OpenAI sends them, `reason`, `cache_missed_tokens`, and `comparison_reusable_tokens`, with the API field names. Missing diagnostics do not fail the call.
- OpenAI reports input items it changed before generation, such as dropped reasoning items, as `ProviderMetadata.openai.inputTransformations` for generation, streaming (on the `finish` part, also after `response.incomplete` or `response.failed`), and batch results. Each entry holds `type`, `action`, `reason`, and, when OpenAI sends it, `id`. They are plain strings, so new actions and reasons pass through. To check whether reasoning was dropped, read it after a call that replays earlier reasoning: `[{ id: 'rs_...', type: 'reasoning', action: 'removed', reason: 'prefix_mismatch' }]`. `reason` is for example `prefix_mismatch`, `model_mismatch`, or `end_user_mismatch`. An empty array means nothing was removed. The key is left out when OpenAI does not report it, so "nothing dropped" and "not reported" stay different.
- OpenAI refusals (`ChatModel` and `ResponsesModel`) arrive as text. When the model declines a request, the Chat Completions `refusal` field and the Responses `refusal` content part (`response.refusal.delta` while streaming) become text content, so the refusal explanation is in the result text and in `NoObjectGeneratedError.Text` for structured output. Their finish reason is `ContentFilter` (the raw Chat Completions reason stays `stop`). Refusals mainly appear with structured outputs. A plain-text decline can still arrive as normal text with finish reason `Stop`. When a Chat Completions answer has both `content` and `refusal`, the content text comes first, and the refusal is a second text part (streamed deltas go to the same text part). A Responses refusal in the same response as a client-side function call keeps the `ToolCalls` finish reason, so the tools still run. The Responses batch results convert refusals the same way.
- OpenAI Responses turns a provider-executed `web_search_call` into a provider-executed tool call (input `{}`) followed by a tool result, in both generation and streaming (where the call is emitted with `tool-input-start` and `tool-input-end` when the item is added). Both use the name the request gives the `web_search` or `web_search_preview` tool (for example `webSearch`), or `web_search` when the request declares none. A replayed provider-executed web search call is not sent back as a function call.
- OpenAI Responses adds `web_search_call.action.sources` to `include` when the request has a `web_search` or `web_search_preview` tool, so the visited URLs come back as sources. Set `includeWebSearchSources: false` in the `openai` provider options to turn it off. `OpenAIResponsesLanguageModel.SupportsWebSearchSourcesInclude` (default true) turns it off for every call of a model.
- OpenAI Responses exposes the URLs in `web_search_call.action.sources` as `GeneratedSource` parts, and each retrieved `file_search_call` file once as a `GeneratedDocumentSource` (even when several passages come from one file, or the file is not cited). Inline `url_citation`, `file_citation`, and `container_file_citation` annotations become `GeneratedText.Citations`, with the output range as `StartIndex` and `EndIndex`, and on a streamed `TextEndStreamPart` for each completed message. Repeated ranges and URLs with query strings or fragments stay distinct. The raw annotations stay in the text part's provider metadata as `openai.annotations`. A `file_path` annotation is a generated file, not a citation. When the response has no retrieval data, the annotation's own source is still emitted as a source. In a stream that decision is made when the stream ends. Provider-executed `web_search_call` and `file_search_call` results are reported as tool results named `web_search` and `file_search`.
- OpenAI Responses sends the `reasoningSummary` provider option (any string, such as `auto`, `concise`, or `detailed`; OpenAI validates it) as `reasoning.summary` on reasoning models, next to `reasoning.effort`. When a reasoning effort other than `none` is set and `reasoningSummary` is absent, the summary defaults to `detailed`. Pass `reasoningSummary: null` to send none. Setting `reasoningSummary` on a non-reasoning model drops it with an `unsupported` warning.
- OpenAI Chat and Responses map the top-level `Reasoning` option to an effort the model supports. The supported efforts are: GPT-5 (and its `mini`/`nano` variants) `minimal`, `low`, `medium`, `high`; GPT-5.1 `none`, `low`, `medium`, `high`; GPT-5.2 to GPT-5.5 add `xhigh`; GPT-5.6 and GPT-6 Sol/Luna add `max`; other GPT-6 and later models support `low` to `max`. GPT-5 Pro, Codex, and Chat variants declare none, so every level passes through. An unsupported level maps to the next lower supported level, or the next higher one if there is none (`none` is never a fallback), and adds a `compatibility` warning (`reasoning "minimal" is not directly supported by this model. mapped to effort "low".`). Chat Completions does not support `max`, so `max` maps to `xhigh` there. A `reasoningEffort` provider option is sent unchanged and adds no warning.
- `OpenAITools` builds the OpenAI provider-defined tools: `ImageGeneration(OpenAIImageGenerationToolOptions)`, `WebSearch(OpenAIWebSearchToolOptions)`, `Mcp(OpenAIMcpToolOptions)` (one of `ServerUrl`, `ConnectorId`, or `TunnelId` is required; `TunnelId` is the identifier of a configured [OpenAI Secure MCP tunnel](https://developers.openai.com/api/docs/guides/secure-mcp-tunnels) for a private MCP server and cannot be combined with `ServerUrl` or `ConnectorId`; the SDK uses the existing tunnel and does not create one; `ServerUrl` and `ConnectorId` together stay allowed; `Authorization` also applies to tunneled servers), `LocalShell()`, `ApplyPatch(OpenAIApplyPatchToolOptions)`, `Computer()`, `FileSearch(OpenAIFileSearchToolOptions)`, `CodeInterpreter(OpenAICodeInterpreterToolOptions)` (`ContainerId`, or `FileIds`, `MemoryLimit`, `NetworkDisabled`, `AllowedDomains`, and `DomainSecrets` for an automatic container), `Shell(OpenAIShellToolOptions)` (`Environment.ContainerSkills` of a `containerAuto` environment takes `OpenAIShellSkillReference` and `OpenAIShellInlineSkill` entries; an inline skill is a base64 `application/zip` archive in `Data`), `CustomTool(OpenAICustomToolOptions)` (the tool name is `Name`; `GrammarSyntax` is `regex` or `lark`), `ToolSearch(OpenAIToolSearchToolOptions)`, `WebSearchPreview(OpenAIWebSearchPreviewToolOptions)`, and `ProgrammaticToolCalling()`. Each returns a `LanguageModelTool` with the upstream camelCase args, except `Shell` and `ApplyPatch`, which return a `Tool` that you pass to `GenerateTextOptions.Tools` as it is. Their options take the typed `Execute` callback (`OpenAIShellInput` to `OpenAIShellOutput`, `OpenAIApplyPatchInput` to `OpenAIApplyPatchOutput`), `NeedsApproval`, and `NeedsApprovalFunction`, as upstream's `execute` and `needsApproval`. A local shell call and every apply_patch call run through `Execute`. The factories do not validate ranges (upstream validates with zod). `Shell`, `Mcp`, and `CodeInterpreter` set `RedactArgs`, the provider-owned telemetry view of the tool arguments (upstream's `redactArgs`): it replaces the domain secret `Value`s of a `containerAuto` shell environment or code interpreter container, and the `authorization` and header values of an MCP tool, with `[redacted]`. `RedactTelemetryArgs()` applies it without changing the tool's `Args`, and returns null when the redactor throws. OpenAI Responses also annotates the raw bodies (upstream's `withOpenAIResponseRedaction`): `LanguageModelStreamRequest.RedactBody` and `LanguageModelGenerateResult.RedactRawResponse` are the telemetry view of a body that has a `tools` array, with every tool redacted as above (in the wire format, `network_policy` and `domain_secrets`). `RedactTelemetryBody()` and `RedactTelemetryRawResponse()` apply it without changing the body, and return null when the redactor throws. `TelemetryRedaction.WithTelemetryRedaction` and `RedactTelemetryValue` (upstream's functions of the same name) are the shared helpers. The port has no telemetry event that carries a request or response body, so nothing applies the view automatically yet.
- OpenAI Responses maps provider-defined tools (`LanguageModelTool.ProviderDefined(id, name, args)`) to hosted tools. `args` use the upstream camelCase names. `LanguageModelTool.ProviderDefined(id, name, args, redactArgs)` and `Tool.RedactArgs` take an optional telemetry redactor for `args`. Supported ids are `openai.file_search`, `openai.local_shell`, `openai.shell`, `openai.apply_patch`, `openai.computer`, `openai.web_search`, `openai.web_search_preview`, `openai.code_interpreter`, `openai.image_generation`, `openai.mcp`, `openai.custom`, `openai.programmatic_tool_calling`, and `openai.tool_search`. A `ToolChoice.Tool(name)` that names `code_interpreter`, `file_search`, `image_generation`, `web_search_preview`, `web_search`, `mcp`, `apply_patch`, `computer`, or `programmatic_tool_calling` is sent as that hosted tool, a name that matches an `openai.custom` tool is sent as a `custom` choice, and any other name is a function choice. A hosted `openai.mcp` call comes back as a provider-executed, dynamic tool call named `mcp.<tool name>` and a tool result `{ type: "call", serverLabel, name, arguments, output?, error? }` with the output item id in `providerMetadata.openai.itemId`. The `mcp_list_tools` item is not exposed. With `requireApproval` set, an `mcp_approval_request` item comes back as a provider-executed, dynamic `mcp.<tool name>` tool call with a generated tool call id, followed by a `GeneratedToolApprovalRequest` (`ApprovalId` is the `approval_request_id`, or the item id). `StreamText` and the model stream emit the same call and a `ToolApprovalRequestStreamPart`. Answer it with a `ToolApprovalResponseModelMessage`: the request becomes an `mcp_approval_response` item (`approval_request_id`, `approve`), preceded by an `item_reference` when `store` is true and the request has no `conversation` or `previousResponseId`. A repeated approval id is sent once. The `execution-denied` result of a denied call is not sent. A later `mcp_call` whose `approval_request_id` matches takes the generated tool call id, from the same stream or from an assistant tool call in the prompt whose `openai` provider options carry `approvalRequestId`. The provider validates `args` with the upstream schemas and throws `TypeValidationException` for invalid ones. It also reads the OpenAI function tool options in `LanguageModelTool.ProviderOptions` (`{ "openai": { ... } }`): `namespace` (`name` and `description`, grouped into one `namespace` tool), `deferLoading`, `allowedCallers`, `async`, and `outputSchema`. `async` is dropped with a warning on models before GPT-6. A `skillReference` skill of `openai.shell` resolves its `providerReference` for `openai` and defaults `version` to `latest`. OpenAI batches accept function and provider-defined tools. A provider tool other than `openai.code_interpreter`, `openai.custom`, `openai.file_search`, `openai.web_search`, and `openai.web_search_preview` adds an `unsupported` warning, because the batch results cannot convert its output.
- A `shell` tool result may carry `maxOutputLength`. OpenAI Responses sends it back as `shell_call_output.max_output_length`, and a `shell_call` input keeps `timeoutMs` and `maxOutputLength` as `timeout_ms` and `max_output_length`.
- An `apply_patch` tool result (`{ status: "completed" | "failed", output?: string }`) is sent back as an `apply_patch_call_output` item with `call_id`, `status`, and `output`, also when it shares a tool message with other tool results. An assistant `apply_patch` call replays as an `item_reference` when `store` is on (nothing with a previous response id or a conversation, which already hold it). Otherwise it is rebuilt as an `apply_patch_call` item (`call_id`, `id`, `status: "completed"`, `operation`), so the `apply_patch_call_output` has a matching call. A `local_shell` tool result (`{ output: string }`) is sent back as a `local_shell_call_output` item, and an assistant `local_shell` call replays the same way: an `item_reference` when `store` is on (nothing with a previous response id or a conversation), otherwise a `local_shell_call` item (`call_id`, `id`, and an `exec` `action` with `command`, `timeout_ms`, `user`, `working_directory`, and `env`). `OpenAIResponsesLanguageModel` also reads a `local_shell_call` output item in `DoGenerateAsync` and `DoStreamAsync` as a tool call with the `local_shell` tool name, `{ action }` input, and the item id in `providerMetadata.openai.itemId`. `DoGenerateAsync` keeps the action as the API returned it (`timeout_ms`, `working_directory`), `DoStreamAsync` camel-cases it (`timeoutMs`, `workingDirectory`), as upstream does.
- OpenAI Responses loads tools after an ordinary tool result. Set `additionalTools` in the `openai` (or `azure`) provider options of a `ToolModelMessage` (its `providerMetadata`), typed as `OpenAIResponsesToolResultOptions` (`ToProviderMetadata()`, with `OpenAIResponsesFunctionToolDefinition`, `OpenAIResponsesCustomToolDefinition` and `OpenAIResponsesNamespaceToolDefinition` entries), to a list of OpenAI-format tool definitions: `type: "function"` with `name`, `parameters`, and optional `description`, `strict` and `defer_loading`; `type: "custom"` with `name` and optional `description` and `format` (`{ type: "text" }`, or `{ type: "grammar", syntax: "regex" | "lark", definition }`); `type: "namespace"` with `name`, `description`, and a non-empty `tools` list of function definitions. Other types or keys, or a malformed list, throw. Messages can contain replayed, client-supplied history, so each entry only selects a tool that is registered in the request `Tools`: the provider sends the definition it derives from that tool, never the text in the entry. Function and namespace entries use the registered tool's description, input schema, `Strict` setting, and `deferLoading` option, and custom entries use the `description` and `format` of the registered `openai.custom` tool; the `description`, `parameters`, `strict`, `defer_loading` and `format` in an entry are not sent. Entries for tools that are not registered are dropped with one `unsupported` warning (`additionalTools`) per request. A namespace entry loads only the registered tools of that namespace whose names it lists. An entry whose `strict` or `defer_loading` value differs from the registered tool produces a warning. A tool that sets `allowedCallers`, a tool outside the `allowedTools` provider option, and a namespace entry while `allowedTools` is set cannot be expressed by a definition from history, so they are not loaded and a warning explains why. A repeated definition is sent once. Register every tool you load this way, usually with `omitFromRequestTools`. The options under the provider name win; when they carry no `additionalTools`, the `openai` ones are used. The provider keeps the `function_call_output` and sends an `additional_tools` item (`role: "developer"`) right after it, so replaying the same messages keeps the insertion point. An empty list sends no item. To load tools at any other point in the conversation, add an empty `SystemModelMessage` whose `openai` (or `azure`) provider options hold `additionalTools` (`OpenAIResponsesSystemMessageOptions.ToProviderOptions()`, which also types `ReasoningEffortUpdate` and `TextFormatUpdate` as an `OpenAIResponsesTextFormat`): an `additional_tools` item is sent in its place, so the earlier history and the request `tools` stay unchanged. The same registration check applies to these entries. The message must have empty content, and it needs no configuration update support from the model. When the same message also sets `reasoningEffortUpdate` or `textFormatUpdate`, the `configuration_update` item comes first. Set `Tool.ProviderOptions` (or `LanguageModelTool.ProviderOptions`) to `new OpenAIToolOptions { OmitFromRequestTools = true }.ToProviderOptions()` (`{ "openai": { "omitFromRequestTools": true } }`; `OpenAIToolOptions` also types `AllowedCallers`, `Async`, `DeferLoading`, `OutputSchema` and `Namespace`) to keep a discovered tool active for validation and execution without listing its definition in the request `tools`. Omission only affects serialization: decide which tools a user may run by choosing which tools to register on the server, not by leaving them out of the request. Forcing such a tool with `ToolChoice.Tool(name)` throws `UnsupportedFunctionalityException`. Set the `allowedTools` provider option (`{ "toolNames": [...], "mode": "auto" | "required" }`) to send `tool_choice: { type: "allowed_tools" }` and replace the request tool choice. It lists function, hosted, MCP, and custom tools by tool name or by provider tool name. Tools omitted from the request, deferred tools, tools in a namespace, and `tool_search` cannot be listed, so they are dropped with a warning. If no entry is left, the call throws `UnsupportedFunctionalityException`. `OpenAIResponsesLanguageModel.PrepareResponsesTools` takes the same settings in `PrepareResponsesToolsOptions`. `GenerateTextAsync` and `StreamTextAsync` both support history-loaded tools: to replay a conversation, add `GenerateTextResult.ResponseMessages`, or `await StreamTextResult.ResponseMessages` for a stream, to the next call's messages, and keep the original messages, including the `additionalTools` option.
- OpenAI Responses reports each `function_call` as a `GeneratedToolCall` (a `ToolCallStreamPart` when streaming, after `tool-input-start`, `tool-input-delta`, and `tool-input-end` parts). Its `openai` provider metadata carries `itemId`, plus `async`, `namespace`, and `caller` when the output item has them. A `program` caller becomes `{ type: "program", callerId }`. Other callers pass through unchanged. A streamed `tool-input-end` carries `namespace` too. When the model calls an internal `parallel` wrapper (`tool_uses` with `functions.<name>` recipients) and you have not declared a tool named `parallel`, each nested call whose tool you declared becomes its own tool call with the id `{callId}_{index}` and an `openai.parallelToolCall` metadata object (`itemId`, `toolCallId`, `toolName`, `input`, `index`, `count`). If any recipient is undeclared, the wrapper is returned as a plain `parallel` call. A streamed wrapper buffers its input and emits the expanded calls when the item completes.
- OpenAI Responses reports a `program` output item (programmatic tool calling) as a provider-executed `GeneratedToolCall` (`ProviderExecuted = true`; a `ToolCallStreamPart` when streaming, on `response.output_item.done`). The tool name is the name you gave `OpenAITools.ProgrammaticToolCalling()` (or `LanguageModelTool.ProviderDefined("openai.programmatic_tool_calling", name, args)`). Its input is `{"code","fingerprint"}`. A `program_output` item becomes a `GeneratedToolResult` (a `ToolResultStreamPart` when streaming) with `{"result","status"}` (`completed` or `incomplete`). Both carry `openai.itemId` in provider metadata. Function calls the program makes carry `openai.caller` as described above. When you send these parts back, a program call becomes an `item_reference` (with `store`) or a `program` item, its result becomes an `item_reference` or a `program_output` item, and a function call or its result that has an `openai.caller` keeps it as `caller`.
- OpenAI Responses reads a `custom_tool_call` as a `GeneratedToolCall` whose `ArgumentsJson` is the JSON-encoded input string and whose finish reason is `ToolCalls`, and streams it as a `ToolInputStartStreamPart` with `InputEncoding = ToolInputEncoding.Text`, one `ToolInputDeltaStreamPart` per `response.custom_tool_call_input.delta` event (the raw text, not JSON-escaped), a `ToolInputEndStreamPart`, and a `ToolCallStreamPart` with the same `ArgumentsJson`. Its `openai` provider metadata carries `itemId`, plus `async` when present.
- A generated OpenAI Responses call (`DoGenerateAsync`, also Azure) reports the response `id` as `responseId` in `LanguageModelGenerateResult.ProviderMetadata.openai` (`azure` for Azure), plus `serviceTier` when the response has a string `service_tier` and `reasoningContext` when `reasoning.context` is not null, like the streamed finish part.
- A streamed OpenAI Responses call reports the `responseId` of the `response.created` event in the finish part's provider metadata, plus `serviceTier` and `reasoningContext` when the completed response has them. Providers that rotate item ids between events keep one id per output item: the text parts and the reasoning parts use the id the item had when it was added, found by `output_index`.
- A streamed OpenAI Responses call validates every chunk against the upstream chunk schema (`OpenAIResponsesSchema.IsValidChunk`). An invalid chunk, including a JSON value that is not an object, emits an `ErrorStreamPart` whose `Exception` is a `ProviderUtils.TypeValidationException` (`Type validation failed: Value: ...`, with `Value` the chunk) and the stream finishes with the `Error` reason. A chunk of an unknown type, or an output item of an unknown type, is ignored (upstream's `unknown_chunk`). A Chat Completions chunk (an object with a `choices` array and no `type`) emits an `ErrorStreamPart` whose `Exception` is an `ApiCallError` with the message `Received a Chat Completions stream while using the OpenAI Responses API...`, which points to `ChatModel(id)` or `OpenAICompatible`. It has the request `Url`, `RequestBodyValues`, `ResponseHeaders`, `ResponseBody` and `Data` (the chunk JSON), a `TypeValidationException` cause, and is not retryable. `OnError` receives that exception.
- `LanguageModelGenerateResult.StructuredOutputText` is the text a JSON `Output` parses when it differs from the concatenation of all text parts (null by default; an empty string is an explicit selection and is not replaced by the text parts). `GenerateText` parses it instead of `Text` for the final step only, while `Text`, `Content`, `ResponseMessages`, and usage keep the whole response. OpenAI and Azure Responses set it for a JSON call (`LanguageModelCallOptions.JsonResponse`) to the text of the last message with `phase: "final_answer"`, so commentary or repeated final answers are not joined into invalid JSON. Every message stays in `Content`, and the message `phase` is reported in the text part provider metadata as `openai.phase` for OpenAI and `azure.phase` for Azure, including streamed `TextStartStreamPart` and `TextEndStreamPart` values. Requested response logprobs are reported under the same provider key in `LanguageModelGenerateResult.ProviderMetadata`. A text part with an empty string is kept. `ExtractJsonMiddleware` (including a custom `transform` passed to its constructor) and `ExtractReasoningMiddleware` apply their transform to it too.
- `LanguageModelGenerateResult.Request` (maps to `request` of `doGenerate`) carries the request body that was sent to the provider, as `LanguageModelStreamResult.Request` does for `DoStreamAsync`. It is null when the model does not report it. `MistralConversationLanguageModel` sets it.
- OpenAI Responses also reads these provider options:
  - `reasoningMode` (`standard` or `pro`) is sent as `reasoning.mode`, and `reasoningContext` (`auto`, `current_turn`, or `all_turns`) as `reasoning.context`. Both warn on non-reasoning models. `reasoningMode: "pro"`, `contextManagement`, and `truncation: "auto"` do not allow `reasoningEffortUpdate`.
  - `contextManagement` (a list of `{ "type": "compaction", "compactThreshold": n }`) is sent as `context_management`. `compactionTrigger: true` appends a `compaction_trigger` item as the last input item. A `compaction` output item becomes a `custom` part of kind `openai.compaction` (in generation and streaming) whose `openai` metadata holds `type: "compaction"`, `itemId`, and `encryptedContent`. Replay it as an assistant `custom` part with the same kind and `openai` provider options: it goes back as an `item_reference` when `store` is on, as the full `compaction` item otherwise, and is skipped when a `conversation` is set.
  - `conversation` is sent as `conversation`. It warns when combined with `previousResponseId`. With either `conversation` or `previousResponseId`, assistant reasoning that carries an OpenAI `itemId` is not resent. Build that reasoning with the `AssistantModelMessage` constructor that takes `reasoningProviderOptions`. Without a continuation, a reasoning `itemId` is sent as an `item_reference` (or as a `reasoning` item when `store` is `false`).
  - With `store` enabled, an assistant text part that carries an OpenAI `itemId` (an `AssistantTextPart` with provider options) is sent as an `item_reference`. For a reasoning model, the reference is sent only when a reasoning part comes before the text in the same assistant message, because OpenAI rejects a stored message that is sent without its reasoning item. When the reasoning has been removed (for example by pruning), the text is sent inline instead. With `conversation`, text parts that carry an `itemId` are skipped because the conversation already holds them.
  - In multi-step `GenerateText` and `StreamText`, the provider metadata of reasoning (`GeneratedReasoning.ProviderMetadata`, or the metadata on `ReasoningStartStreamPart` and `ReasoningEndStreamPart`) is carried to the next step as `AssistantModelMessage.ReasoningProviderOptions`. OpenAI Responses reasoning returns `itemId` and `reasoningEncryptedContent` this way, so with `store: false` the encrypted reasoning is sent back.
- Azure OpenAI Responses: `AzureOpenAIProvider.ResponsesModel(id)` posts to `/openai/v1/responses` (provider id `azure.responses`). Provider options and message options are read from `azure` first and fall back to `openai` only when the `azure` key is absent. An empty `azure` object therefore hides the `openai` options.
- Azure Responses tools (maps to `azure.tools`): `AzureOpenAITools` builds the provider-defined tools Azure Responses supports: `ApplyPatch(OpenAIApplyPatchToolOptions)`, `CodeInterpreter(options)`, `CustomTool(OpenAICustomToolOptions)`, `FileSearch(options)`, `ImageGeneration(options)`, `Shell(OpenAIShellToolOptions)`, `ToolSearch(options)`, `WebSearch(options)` and the obsolete `WebSearchPreview(options)`. They are the `OpenAITools` factories, so the request body is the same. Availability depends on the Azure deployment and API version. Tool calls carry the output item id under the `azure` provider key (`providerMetadata.azure.itemId`), and a tool call you send back reads its `itemId` from `azure` provider options. With a stored response (`store` is true, the default) apply_patch and shell calls replay as `item_reference` items. With `store: false` they replay as `apply_patch_call` and `shell_call` items. A shell tool whose environment is `containerAuto` or `containerReference` runs on the Azure service, so its calls are provider-executed (`GeneratedToolCall.ProviderExecuted`) and the `shell_call_output` result arrives as a tool result. A `local` shell (the default) is executed by your application. `OpenAIResponsesLanguageModel` reads `shell_call`, `shell_call_output` and `apply_patch_call` output items in both `DoGenerateAsync` and `DoStreamAsync`. The streamed apply_patch input is built from the `apply_patch_call_operation_diff` events. `examples/AzureResponsesTools` runs the custom tool, a `containerAuto` shell, and a `containerAuto` shell with an inline skill against a deployment (`AZURE_API_KEY`, `AZURE_RESOURCE_NAME`). `ApplyPatch` and `Shell` return a `Tool` with `Execute` and `NeedsApproval` options: pass it to `GenerateTextOptions.Tools` as it is. Pass the result of another factory with `Tool.ProviderDefined(tool.ProviderToolId!, tool.Name, tool.Args)`.
- Azure deployment and model IDs: `AzureOpenAIProvider.ResponsesModel(deploymentId, settings)`, `ChatModel(deploymentId, settings)` and `CompletionModel(deploymentId, settings)` take an optional `AzureOpenAILanguageModelSettings { ModelId = "gpt-5" }`, for example `provider.ResponsesModel("production-chat", new AzureOpenAILanguageModelSettings { ModelId = "gpt-5" })`. The model reports `ModelId` as its identity (including telemetry) and, for Responses and Chat, uses it to detect capabilities such as reasoning support (no `temperature`, `developer` system role). API requests (and the deployment route, with `UseDeploymentBasedUrls`) still use the deployment name. The model ID Azure returns in response metadata is preserved. If `ModelId` is omitted, the deployment name is used for identity and capability detection. Keep the supplied model ID aligned with the model running in your deployment. The Responses reasoning-effort update check reads the model ID, so a custom deployment name such as `production` is only recognized when you pass `ModelId = "gpt-6-astra"` (or another supported GPT-6-or-later model).
- TypeSafe (`TYPESAFE_AI_API_KEY`) posts the state and questions to `/systemone` through `TypeSafeAiProvider.DecisionModel(id)` (provider id `typesafe.decision`). `providerOptions.typesafe.longContext` raises the context window from 64k input and 32k output tokens to 262k. PNG and JPEG file parts are sent as `multipart/form-data` images; other files throw `UnsupportedFunctionalityException`. See [TypeSafe](typesafe-ai.md).
- Cerebras `providerOptions.cerebras.serviceTier` (`auto`, `default`, `flex`, `priority`) sets `service_tier`. When the response has a string `service_tier`, the effective tier is `ProviderMetadata.cerebras.serviceTier` on the result and on the stream's finish part. It is omitted when the field is missing or `null`, and is never inferred from the requested tier. `queueThreshold` (an integer from 50 to 20000 milliseconds) is sent as the `queue_threshold` header (it only has an effect on `flex` and `auto` requests, on a supported endpoint) and replaces a header of that name in any casing. A value outside the range, or not an integer, throws `InvalidArgumentException` on both `DoGenerateAsync` and `DoStreamAsync`. Service tiers need a preview-enabled dedicated endpoint, so `examples/GenerateTextCerebrasServiceTier` and `examples/StreamTextCerebrasServiceTier` read `CEREBRAS_API_KEY` and `CEREBRAS_BASE_URL` and send `serviceTier: "auto"` with `queueThreshold: 200`. `OpenAICompatibleOptions.MetadataExtractor` (`OpenAICompatibleMetadataExtractor`) lets a provider built on the OpenAI-compatible chat model add its own provider metadata from the response body and stream chunks.
- `CerebrasLanguageModelChatOptions` builds the `cerebras` provider options with typed members: `ServiceTier` (`CerebrasServiceTier`: `Auto`, `Default`, `Flex`, `Priority`), `ReasoningEffort` (`CerebrasReasoningEffort`: `None`, `Low`, `Medium`, `High`), `ReasoningFormat` (`CerebrasReasoningFormat`: `None`, `Parsed`, `TextParsed`, `Raw`, `Hidden`), plus `User`, `StrictJsonSchema`, `ParallelToolCalls`, `Logprobs`, `TopLogprobs`, `LogitBias`, `QueueThreshold`, `Prediction` and `PromptCacheKey`. `ToProviderOptions()` returns the value for `LanguageModelCallOptions.ProviderOptions` and throws `InvalidArgumentException` for an out-of-range value.
- `ZaiLanguageModelChatOptions` builds the `zai` provider options with typed members: `DoSample`, `Thinking` (`ZaiThinking` with `Type` (`ZaiThinkingType`: `Enabled`, `Disabled`) and `ClearThinking`), `ReasoningEffort` (`ZaiReasoningEffort`: `None`, `Minimal`, `Low`, `Medium`, `High`, `XHigh`, `Max`), `ToolStream`, `RequestId` (6 to 64 characters) and `UserId` (6 to 128 characters). `ToProviderOptions()` returns the value for `LanguageModelCallOptions.ProviderOptions` and throws `InvalidArgumentException` for an out-of-range value.
- `ZaiChatLanguageModel` is the chat model that `ZaiProvider` returns. `Serialize()` returns the model id and config for a workflow boundary (provider id, base URL, and headers). It leaves out the API key and any header whose name contains `authorization`, `key`, `token`, `secret`, or `cookie`. `OpenAICompatibleOptions.ApiKey` is a `Resolvable<string>`: pass a string, a `Task<string>`, or a callback that is resolved before each request. `ZaiChatLanguageModel.Deserialize(serialized, apiKey)` restores the model; without `apiKey` it reads `ZAI_API_KEY`.
- `CerebrasModelCatalog.Entries` maps every known language model id (`LanguageModelIds`: `gpt-oss-120b`, `gemma-4-31b`) to a `CerebrasLanguageModelEntry`. `GetCerebrasLanguageModelEntry(modelId)` returns the entry of one model and accepts any model id; an unknown id gets an entry with `Unknown` support. `Language` declares the model's capabilities as `ModelCapability` values (`Inputs`, `ObjectGeneration`, `Tools`, `Reasoning`, `Sampling`) plus `StructuredOutput.JsonSchema`, which is `true` for every id. `ProviderOptions` declares per-model support for the options in `CerebrasProviderOptions.Language`. `serviceTier` needs a preview-enabled dedicated endpoint, and `queueThreshold` needs `flex` or `auto` on one. The declarations are metadata; they do not validate requests.
- `MistralModelCatalog.Entries` maps every Mistral model id that declares capabilities to a `MistralModelEntry` with a `Language`, `Transcription`, `Speech` or `Embedding` section. `GetMistralLanguageModelEntry(modelId)`, `GetMistralTranscriptionModelEntry(modelId)` and `GetMistralSpeechModelEntry(modelId)` accept any model id; an unknown id gets an entry with `Unknown` support. The language entry declares inputs, tools, sampling and the reasoning efforts of the Chat API (`Language.Reasoning`) and the Conversations API (`Language.Endpoints.Conversation.Reasoning`). `MistralProvider` reads these efforts to resolve `reasoning` and to warn on models without reasoning configuration. `ProviderOptions` declares per-model support for the options in `MistralProviderOptions` (`Language`, `Embedding`, `Transcription`, `Speech`). The declarations are metadata; they do not validate requests.
- HeyGen (`HEYGEN_API_KEY`) posts the state and questions to `/v3/models/decisions` through `HeyGenProvider.DecisionModel(id)` (provider id `heygen.decision`). See [HeyGen](#heygen).
- Liquid (`LIQUID_API_KEY`) posts the state and questions to `/decisions/v1/systemone` through `LiquidProvider.DecisionModel(id)` (provider id `liquid.decision`). See [Liquid](#liquid).
- ScaleDown (`SCALEDOWN_API_KEY`) posts the state and questions to `/v1/scaledown` through `ScaleDownProvider.DecisionModel(id)` (provider id `scaledown.decision`), and extracts text fields through `ScaleDownProvider.ExtractionModel(id)` (`/v1/chat/completions`, provider id `scaledown.extraction`). See [ScaleDown](scaledown.md).
- Parallel (`PARALLEL_API_KEY`) posts the state and questions to `/decisions` through `ParallelProvider.DecisionModel(id)` (provider id `parallel.decision`) and answers them against evidence from the live web. See [Parallel](parallel.md).
- Anthropic speaks the Messages API. `AnthropicAwsProvider` points that body at the AWS external endpoint. `AnthropicOptions.ApiKey` is a `Resolvable<string>` (`Vercel.AI.ProviderUtils`): pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`. A callback is resolved before each request, and the callback handles its own caching and renewal. A resolved key that is blank falls back to `ANTHROPIC_AWS_API_KEY`, then SigV4. `AnthropicAwsApiKeyFetch.SendAsync` resolves a `Resolvable<string>` key per call and throws when it resolves to an empty string. `DecisionModel(id)` answers `Decide` questions by wrapping the Messages language model in `DecisionLanguageModel` (provider id `anthropic.decision`). `AnthropicProvider` implements `IDecisionProvider`, and the deprecated `IEvaluationProvider.EvaluationModel(id)` resolves to the same model. `AnthropicProvider.EvaluationModel(id)` is a deprecated alias of `DecisionModel(id)`. `AnthropicProvider.RubricModel(id)` returns the rubric model. Anthropic tool calls emit `ToolInputStartStreamPart`, each non-empty input fragment as `ToolInputDeltaStreamPart`, and `ToolInputEndStreamPart` before `ToolCallStreamPart`; deferred tool calls pre-populated in `message_start` emit the same sequence with their complete input as one delta. A stream whose events end without a `message_stop` for the last message finishes with `FinishReason.Interrupted` and no raw finish reason, even when an earlier message in the same stream completed. A stream that ends after an error event or an invalid event gets no interrupted finish. A stream that ends before it produced any output also gets no interrupted finish, and the provider reports no finish part for it (the core no-output rejection is tracked in #769). The interrupted finish keeps the usage and provider metadata of the last completed message, or unset usage and no provider metadata when none completed.
  - `providerOptions.anthropic.includePings` (`true`, default `false`) emits a typed `PingStreamPart` for each streaming `ping` event, which `StreamTextAsync` forwards as `PingPart`. It is handled locally and never sent in the request body, and it has no effect on non-streaming calls. Pings carry no output, usage, or timeout reset, and stay out of the content, the text, and UI message streams. Raw `ping` events remain available through `Include.RawChunks` independently of this option. It applies to the Anthropic Messages implementation and the providers that wrap it (the custom provider key works too); Amazon Bedrock Converse does not emit typed pings.
  - `providerOptions.anthropic.serviceTier` (`auto` or `standard_only`) is sent as `service_tier`. A tier that belongs to MiniMax (`standard`, `priority`) is omitted with an `unsupported` `serviceTier` warning, and a value that is not one of the four known tiers throws `InvalidArgumentException`. The applied tier comes back as `ProviderMetadata.anthropic.serviceTier` for generate, stream, and batch results. It is read from `usage.service_tier`, with the top-level `service_tier` as the fallback that compatible providers use. In a stream, `message_start` sets it and a later `message_delta` usage can update it; a message that reports none does not keep the previous message's tier. The raw `usage.service_tier` stays in `ProviderMetadata.anthropic.usage`.
  - `GeneratedSource` parts come from `web_search_tool_result` blocks, one per search result. The citations on a text block are attached to its `GeneratedText.Citations` (a `Citation` for each of `web_search_result_location`, `search_result_location`, `page_location`, `char_location`, and `content_block_location`), including on a streamed `TextEndStreamPart`. They are not repeated as sources. A `web_search_result_location` citation only becomes a `GeneratedSource` when the response has no search results. The raw citations stay in the text part's provider metadata as `anthropic.citations`. Page, character, and block locations refer to the source document, not to the generated answer. A document citation uses the `text/plain` media type, and the title falls back to `Document {index}`, because the port does not track the prompt's documents.
  - `compaction` in the `anthropic` provider options (`type` and optional `instructions`) asks Anthropic to summarize the conversation on demand. It is sent as `compaction` with the `compact-2026-09-04` beta header. Combining it with `contextManagement` throws `InvalidArgumentException` for `providerOptions`. A `compaction` response block becomes a text part whose `anthropic` metadata holds `type: "compaction"` and its `signature`. Replay it as an assistant text part with that same `anthropic` provider metadata, and the signed `compaction` block (with `signature`) goes back to the API.
  - `claude-sonnet-5-5` always thinks and rejects thinking token budgets and forced tool use. Its lowest thinking setting is `thinking: { type: "between_tools" }`: no upfront thinking, only short progress notes between tool calls. `thinking: { type: "disabled" }` and `reasoning: "none"` are sent as `between_tools`, each with a warning. `between_tools` is accepted only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning. A `required` or named tool choice falls back to `auto`, and `structuredOutputMode: "jsonTool"` falls back to native `output_config.format`, each with a warning.
  - `claude-haiku-5-5` supports every effort level, including `xhigh`. It uses adaptive thinking and rejects thinking token budgets: `thinking: { type: "enabled", budgetTokens }` is sent as `{ type: "adaptive" }` with a warning, so use `effort` to control how much it thinks. Unlike `claude-sonnet-5-5`, thinking can be disabled, but only at `low`, `medium`, and `high` effort. At `xhigh` or `max` the effort is lowered to `high` with a warning. Structured output uses native `output_config.format`.
  - With manual thinking (`thinking: { type: "enabled", budgetTokens }`), a `required` or named tool choice falls back to `auto` with a warning. A named tool choice sends only the selected tool. This also applies when the top-level `Reasoning` option enables manual thinking, including when structured output uses the internal `json` response tool. Tool use is no longer guaranteed: instruct the model to use the tool in your prompt and verify that a tool call was made. Inline tool additions must use the selected tool's name when a named choice falls back to `auto`. Conflicting additions, including definitions replayed from conversation history, throw `InvalidArgumentException` before the request is sent.
  - An effort-only `SystemModelMessage` (empty content, `providerOptions.anthropic.effort` set to `low`, `medium`, `high`, `xhigh`, or `max`) is sent as a `system` message with `content: []` and `output_config.effort`, together with the `mid-conversation-output-config-2026-07-01` beta header. It may come first or later in the conversation, including several in a row, and the order is kept. The level applies from the next user turn until a later message changes it. Initial instruction text goes in the top-level `system` field, which is an empty array when only effort messages come first. `clearAt` (`next_user_message`) applies only to mid-conversation system messages, adds the `mid-conversation-system-clear-at-2026-08-21` beta, and supports text only, so put an effort update in a separate message. On an initial system message, `clearAt` and `effort` set together with text, or `clearAt` set at all, are ignored with a warning. Use an empty system message for an initial per-message effort, or the top-level `effort` provider option for the whole request.
  - A `SystemModelMessage` placed after the first message can change the tool set mid-conversation through `providerOptions.anthropic.toolChanges`. A `tool_removal` references a tool by `toolName`. A `tool_addition` either references a tool by `toolName` (declare it in the tools option, typically with `deferLoading: true`, and the `mid-conversation-tool-changes-2026-07-01` beta header is added) or defines it by value with `tool`, a `function` or `provider` tool in the same shape as an entry of the tools option (`{ type: "function", name, description, inputSchema }` or `{ type: "provider", id, name, args }`). A tool defined by value does not need to be declared up front, which lets you introduce tools that did not exist when the conversation started, for example after an MCP server connects. Defining a tool under an existing name replaces the earlier definition from that point on, so this also changes a tool's schema mid-conversation. The `inline-tools-2026-09-15` beta header is added automatically. When every tool is defined by value, the request has no top-level `tools`, but the tool choice and `disableParallelToolUse` are still sent. The computer use provider tools cannot be defined in a message: they are ignored with a warning, so declare them in the tools option and add them by reference. Tool changes on the initial system message are ignored with a warning. The converter hook `AnthropicPrompt.Convert(..., convertInlineTool)` converts a tool added by value, and a prompt that adds a tool by value without it throws `UnsupportedFunctionalityException`.
  - The JSON-tool fallback (`structuredOutputMode: "jsonTool"`) limits which application tools are available: `ToolChoice.None` sends only the internal `json` response tool, and a named tool choice sends only that application tool next to it. These restrictions also apply to inline tool definitions: a named choice allows only additions with the selected tool's name, and `ToolChoice.None` rejects all inline additions. Conflicting definitions throw `InvalidArgumentException` before the request is sent. Tool changes on the initial system message remain ignored with a warning.
  - Models that reject forced tool use (`claude-sonnet-5-5`, `claude-opus-5-5`, `claude-fable-5-1`, `claude-fable-5-2`) send native `output_config.format` even when the host adapter (Amazon Bedrock, Google Vertex AI) turns native structured output off, and add an `unsupported` warning for `structuredOutputMode` whenever the host adapter had native output off (or `jsonTool` was requested). This follows the model's own capability, not the Bedrock Converse table. Native structured output cannot represent every schema, so `auto` keeps the JSON response tool for a schema with a self-referencing `$ref`, an open object (a map, `additionalProperties: true`, `patternProperties`, or a free-form object), or more than 10 optional properties. Without native output, these models fall back to the JSON response tool, which keeps `none` and named tool choices (`none` sends only the `json` tool). A strict tool only adds the `structured-outputs-2025-11-13` beta when the host supports both native structured output and strict tools. A top-level `$ref` keeps its `definitions` and `$defs` in the sanitized schema.
  - When the JSON response tool is in use but cannot be forced (for example with manual thinking, where the forced tool choice becomes `auto`) and the model answers in text without calling it, `DoGenerateAsync` and `DoStreamAsync` return the text of the final answer instead of dropping it. Text before a tool call or result, and text of a message that stops for a tool call or `pause_turn`, is still dropped. A call to the `json` tool keeps replacing all other text. A stream holds the text back until the message ends, then emits it with its citations.
  - Provider tools declared with `LanguageModelTool.ProviderDefined` (or in `providerOptions.anthropic.providerTools`, and the provider tools that tool changes add by value) keep your tool name in the response, in `DoGenerateAsync` and `DoStreamAsync`: a call to the API tool `web_search` comes back under the name you gave the tool. A declared tool wins when an inline tool maps to the same API name.
  - `claude-fable-5-2` is known to the capability table with the same capabilities as `claude-fable-5-1`: a 128k output limit, always-adaptive thinking, `xhigh` and `max` effort, no sampling parameters, and no forced tool use. `thinking: { type: "disabled" }` is removed with a warning, and `effort` stays as set, including `xhigh`. A `required` tool choice falls back to `auto` with a warning. Effort defaults to `high` on the API. The `medium` and `xhigh` levels behave differently than on `claude-fable-5-1`, so start at `high` and re-test `medium` and your current setting. The Amazon Bedrock ids `anthropic.claude-fable-5-2`, `us.anthropic.claude-fable-5-2` and `global.anthropic.claude-fable-5-2` follow the same rules as the `claude-fable-5` ids there: strict tools and native structured output are off.
- Google speaks Gemini `generateContent`. `GoogleOptions.ApiKey` (and `VertexOptions.ApiKey`, the Vertex Express Mode key) is a `Resolvable<string>` (`Vercel.AI.ProviderUtils`): pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`. A callback is resolved before each request on every Google model (language, embedding, speech, transcription, Interactions, batch, files, realtime), so a rotated key is picked up without recreating the provider, and the callback handles its own caching and renewal. A Vertex `ApiKey` that is set, even to a callback, selects Express Mode. `GoogleVertexProvider` changes the base URL and sends a bearer token. `GoogleVertexAnthropicProvider` reads `providerOptions.vertex.labels`, a string-to-string map for billing labels, and sends it as the base64 JSON `x-vertex-ai-labels` header for generate and stream (never in the body). Invalid labels throw `InvalidArgumentError` before any request, and an explicit `x-vertex-ai-labels` header, from the call or `GoogleVertexAnthropicOptions.Headers`, wins. Native structured output is opt-in on `GoogleVertexAnthropicProvider`. By default it uses the JSON response tool. Set `GoogleVertexAnthropicOptions.SupportsNativeStructuredOutput = true` to let `structuredOutputMode: "auto"` use the native output format for Claude 4.5 and newer model IDs (including Vertex IDs with an `@` suffix). Older models, IDs without a recognizable Claude version, and schemas native output cannot represent (such as open objects) keep the JSON tool. Google Cloud denies the feature by default through `constraints/vertexai.allowedPartnerModelFeatures`, so allow `structured_outputs` for each model you use. The setting does not enable strict tool input schemas, and policy rejections are returned as errors without falling back to tool emulation. Models that reject forced tool use (for example Opus 5.5) still switch to native output when the schema can be represented, with a warning if the opt-in is off. A per-request `providerOptions.anthropic.structuredOutputMode` still applies.
- `GoogleVertexProvider` downloads HTTP(S) file URLs in tool results before it converts the prompt, and sends them as inline data. `VertexOptions.ToolResultDownloads.MaxBytes` sets the size limit of each file (default 7 MiB), and `VertexOptions.DownloadFetch` sets the transport. The Vertex credentials are never sent to the file host. Gemini 3 and later models receive `gs://` URLs of `image/png`, `image/jpeg`, `image/webp`, `application/pdf`, and `text/plain` files as `fileData`. A tool result built from `UserContentPart` parts (`ToolModelMessage.OutputContent`) is sent as function response parts on Google too. `GoogleVertexMaasProvider.ImageModel` creates an image model, and `GoogleVertexPartnerOptions.DownloadHttpClient` sets the client for image URLs on another origin.
- Google and Google Vertex video file parts accept `videoMetadata` in their per-file provider options. Use the `google` namespace for `GoogleProvider`, or `googleVertex` (and the legacy `vertex`) for `GoogleVertexProvider`. `GoogleVideoMetadata.StartOffset` and `EndOffset` are durations in seconds such as `"60s"` or `"3.5s"`; `Fps` must be in the range `(0, 24]` and defaults to `1`. Metadata works with YouTube and Cloud Storage URLs and inline video data; uploaded Google file references are also supported by `GoogleProvider`.

  ```csharp
  var video = new FileContentPart(
      "video/mp4",
      "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
      data: null,
      fileName: null)
  {
      ProviderOptions = new Dictionary<string, JsonElement>
      {
          ["google"] = JsonSerializer.SerializeToElement(new
          {
              videoMetadata = new GoogleVideoMetadata
              {
                  StartOffset = "60s",
                  EndOffset = "600s",
                  Fps = 1,
              },
          }),
      },
  };
  ```
- A Gemini stream that carries an `error` object mid-stream (`code`, `message`, `status`, `details`) emits an `ErrorStreamPart` whose `Error` is a `ProviderStreamError` (`Type` is the `status`, `Code` and `StatusCode` the `code`, `Data` the whole error object) and finishes with `FinishReason.Error`. `IsRetryable` is true for codes 408, 409, 429, and 500 and above. Text already streamed is still closed and its usage is reported. `GoogleVertexProvider` language models do the same.
- Google `responseModalities` (a `google` provider option) lists the response modalities: `TEXT`, `IMAGE`, and `AUDIO`. Support is model-dependent. When it is absent or empty, the model returns only text. Non-text `inlineData` in the response, such as `audio/wav`, comes back as a `GeneratedFile` with its media type.
  - Long decode (Gemini 4 models such as `gemini-4-argon`): `MaxOutputTokens` is a cumulative budget across requests, not a target length. A request that reaches the per-request cap while budget remains finishes with `CONTINUATION`, which maps to `FinishReason.Length` and keeps the raw reason. `DoGenerateAsync` and `DoStreamAsync` then send the same request again with a top-level `continuationToken` and combine the results into one response. Set `MaxOutputTokens` above the per-request cap to enable this. Without it the call ends after one request. The stream keeps one text block across requests. Allow HTTP read timeouts of at least 15 to 20 minutes, because a complex request can run for several minutes.
  - `LanguageModelUsage` is the sum of all requests, which matches billing. Each continued request re-sends the prompt plus the output so far, so do not use the summed input tokens for context window checks. When a response spanned more than one request, `ProviderMetadata` `google.initialPromptTokenCount` holds the prompt token count of the first request.
  - Grounding across requests: when a response is continued automatically, `ProviderMetadata` `google.groundingMetadataPerRequest` holds the grounding metadata of each request, in order. An entry is `null` when that request returned none. This keeps earlier Search results when the final request has none, and keeps citation indices scoped to their original request. `google.groundingMetadata` keeps its current meaning (the last request only). Both `DoGenerateAsync` and `DoStreamAsync` set it, and `GoogleVertexProvider` language models do the same.
  - A response that used up its `MaxOutputTokens` budget finishes with `FinishReason.Length` and exposes `ProviderMetadata` `google.continuationToken`. Pass it back as `providerOptions.google.continuationToken` with a higher `MaxOutputTokens` and the original prompt to resume. A stream exposes the token only when it ended with `MAX_TOKENS` or `CONTINUATION`. If a continuation request fails, the stream emits an `ErrorStreamPart` and finishes with `Length`, the raw reason `CONTINUATION`, and the last token. `GoogleVertexProvider` language models do the same.
  - `GoogleProvider.Interactions(id)` with `providerOptions.google.background: true` runs the model call on the server, and `DoGenerateAsync` sends `generation_config.max_output_tokens` from `MaxOutputTokens` and polls `GET /interactions/{id}` until the status is terminal. The server runs every request, so there is no client-side continuation. Set `MaxOutputTokens` above 65,536 for long decode. Without `background: true`, an interaction generates at most 65,536 tokens. Polling starts at 1 second, doubles up to 10 seconds, and gives up after 30 minutes. Set `providerOptions.google.pollingTimeoutMs` (a positive integer, in milliseconds) to change that limit for slower runs. If you cancel the call while it polls, the SDK sends a best-effort `POST /interactions/{id}/cancel` so the run stops on Google's side. A polling timeout only stops the wait and does not send that cancel request, so the server-side interaction may keep running and keep billing.
  - `providerOptions.google.store` (a boolean, default `true`) is sent as `store`. Set it to `false` for stateless multi-turn conversations: re-send the full message history on every turn. The API then returns no interaction id.
  - `providerOptions.google.labels` (an object of string values) is sent as the top-level `labels` request field, for generate and stream. Use it for request metadata such as `{ "safety_identifier": "opaque_user_123" }`. Other labels are kept as they are. A `null` or missing value omits the field, and a non-string value throws before any request. Vertex Interactions models also read labels from `providerOptions.google`, not from `googleVertex` or `vertex`.
  - `providerOptions.google.previousInteractionId` is sent as `previous_interaction_id`. With `store: false` the SDK sends both and adds an `other` warning, because the server has nothing stored to refer to. The SDK does not drop earlier turns from the history yet: it always sends the whole prompt. `providerOptions.google.serviceTier` (`flex`, `standard`, or `priority`) is sent as `service_tier`. The applied tier comes back as `ProviderMetadata` `google.serviceTier`, from the response body or, as a fallback, the `x-gemini-service-tier` header. `ProviderMetadata` `google.interactionId` and `LanguageModelGenerateResult.ResponseId` carry the interaction id when the API returns one. `google.outputTokensByModality` maps each output modality to its token count, for example `{ "video": 57920, "text": 19 }`, and is absent when the response has no breakdown.
  - `Temperature`, `TopP`, `TopK`, `Seed`, `StopSequences`, `MaxOutputTokens`, `Reasoning` (see below), `providerOptions.google.thinkingLevel` (`minimal`, `low`, `medium`, `high`, or `xhigh`; `xhigh` is only supported by `gemini-4-hydrogen` and `gemini-4-neon`), and `providerOptions.google.thinkingSummaries` (`auto` or `none`) go into `generation_config`. `Reasoning` maps to `generation_config.thinking_level` using the levels the model supports. `xhigh` is supported only by `gemini-4-hydrogen` and `gemini-4-neon`. An unsupported level falls back to a supported one with a `compatibility` warning, and `none` maps to the lowest supported level. A model without thinking levels adds an `unsupported` warning and omits the setting. An omitted or `provider-default` `Reasoning` keeps the API default, and an explicit `providerOptions.google.thinkingLevel` wins. `FrequencyPenalty` and `PresencePenalty` are not supported and add an `unsupported` warning. `JsonResponse` sends `response_format: [{ type: "text", mime_type: "application/json", schema }]`, with `JsonSchema` as the schema when set. An agent call drops it with an `other` warning.
  - `GoogleProvider.Interactions(new GoogleInteractionsAgent(name))` calls a Gemini agent such as `deep-research-pro-preview-12-2025`. `GoogleInteractionsAgent.Managed(name)` calls a managed agent you created on Google's side. The request sends `agent` instead of `model` and no `generation_config`. `Temperature`, `TopP`, `TopK`, `FrequencyPenalty`, `PresencePenalty`, `Seed`, `StopSequences`, `MaxOutputTokens`, `Reasoning`, `thinkingLevel`, and `thinkingSummaries` are dropped with one `other` warning. `DoGenerateAsync` polls `GET /interactions/{id}` until the status is terminal, like a `background: true` call. Agent calls accept `providerOptions.google.agentConfig` (`{ "type": "dynamic" }`, or `{ "type": "deep-research", "thinkingSummaries", "visualization", "collaborativePlanning" }`, sent as `agent_config` with snake_case fields) and `providerOptions.google.environment` (`"remote"`, an existing environment id, or `{ "type": "remote", "sources", "network" }`, sent as `environment`). `environment` on a model-id call is dropped with an `other` warning.
  - Interactions requests send `Tools` and `ToolChoice` through `GoogleInteractionsTools.Prepare`. Function tools and the provider-defined tools `google.google_search`, `google.code_execution`, `google.url_context`, `google.file_search`, `google.google_maps`, `google.computer_use`, `google.mcp_server` and `google.retrieval` are mapped to `tools`, and `ToolChoice` is sent as `generation_config.tool_choice` (`auto`, `any`, or `allowed_tools`) when a function tool is present. `ToolChoice.None` omits every tool. Any other provider-defined tool is dropped with an `unsupported` warning. An agent call sends `tools` but no `tool_choice`, because it sends no `generation_config`.
  - `DoStreamAsync` posts the call with `stream: true` and reads the event stream as it arrives. Text, reasoning (with its signature), client function calls (`tool-input-start`, `tool-input-delta`, `tool-input-end`, `tool-call`), built-in tool calls and results (`providerExecuted` calls and `tool-result` parts), sources (from `text_annotation` deltas and built-in tool results, each URL or file once), `google.processing_call` and `google.processing_result` custom parts, response metadata, errors, and the finish part come through as they arrive. The `serviceTier` on the finish part comes from `interaction.completed`, with the `x-gemini-service-tier` response header as the fallback (not read for a background call). With `background: true`, the call is posted first, then the SDK streams `GET /interactions/{id}?stream=true`. When the connection drops, the SDK reconnects with `last_event_id` and keeps the stream going. After three failed attempts in a row the stream throws. If the POST already returns a terminal status, the stream replays that response. If you cancel the call, or stop reading before the run ends, the SDK sends a best-effort `POST /interactions/{id}/cancel`. Image and video outputs are not yet streamed: `FileStreamPart` carries bytes only, and the API can return a `uri`. `LanguageModelStreamResult.Request` holds the request body. `LanguageModelStreamResult.Response.Headers` holds the response headers. With `providerOptions.google.background: true`, `DoStreamAsync` posts the call first and then streams `GET /interactions/{id}?stream=true`; `Response.Headers` then holds the headers of the background POST response, both when the POST is already terminal and its result is replayed and when the events are streamed from the GET.
- Amazon Bedrock signs a Converse request with Signature Version 4. It signs the caller's headers too, and sends a header with a non-ASCII value without signing it. Provider options come from `amazonBedrock`, then `bedrock`. `reasoningConfig` (`type`, `budgetTokens`, `maxReasoningEffort` of `low`, `medium`, `high`, `xhigh`, or `max`, and `display`) becomes `additionalModelRequestFields.thinking` and `output_config.effort` on Anthropic models, `reasoning_effort` or `reasoning.effort` on OpenAI models, and a `reasoningConfig` field on other models such as Nova. The top-level `Reasoning` option fills in `reasoningConfig`, and fields you set win. The efforts follow the closest-level rule: on Anthropic models without `xhigh` effort, `xhigh` is sent as `high`. OpenAI `gpt-oss` models accept `low`, `medium`, and `high` (`none` is sent as `low`, `max` as `high`), other OpenAI models receive the value directly (including `none`), and Nova 2 accepts `low`, `medium`, and `high`. `none` on Nova 2 sends nothing, because its reasoning is off unless an effort is sent. `none` on a model without reasoning configuration adds an `unsupported` warning. `none` (or `reasoningConfig.type: "disabled"`) on an Anthropic model sends `thinking: { type: "disabled" }`, except for Claude 3.5 and earlier, which do not accept a `thinking` field and get none. Models that reject disabled thinking get `between_tools` thinking when they support it, or adaptive thinking otherwise, with a warning (`thinking cannot be disabled for <model>. Using '<type>' thinking instead.`, a `compatibility` warning for `none` and an `unsupported` warning for an explicit `reasoningConfig`). For the adaptive fallback, portable `none` uses an explicit `maxReasoningEffort`, then a raw `output_config.effort`, and defaults to `low` when neither is set. If a model rejects disabled or `between_tools` thinking at `xhigh` or `max` effort, the effort is lowered to `high` with an `unsupported` warning. `serviceTier` (`reserved`, `priority`, `default`, or `flex`) selects the inference service tier and is sent as `serviceTier.type`. `Temperature` is clamped to the 0 to 1 range with a warning. Models that reject sampling settings, such as Claude Opus 4.7, drop `Temperature`, `TopP`, and `TopK` with a warning. OpenAI models drop `StopSequences`, and OpenAI models other than gpt-oss also drop `Temperature` and `TopP`, each with a warning. Structured output (`JsonSchema`) on an Anthropic model follows `structuredOutputMode` (`outputFormat`, `jsonTool`, or `auto`, read from `amazonBedrock`, then `anthropic`). In `auto`, models that support it send a sanitized `additionalModelRequestFields.output_config.format`, and `jsonTool` removes a manually set `format`. Otherwise a forced `json` tool carries it, and the tool input comes back as the response text with `providerMetadata.bedrock.isJsonResponseFromTool` set. Bedrock declares support per Claude model, and a Claude model it does not know (for example a future id) gets neither strict tools nor native structured output. `claude-sonnet-5`, `claude-opus-5`, `claude-opus-5-5`, `claude-sonnet-5-5`, and `claude-haiku-5-5` support both, and `claude-fable-5` through `claude-fable-5-2` support neither on Converse. Models that reject forced tool use, and models that reject `strict` tools when you also pass tools, get a JSON instruction in the system message instead, except that a model that rejects forced tool use but supports native output falls back to native `output_config.format` unless the schema cannot be represented natively, and the first JSON object in the reply becomes the text. `DoStreamAsync` calls `/converse-stream` and decodes the AWS event stream (`application/vnd.amazon.eventstream`) as it arrives, so text, reasoning, and tool input are emitted as they are generated. A frame that fails its checksum, or bytes left over when the stream ends, throw. A modeled exception frame (`internalServerException`, `modelStreamErrorException`, `serviceUnavailableException`, `throttlingException`, `validationException`) becomes an `ErrorStreamPart` whose `Error` carries `StatusCode` (500, 424, 503, 429, 400) and `IsRetryable`, and the stream finishes with `FinishReason.Error`. Streamed reasoning text carries its signature on a `ReasoningDeltaStreamPart.ProviderMetadata`, and redacted reasoning content is attached once on the `ReasoningEndStreamPart`. A `TextContentPart` or an image `FileContentPart` with bytes sends a `guardContent` block when its `ProviderOptions` hold `guardContent: true` under `amazonBedrock` or `bedrock`. A text part can also set `guardContentQualifiers` (`grounding_source`, `query`, or `guard_content`). A `cachePoint` in `ProviderOptions` (for example `{ "type": "default", "ttl": "1h" }`, with `ttl` of `5m` or `1h`) adds a `cachePoint` block right after the content it belongs to. It works on `SystemModelMessage`, `UserModelMessage`, `AssistantModelMessage`, `TextContentPart`, `FileContentPart`, `ToolModelMessage` (set `ProviderOptions` on each tool result, and `MessageProviderOptions` to add one after all results of a grouped message), and the parts of an assistant message, reasoning parts included. An assistant message left with only cache points is dropped. A provider-executed `AssistantToolResultPart` ends the assistant message in front of it and is sent as a user `toolResult` message, so a result that ends an assistant message joins the next user message. Assistant reasoning is replayed only when its `ProviderOptions` hold a `signature`, `redactedContent`, or `redactedData` under `amazonBedrock` or `bedrock`. Reasoning without one of them is left out, and the text of signed reasoning is sent untrimmed. An empty or whitespace text part is left out unless the message has a reasoning part. The last assistant message of a prompt has its trailing whitespace trimmed, because Bedrock rejects it in a pre-filled response. A named tool choice sends every function tool on Anthropic models and puts `tool_choice: { type: "tool", name, disable_parallel_tool_use: true }` in `additionalModelRequestFields`, so the response stays on the named tool. Set `disableParallelToolUse: false` under the `anthropic` provider option to allow parallel calls and send the Converse `toolChoice` instead. Other models send only the named tool. Anthropic provider tools (such as `bash`, `memory`, `text_editor`, `computer`, and `code_execution`) go into `toolConfig` with their input schema, and their `tool_choice` and beta headers go into `additionalModelRequestFields` (`anthropic_beta`, merged after any `anthropicBeta` you set; `anthropicBeta` is sent as `anthropic_beta` even when no tool adds a beta). Provider tools on other models, and unknown provider tools, are dropped with an `unsupported` warning. `web_search_20250305`, `web_search_20260318`, and `web_fetch_20260318` are not supported on Bedrock and are dropped with an `unsupported` warning. A model that rejects forced tool use sends `auto` instead of a `required` or named tool choice, with an `unsupported` warning, and a named choice then sends only the named tool. An application inference profile ARN does not name its model, so pass `AmazonBedrockChatModelSettings { ModelFamily = AmazonBedrockChatModelFamily.Anthropic }` to `AmazonBedrockProvider.LanguageModel(modelId, settings)` to get the Anthropic request fields. Every provider option that is not one of `reasoningConfig`, `additionalModelRequestFields`, `serviceTier`, `structuredOutputMode`, or `requestMetadata` (for example `guardrailConfig`) goes into the request as is. Anthropic models also ask Bedrock for `/delta/stop_sequence` so `providerMetadata.bedrock.stopSequence` is set. `AmazonBedrockOptions.Headers` and `LanguageModelCallOptions.Headers` are sent on Converse, Converse stream, embedding, and rerank requests before signing. `AmazonBedrockOptions.HeadersProvider` is called on every request (upstream's resolvable `headers`); its values win over `Headers`, and a null value drops a header. Call headers win over provider headers, and every request carries the `ai-sdk-amazon-bedrock` user agent. `AmazonBedrockFetch` and `AmazonBedrockSigV4Fetch` apply the same signing and the `ai-sdk-amazon-bedrock` user agent to any other Bedrock call. `DoStreamAsync` returns a `LanguageModelStreamResult`: `Stream` holds the parts, `Request.Body` holds the JSON request body, and `Response.Headers` holds the response headers. `DoGenerateAsync` returns the headers on `LanguageModelGenerateResult.ResponseHeaders`. A stream that ends without a `messageStop` frame finishes with `FinishReason.Interrupted`, unless a chunk or modeled exception frame already set `FinishReason.Error`.
- Azure OpenAI uses the `api-key` header and the `/openai/v1` routes (`/openai/v1/responses`, `/openai/v1/chat/completions`, `/openai/v1/completions`, `/openai/v1/embeddings`) with `api-version=v1`. Set `AzureOpenAIOptions.UseDeploymentBasedUrls = true` for the legacy `/openai/deployments/{deployment}/...` routes; `ApiVersion` sets the `api-version` value. `AzureOpenAIProvider.LanguageModel(id)` returns a Responses model. Use `ChatModel(id, settings)` for Chat Completions. Breaking changes: Azure chat now runs on `OpenAIChatLanguageModel` (`ChatModel(id, settings)` returns it), so its error parsing and usage follow that class. It reads the `openai` and the `azure` provider options and merges them, and `azure` wins when both set the same option (like Azure completion models). Responses models use `azure` when it is present and `openai` otherwise. `ChatUri`, `CompletionsUri`, `EmbeddingsUri` and `CreateChatModel(id)` on an Azure provider route to `/openai/v1`. An empty or null `ApiVersion` falls back to `v1`.
- Azure base URL (breaking change: a host-only `BaseUrl` such as `https://my-resource.openai.azure.com` no longer gets `/openai` appended, so add `/openai` to it): `AzureOpenAIOptions.BaseUrl` is the full prefix, for example `https://my-resource.openai.azure.com/openai`, and defaults to `https://{ResourceName}.openai.azure.com/openai`. A host ending in `.openai.azure.com`, `.services.ai.azure.com` or `.cognitiveservices.azure.com` is Azure: an unversioned URL gets `/v1{path}?api-version=...`, and a complete `/openai/v1` URL is used as is without `api-version`. A Foundry project URL (`https://{resource}.services.ai.azure.com/api/projects/{project}/openai`) gets `/v1{path}` without `api-version`, and its Responses model sends `type: "message"` on message items. Any other host is a custom gateway: the URL is used as is (`{BaseUrl}/chat/completions`) and no `api-version` is added. `UseDeploymentBasedUrls` always uses `{BaseUrl}/deployments/{id}{path}?api-version=...`.
- Azure API key: `AzureOpenAIOptions.ApiKey` is a `Resolvable<string>`: a string, a `Task<string>`, or `Resolvable<string>.From(...)` with a synchronous or asynchronous callback (`Func<string>`, `Func<Task<string>>`, or `Func<CancellationToken, Task<string>>`). A callback is called before every request, so it can supply a refreshed key. It still falls back to `AZURE_API_KEY`, and `TokenProvider` still replaces it; setting both throws. Custom `Headers` stay a dictionary.
- Azure DeepSeek chat models: `AzureOpenAIProvider.DeepSeek(deploymentName)` (maps to `deepseek`, `createDeepSeekModel`) returns a `DeepSeekChatLanguageModel` with provider id `azure.deepseek` for Azure-hosted DeepSeek deployments, such as `deepseek-v4-pro` and `deepseek-v4-flash`. It posts to the Azure chat completions route and uses the Azure endpoint capabilities: penalty sampling is supported (`FrequencyPenalty` and `PresencePenalty` are sent), thinking cannot be configured (no `thinking` object is sent, and `temperature` and `topP` are always sent), and a JSON schema is sent as `response_format: json_schema`. A top-level `Reasoning` level is sent as `reasoning_effort`. Provider options are read from the `azure` key, and the provider metadata under `azure` carries the same fields as [DeepSeek chat](#deepseek-chat). `provider.LanguageModel(id)` still returns a Responses model.
- Azure image models: `AzureOpenAIProvider.Image(id)` (alias `ImageModel(id)`, provider id `azure.image`) returns an `AzureImageModel` that picks the API for each call. MAI-Image deployments (`MAI-Image-2.5`, `MAI-Image-2.5-Flash`, `MAI-Image-2.5-Pro`, `MAI-Image-2.6`, `MAI-Image-2.6-Flash`, `MAI-Image-2.7`, `MAI-Image-2.7-Flash`, matched case-insensitively) use the MAI image API at `{MaiBaseUrl}/images/generations` and `/images/edits` (default `https://{resource}.services.ai.azure.com/mai/v1`, with the `api-key` header or the Entra token), every other ID uses the Azure OpenAI `/openai/v1/images/generations` and `/images/edits` routes. Set `{ "azure": { "api": "openai" | "mai" } }` in the provider options to override the default for one call. MAI takes `width` and `height`, so `Size` is sent as is and `AspectRatio` is mapped to dimensions (at least 768 px per side, about one megapixel; `Size` wins and adds a warning). It returns one PNG per request, so `N > 1` sends one request per image, and it accepts reference images but not masks (warning `mask`) or a seed (warning `seed`). The MAI options `autoAspectRatio` and `webGrounding` (MAI-Image-2.6 and MAI-Image-2.7 only) go under `azure`; on the OpenAI API they are reported as an `unsupported` warning `providerOptions.azure.{name}`. `background` (`transparent` or `opaque`, MAI-Image-2.7 only; the API defaults to `opaque`, and `transparent` returns a PNG with an alpha channel) is sent as a JSON field for generations and a form field for edits, only when set. It is shared with the OpenAI API, which also accepts `auto`: on the OpenAI API `background` is passed through without a warning, and on the MAI image API `auto` is not sent and adds an `unsupported` warning `providerOptions.azure.background`. Any other value throws `InvalidArgumentException` before a request. Each 2.7 output is at most 2048x2048. The OpenAI image options (`quality`, `background`, `outputFormat`, `complexity`, `outputCompression`, `user`, `safetyIdentifier`, plus `style`, `frameCount`, `frameDurationMs` and `moderation` for generations and `inputFidelity`, `markup` and `moderation` for edits) are accepted under `azure` as well as `openai`; both are merged and `azure` wins. `markup` is a base64 `data:` URL or a URL, and cannot be combined with a mask. `MaxImagesPerCall`, `SupportsFileInputs` and `SupportsMaskInputs` follow the default route (MAI: 1, true, false; OpenAI: by model ID, unknown, unknown) and do not change with an `api` override. Image URLs in `Files`, `Mask` and `markup` are downloaded without the Azure credentials or provider headers: only the `User-Agent` is sent, each redirect hop is validated, and with a handler passed to `AzureOpenAIProvider.Create` the download uses that handler but not the Entra token handler. `OpenAIImageFile.FromUrl(url)` sends a URL image to any `OpenAIImageModel`, and the OpenAI image model reads `complexity`, `frameCount`, `frameDurationMs`, `safetyIdentifier` and `markup`. The `gpt-image-2.6-flare` and `gpt-image-2.6-sunburst` models accept them. `outputFormat: "gif"` (with `frameCount` 2-64 and `frameDurationMs` 20-2000) is generation only: an edit with `gif` is rejected with an `InvalidArgumentException`. `OpenAIImageModel.SupportsFileInputs` and `SupportsMaskInputs` are `true` for the known GPT Image, `dall-e-2` and `chatgpt-image-latest` ids, `false` for `dall-e-3`, and `null` for any other id. `OpenAIImageModel` also has a constructor that takes an `AzureOpenAIProvider` and a deployment ID.
- Azure speech: `AzureOpenAIProvider.Speech(id)` (alias `SpeechModel(id)`, which returns `ISpeechModel`) returns an `AzureSpeechModel` with provider id `azure.speech`. Its `GenerateAsync(OpenAISpeechCall)` returns the audio, the warnings, the response headers and `RequestBody`. The model picks the API for every call: `providerOptions.azure.api` (`"openai"` or `"speech"`) wins, and otherwise MAI-Voice model IDs (`mai-voice-2.1-flash`, `mai-voice-2.1`, `mai-voice-2-flash`, `mai-voice-2`, matched case-insensitively) use the Azure Speech API and every other ID, including custom deployment names, uses the Azure OpenAI speech API (`/openai/v1/audio/speech`, with `ApiVersion` and `UseDeploymentBasedUrls`). The Speech API posts SSML to `{SpeechBaseUrl}/tts/cognitiveservices/v1` (default `https://{resourceName}.cognitiveservices.azure.com`) with the `Ocp-Apim-Subscription-Key` header, or the Entra bearer token when `TokenProvider` is set; it does not use `BaseUrl` or `ApiVersion`. `Voice` is a voice ID that includes its locale, such as `en-US-Harper` (the default), `de-DE-Mia` or `es-MX-Valeria`; the model name is appended (`en-US-Harper:MAI-Voice-2`), and a full name such as `es-MX-Valeria:MAI-Voice-2` is sent unchanged. `Language` picks a default voice when `Voice` is not set (`de`, `en`, `es`, `fr`, `hi`, `hu`, `it`, `ko`, `nl`, `pt`, `ro`, `ru`, `th`, `tr`, `zh`); other languages and `auto` use `en-US-Harper` with a warning, and a `Language` that conflicts with the locale of an explicit voice is ignored with a warning. `OutputFormat` accepts `mp3` (default, 24 kHz), `wav`, `pcm`, `opus`, or any `X-Microsoft-OutputFormat` value such as `audio-48khz-192kbitrate-mono-mp3`; other values fall back to `mp3` with a warning. `Speed` becomes an SSML `prosody` rate. `Instructions` is not supported on the Speech API and warns. `providerOptions.azure.style` (for example `excited`) and `styleDegree` (0.01 to 2, requires `style`) set `mstts:express-as`. Text is escaped and always spoken as plain text. OpenAI speech options (`instructions`, `speed`) are read from `providerOptions.openai` and `providerOptions.azure` and merged, and `azure` wins. Speech-only options (`style`, `styleDegree`) used on the OpenAI API, and OpenAI-only options used on the Speech API, produce an `unsupported` warning for `providerOptions.azure.{key}`. An unknown `azure` key or an out-of-range value throws `InvalidArgumentError` before any request. A Speech API error uses the JSON `error.message` when present; an empty 400 answers "Azure Speech request failed with status 400. Check the voice name, style, and output format." and other statuses keep their retryability. `AzureSpeechModelOptions`, `AzureSpeechSpeechModelOptions` and `OpenAISpeechModelOptions` describe the options; `AzureSpeechModelOptions.GetMAIVoiceModel(id)` returns the voice model name of a MAI-Voice ID. `OpenAISpeechModel(AzureOpenAIProvider, id)` builds the Azure OpenAI speech model. `OpenAISpeechModel` implements `ISpeechCaller`, so `GenerateSpeech.GenerateSpeechAsync` accepts it.
- Bedrock Runtime and Agent Runtime origins resolve in this order: `AmazonBedrockOptions.BaseUrl` (or `AmazonBedrockAnthropicOptions.BaseUrl`) and `AgentRuntimeBaseUrl` for rerank, then `AWS_ENDPOINT_URL_BEDROCK_RUNTIME` or `AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME`, then `AWS_ENDPOINT_URL`, then `https://{service}.{region}.{suffix}`. The region is only validated when it builds the host. `AmazonBedrockProvider` Converse, embedding, and rerank models and `AmazonBedrockAnthropicProvider` all follow it.
- `AmazonBedrockOptions.ApiKey` (Converse, embedding, and rerank) is a `Resolvable<string>` too. When it or `AWS_BEARER_TOKEN_BEDROCK` is set, the models send `Authorization: Bearer` instead of SigV4, and a callback is resolved before each request.
- `AmazonBedrockAnthropicProvider` runs the Anthropic Messages model through Bedrock `InvokeModel` (`/model/{id}/invoke`, or `/invoke-with-response-stream` for streaming), so tools, files, images, reasoning, structured output, and `anthropic` provider options all work as they do on `AnthropicProvider`. It signs the request with Signature Version 4, or sends `Authorization: Bearer` when `ApiKey` or `AWS_BEARER_TOKEN_BEDROCK` is set. `AmazonBedrockAnthropicOptions.ApiKey` is a `Resolvable<string>`: pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`. A callback is resolved before each request, and the callback handles its own caching and renewal. A static blank key selects SigV4, while a callback or task that resolves to a blank key throws "must be a non-empty string". Before sending it drops `model` and `stream`, adds `anthropic_version: bedrock-2023-05-31`, keeps only the `type` and `name` of `tool_choice`, upgrades the `bash_20241022`, `text_editor_20241022`, and `computer_20241022` tools to the versions Bedrock supports, turns a tool's `eager_input_streaming` into the `fine-grained-tool-streaming-2025-05-14` beta, and moves the betas into `anthropic_beta`. Bedrock does not accept URL sources, so images and files are downloaded and sent as base64. Bedrock errors become Anthropic error bodies, and the AWS event stream becomes server-sent events (`AmazonBedrockAnthropicFetch.Create` wraps any `HttpMessageHandler` with that conversion). `AmazonBedrockAnthropicOptions.Headers` are sent with every call. `CredentialProvider` supplies SigV4 keys for each request instead of `AccessKeyId`, `SecretAccessKey`, and `SessionToken`; the region still comes from `Region`. `AmazonBedrockAnthropicProvider.Tools` lists the Anthropic provider tool ids, and `EmbeddingModel` and `ImageModel` throw `NoSuchModelError`.
- `BedrockMantleProvider` runs the OpenAI Chat Completions model (`LanguageModel`, `Chat`) or the OpenAI Responses model (`Responses`) against the Mantle endpoint, with the request signed for the `bedrock-mantle` service (or sent with a bearer token). `BedrockMantleOptions.ApiKey` is a `Resolvable<string>` (`Vercel.AI.ProviderUtils`): pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`. A callback is resolved before each request, and the callback handles its own caching and renewal. A static blank key selects SigV4, while a callback or task that resolves to a blank key throws "must be a non-empty string". Tools, files, images, reasoning, structured output, streaming, and `openai` provider options work as they do on `OpenAIProvider`. `BedrockMantleOptions.Headers` are sent with every call. The Mantle Responses model never adds `web_search_call.action.sources` to `include`. `CredentialProvider` supplies SigV4 keys for each request instead of `AccessKeyId`, `SecretAccessKey`, and `SessionToken`. `EmbeddingModel` and `ImageModel` throw `NoSuchModelError`.
- Cohere speaks chat, embed, and rerank. `CohereOptions.ApiKey` is a `Resolvable<string>` (`Vercel.AI.ProviderUtils`): pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`. A callback is resolved before each request, and the callback handles its own caching and renewal. A chat stream that ends without a `message-end` event finishes with `FinishReason.Interrupted`, unless an invalid chunk already set `FinishReason.Error`.
- OpenResponses posts to `{base}/responses`. The default base is the Gateway Open Responses route. `OpenResponsesOptions.ApiKey` is a `Resolvable<string>` (`Vercel.AI.ProviderUtils`): pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`. A callback is resolved before each request, and the callback handles its own caching and renewal. `OpenResponsesOptions.Headers` is a `Resolvable<IReadOnlyDictionary<string, string>>` that is resolved before each request too. `OpenResponsesOptions.Name` (default `open-responses`) is the key for provider options and provider metadata, and the model's provider id is `{Name}.responses`. System messages become `instructions`. Text and tool calls stream as they arrive, and reasoning streams from `reasoning_summary_text` and `reasoning_text` deltas. Reasoning, text, and tool-call parts carry their `itemId` in provider metadata (`GeneratedText.ProviderMetadata` and streamed `text-end` parts also carry `url_citation` annotations), and reasoning keeps its summary, content, and encrypted content. Each reasoning content part becomes its own `GeneratedReasoning`. A `custom_tool_call` item streams as `tool-input-start`, `tool-input-delta` (from `response.custom_tool_call_input.delta`), `tool-input-end`, and a `tool-call` whose input is the JSON string of the complete custom input. When no deltas arrive, the complete input comes from `response.custom_tool_call_input.done` or from the completed item. A completed `function_call` item with non-empty `arguments` takes precedence over the accumulated argument events, so endpoints that omit those events still produce a tool call. `AssistantModelMessage.Parts` keeps the text, reasoning, and tool-call parts in order, each with its own provider options (`AssistantTextPart`, `AssistantReasoningPart`, `AssistantToolCallPart`), so a later turn replays the items in the original order with their ids and annotations. `GenerateText` builds these parts from the model output. `StrictResponseInput` serializes assistant history using the strict OpenAI Responses input schemas: assistant text without an item id is sent as an easy input message, and text with an item id as a complete output message. A tool result built from text and file parts (`new ToolModelMessage(id, name, parts)`) becomes a `function_call_output` whose `output` is a list of `input_text`, `input_image`, and `input_file` items, as upstream does. Image parts keep their `imageDetail` provider option (default `auto`), and a file part with a provider reference is dropped with an `other` warning.
- OpenResponses warns and ignores `StopSequences`, `TopK`, and `Seed`. `OpenResponsesOptions.Headers` are sent with every request. It sends function tools. `OpenResponsesProvider.Tools.CustomTool(new OpenResponsesCustomToolOptions { Name, Description, Format, Execute })` creates a caller-executed OpenAI-compatible custom tool that takes a raw string input, optionally constrained by a grammar (`Format = new OpenResponsesCustomToolFormat { Type = "grammar", Syntax = "regex" | "lark", Definition = ... }`, or `Type = "text"`). It requires endpoint support. `OpenResponsesOptions.CustomToolId` (default `open-responses.custom`, shaped `<provider>.<tool>`) is the provider-tool ID of those tools. The request sends them as `{ "type": "custom", ... }` tools, `ToolChoice.Tool(name)` of a custom tool as `{ "type": "custom", "name" }`, and a generated `custom_tool_call` item becomes a tool call whose input is the JSON string of the custom input. The history replays custom calls and results as `custom_tool_call` and `custom_tool_call_output` items. A provider-defined tool (`LanguageModelTool.ProviderDefined(id, name, args)`) is omitted with an `unsupported` warning (`provider-defined tool <id>`) unless an extension registers it. A `ToolChoice.Tool(name)` that selects an omitted provider tool is omitted too.
- `OpenResponsesOptions.StructuredOutputs` (default `true`) says whether the endpoint supports `text.format`. When it is `false`, a JSON response format is omitted and reported as an `unsupported` `responseFormat` warning. `OpenResponsesOptions.UserAgentSuffix` is appended to the `User-Agent` header and defaults to `ai-sdk-open-responses/{version}`. Usage reports `input_tokens_details.cache_write_tokens` as `CacheWriteTokens`, and `NoCacheInputTokens` excludes both the cached and the cache-write tokens.
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

Some Responses-compatible implementations document non-namespaced extension discriminators, such as `web_search`, `web_search_call`, or `response.custom_tool_call_input.delta`. These are not portable Open Responses types. Register each exact value through the dedicated bare fields to opt in:

```csharp
var webSearch = new OpenResponsesExtension
{
    Id = "acme.web_search",
    AllowBareTypes = true,
    BareToolType = "web_search",
    BareItemTypes = new[] { "web_search_call" },
    BareEventTypes = new[] { "response.web_search_call.completed" },
    EncodeTool = context => new JsonObject
    {
        ["name"] = context.Name,
        ["search_context_size"] = context.Args.GetProperty("searchContextSize").GetString(),
    },
    DecodeItem = context => new GeneratedContent[]
    {
        new GeneratedToolCall(
            (string)context.Item["call_id"]!,
            (string)context.Item["name"]!,
            new JsonObject { ["query"] = context.Item["query"]!.DeepClone() }.ToJsonString())
        {
            ProviderExecuted = true,
        },
    },
    DecodeEvent = _ => null,
};
```

A runnable example with a mocked `HttpClient` is in `examples/OpenResponsesBareExtension`. It encodes a bare tool, decodes a provider-executed call, and replays the original item on the next turn.

Set `AllowBareTypes = true` when using `BareToolType`, `BareItemTypes`, or `BareEventTypes`, and only then. These fields accept only non-namespaced values. They do not enable unknown JSON passthrough: a bare discriminator is handled only when it exactly matches a registered value. Core Open Responses discriminators and duplicate registrations are rejected, and `ToolType` and `BareToolType` cannot be set together. Use the normal namespaced fields whenever the implementation supports them.

Set `ProviderExecuted` on each decoded tool call or `ToolInputStartStreamPart`. It can vary by item or event. An extension can define:

- `EncodeTool`: encodes a provider tool from `OpenResponsesToolContext` (`Name`, `Args`). Returning `null` omits the tool with an `unsupported` warning. A tool choice that selects the omitted tool is omitted too.
- `EncodeToolChoice`: encodes a specific tool choice. The default is an object with only the registered `ToolType` or `BareToolType`.
- `DecodeItem`: maps a completed item (`OpenResponsesDecodeItemContext.Item`, plus `Mode` of `Generate` or `Stream`) to content parts.
- `EncodeInputItem`: encodes a tool call or tool result in history (`AssistantToolCallPart` or `AssistantToolResultPart`) when the original wire item is unavailable. Every returned item needs `id`, `status`, and a `type` registered through `ItemTypes` or `BareItemTypes`.
- `DecodeEvent`: maps a streaming event to stream parts. `OpenResponsesDecodeEventContext.State` lasts for one response stream.

Tool, item, and event capabilities are independent. An item-only extension, for example, sets `ItemTypes` (or `BareItemTypes`) and `DecodeItem` without a tool type or `EncodeTool`. Capability fields must be provided in pairs.

Each decoded item adds a `GeneratedCustom` part of kind `open-responses.extension-replay` (a `CustomStreamPart` when streaming). It stores the original item in provider metadata, and the other decoded parts reference it by `itemId`. Pass the result back with `AssistantModelMessage.FromGeneratedContent(result.Content)` to replay the original item once, opaque fields included. This also preserves items that decode only to source parts.

The model validates only the extension discriminator fields. Validate the other item and event fields in the codec. The original item is stored in provider metadata and can appear in persisted messages, telemetry, logs, or UI payloads.

Extension delegates cannot cross a workflow boundary. `OpenResponsesLanguageModel.Serialize()` throws a `SerializationError` when extensions are registered. Create the provider inside the workflow step.
- Perplexity `DecisionModel(id)` answers `Decide` questions through the Decisions API (`POST {base}/v1/decisions`; provider id `perplexity.decision`). `PerplexityProvider` implements `IDecisionProvider`. Any model id is sent unchanged (for example `pplx-decider-latest`, `pplx-decider-flash-latest`, `pplx-decider-v1.2`, or `perplexity/pplx-decider-latest`). A `boolean` question is sent as Perplexity's native `noul` type, and the `noul` value comes back as the answer's `Probability`. Choice and Score answers keep the API's probability distributions without rounding or normalizing, so no `DecisionRounding` is declared. A `box` question throws `DecisionUnsupportedQuestionTypeException`. State parts map to the native `state` field: a single text, object, or array part is sent as is, and anything else is sent as an array. PNG, JPEG, and WebP files become base64 `image_url` data URLs, at most eight per request, counting images already nested in a json part. A file with another media type, or a URL file, throws `UnsupportedFunctionalityException`. The model rejects, before any request, more than 128 questions, an empty question name, more than 255 choice options, and a score with no level, more than 10 levels, or a null level. A single score level returns score `0` with probability `1`. Choice and Score confidence is in `ProviderMetadata` at `perplexity.confidence`, and Score legends at `perplexity.legend`, both keyed by question id. The model accepts two provider options under `providerOptions.perplexity`. `mode` (`"web_grounding"`) searches the web before answering, and grounded requests support up to 100 questions. `maxStaleness` (a non-negative integer, in seconds) is the maximum age of a fetched page. It applies to URL-only state with web grounding and does not filter search results. Omit it to use a stored page without an age limit. They are sent as `mode` and `max_staleness`, for any model id. Any other value throws `InvalidArgumentException` before a request, and any other `perplexity` option is reported as an `unsupported` warning and not sent. When the API returns them, `ProviderMetadata` at `perplexity` carries `searchResults` (`title`, `url`, `snippet`, and optional `date` and `last_updated`), `fetchedResults` (`title`, `url`, `snippet`), and `toolCalls` (invocation counts keyed by tool name, such as `search_web` and `fetch_url`, each as `{ invocation }`). The counts do not depend on the number of evidence results. `ApiException.ResponseHeaders` keeps `x-perplexity-search-queries` and `x-perplexity-fetched-urls` on HTTP errors when present. A disconnected request may have no headers, so missing counts do not prove that no search completed. The response `Id` is the `x-request-id` header. A failed request throws an `ApiException` that carries the response headers in `ResponseHeaders`, so a caller can read `retry-after`. `PerplexityDecisionModel.Serialize()` returns a `SerializedModelOptions` with the model id, `baseURL`, and the resolved request headers, including the `Authorization` header. `PerplexityDecisionModel.Deserialize(options, httpClient)` restores the model. The HTTP client is not serialized. Runnable examples are in `examples/DecidePerplexityBasic`, `examples/DecidePerplexityCriteriaOnly`, and `examples/DecidePerplexitySingleLevelScore`, and `examples/DecidePerplexityWebGrounding`.
- Perplexity language generation posts to `{base}/v1/agent` (the Agent API). Preset ids are `fast`, `low`, `medium`, `high`, and `xhigh`. Any other id is sent as an Agent API model id. Legacy Sonar model ids are not aliased, and Sonar provider options are not translated. Embeddings stay on the OpenAI-compatible embeddings route. Set `ToolChoice = ToolChoice.None` to disable all tools. This omits function tools and native tools from the request, including MCP and connector configurations, and sends `max_tool_calls: 0` to disable preset tools. It overrides `max_tool_calls` in the `perplexity` provider options and adds no warning. Sonar PDF input, video input, and image or video results have no Agent API equivalent. A stream that ends without a `response.completed` or `response.incomplete` event finishes with `FinishReason.Interrupted`. One of those events without response details finishes with `FinishReason.Other`.

Set `LanguageModelCallOptions.JsonResponse` to ask for JSON without a schema (upstream `responseFormat: { type: 'json' }`). OpenAI, OpenAI-compatible, Open Responses, Cohere, and xAI providers send `json_object`, Google sends `responseMimeType: application/json`, and the Gateway sends `json`. Anthropic ignores it and warns that the JSON response format requires a schema. Amazon Bedrock and Hugging Face also ignore it, as upstream does. `JsonSchema` wins when both are set.

The portable `Reasoning` option on `GenerateTextOptions` and `LanguageModelCallOptions` accepts `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, and `max`. Every provider resolves it with the same rule:

- A setting in the provider options (such as `reasoningEffort`) wins. It is not validated, rounded, or dropped, and it is sent unchanged.
- Otherwise `Reasoning` maps to the closest level the model supports, rounding down. The levels are ordered `none` < `minimal` < `low` < `medium` < `high` < `xhigh` < `max`, so `xhigh` becomes `high` on a model without an `xhigh` level. If there is no lower level, the next higher one is used. The provider returns a `compatibility` warning whenever the level it sends differs from the one you requested.
- Rounding never goes down into `none`. `none` on a model that cannot disable reasoning maps to the model's lowest level instead of being dropped.
- Providers round only for models and model families they recognize by model id, so a new model in a known family uses that family's levels. An unrecognized model id, such as a fine-tuned or custom model, and an OpenAI-compatible endpoint receive the value directly and the API decides whether it supports it. OpenResponses is the exception: its effort map is fixed, so `minimal` is sent as `low` with a `compatibility` warning. Use the `reasoningEffort` provider option to send a value unchanged.
- Providers that use a token budget map the level to a percentage of the model's maximum output tokens (95 percent for `max`) within the provider's budget limits. `none` maps to the minimum budget with a `compatibility` warning when the model cannot disable reasoning.

`ReasoningMap.MapReasoningToProviderEffort` (upstream `mapReasoningToProviderEffort`) and `ReasoningMap.MapReasoningToProviderBudget` implement the rule. The effort map lists only the levels the model supports, and `none` when it can disable reasoning. `OpenAICompatibleOptions.MapReasoning` (upstream `mapReasoning`) maps the top-level `Reasoning` for a provider built on the OpenAI-compatible chat model. It is not applied to a `reasoningEffort` provider option.

Provider details:

- Anthropic, DeepSeek, and OpenAI models that list `max` send it unchanged, with no warning. Anthropic models without `xhigh` effort send `xhigh` as `high`. OpenAI models with a known list of efforts, such as GPT-6, round to it: `none` and `minimal` on `gpt-6-astra` are sent as `low`. A `reasoningEffort` provider option is sent unchanged, even when the model does not list it.
- Google (Gemini 3) and Groq send `high` for `xhigh` and `max`. Gemini 3 sends `none` as the lowest thinking level with a warning, Gemini 2.5 Pro sends `none` as a thinking budget of 128 with a warning (it cannot disable thinking), and other Gemini 2.5 models send `none` as a budget of 0. Groq `openai/gpt-oss-*` models always reason, so `none` is sent as `low`. Groq models without reasoning get an `unsupported` warning for `none`, and `qwen/qwen3.6-27b` accepts `none`. Google `gemini-4-hydrogen` (and `gemini-4-hydrogen-eap`) and `gemini-4-neon` support `low`, `medium`, `high`, and `xhigh`: `xhigh` sends `xhigh`, `max` maps to `xhigh`, and `minimal` maps to `low`. Other Gemini models stay capped at `high`. Hydrogen and Neon also accept `providerOptions.google.thinkingConfig.thinkingLevel = "xhigh"`, which wins over `Reasoning`. Hydrogen and Neon default to `high` when the level is unset. Since the November 11, 2026 update, `xhigh` matches the previous `high`, `high` matches the previous `medium`, and `low` is unchanged, so re-run prompts tuned before then. Each change adds a warning. Fireworks sends every level directly, except for model families that reject one: `gpt-oss`, `glm-5p3` and `muse-spark` map `none` to `minimal`, and `nemotron` and `inkling` map `minimal` to `low`, each with a warning. A `reasoningEffort` provider option wins over `Reasoning` and adds no warning.
- Perplexity sends `xhigh` for `max`. `none` maps to `minimal` with a warning, because Perplexity reasoning cannot be disabled.
- Mistral models with the efforts `none` and `high` send `high` for every level except `none`. Models with `low`, `high`, and `max` round down (`medium` is sent as `low`, `xhigh` as `high`) and send `none` as `low`. Each change returns a `compatibility` warning. Models without reasoning effort (such as `mistral-large-latest`) get no `reasoning_effort` and an `unsupported` warning. A `reasoningEffort` provider option wins.
- OpenResponses sends `reasoning.effort` (`none`, `low`, `medium`, `high`, `xhigh`, and `max` are sent unchanged, and `minimal` becomes `low`). The provider options `reasoningEffort` and `reasoningSummary`, under the provider `Name`, set `reasoning.effort` and `reasoning.summary`.
- xAI Responses (`XaiProvider.LanguageModel` and `XaiProvider.ResponsesModel`) sends `reasoning.effort` (`minimal` becomes `low`). `xhigh` and `max` are sent as `xhigh` on `grok-4.6` and as `high` on other models, with a `compatibility` warning when the level changes. Grok 4.5 and later cannot disable reasoning, so `none` is sent as `low` with a warning. The `grok-4.20` reasoning and non-reasoning models reject the parameter: they get no `reasoning` object and an `unsupported` warning. A `reasoningEffort` provider option wins and adds no warning.

Moonshot native structured outputs are enabled for every model id, including Kimi, Moonshot V1, and custom or future ids. A request with a JSON schema sends `response_format.type = json_schema` with `strict: true`. A JSON request with no schema uses `json_object`.

Moonshot validates tool and response schemas with MFJS, so Moonshot requests normalize them first. Response schemas also omit the top-level `$schema` keyword because kimi-k2.5 produces invalid output when it is present. The root must be an object: a root `anyOf` or `oneOf` whose branches are all object schemas (a branch may be a local `#/$defs/X` or `#/definitions/X` reference) gets `type: "object"` added, and a root that already has `type: "object"` next to `anyOf` keeps it. Any other root throws `UnsupportedFunctionalityException`, such as a nullable object branch or a non-local `$ref`. Below the root, tuple `items` become `prefixItems`, and a `type` next to `anyOf` moves into the branches. The original schema still validates the result.

Moonshot maps the top-level `Reasoning` option by model family. Kimi K3 (`kimi-k3`, `kimi-k3.1-pro`, `kimi-k3.1-pro-highspeed`) always reasons, so it sends `reasoning_effort` only: efforts are `low`, `high`, and `max`, other levels map to the closest one (`medium` to `low`, `xhigh` to `high`), and `none` maps to `low` with a `compatibility` warning. Kimi K2.7 cannot disable thinking, so `none` sends no `thinking` field and returns a `compatibility` warning. Kimi K2.5 and K2.6 map `none` to `thinking: { type: "disabled" }` and other levels to `enabled`. Unknown model ids send `thinking: { type: "disabled" }` for `none` and map other levels to `reasoning_effort`. A `reasoningEffort` provider option always wins.

Groq rejects `response_format` together with tools, and Gemini before Gemini 3 rejects JSON response formatting together with function tools. When a call has a JSON schema and at least one function tool, these providers send an internal function tool whose arguments are the structured output, and turn that tool call back into text. The tool is named `json`, or `json_1`, `json_2`, and so on when a caller tool already uses the name. Its description is the output description (`OutputSpec.Object(schema, name, description)`, or `LanguageModelCallOptions.JsonSchemaDescription`), or `Respond with a JSON object.`. Tool choice `Tool("name")` is kept, `None` selects the response tool, and any other choice becomes `Required`. Groq sets `parallel_tool_calls` to false and skips the `structuredOutputs` warning on this path. Gemini 3 keeps native `responseMimeType` and `responseJsonSchema` with automatic or omitted tool choice, and uses the response tool only for `Required` or a named tool. Conversational text is dropped, calls to your own tools stay tool calls, and a response that has only the response tool finishes with `Stop`. Without function tools, both providers keep native structured output. `JsonResponseTools.ConvertJsonResponseToolStream` applies the same rewrite to a stream of `LanguageModelStreamPart`.

OpenAI-compatible providers (Alibaba, Groq, Mistral, Together, and the other chat wrappers; DeepSeek chat has its own model, see [DeepSeek chat](#deepseek-chat)) are thin wrappers over `Vercel.AI.OpenAICompatible`. They set the base URL, the provider id, and the environment variable. Perplexity embeddings use that client. Perplexity language generation uses the Agent API described above.

The `reasoningEffort` provider option may be `null`. The request then carries `"reasoning_effort": null`, for providers that treat an explicit `null` differently from an omitted field. It wins over the top-level `Reasoning` level. When no effort is set, `reasoning_effort` is not written, so a raw `reasoning_effort` provider option passes through unchanged.

An OpenAI-compatible image model (`ImageModel(id)`) reads image generation and editing responses that hold base64 data (`b64_json`), image URLs (`url`), or a mix of both, and keeps the images in order. A `url` image is downloaded, so `GeneratedImage.Data` and `GeneratedImage.Base64` are filled. When an image has both fields, base64 wins and nothing is downloaded. A data item with neither field throws `InvalidResponseDataException`. The download request carries the provider headers only when the URL is on the provider origin. Any other host is checked first (private addresses and non-http(s) schemes throw `DownloadException`), redirects are checked hop by hop, and no provider header is sent. Set `OpenAICompatibleOptions.DownloadHttpClient` to control the client used for those hosts.

Speech, transcription, image, video, and Voyage each have a package that calls that provider’s public HTTP API. Use `SpeechModel`, `TranscriptionModel`, `ImageModel`, `VideoModel`, or `EmbeddingModel` rather than `LanguageModel`. Every provider (`ProviderBase.SpecificationVersion`) and every video model (`IVideoModel.SpecificationVersion`) reports `v4`.

## DeepSeek chat

`DeepSeekProvider.ChatModel(id)` (also `LanguageModel(id)`, maps to `chat`) returns a `DeepSeekChatLanguageModel` with provider id `deepseek.chat`. It is a dedicated model, not the OpenAI-compatible chat model. `ResponsesModel(id)` still returns the Responses model.

Pass options as JSON under `deepseek` in `LanguageModelCallOptions.ProviderOptions`, or build them with `DeepSeekLanguageModelChatOptions.ToProviderOptions()`.

| Option | Request field | Notes |
| --- | --- | --- |
| `logprobs` | `logprobs` | Returns token log probabilities in `providerMetadata.deepseek.logprobs`. |
| `topLogprobs` | `top_logprobs` | 0 to 20. Setting it turns `logprobs` on. |
| `userId` | `user_id` | ASCII letters, digits, `_` and `-`, at most 512 characters. |
| `thinking.type` | `thinking.type` | `enabled` or `disabled`. `adaptive` is accepted and sent as `enabled` with a `compatibility` warning. |
| `reasoningEffort` | `reasoning_effort` | `low`, `high` or `max`. A top-level `Reasoning` of `medium` is sent as `low` and `xhigh` as `high`, each with a `compatibility` warning. |
| `strictJsonSchema` | `response_format.json_schema.strict` | Only on endpoints that support JSON schema response formats (Azure). Defaults to `true`. |

- A top-level `Reasoning` level sets `thinking` (`none` is `disabled`, any other level is `enabled`) and `reasoning_effort` (`low`, `high` and `max` are sent as they are, and `minimal`, `medium` and `xhigh` round down to the nearest supported level with a `compatibility` warning: `minimal` is sent as `low`, `medium` as `low`, and `xhigh` as `high`). The `thinking` and `reasoningEffort` options win.
- Thinking is on by default for V4 and newer model ids. `temperature` and `topP` have no effect while thinking is on: they are omitted with an `unsupported` warning. Set `thinking.type` to `disabled` to use them.
- DeepSeek deprecated `FrequencyPenalty` and `PresencePenalty`: they are omitted with a `deprecated` warning, except on endpoints that support penalty sampling (Azure). `TopK` and `Seed` return `unsupported` warnings.
- Prior-turn reasoning is sent back as `reasoning_content` for V4 and newer ids (an empty string when the assistant message has none) and dropped before the last user message for older ids such as `deepseek-reasoner`.
- Message options under `deepseek`: `name` on system, user and assistant messages, and `prefix: true` on the final assistant message for [chat prefix completion](https://api-docs.deepseek.com/guides/chat_prefix_completion/). Prefix completion and strict tools need a base URL that ends in `/beta`: otherwise the call throws `UnsupportedFunctionalityException`. With strict tools every function tool of the request must set `Strict = true`.
- Image parts accept JPEG, PNG, GIF and WebP, as URLs (up to 8192 characters, `http` or `https`), inline data, or a provider reference (a `file_id`). The file part options `imageDetail` (`low`, `high`, `original`, `auto`) and `fileData: true` (send inline data as a `file` part with `file_data`, not combinable with `imageDetail` or a URL) go under `deepseek` on the file part.
- A JSON response with a schema is sent as an instruction in a system message (with a `compatibility` warning) on `deepseek.chat`, and as `response_format: json_schema` on endpoints that support it. Without a schema it is `json_object`.
- The result's provider metadata under `deepseek` (or the provider name's first segment) has `promptCacheHitTokens`, `promptCacheMissTokens`, `responseObject`, `choiceIndex`, `messageRole`, `toolCallTypes`, `logprobs` and `systemFingerprint` when the response has them. A stream that ends without a finish reason finishes with `FinishReason.Interrupted`, an error chunk becomes an `ErrorStreamPart` whose `Error` is a `ProviderStreamError` with a status code and retry flag read from the DeepSeek error `code` and `type`, and `insufficient_system_resource` finishes with `FinishReason.Error`.


### DeepSeek model catalog

`DeepSeekModelCatalog.Entries` maps every known model id to a `DeepSeekLanguageModelEntry`. `DeepSeekModelIds.Chat` (`deepseek-flash`, `deepseek-v4-flash`, `deepseek-v4-pro`, `deepseek-v4-flash-vision-exp`) and `DeepSeekModelIds.Responses` (`deepseek-flash`, `deepseek-v4-pro`) list the ids per endpoint. `GetDeepSeekLanguageModelEntry(modelId, endpoint)` returns the entry of one model on an endpoint and accepts any model id; an unknown id gets an entry with `Unknown` support and the current protocol (assistant reasoning history on, unless the id is a legacy alias or a pre-V4 version). `DeepSeekEndpointOptions` (`SupportsAssistantPrefixCompletion`, `SupportsStrictToolCalls`, `SupportsPenaltySampling`, `SupportsThinking`, `SupportsStructuredOutputs`) fills the endpoint-dependent capabilities. `Language` declares the capabilities: `TextGeneration`, `Inputs`, `ObjectGeneration`, `Tools` (`Usage`, `Streaming`, `Strict`), `Reasoning` (`Support`, `Efforts`, `Configurable`, `EnabledByDefault`, `AssistantHistory`), `Sampling`, `StructuredOutput.JsonSchema` and `Endpoints.AssistantPrefixCompletion`. `ProviderOptions` declares per-model support for the options in `DeepSeekProviderOptions.Language`. The declarations are metadata; the chat model reads them to shape the request.

## Alibaba chat

The Alibaba chat model (`AlibabaProvider.LanguageModel(id)`) builds its own request from the OpenAI-compatible chat model. Pass options as JSON under `alibaba` in `LanguageModelCallOptions.ProviderOptions`.

| Option | Request field | Notes |
| --- | --- | --- |
| `enableThinking` | `enable_thinking` | Turns thinking mode on or off. |
| `thinkingBudget` | `thinking_budget` | Maximum reasoning tokens. |
| `preserveThinking` | `preserve_thinking` | Replays earlier assistant reasoning as `reasoning_content`. Defaults to `true` for models Alibaba documents as supporting it (such as `qwen3.7-max`). Other models send the field only when you set it. |
| `parallelToolCalls` | `parallel_tool_calls` | Sent only when the call has tools. |

- `LanguageModelCallOptions.Reasoning` maps to `enable_thinking` and `thinking_budget`. `none` sends `enable_thinking: false`. Any other level sends `enable_thinking: true` with a budget. `enableThinking` and `thinkingBudget` win over `Reasoning`. Alibaba never receives `reasoning_effort`.
- `LanguageModelCallOptions.TopK` is sent as `top_k`. `FrequencyPenalty` is not sent, and the call gets an `unsupported` warning for `frequencyPenalty`.
- Function tools are sent with their `strict` setting when you set one. A provider-defined tool is left out and the call gets an `unsupported` warning, `provider-defined tool <id>`. `ToolChoice` maps to `auto`, `none`, `required`, or a named function.
- Reasoning from the current tool-call round (after the last user message) always goes back with the tool results. Earlier reasoning is sent only when `preserveThinking` is on. An assistant message with no text, no tool call, and no reasoning to send is left out.
- Models `qwen3.7-plus`, `qwen3.7-flash`, `qwen3.7-max`, `qwen3.8-max`, and `qwen3.8-flash` (and their dated ids) use `response_format.type = json_schema` when the call has a JSON schema. Other models use `json_object`, inject the schema into the system message, and add a `compatibility` warning. A JSON call with no schema uses `json_object` and the instruction `You MUST answer with JSON.`.
- `DoStreamAsync` streams reasoning, text, and tool calls as separate blocks. A reasoning block ends when text or a tool call starts, and a text block ends when reasoning or a tool call starts. Text uses block id `0`, and each reasoning block gets a new generated id. Every tool call streams `tool-input-start`, `tool-input-delta`, and `tool-input-end` parts before its `tool-call` part. A chunk with an empty `tool_calls` array does not end the reasoning block.
- Usage reports `cache_creation_input_tokens` as cache write tokens. Cache reads and writes are both inside `prompt_tokens`, so `NoCacheInputTokens` excludes both. The raw usage object keeps fields the SDK does not map, such as `cache_type`.

### Prompt caching

Alibaba caches repeated prompts without any setting (implicit caching). For more control, mark content with `alibaba.cacheControl` (explicit caching). The value, for example `{ "type": "ephemeral" }`, is sent as `cache_control` and Alibaba validates it. `alibaba.cache_control` works too.

Set it in the `ProviderOptions` of a `SystemModelMessage`, `UserModelMessage`, `AssistantModelMessage`, `TextContentPart`, `FileContentPart`, or `ToolModelMessage`. The marked content is sent as a text block with `cache_control`, or as a part that carries it. A part-level marker wins over a message-level marker. A message-level marker on a user message marks its last part. On a tool message, set `ProviderOptions` on a result to mark that result, or `MessageProviderOptions` to mark the last result.

```csharp
var system = new SystemModelMessage(
    "You are a helpful assistant. [... long system prompt ...]",
    new Dictionary<string, JsonElement>
    {
        ["alibaba"] = JsonDocument.Parse("{\"cacheControl\":{\"type\":\"ephemeral\"}}").RootElement.Clone(),
    });
```

```csharp
var message = new UserModelMessage(new UserContentPart[]
{
    new TextContentPart("Context: Please analyze this document."),
    new TextContentPart(longDocument)
    {
        ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["alibaba"] = JsonDocument.Parse("{\"cacheControl\":{\"type\":\"ephemeral\"}}").RootElement.Clone(),
        },
    },
});
```

The minimum content length for a cache block is 1,024 tokens. Alibaba allows 4 cache breakpoints per request. A fifth marker adds an `other` warning, `Max breakpoint limit exceeded. Only the last 4 cache markers will take effect.`, and is still sent.

## Cohere sources

Ground a Cohere response in documents by adding text files to the prompt as `FileContentPart` values. When the model cites one of these documents, the provider returns the cited text span as a source.

With `GenerateText`, the cited documents are available in the result sources. With `StreamText`, the same citations arrive as `DocumentSourceStreamPart` values:

```csharp
var model = CohereProvider.Create(new CohereOptions { ApiKey = apiKey }).LanguageModel("command-a-03-2025");
var options = new LanguageModelCallOptions
{
    Prompt = new ModelMessage[]
    {
        new UserModelMessage(new UserContentPart[]
        {
            new TextContentPart("What is the capital of Zephyria?"),
            new FileContentPart("text/plain", null, File.ReadAllBytes("data/zephyria.txt"), "zephyria.txt"),
        }),
    },
};

await foreach (var part in await model.DoStreamAsync(options, cancellationToken))
{
    if (part is DocumentSourceStreamPart source)
    {
        Console.WriteLine(source.Title); // zephyria.txt
        Console.WriteLine(source.ProviderMetadata); // { "cohere": { ... } }
    }
}
```

Each source has the media type `text/plain`, the document title (`Document` when Cohere does not return one), and the citation details in `ProviderMetadata` under `cohere`:

- `start` and `end`: character offsets of the cited text in the response.
- `text`: the cited text.
- `sources`: the raw Cohere sources, including each document's `id`, `text`, and `title`.
- `citationType`: the Cohere citation type, such as `TEXT_CONTENT`.

Keys that Cohere omits are absent from the metadata. Citations that only reference tool output have no document to cite and are not emitted as sources. A response with partial citations, an untitled document, or no `usage` or token counts is accepted. Token counts come from `usage.tokens`. Billed units are a different measure, so they are kept in the raw usage and never reported as token counts.

Set `CohereOptions.GenerateId` to control the ids of the sources (upstream `generateId`).

## Mistral embeddings

`MistralProvider.EmbeddingModel(id)` creates a `MistralEmbeddingModel` (provider id `mistral.embedding`, at most 32 values per call, no parallel calls). It posts to `{base}/embeddings` with `encoding_format` set to `float`. `MistralEmbeddingModelOptions` (maps to `mistralEmbeddingModelOptions`) reads `providerOptions.mistral`: `metadata` is attached to the request, `outputDimension` is sent as `output_dimension` and wins over the `dimensions` call setting, and `outputDtype` (`float`, `int8`, `uint8`, `binary` or `ubinary`) is sent as `output_dtype`. A value of the wrong shape throws `InvalidArgumentException`. The model ids and per-model option support are in `MistralModelCatalog`.

## Mistral Conversations API

`MistralProvider.Conversation(modelId)` creates a `MistralConversationLanguageModel` that calls Mistral's Conversations API (`POST {base}/conversations`, provider id `mistral.conversation`). It works with `GenerateTextAsync` and `StreamTextAsync`, and it adds Mistral's built-in web search. `MistralProvider.LanguageModel(id)` and the default chat model keep using Chat Completions, which has no web search.

### Web search

`MistralTools.WebSearch(name)` (`mistral.web_search`) searches the web. `MistralTools.WebSearchPremium(name)` (`mistral.web_search_premium`) also searches verified news through Mistral's news providers. `name` is the name the tool goes by in the call and its result. It defaults to `web_search` and `web_search_premium`. Mistral runs both tools on the server, so they need no `Execute` function.

```csharp
var provider = MistralProvider.Create();
var result = await AiClient.GenerateTextAsync(new GenerateTextOptions
{
    Model = provider.Conversation("mistral-medium-latest"),
    Tools = new[] { MistralTools.WebSearch() },
    Prompt = "Use web search to find Mistral AI's official website. Cite your source.",
});

Console.WriteLine(result.Text);
Console.WriteLine(result.Sources.Count);
```

Each search comes back as a `GeneratedToolCall` with `ProviderExecuted` set (input `{ "arguments": "<query>" }`, provider metadata `mistral.type`, `mistral.name`, and `mistral.function` when Mistral reports it) and a `GeneratedToolResult` whose result is `{ "info": ... }`. URL citations (`tool_reference` chunks) become `GeneratedSource` parts, once per URL, titled with the URL when Mistral sends no title. A function tool next to a search tool finishes with `FinishReason.ToolCalls`. A stream ends with `FinishReason.Error` when Mistral sends an error event, an event this port cannot read, or no `conversation.response.done`, or when a search never finishes.

The model keeps no history between calls. Pass the previous messages, including the search calls and results, with the next question. A later call replays each search as a function call and result, so it works when the search tool is no longer in `Tools`. `ToolChoice.Required` is sent as `any`, and `ToolChoice.Tool(name)` keeps only that tool and sends `any`.

### Request mapping and options

System messages become `instructions`, joined with a blank line. The sampling settings go in `completion_args` (`max_tokens`, `temperature`, `top_p`, `frequency_penalty`, `presence_penalty`, `stop`, `random_seed`, `reasoning_effort`, `response_format`, `tool_choice`). `TopK` is reported as an `unsupported` warning. The request sets `store` to `false`. Mistral accepts `reasoning_effort` `none` and `high` here, and only on the models listed for the Conversations API, so other models get an `unsupported` warning when `Reasoning` is set. The `mistral` provider options `reasoningEffort`, `structuredOutputs`, and `strictJsonSchema` apply. `safePrompt`, `documentImageLimit`, `documentPageLimit`, `promptCacheKey`, and `parallelToolCalls` are Chat API options: they are not sent and each adds an `unsupported` warning. User images and PDFs are kept as `image_url` and `document_url` parts, and `SupportedUrls` is `application/pdf` for `https` URLs. `OpenAICompatibleOptions.GenerateId` sets the id generator for sources, as upstream `generateId` does.

## Mistral transcription

`MistralProvider.TranscriptionModel(id)` returns a `MistralTranscriptionModel` with provider id `mistral.transcription`. It implements `ITranscriptionModel` and `ITranscriptionCaller`, so it works with `Transcribe`. It posts a multipart request to `{base}/audio/transcriptions`. It also streams live audio; see [Mistral realtime transcription](#mistral-realtime-transcription). For voice agents, see [Mistral voice agent](#mistral-voice-agent).

```csharp
var result = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = (ITranscriptionCaller)mistral.TranscriptionModel("voxtral-mini-transcribe-3-0"),
    Audio = await File.ReadAllBytesAsync("audio.mp3"),
});
```

Read the options from `providerOptions.mistral`. `MistralTranscriptionModelOptions.Parse` validates them and throws `InvalidArgumentException` (`invalid mistral provider options`) for a bad shape or range.

- `language`: the language of the audio, such as `en`.
- `returnLanguage`: return the detected language. It cannot be `true` when `language` is set; the model throws `InvalidArgumentException`.
- `temperature`: the sampling temperature.
- `timestampGranularities`: `segment` and `word`, sent as repeated `timestamp_granularities` fields. For model ids whose catalog entry has `LanguageWithTimestamps` unset (the legacy ids `voxtral-mini-2507`, `voxtral-mini-2602`, and `voxtral-mini-latest`), combining it with `language` throws `InvalidArgumentException`. Other and unknown model ids send the combination to Mistral.
- `diarize`: identify speakers. Mistral validates the timestamp requirement.
- `contextBias`: words, phrases, or descriptions that guide spelling. Entries must not be empty. They are sent as repeated `context_bias` fields, with no entry limit in the SDK.
- `targetStreamingDelayMs`, `speakerReferences`, and `turnDetection` are realtime only. `DoGenerateAsync` does not send them and each adds an `unsupported` warning.

The result carries the text, language, segments, and duration (`prompt_audio_seconds`, else the end of the last segment). `Usage` is the raw Mistral usage. Token and prompt-audio usage is in `ProviderMetadata.mistral.usage`, and segments with a type, score, or speaker id are in `ProviderMetadata.mistral.segments`. `Response.ModelId` is the model id Mistral returns.

### Mistral voice agent

`MistralProvider.ExperimentalRealtime` (upstream `experimental_realtime`) is a `MistralRealtimeFactory`. `Model(id)` (also `MistralProvider.RealtimeModel(id)`) returns a `MistralRealtimeModel` with provider id `mistral.realtime`. It touches no API key. Its `Capabilities` are `turn-based`, transports `webrtc`, connections `provider`, `Startup` `session-update` and `Finalization` `transport-close`. It has no WebSocket, so `BuildUri` and `ConnectAsync` throw, and `SerializeClientEvent` throws `UnsupportedFunctionalityException`.

Call these on the server with the Mistral API key:

```csharp
var session = await mistral.ExperimentalRealtime.CreateSessionAsync("mistral-small-latest", sessionConfig);
// Hand session.Id, session.ParticipantToken and session.ServerUrl to the browser.
await mistral.ExperimentalRealtime.CloseSessionAsync(session.Id);
```

`CreateSessionAsync` posts to `{base}/audio/sessions` and returns a `MistralRealtimeSession` (`Id`, `ParticipantToken`, `ServerUrl`). `CloseSessionAsync` sends `DELETE {base}/audio/sessions/{id}`. An empty or `.`/`..` id throws `InvalidArgumentException` before any request.

`MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig` maps a `RealtimeModelV4SessionConfig` to the Mistral body. `providerOptions.mistral` accepts `stt`, `tts`, `turn`, `welcomeMessage`, `agentId` and `agentVersion`; unknown keys and out-of-range values throw. Input and output audio formats, turn detection, `inputAudioTranscription.prompt`, and `outputModalities` without `audio` throw `UnsupportedFunctionalityException`. With `update: true` only `Instructions`, `Tools` and `providerOptions.mistral.turn` are accepted.

`MistralRealtimeModel` implements `IProviderTransportRealtimeModel`: `CreateTransport(options)` returns a `MistralRealtimeTransport` (upstream `createTransport` and `MistralRealtimeTransport`) and opens nothing until `ConnectAsync`. The transport posts `{ "sessionConfig": ... }` to the application endpoint `Api`, joins the returned LiveKit room, waits for the agent, and deletes the session on `CloseAsync` (`DELETE {api}?sessionId=...`). It maps `transcription.segment`, `session.updated`, `error` and `agent.state.changed` data events to normalized events, answers the agent's `mistral.function_tool` RPC from `function-call-output` items, and sends `session-update` through the `mistral.session_update` RPC.

.NET has no `livekit-client` module, so the host supplies the LiveKit client through `ILiveKitClient` (`CreateRoom`, `CreateLocalAudioTrackAsync`) and the `ILiveKit*` interfaces that mirror the `Room`, participant and track members the transport uses. Pass it as `new MistralRealtimeModel(id, liveKit)`; `MistralProvider.ExperimentalRealtime.Model(id)` creates a model with no client, so its transport throws on connect. A caller-owned `LiveKitMediaStream` is never stopped by the transport. The `Vercel.AI.Mistral.LiveKit` package (`net10.0`, native dependency `Livekit.Rtc.Dotnet`) ships `LiveKitRtcClient`, an `ILiveKitClient` over a real .NET LiveKit client: `new MistralRealtimeModel(id, new LiveKitRtcClient())` or `provider.ExperimentalRealtime.Model(id, liveKit)`. A .NET host has no browser microphone or speaker, so it captures microphone frames into `LiveKitRtcClient.MicrophoneSource` and receives the agent's voice through `LiveKitRtcClientOptions.OnRemoteAudioFrame`. `Vercel.AI` itself keeps no native dependency. The transport contract is `IRealtimeModelV4Transport`, `RealtimeModelV4TransportOptions` and `IProviderTransportRealtimeModel.CreateTransport` (upstream `RealtimeModelV4Transport`, `RealtimeModelV4TransportOptions` and `createTransport`). `RealtimeCapabilities` gains `Startup` and `Finalization`, and `Connections` may contain `provider`.

### Mistral realtime transcription

Pass `voxtral-mini-transcribe-realtime-3` or `voxtral-mini-transcribe-realtime-3-0` to `StreamTranscribe.Start`. The model opens a WebSocket to `{base}/audio/transcriptions/realtime?model={id}` (the base URL upgraded to `ws(s)`). `MistralOptions.WebSocket` (maps to `webSocket`) takes a `WebSocketFactory` for runtimes whose default `ClientWebSocket` cannot send the `Authorization` header. Custom HTTP handlers do not intercept the socket.

```csharp
var mistral = MistralProvider.Create(new MistralOptions { ApiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY") });
var result = StreamTranscribe.Start(new StreamTranscribeRequest
{
    Model = (ITranscriptionCaller)mistral.TranscriptionModel("voxtral-mini-transcribe-realtime-3"),
    // Audio, InputAudioFormat = new AudioFormat("audio/pcm", 24000), ...
});
```

Audio must be raw mono, signed 16-bit little-endian PCM (`audio/pcm`), as bytes or base64. Any other type, or a rate that is not a positive integer, throws `InvalidArgumentException` before connecting. The rate defaults to 16000. The model sends `session.update` after `session.created`, starts audio after `session.updated`, splits chunks into 64 KiB messages, waits while the socket buffer is full, sends `input_audio.end`, and finishes on `transcription.done`. Realtime accepts `language`, `contextBias`, `diarize`, `targetStreamingDelayMs`, `speakerReferences` and `turnDetection`. `returnLanguage`, `temperature` and `timestampGranularities` are not sent and each adds an `unsupported` warning on `stream-start`.

Parts: `transcript-delta` for each text delta, `transcript-final` for each segment (with `providerMetadata.mistral.speakerId` when diarized and no timestamps invented for untimed segments), then `response-metadata` and `finish` with the text, timed segments, language, `prompt_audio_seconds` as the duration, the raw usage, and `providerMetadata.mistral.segments`. Turn events and unknown events appear as `raw` parts when `IncludeRawChunks` is set. A provider `error` event, a socket error, or a close before `transcription.done` fails the stream. Abort, cancel and every failure close the socket and cancel the audio stream.

## Mistral speech

`MistralProvider.SpeechModel(id)` returns a `MistralSpeechModel` with provider id `mistral.speech`. It implements `ISpeechModel` and `ISpeechCaller`, so it works with `GenerateSpeech`. It posts a non-streaming JSON request to `{base}/audio/speech` and decodes the `audio_data` base64 string.

`Voice` maps to `voice_id`. `OutputFormat` accepts the formats the catalog lists (`mp3`, `wav`, `pcm`, `flac`, `opus`) and defaults to `mp3`; any other format adds an `unsupported` warning and sends `mp3`. `Speed` and `Language` always add an `unsupported` warning. `Instructions` adds one when the catalog declares the model without instruction support. Put tags such as `[emotion=happy]` and `[pace=slow]` in the text.

`MistralSpeechModelOptions` reads `providerOptions.mistral`:

- `refAudio`: base64 reference audio for one-off voice cloning. It takes precedence over `Voice`.
- `expressiveness`: a number from 1 to 5.
- `phonemeHints`: a map from words to non-empty lists of IPA phonemes.

`SpeechModelResult.Request.Body` and the `RequestBodyValues` of an `ApiException` replace `ref_audio` with `[redacted]`. Only use reference audio with the speaker's explicit consent.

## Baseten

### DeepSeek V4 Pro structured output

On Baseten Model APIs, `deepseek-ai/DeepSeek-V4-Pro-0813` can return an empty answer when JSON mode and reasoning are combined. When reasoning is enabled (the default), `BasetenProvider` puts JSON instructions and your schema in a system message and omits `response_format`. It emits a `compatibility` warning for `responseFormat` because the server does not constrain the output; the SDK still validates the result. Set reasoning to `none` to use native JSON Schema enforcement instead. Dedicated deployments (`ModelUrl`) are not affected.

## Fal, Google, Google Vertex and Replicate video models

`FalProvider`, `GoogleProvider`, `GoogleVertexProvider`, and `ReplicateProvider` create video models with `VideoModel(id)` (alias `Video(id)`). Each model implements `IVideoCaller` with `DoStartAsync` and `DoStatusAsync`, so pass it as the `Model` of `GenerateVideoRequest`; `GenerateVideo` polls until the video is ready.

| Model | Provider id | `MaxVideosPerCall` | Operation | Provider options |
| --- | --- | --- | --- | --- |
| `FalVideoModel` | `fal.video` | 1 | `{ responseUrl, submitUrl }` | `fal` (`loop`, `motionStrength`, `resolution`, `negativePrompt`, `promptOptimizer`; other keys are sent as is) |
| `GoogleVideoModel` | `google.generative-ai` | 4 | `{ operationName }` | `google` |
| `GoogleVertexVideoModel` | `google.vertex.video` | 4 | `{ operationName }` | `googleVertex`, then `vertex` |
| `ReplicateVideoModel` | `replicate.video` | 1 | `{ getUrl }` | `replicate` (`guidance_scale`, `num_inference_steps`, `motion_bucket_id`, `prompt_optimizer`, and others; unknown keys are sent as is) |

`Seed` is sent whenever it is set, so `0` reaches the provider. Fal and Replicate support webhooks: `HandleWebhookAsync` passes the factory's URL and `Received` task straight through. Fal appends the URL as the `fal_webhook` query parameter. Replicate sends `webhook` with `webhook_events_filter: ["completed"]`. A Fal status call that finds the request still in progress reports `pending`.

## Videos delivered to your storage

A video model can deliver a finished video straight to storage you manage, for example through a signed upload URL. It reports that with a `VideoOutput` of type `uploaded`. `GenerateVideo` does not download it and does not add it to `GenerateVideoResult.Videos`. `GenerateVideoResult.UploadedVideos` counts those videos.

An upload-only call succeeds with an empty `Videos` list. Reading `GenerateVideoResult.Video` on such a result throws `NoVideoGeneratedException`, so use `UploadedVideos` instead. A call that returns neither videos nor uploads still throws `NoVideoGeneratedException`. A result that mixes both has the downloaded videos in `Videos`, and `Video` returns the first of them.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(request);
if (result.Videos.Count == 0)
{
    Console.WriteLine("Videos delivered by the provider: " + result.UploadedVideos);
}
```

An `uploaded` output never carries the signed upload URL. The Gateway video model accepts `uploaded` outcomes in its SSE result and in completed status responses. The [xAI video model](#xai-video-models) returns `uploaded` when you set `output.uploadUrl`. Implement `IVideoCaller` to return `uploaded` from another model.

## Gateway video model

`GatewayProvider.VideoModel(id)` (upstream `GatewayVideoModel`) implements `IVideoCaller` over three routes. `DoGenerateAsync` posts to `/video-model` with `accept: text/event-stream` and reads the first data event, either a `result` or an `error` event. Heartbeat comments are ignored. An `error` event, an empty stream, or an event that does not parse throws a Gateway error. `DoStartAsync` posts to `/video-model/start` and returns the `operation` the Gateway sent. It maps `VideoModelCall.WebhookUrl` onto the Gateway's `callbackUrl` field. `DoStatusAsync` posts `{ operation }` to `/video-model/status`. The Gateway's `cancelled` status surfaces as a terminal `error` with the text `Video generation was cancelled.`. `HandleWebhookAsync` passes the factory's URL and `Received` task straight through. A caller-supplied `idempotency-key` header is forwarded on every call, and the model never invents one.

Videos arrive as `url`, `base64`, or `uploaded` outputs. An `uploaded` output has a media type and no URL or bytes. Byte files in `Image`, `FrameImages`, and `InputReferences` go out as base64, base64 files and URL files pass unchanged, and `ProviderOptions` on a file is kept. `VideoModelFile.FromBase64(base64, mediaType)` creates a file from base64 text, and `VideoModelFile.ProviderOptions` carries per-file options, like `ImageModelFile`. `MaxVideosPerCall` is `int.MaxValue`, so requests are never split client-side.

## Gateway image models

`GatewayProvider.ImageModel(id)` (upstream `GatewayImageModel`) posts the prompt, `n`, `size`, `aspectRatio`, `seed`, files, mask, and provider options to `/image-model`, and `GenerateImage` returns the decoded images. Large images cost memory: by default the whole body is buffered, decoded to text, parsed, and the base64 string is decoded again, which holds about four copies of the image at once.

To reduce peak memory, opt in with the experimental Gateway provider option `experimental_streamingImageDecoding`. Pass it under the `gateway` key of the image call's provider options:

```csharp
var result = await GenerateImage.GenerateImageAsync(
    new GenerateImageRequest
    {
        Model = (IImageCaller)gateway.ImageModel("bfl/flux-kontext-pro"),
        Text = "A mountain lake at sunrise",
        ProviderOptions = JsonDocument
            .Parse("""{ "gateway": { "experimental_streamingImageDecoding": true } }""")
            .RootElement.Clone(),
    }
);
```

The option defaults to `false`, which keeps base64 strings in `ImageModelResult.Images`. When it is `true`, canonical base64 images of at least 64 KiB are decoded while the response arrives, and the model returns them as `byte[]`. If a response mixes extracted bytes with smaller images, every image in that result is returned as `byte[]`. A string that cannot be extracted (base64url, a data URL, whitespace, escape sequences, or a missing pad) keeps the regular decoding path. A response whose `images` are not strings fails with a `GatewayResponseError` that does not carry the image. Validation and parse errors hold a short placeholder such as `[binary data: 2097152 bytes; ref ...]` in place of the extracted data.

The option is handled by the client. The image model removes it from `providerOptions` before the request, drops an empty `gateway` object, and omits `providerOptions` when nothing else is left. Other `gateway` options and other providers' options are sent unchanged.

## HeyGen

The HeyGen provider (`Vercel.AI.HeyGen`) supports `heygen-video-1`, a video generation model with text-to-video, image-to-video, and reference-to-video modes. Generated clips include audio. It also supports [HeyGen Decision](#decision-models) models, which classify, check, and score text, images, and video. HeyGen's avatar, voice, translation, and video editing APIs are not included.

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

Reference-to-video requires at least one image or video. Audio-only conditioning is not supported. The adapter accepts up to 9 image references, 3 video references, and 3 audio references, with a combined maximum of 12. Reference videos share a 15-second input budget across the request. Footage within that budget is used in full; longer references are allocated and trimmed by HeyGen. See the [reference-video rules](https://developers.heygen.com/docs/models/heygen-video#references-and-prompt-labels).

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
- **promptEnhancement** _`turbo` | `quality` | `ultraquality` | `disabled`_: HeyGen's prompt enhancement setting. The API defaults to `quality`.
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
- The SDK accepts positive whole-second durations and forwards them unchanged. HeyGen currently supports 5–30 seconds for text-to-video and image-to-video with `quality` or `turbo` prompt enhancement, and 5–15 seconds for reference-to-video or `ultraquality`/`disabled` enhancement. The default duration is 5. The API enforces these limits; see [duration limits](https://developers.heygen.com/docs/models/heygen-video#duration-limits).
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


### Video model capabilities

| Model | Image input | First frame | Last frame | Reference images | Reference video | Audio | Aspect ratio | Resolution | Duration | FPS | Seed |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `heygen-video-1` | Yes | Yes | No | Yes | Yes | Conditional: Audio is always generated; disabling it is unsupported. | Conditional: Supported outside reference-to-video mode. | Conditional: Supported outside reference-to-video mode. | Yes | Conditional: Fixed at 24 fps. | Yes |

`HeyGenModelCatalog.Entries` maps every known model id (`VideoModelIds` and `DecisionModelIds`) to a `ModelEntry<object>`, the shared catalog contract in `Vercel.AI`. `GetHeyGenVideoModelEntry(modelId)` and `GetHeyGenDecisionModelEntry(modelId)` return the entry of one model, and they accept any model id. `Capabilities` declares what the model supports (`Video`, `Decision`) as `ModelCapability` values. `ProviderOptions.Video` declares per-model support for the options in `HeyGenProviderOptions.Video`, each `Unknown`. The declarations are metadata; they do not validate requests.

### Video provider options

| Operation | Option | Description |
| --- | --- | --- |
| video | `mode` | Inferred from the supplied image and references when omitted. |
| video | `resolution` | Output size class. Takes precedence over the standard resolution option. |
| video | `promptEnhancement` | Defaults to quality. Use disabled to preserve the original prompt. |
| video | `image` | First frame, including an existing HeyGen asset. Cannot be combined with a standard image. |
| video | `referenceImages` | Additional references, appended after standard inputReferences of the same kind. |
| video | `referenceVideos` | referenceVideos |
| video | `referenceAudio` | referenceAudio |

### Decision models

Use `heygen.DecisionModel(modelId)` with `Decide.DecideAsync` to call the [HeyGen Decision API](https://developers.heygen.com/docs/models/heygen-decision) (`POST {BaseUrl}/v3/models/decisions`; provider id `heygen.decision`). This API is experimental and may change in patch releases. `HeyGenProvider` implements `IDecisionProvider`, and `DecisionHeyGenModel` implements `IDecisionCaller`.

```csharp
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = heygen.DecisionModel("heygen-decision-1"),
    State = "Video export fails for everyone on our team after today's update. We have no workaround.",
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["team"] = new("choice", "Which support team should handle this request?", new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?>
        {
            ["video"] = "Video creation, rendering, or export problems",
            ["account"] = "Sign-in, access, or workspace administration",
            ["billing"] = "Payments, invoices, or subscription charges",
        })),
        ["blocked"] = new("boolean", "Is the customer unable to work without a workaround?"),
        ["impact"] = new("score", "How much does this issue affect the customer's work?", new DecisionScoreCriteria(["Cosmetic", "Slower, with a workaround", "Blocked"])),
    },
});

result.Answers["blocked"].Probability; // P(true), from 0 to 1
result.Answers["team"].Choice;         // "video", "account", or "billing"
result.Answers["impact"].Score;        // Expected zero-based level, from 0 to 2
```

`heygen-decision-1` and `heygen-decision-1-turbo` are available. Other model ids are sent unchanged. The model maps a `boolean` question to HeyGen's native `noul` type and returns Choice and Score probability distributions without rounding. A `box` question throws `DecisionUnsupportedQuestionTypeException`. Examples are in `examples/DecideHeyGenBasic` and `examples/DecideHeyGenVideo`.

#### Images and video

Add image and video file parts to the state. The model sends them as HeyGen `assets` and keeps text and JSON parts in the native `state`. HeyGen reads images first, then videos, then the state, each in request order, so describe each input's role in the text by its order. Inline file data is sent as base64. When you call the model directly, an HTTPS URL file is passed to HeyGen as a URL, and a provider reference such as `{ "heygen": assetId }` is sent as an uploaded HeyGen asset. `Decide.DecideAsync` downloads URL file state before it calls the model. A file whose media type is not an image or a video, or an inline text file, throws `UnsupportedFunctionalityException`. A media type without a subtype, such as `image`, is detected from the bytes, and a failed detection throws `InvalidArgumentException`.

#### Confidence, legends, and response metadata

`ProviderMetadata` at `heygen` has `confidence` (Choice and Score confidence, keyed by question id), `legend` (Score index-to-description maps), `answerWarnings` (diagnostic warnings HeyGen returns for an answer), and `requestId`. Confidence is distinct from the selected option's probability. `Response.Id` is the HeyGen decision id, and `Usage` keeps the input and output token counts.

#### Decision limits

The model throws `InvalidArgumentException` before any request when the call goes over these limits:

| Input | Limit |
| --- | --- |
| Questions | 1 to 400. Names are nonempty, without colons, newlines, or surrounding whitespace. |
| Choice options | 1 to 255 |
| Score levels | 1 to 10, all non-null |
| Media | At most 4 images and 1 video. HTTPS URLs only. |

HeyGen enforces its own [media limits](https://developers.heygen.com/docs/models/heygen-decision#media-limits) and [context limits](https://developers.heygen.com/docs/models/heygen-decision#context-limits). `MaxOutputTokens`, `Reasoning`, and every `providerOptions.heygen` entry are not sent. Each becomes an `unsupported` warning. A response with a missing, extra, or invalid answer throws `InvalidResponseDataException` or an `ApiException`. A HeyGen error body `{ error: { code, message, param } }` becomes the message `message (code) [param]`.

`DecisionHeyGenModel.Serialize()` returns a `SerializedModelOptions` with the model id, `baseURL`, and the resolved request headers, including the `x-api-key` header. `DecisionHeyGenModel.Deserialize(options, httpClient)` restores the model. The HTTP client is not serialized.

| Model | Boolean | Choice | Score | Box | Image input | Reasoning | Max output tokens |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `heygen-decision-1` | Yes | Yes | Yes | No | Yes | No | No |
| `heygen-decision-1-turbo` | Yes | Yes | Yes | No | Yes | No | No |

## Liquid

The Liquid provider (`Vercel.AI.Liquid`) supports Liquid's hosted [decision models](https://docs.liquid.ai/lfm/models/decision-models), which classify, check, score, and locate objects in text, images, and audio in a single call with calibrated probabilities. Liquid's OpenAI-compatible chat completions API is not included.

```csharp
using Vercel.AI.Liquid;
using Vercel.AI.OpenAICompatible;

var liquid = LiquidProvider.Create(new OpenAICompatibleOptions { ApiKey = Environment.GetEnvironmentVariable("LIQUID_API_KEY") });
```

The following options are optional:

- **ApiKey** _string_: The key sent as a bearer token in the `Authorization` header. Defaults to the `LIQUID_API_KEY` environment variable. The key is read when a model is used, so a missing key fails then and not when the provider is created.
- **BaseUrl** _string_: The API URL prefix. Defaults to `https://api.liquid.ai`.
- **Headers** _IDictionary&lt;string, string&gt;_: Additional request headers.

`AddLiquid` registers the provider with dependency injection. `LanguageModel`, `EmbeddingModel`, and `ImageModel` throw an `AiSdkException`.

### Decision models

Use `liquid.DecisionModel(modelId)` with `Decide.DecideAsync` to call the [Liquid Decisions API](https://docs.liquid.ai/api/reference/decision-models/make-decisions) (`POST {BaseUrl}/decisions/v1/systemone`; provider id `liquid.decision`). This API is experimental and may change in patch releases. `LiquidProvider` implements `IDecisionProvider`, and `DecisionLiquidModel` implements `IDecisionCaller`.

```csharp
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = liquid.DecisionModel("d1.2"),
    State = "Our production API is returning 500 errors on every request. All customers are affected.",
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["team"] = new("choice", "Which team should handle this ticket?", new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?>
        {
            ["billing"] = "Charges, invoices, refunds, or payment methods",
            ["engineering"] = "Bugs, outages, and technical malfunctions",
            ["account"] = "Sign-in, permissions, or plan changes",
        })),
        ["outage"] = new("boolean", "Is the customer reporting a service outage?"),
        ["urgency"] = new("score", "How urgent is this ticket?", new DecisionScoreCriteria(["Can wait", "Handle today", "Time-sensitive", "Blocking revenue"])),
    },
});

result.Answers["outage"].Probability; // P(true), from 0 to 1
result.Answers["team"].Choice;        // "billing", "engineering", or "account"
result.Answers["urgency"].Score;      // Expected zero-based level, from 0 to 3
```

`d1.2` and `d1.2-ultra` are available. `d1.2-ultra` is the more capable model for harder decisions. Other model ids are sent unchanged. The model maps a `boolean` question to Liquid's native `noul` type and returns Choice and Score probability distributions without rounding. Every question requires instructions. Examples are in `examples/DecideLiquidBasic`, `examples/DecideLiquidImage`, and `examples/DecideLiquidAudio`.

#### Images and audio

Add image and audio file parts to the state. The model sends them in Liquid's top-level `images` and `audio` arrays as base64 data URLs, and keeps text and JSON parts in the native `state`. Liquid reads images and audio before the state, each in request order, so describe each input's role in the text by its order. A state that contains only files is sent as `null`.

Images must be JPEG, PNG, or WebP. Audio must be WAV, MP3 (`audio/mpeg`), FLAC, or OGG. A media type without a subtype, such as `audio`, is detected from the bytes. Liquid does not accept remote URLs, so pass file data inline. `Decide.DecideAsync` downloads URL file state before it calls the model. A file whose media type is not an image or audio, a media type that is not listed, a media type that cannot be detected, a URL file, a provider reference, or an inline text file throws `UnsupportedFunctionalityException`.

A single text part is sent as a string and a single JSON part as structured JSON. Other combinations of text and JSON parts are joined with newlines.

#### Box questions

A `box` question locates an object in a picture and requires exactly one image in the state. Box edges are fractions of the image's height (`Top`, `Bottom`) and width (`Left`, `Right`), measured from its top-left corner: read them from `Answers["id"].Box`.

When Liquid does not find the object (its `found` probability is below 0.5), it returns no box. The model maps this to a `refusal` answer, so `Decide.DecideAsync` throws `DecisionRefusalError` with the question id. The `found` probability is available at `ProviderMetadata.liquid.found`.

#### Confidence, legends, and response metadata

`ProviderMetadata` at `liquid` has `confidence` (Choice, Score, and Box confidence, keyed by question id), `legend` (Score index-to-description maps), `found` (Box found probabilities), and `cost` (in USD, when Liquid reports it). Confidence is distinct from the selected option's probability. `Usage` keeps the input and output token counts. Liquid bills input tokens only, so `OutputTokens` is `0`.

#### Decision limits

The model throws `InvalidArgumentException` before any request when the call goes over these limits:

| Input | Limit |
| --- | --- |
| Questions | 1 to 128, each with instructions |
| Choice options | 2 to 255 |
| Score levels | 2 to 10, all non-null |
| Media | At most 8 images and 8 audio clips |
| Box questions | Exactly one image in the state |

Liquid enforces its own [image requirements](https://docs.liquid.ai/lfm/models/d1#image-requirements) and [audio requirements](https://docs.liquid.ai/lfm/models/d1#audio-requirements). `MaxOutputTokens`, `Reasoning`, and every `providerOptions.liquid` entry are not sent. Each becomes an `unsupported` warning. A response with a missing, extra, or invalid answer throws `InvalidResponseDataException` or an `ApiException`. A Liquid error body `{ error: { message, code } }` becomes the message `message (code)`.

`DecisionLiquidModel.Serialize()` returns a `SerializedModelOptions` with the model id, `baseURL`, and the resolved request headers, including the `Authorization` header. `DecisionLiquidModel.Deserialize(options, httpClient)` restores the model. The HTTP client is not serialized.

`LiquidModelCatalog.Entries` maps every known decision model id (`DecisionModelIds`) to a `ModelEntry<object>`, the shared catalog contract in `Vercel.AI`. `GetLiquidDecisionModelEntry(modelId)` returns the entry of one model and accepts any model id. The declarations are metadata; they do not validate requests.

| Model | Boolean | Choice | Score | Box | Image input | Reasoning | Max output tokens |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `d1.2` | Yes | Yes | Yes | Yes | Yes | No | No |
| `d1.2-ultra` | Yes | Yes | Yes | Yes | Yes | No | No |

Both models also accept audio input.

## Kling AI

The Kling AI provider (`Vercel.AI.KlingAI`) supports Kling AI video generation models: text-to-video, image-to-video, reference-to-video (multi-image), motion control, and multi-shot video generation. It supports video generation only.

### Provider instance

```csharp
using Vercel.AI.KlingAI;

var klingai = KlingAIProvider.Create(new KlingAIOptions { ApiKey = Environment.GetEnvironmentVariable("KLINGAI_API_KEY") });
```

These settings are optional:

- **ApiKey** _Resolvable&lt;string&gt;_: The Kling AI API key, sent as a bearer token. Pass a string, a `Task<string>`, or a callback such as `Resolvable<string>.From(ct => GetKeyAsync(ct))`; a callback is resolved before each request. Defaults to the `KLINGAI_API_KEY` environment variable. Create one in the [Kling AI developer console](https://kling.ai/dev/api-key).
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

## Tencent Cloud

The Tencent Cloud provider (`Vercel.AI.Tencent`) supports two Tencent Cloud services:

- [TokenHub](https://www.tencentcloud.com/products/tokenhub), Tencent's model platform, for language models (such as Hunyuan) over an OpenAI-compatible API and for Kling video models.
- [Video on Demand (VOD)](https://www.tencentcloud.com/products/vod) AIGC, for video models such as Seedance (`VS`), Kling, Hailuo, and Vidu.

The two services use different credentials. TokenHub uses an API key. VOD uses a Tencent Cloud API key pair (SecretId and SecretKey) and a VOD application ID.

```csharp
using Vercel.AI.Tencent;

var tencent = TencentProvider.Create(new TencentOptions { ApiKey = Environment.GetEnvironmentVariable("TENCENT_TOKENHUB_API_KEY") });
```

`TencentOptions` extends `OpenAICompatibleOptions`. The settings are:

- **ApiKey** _string_: The TokenHub API key (`sk-...`), used by language models and TokenHub Kling video models. Defaults to the `TENCENT_TOKENHUB_API_KEY` environment variable.
- **BaseUrl** _string_: The TokenHub base URL. Defaults to `https://tokenhub-intl.tencentcloudmaas.com/v1`.
- **SecretId** _string_: The Tencent Cloud API SecretId, used by VOD video models. Defaults to the `TENCENTCLOUD_SECRET_ID` environment variable.
- **SecretKey** _string_: The Tencent Cloud API SecretKey, used by VOD video models. Defaults to the `TENCENTCLOUD_SECRET_KEY` environment variable.
- **SessionToken** _string_: The session token for temporary Tencent Cloud credentials, sent as `X-TC-Token`. Not needed with a long-term key pair. Defaults to the `TENCENTCLOUD_SESSION_TOKEN` environment variable.
- **SubAppId** _string_: The VOD application ID (`SubAppId`), used by VOD video models. It must be a positive integer. Defaults to the `TENCENTCLOUD_VOD_SUB_APP_ID` environment variable.
- **Region** _string_: The Tencent Cloud region sent as `X-TC-Region` on VOD requests. VOD AIGC requests do not require a region.
- **VodBaseUrl** _string_: The VOD API base URL. Defaults to `https://vod.intl.tencentcloudapi.com`.
- **Headers** _Dictionary&lt;string, string&gt;_: Custom headers to include in the requests.

`AddTencent` registers the provider with dependency injection. `EmbeddingModel` and `ImageModel` throw a `NoSuchModelError`. The credentials are read when a request is sent, so a missing key fails then and not when the provider is created. TokenHub-only use needs no VOD credentials.

### Language models

`tencent.LanguageModel("hy4")` or `tencent.Chat("hy4")` creates a TokenHub language model (provider id `tencent.chat`). It uses the OpenAI-compatible chat completions API and always asks for usage. Options under `ProviderOptions["tencent"]` that the OpenAI-compatible provider does not handle, such as `thinking`, are sent in the request body unchanged. See the [TokenHub model list](https://www.tencentcloud.com/document/product/1300/78934) for available models.

The model is a `TencentChatLanguageModel`. To pass it across a workflow boundary, call `Serialize()` and persist the result as JSON. Restore it with `TencentChatLanguageModel.Deserialize(serialized, apiKey)`. The TokenHub endpoint (`BaseUrl`) and non-sensitive headers are kept. The API key is not serialized, so supply it again when you restore the model, or set `TENCENT_TOKENHUB_API_KEY`.

### Video models

Create a video model with `tencent.VideoModel(id)` or its alias `tencent.Video(id)` (provider id `tencent.video`). `TencentVideoModel` implements `IVideoCaller`, so pass it as the `Model` of `GenerateVideoRequest`.

- TokenHub Kling models use their TokenHub model id, for example `kling-video-v3`.
- VOD AIGC models use `vod/<ModelName>/<ModelVersion>`, for example `vod/VS/2.0` or `vod/Kling/3.0`.

Any other id throws a `NoSuchModelError` when the model is created. Video generation is asynchronous: the model submits a task and `GenerateVideo` polls for the result. Tencent video tasks do not accept a per-request callback URL, so `WebhookUrl` is ignored with a warning and `CanWebhook` is false.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)tencent.Video("kling-video-v3"),
    Prompt = new VideoPrompt("A chicken flying into the sunset in the style of 90s anime."),
    AspectRatio = "16:9",
    Duration = 5,
});
```

#### TokenHub Kling

A TokenHub Kling model picks the endpoint from the inputs:

- No image: text-to-video.
- A first frame (`Image` or `FrameImages`), optionally with a last frame: image-to-video.
- Reference images or videos (`InputReferences`) or voices: omni video generation. `kling-video-v3-omni` always uses omni generation. `kling-video-v4` uses it when references are present.
- `kling-video-motion-control-*` models: motion control, with the character image from `Image` (or the first image reference) and the motion from the first video reference.

Reference images and videos are numbered in the order you pass them and can be referenced in the prompt as `@image_1`, `@video_1`, and so on.

Top-level options map as follows:

- `Resolution` maps to the `720p`, `1080p`, and `4k` tiers by its shorter side.
- `AspectRatio` applies to text-to-video and omni generation. Image-to-video and motion control output follows the input media.
- `Duration` applies to all modes except motion control, whose output length follows the motion video.
- `GenerateAudio` maps to native audio or no audio. Without it, `kling-video-v3` output is silent. For motion control, `false` removes the motion video's audio, and otherwise the original audio is kept. `kling-video-v3-turbo` and `kling-video-v4` do not accept an audio setting and always include audio.
- `Seed`, `Fps`, and `N` greater than 1 are not supported.

Unsupported options are ignored with an `Unsupported` warning.

For multi-shot output, set `multiShot: true` and describe each shot in the prompt as `shot <n>, <seconds>, <prompt>;`. The shot durations must add up to `Duration`:

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)tencent.Video("kling-video-v3"),
    Prompt = new VideoPrompt("shot 1, 3, a wide shot of a lighthouse at dawn; shot 2, 3, a close-up of the lighthouse lamp turning;"),
    Duration = 6,
    ProviderOptions = OperationJson.Parse("""{ "tencent": { "multiShot": true } }"""),
});
```

A response with a non-zero `code` is an error even when the HTTP status is 2xx. It throws an `ApiException` that is not retryable.

#### VOD

VOD models use the `CreateAigcVideoTask` API. Seedance is available as `vod/VS/2.5`, `vod/VS/2.0`, `vod/VS/2.0-fast`, and `vod/VS/2.0-mini`.

Inputs map to VOD file inputs: `Image` or a first frame becomes the first frame, a last frame becomes the last frame, and `InputReferences` become references whose category (image, video, or audio) follows their media type. Inputs are sent as URLs or Base64 data. Base64 inputs must stay under the VOD request size limit.

Top-level options map as follows:

- `Resolution` maps to the `480P`, `720P`, `1080P`, `2K`, and `4K` tiers by its shorter side.
- `AspectRatio` also accepts `adaptive`, which follows the input image.
- `Duration` and `Seed` are passed through. Seedance does not guarantee identical output for the same seed.
- `GenerateAudio` maps to `AudioGeneration`. Seedance includes audio when `GenerateAudio` is not set.
- `Fps` and `N` greater than 1 are not supported.

Images that show real people may be rejected unless they are first registered as VOD assets (`CreateAigcMaterial`) in your Tencent Cloud account. Pass registered assets by their `asset://` URL with the `assets` provider option. `InputReferences` only accepts `http(s)` URLs and file data.

VOD requests are signed with TC3-HMAC-SHA256. VOD reports API errors inside a successful HTTP response. The model raises them as an `ApiException` (a subclass for the status, such as `BadRequestException`) with a status code derived from the Tencent Cloud error code. For example, `AuthFailure` becomes 401, `InvalidParameterValue` becomes 400, and `RequestLimitExceeded` becomes 429 and is retryable. `TencentCloudApi` exposes `Sha256Hex`, `BuildTc3CanonicalRequest`, `CreateTc3Authorization`, and `GetTencentCloudErrorStatusCode`.

#### Video provider options

Set the options under the `tencent` key of `ProviderOptions`. `TencentVideoModelOptions.Parse` reads them, and an option of the wrong type or with an unknown enum value throws an `InvalidArgumentException`. Options that do not apply to the selected model are ignored with an `Unsupported` warning.

Options for both services:

- **resolution** _string_: The output resolution tier, sent as is. Takes precedence over the top-level `Resolution`.
- **watermark** _boolean_: Whether to add a visible label that marks the video as AI-generated.
- **referenceVideoType** _`feature` | `base`_: How reference videos are used: as a feature reference (default) or as a video to edit.

TokenHub Kling options:

- **audio** _`native` | `original` | `off`_: The audio mode. `original` keeps the audio of a reference video. Takes precedence over `GenerateAudio`.
- **multiShot** _boolean_: Whether a multi-shot prompt produces multi-shot output (`kling-video-v3` and `kling-video-v3-omni`).
- **characterOrientation** _`image` | `video`_: Motion control only. Whether the character orientation follows the image or the motion video. Required by motion control models.
- **elements** _array of `{ elementId, id? }`_: Custom elements created through the TokenHub element API. Motion control requires `id`.
- **voices** _array of `{ voiceId, id }`_: Omni voices, referenced in the prompt as `@id`.
- **externalTaskId** _string_: A custom task id, unique within the account.

VOD options:

- **negativePrompt** _string_: What the model should avoid.
- **enhancePrompt** _boolean_: Whether to optimize the prompt automatically.
- **inputRegion** _`Oversea` | `OverseaUSWest` | `Mainland`_: Where input files are stored. Tencent defaults to `Mainland`.
- **sceneType** _string_: The scenario type, for example `motion_control` for Kling.
- **personGeneration** _`AllowAdult` | `Disallowed`_: Whether people and faces may be generated.
- **inputComplianceCheck**, **outputComplianceCheck** _boolean_: Whether to run compliance checks on the input or output.
- **enhanceSwitch** _boolean_: Whether to upscale output with video enhancement.
- **offPeak** _boolean_: Whether to run the task in off-peak mode.
- **sessionId**, **sessionContext** _string_: A deduplication identifier and pass-through context.
- **tasksPriority** _integer_: The task priority from -10 to 10.
- **extInfo** _string_: A reserved field for model-specific parameters, sent as is. For example, `{"AdditionalParameters":"{\"return_last_frame\":true}"}` asks Seedance to also return the last frame as an image, reported in `providerMetadata.tencent.lastFrameUrl`.
- **assets** _array of `{ url, usage?, category? }`_: Registered VOD assets, referenced by their `asset://` URL. `usage` is `FirstFrame`, `LastFrame`, or `Reference` (the default). `category` is `Image` (the default), `Video`, or `Audio`. Assets are sent after the other file inputs. VOD accepts one first frame and one last frame, so a `FirstFrame` or `LastFrame` asset is ignored with a warning when that frame is already set.

#### Provider metadata

A completed video result has `providerMetadata.tencent` with the `taskId`, the `videos` (URL, duration, and `fileId` when VOD stores the file), and usage: `usage.totalTokens` for TokenHub Kling models, and for VOD models `usage.outputSeconds`, `usage.inputSeconds` (reference video length), `usage.totalSeconds`, and `usage.specificationUsages` (the billed specification, such as `SV2.0_720P`). VOD results also have the resolved output `resolution` (for example `720P`) and, when requested, the `lastFrameUrl`.

#### Model ids

Language models: `hy4`, `hy4-preview`, `hy3`, `hy-mt2-pro`, `hy-mt2-plus`, `hy-mt2-lite`, `deepseek-v4-pro-0813`, `deepseek-v4.1-flash`, `glm-5.3`, `glm-5.3-flash`, `glm-5.2`, `mimo-v2.6-pro`, `mimo-v2.6-flash`, `step-5-preview`. Other ids are accepted.

Video models (`TencentModelCatalog.TencentVideoModelIds`): `kling-video-v4`, `kling-video-v3`, `kling-video-v3-turbo`, `kling-video-v3-omni`, `kling-video-motion-control-v3.0`, `kling-video-motion-control-v2.6`, `vod/VS/2.5`, `vod/VS/2.0`, `vod/VS/2.0-fast`, `vod/VS/2.0-mini`, `vod/Kling/3.0`, `vod/Kling/3.0-Omni`, `vod/Kling/3.0-turbo`, `vod/Kling/2.6`, `vod/Hailuo/2.3`, `vod/Vidu/q3-pro`. Any id that starts with `kling-video-` or has the form `vod/<ModelName>/<ModelVersion>` is accepted.

#### Model catalog

`TencentModelCatalog.Entries` maps every known model id to a `ModelEntry<TencentRequestConfig>`, the shared catalog contract in `Vercel.AI`. `Capabilities` declares what the model supports by operation (`Language`, `Video`), each value a `ModelCapability` (`Supported`, `Unsupported`, `Unknown`, or `When(condition)`). `ProviderOptions` declares per-model support for the options in `TencentProviderOptions`, and `RequestConfig.Video` holds the request-shaping policy. A null section is undeclared, not unsupported. The declarations are metadata; they do not validate requests.

## Black Forest Labs

The Black Forest Labs provider (`Vercel.AI.BlackForestLabs`) generates images with `ImageModel(id)` or `Image(id)` (provider id `black-forest-labs.image`) and videos with `VideoModel(id)` or `Video(id)` (provider id `black-forest-labs.video`, see [Black Forest Labs video models](#black-forest-labs-video-models)). It reads `BFL_API_KEY` and sends it as `x-key`. `LanguageModel` and `EmbeddingModel` throw `NoSuchModelError`.

The model submits the request, polls `polling_url` (with `?id=` added when missing) until the task is `Ready`, then downloads the sample. `BlackForestLabsProvider.Create(options, handler, pollIntervalMillis, pollTimeoutMillis)` sets the poll interval (default 500 ms for images, 2 s for video) and the overall poll timeout (default 60 s for images, 10 minutes for video) for every image and video model. The `pollIntervalMillis` and `pollTimeoutMillis` provider options override them for one call. A timeout throws `AiSdkException` ("Black Forest Labs generation timed out."). A task that ends as `Error` or `Failed`, or is `Request Moderated`, `Content Moderated`, or `Task not found`, throws `AiSdkException`. An HTTP error throws `ApiException` with the `detail` (or `message`) of the body; an HTTP 503 whose body carries a terminal `status` or `state` is not retryable.

Credentials go to the polling URL only when it is on the provider origin or an https host under `bfl.ai` (for example `api.us1.bfl.ai`). The download sends no credentials, except that a custom base URL outside `bfl.ai` receives the provider and call headers when the sample is on the same origin. Every other hop carries only the SDK `User-Agent` header. Other hosts are checked first (private addresses and non-http(s) schemes throw `DownloadException`).

`BlackForestLabsImageModel.SupportsFileInputs` and `SupportsMaskInputs` report what the model id accepts (`null` when unknown). The model id picks the API: ids such as `flux-pro-1.1`, `flux-kontext-pro`, `flux-pro-1.0-fill`, `flux-1.*`, and `flux-2-*` use the legacy API. `flux-3-image` uses the FLUX 3 API. Any other id (for example `flux-3-image-klein`) uses the latest API.

- Legacy: `AspectRatio` wins over `Size`. A `Size` adds an `unsupported` `size` warning, and its width and height are sent unless the `width` and `height` provider options are set. Files are sent as `input_image`, `input_image_2`, and so on (`image` for `flux-pro-1.0-fill`), and the mask as `mask`.
- FLUX 3 and latest: files are sent in `images` (URLs as is, bytes as base64), at most 10 (more throws `AiSdkException`). A mask throws `UnsupportedFunctionalityException`. The aspect ratio comes from `AspectRatio` or from `Size`; `7:3` and `3:7` are sent as `21:9` and `9:21`, and a ratio the endpoint does not take is left out with an `unsupported` `aspectRatio` warning. FLUX 3 adds an `unsupported` `seed` warning for a `Seed`. The latest API sends `Seed`, and its `Size` becomes `width` and `height` (the `width` and `height` provider options win); with dimensions set, no aspect ratio or resolution is sent.
- Provider options are read from `blackForestLabs` and validated before the request (`InvalidArgumentException`). An option the model does not support is dropped with an `unsupported` `blackForestLabs.<name>` warning. Ranges: `safetyTolerance` 0 to 6 (legacy) or 0 to 7, `width` and `height` 256 to 1920 (legacy) or 64 to 6256 together with at most 16,822,272 pixels (latest), `steps` 1 to 16 (latest), and `resolution` one of `768sq`, `1k`, `1.5k`, `2k`, `4k`.
- The `webhookUrl` option (legacy models) is sent as `webhook_url`, with `webhookSecret` as `webhook_secret`. When it is set, Black Forest Labs answers the submit request without a `polling_url`. The webhook is delivered as configured, and the call still waits for completion (bounded by `pollTimeoutMillis`) by polling the `get_result` endpoint of the configured base URL by request id. This fallback is best effort: the task must become visible on `get_result` before the poll timeout. An HTTP 404 or `Task not found` answer is treated as pending for up to 20 attempts (about 10 s at the default interval). A task that never appears throws `AiSdkException` ("Black Forest Labs task <id> was not found via get_result before timing out"). A 404 whose body is not `Task not found` is not retried.

`GenerateImage` returns `providerMetadata.blackForestLabs.images[0]` with `seed`, `start_time`, `end_time`, `duration`, `cost`, `inputMegapixels`, and `outputMegapixels` when the API sent them, and the response `Timestamp`, `ModelId`, and `Headers` of the download.

## Black Forest Labs video models

`BlackForestLabsProvider.VideoModel(id)` (also `Video(id)`) returns a `BlackForestLabsVideoModel` for FLUX 3 video (`flux-3-video`). It generates one video per call (`MaxVideosPerCall` is 1) and supports `DoGenerateAsync`, `DoStartAsync`, and `DoStatusAsync`. The video is returned as a signed MP4 URL (`VideoOutput` of type `url`, media type `video/mp4`) and is not downloaded. The URL expires, so keep the bytes you need. It is also in `providerMetadata.blackForestLabs.videos[0].videoUrl`, next to `id`, `seed`, `start_time`, `end_time`, `duration`, `draftCache`, `cost`, `inputMegapixels`, and `outputMegapixels` when the API sent them. `cost` is the settled cost of the poll response when it has one, else the estimate of the submit response. The API has no webhook input, so `CanWebhook` is false.

`DoStartAsync` submits the job and returns an operation: a JSON object with `requestId`, `pollingUrl`, and the `cost`, `inputMegapixels`, and `outputMegapixels` of the submit response. `DoStatusAsync` reads it once and returns `pending`, `completed`, or `error` (a terminal status of `Error`, `Failed`, `Request Moderated`, `Content Moderated`, or `Task not found`, with the `details` of the poll in the message). The polling URL gets `?id=` when it has none, and the same trust rules as the image model apply to the credentials. `DoGenerateAsync` starts a job and polls it every `PollIntervalMillis` (default 2 s). It throws `AiSdkException` when the job fails ("Black Forest Labs video generation failed with status ...") or when `PollTimeoutMillis` (default 10 minutes) passes ("Black Forest Labs video generation timed out after ...ms").

- Modes: `mode` comes from the inputs. A prompt alone is text to video (`t2v`). An `Image` or a `first_frame` frame image is image to video (`i2v`); a `last_frame` closes the clip, and a `last_frame` without a `first_frame` is dropped with a warning. A video in `InputReferences` continues from that video (`v2v`; only the first is used). Keyframes and a video to continue from exclude each other, and keyframes win. FLUX 3 has no reference-image input, so an image in `InputReferences` is ignored with a warning. A reference with no media type is treated as the video to continue from, with a `compatibility` warning.
- `Duration` is a whole number of seconds from 5 to 20. A fraction is rounded and an out-of-range value is clamped, each with a warning. Without a `Duration` the API picks one.
- `Resolution` is `hd` or `fhd`, or `{width}x{height}`, which maps onto a tier by the shorter side (720 or less is `hd`, else `fhd`; a `compatibility` warning when the side is not 720 or 1080). The `resolution` provider option wins. `AspectRatio` takes `21:9`, `2:1`, `16:9`, `4:3`, `1:1`, `3:4`, or `9:16`; the `aspectRatio` provider option also takes `auto`, and wins. Other values are dropped with a warning.
- `GenerateAudio` is sent as `generate_audio` only when set. `Fps` and `Seed` are not supported and add warnings, and so does an `N` above 1.
- Provider options are read from `blackForestLabs` and validated before the request (`InvalidArgumentException`): `resolution`, `aspectRatio`, `keyframes`, `safetyTolerance` (0 to 4), `draft`, `draftCache`, and `version` (only `latest`). `keyframes` holds 1 to 10 image URLs or base64 strings, or `[seconds, image]` pairs in chronological order (0 to 20 seconds). It wins over `Image` and `FrameImages`, with a warning. Three or more untimed keyframes need a `Duration` (`InvalidArgumentException` otherwise).
- Draft mode: `draft` renders a fast preview and reports `draftCache` in the provider metadata. Passing that URL or its base64 bundle as `draftCache` replays it at full quality. The bundle pins everything else, so the request sends only `mode`, `draft_cache`, and `safety_tolerance`, and every other input is reported as an `unsupported` warning.

## fal image models

`FalImageModel.SupportsFileInputs` and `SupportsMaskInputs` report what the model id accepts. `SupportsFileInputs` is `true` for the known image-to-image and edit models (for example `fal-ai/flux-2/edit` and `fal-ai/flux-pro/kontext`), `false` for known text-to-image models (for example `fal-ai/recraft/v3/text-to-image`), and `null` for any other id. `SupportsMaskInputs` is `true` for `fal-ai/flux-general/inpainting` and `fal-ai/flux-lora/inpainting`, `false` for the other known ids, and `null` when `SupportsFileInputs` is `null`.

## Replicate image models

### Cancellation

A prediction keeps running, and billing, on Replicate until it reaches a terminal status. When an image generation call is abandoned after its prediction has been created, `ReplicateImageModel` stops waiting and sends a best-effort cancel request to `POST /predictions/{prediction_id}/cancel`:

- the call's `CancellationToken` is cancelled while the model polls, or
- polling gives up because `maxPollAttempts` was reached.

Cancellation applies only while the model polls for a result, which happens after the create request has returned. It is not attempted during the initial create request, because no prediction ID is available yet. The cancel URL is built from the configured `BaseUrl` (never from the prediction response), and the request carries only the provider's configured headers. It is not retried, does not receive the cancelled token, and failures are ignored, so it never replaces the cancellation or timeout error.

## MiniMax

`MiniMaxProvider` (`Vercel.AI.MiniMax`) speaks MiniMax's Anthropic-compatible protocol, so the default base URL is `https://api.minimax.io/anthropic/v1`, not the OpenAI-compatible `https://api.minimax.io/v1`. Model calls post to `{base}/messages` with the provider id `minimax.messages`, which makes `minimax` the provider-options namespace. `MiniMaxOptions` sets `ApiKey` (default: the `MINIMAX_API_KEY` environment variable, sent in the `x-api-key` header; a `Resolvable<string>`, so a callback is resolved before each request), `BaseUrl`, and `Headers`. `LanguageModel(id)` and `Chat(id)` return a `MiniMaxChatLanguageModel`. Embedding models throw `AiSdkException`. `Video(id)` and `VideoModel(id)` create a `MiniMaxVideoModel`; see [Video models](#video-models). `Image(id)` and `ImageModel(id)` create a `MiniMaxImageModel`; see [Image models](#image-models).

```csharp
using Vercel.AI.MiniMax;

var minimax = MiniMaxProvider.Create(new MiniMaxOptions { ApiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") });
var model = minimax.LanguageModel("minimax-m3.1-flash");
```

A model id from M3.1 on gets the extra capabilities below: `minimax-m3.1-flash`, `MiniMax-M3.1-Flash-Preview`, `minimax-m3.2`, `minimax-m4`, and so on. The version is compared by its numbers, so `m3.10` is newer than `m3.1`. Older and unrecognized ids (`minimax-m3`, `minimax-m2.7`, `custom-model`) keep the plain Anthropic adapter behavior.

### Sampling

For M3.1 and later ids, `Temperature` accepts `0` to `2` and `TopP` accepts `0` to `1`. Both are kept when adaptive thinking is on or when you set `Reasoning`. A temperature outside the range is clamped with an `unsupported` `temperature` warning that names `minimax`. Omit them to use MiniMax's defaults. Older ids clamp the temperature to `1` and drop `Temperature` and `TopP` when thinking is on.

### Prompt caching

For M3.1 and later ids, mark stable text with `minimax.cacheControl`. It becomes `cache_control` on the matching content block. Set it on a system message, a text part, or a user message (which marks its final content block).

`providerOptions.minimax.serviceTier` (`standard` or `priority`) is sent as `service_tier`. Priority admission is billed at 1.5x the standard price. Omit it to use the API's default tier. Any other value (including `null`) throws `InvalidArgumentException` before a request is sent. A tier that only Anthropic accepts (`auto`, `standard_only`), set under `providerOptions.anthropic`, is omitted with an `unsupported` `serviceTier` warning, and the `minimax` value wins over the `anthropic` one. The tier the API applied comes back as `ProviderMetadata.minimax.serviceTier` when `providerOptions.minimax` was supplied, and always as `ProviderMetadata.anthropic.serviceTier`. A response without a tier leaves it unset; the requested tier is not inferred.

```csharp
var part = new TextContentPart("Long reference material reused across requests...")
{
    ProviderOptions = new Dictionary<string, JsonElement>
    {
        ["minimax"] = JsonDocument.Parse("{\"cacheControl\":{\"type\":\"ephemeral\"}}").RootElement.Clone(),
    },
};
```

Keep the stable prefix unchanged and put the changing question after it. A marker does not guarantee a hit; check the cache-read token usage. `anthropic.cacheControl` still works. When both namespaces set it on a block, `minimax` wins. At most four blocks are cached; the rest add a `cacheControl breakpoint limit` warning. Older ids read only the `anthropic` namespace.

### Image and video input

For M3.1 and later ids, user messages accept images and videos, with `GenerateText` and `StreamText`. Pass bytes or a public http(s) URL; the URL is sent to MiniMax as is. Set the media options on each file part under `minimax`:

- **fps** _number_ (video): the frame sampling rate, from `0.2` to `5`. Omit it for the server default. A value outside the range, or a non-number, throws `InvalidArgumentException` before the request.
- **detail** _string_ (image): `low`, `default`, or `high`. Higher detail can use more tokens. Any other value throws `InvalidArgumentException`.

For a video you already uploaded to MiniMax, use a provider reference. `FileContentPart.FromProviderReference("video/mp4", new Dictionary<string, string> { ["minimax"] = "your-uploaded-file-id" })` is sent as `mm_file://your-uploaded-file-id`; an id that already starts with `mm_file://` is kept. A reference without a `minimax` entry throws `NoSuchProviderReferenceError`, and an image reference throws `UnsupportedFunctionalityException`. The provider does not upload the file. `MiniMaxChatLanguageModel.SupportedUrls` lists `image/*` and `video/*` for M3.1 and later ids, and nothing for older ids, which reject video with `UnsupportedFunctionalityException`.

MiniMax documents JPEG, PNG, GIF, and WebP images (up to 10 MB), and MP4, AVI, MOV, and MKV videos (up to 50 MB inline or by URL, 512 MB uploaded). The request body limit is 64 MB, including base64 overhead. Audio input is not supported.

### Reasoning

Pass thinking and effort under the `minimax` key of `ProviderOptions`:

- **thinking** _object_: `{ "type": "adaptive" }` lets the model decide when to reason. `{ "type": "disabled" }` answers directly. Thinking control works on `minimax-m3`. The M2.x models always think, and `MiniMax-M3.1-Flash` always thinks and returns an error for `disabled`.
- **effort** _string_: `low`, `medium`, `high`, `xhigh`, or `max`, for M3.1 and later ids. It sets `output_config.effort`. When omitted, the server picks its default (`max` for M3.1 Flash).

The output token budget covers the reasoning and the answer, so higher effort may need a larger `MaxOutputTokens`.

For M3.1 and later ids, `LanguageModelCallOptions.Reasoning` maps to the effort instead of an Anthropic thinking budget: `low`, `medium`, `high`, `xhigh`, and `max` map to the same effort, `minimal` maps to `low` with a `compatibility` warning, and `none` also maps to `low` with a `compatibility` `reasoning` warning, because Flash always reasons. `provider-default` or no `Reasoning` leaves the server default. An explicit `minimax.effort` or `anthropic.effort` wins over `Reasoning`. Older ids keep the Anthropic mapping.

`MiniMaxChatLanguageModel.Serialize()` returns the model id and config for a workflow boundary (provider id, base URL, and headers). It leaves out the API key and any header whose name contains `authorization`, `key`, `token`, `secret`, or `cookie`. `MiniMaxChatLanguageModel.Deserialize(serialized, apiKey)` restores the model; without `apiKey` it reads `MINIMAX_API_KEY`.

### Video models

`MiniMaxProvider.Video(id)` and `VideoModel(id)` return a `MiniMaxVideoModel` with the provider id `minimax.video`. Video calls use their own base URL: `MiniMaxOptions.VideoBaseUrl`, default `https://api.minimax.io`. It has no version suffix and is never derived from `BaseUrl`. A trailing slash is removed. Video calls send `Authorization: Bearer <key>` (the same `ApiKey` or `MINIMAX_API_KEY`), your `Headers`, and the user agent `ai-sdk-minimax/<version>`. They do not send `x-api-key` or `anthropic-version`. The model uses the `HttpClient` you gave the provider.

`MiniMaxVideoModelOptions` lists the provider options under `minimax`: **resolution** (`480P`, `768P`, `2K`), **ratio** (`adaptive`, `21:9`, `16:9`, `4:3`, `1:1`, `3:4`, `9:16`), **referenceAudioUrls**, **aigcWatermark**, **pollIntervalMs** (default 10000), and **pollTimeoutMs** (default 600000).

`DoStartAsync` posts to `{VideoBaseUrl}/v2/video_generation`; `DoStatusAsync` reads `/v2/query/video_generation/{taskId}`; `DoGenerateAsync` starts a task and polls it. A webhook URL is forwarded as `callback_url`, and `HandleWebhookAsync` is not supported. The default resolution is `2K` for `MiniMax-H3` (it serves `768P` and `2K`) and `768P` for `MiniMax-H3-Max` (`480P` and `768P`). A top-level `Resolution` that is a named tier (`480P`, `768P`, `2K`, any case) or one of the listed frame sizes maps to its tier. The provider option wins when both are set. The ratio defaults to `16:9` for text-to-video and follows the frame image otherwise. The duration is rounded to a whole number and clamped to 4 to 15 seconds (5 to 15 for `MiniMax-H3-Max`). Each of these adjustments adds an `unsupported` warning.

Frame images (`first_frame`, optional `last_frame`) and reference inputs are exclusive; frame images win. References take at most 9 images, 3 videos, and 3 audio URLs (`referenceAudioUrls`, which need at least one image or video). A reference with no media type is treated as an image with a warning, and `MiniMax-H3-Max` ignores references. `fps`, `seed`, `N` above 1, and `GenerateAudio` are ignored with warnings. `ProviderMetadata.minimax.resolvedInputs` reports what was sent: `imageCount` and the `referenceVideoUrls` that were URLs. The status call reads the task through the provider origin and validates every redirect hop: a hop to another origin must pass the download URL check and carries only the user agent, not your `Authorization` or other headers. A task that is `queued`, `running`, or has an unknown status is pending; `DoStatusAsync` returns an `error` result for a `failed`, `cancelled`, or `expired` task (`MiniMax video generation failed: <message> (<code>). Task ID: <id>`), and `DoGenerateAsync` throws an `AiSdkException` with the same message. A succeeded task with no video URL, a create response with no `task_id`, and a poll that runs past `pollTimeoutMs` also throw. A failed HTTP call throws the matching `ApiException` with MiniMax's `error.message` (or `MiniMax video generation error` when the body has none) and the request `Url`. Cancelling the token stops the call and the polling loop with an `OperationCanceledException`.

### Image models

`MiniMaxProvider.Image(id)` and `ImageModel(id)` return a `MiniMaxImageModel` with the provider id `minimax.image`, for `MiniMax-H3-Image` and `MiniMax-H3-Image-Highspeed`. Image calls use their own base URL: `MiniMaxOptions.ImageBaseUrl`, default `https://api.minimax.io/v1`. A trailing slash is removed. They send the same headers as video calls (`Authorization: Bearer <key>`, your `Headers`, and the user agent `ai-sdk-minimax/<version>`).

Without input images the model posts to `{ImageBaseUrl}/images/generations`. With `Files` it posts to `/images/edits` and sends each file as a data URI, or as the URL for a URL file. It asks MiniMax for URL output and downloads each image; the download carries no API key. One call returns up to 10 images (`MaxImagesPerCall`). The model accepts input images (`SupportsFileInputs`) but not masks (`SupportsMaskInputs`): a `Mask` throws `UnsupportedFunctionalityException` before any request. `Seed` is not supported and adds an `unsupported` `seed` warning.

`MiniMaxImageModelOptions` lists the provider options under `minimax`: **area** (`1K`, `2K`, `4K`; use it without a pixel size, default 4K), **outputFormat** (`jpeg`, `jpg`, `png`, `webp`; default JPEG), **outputCompression** (0 to 100, for JPEG and WebP), and **disablePromptEnhanced** (text-to-image only; an edit ignores it and adds an `unsupported` warning).

If MiniMax fails one image of a batch, the other images are returned and a warning `Image N failed: <message>` names the failed one. Usage is returned as the call usage, and `providerMetadata.minimax.images` has one entry per returned image with `size`, `revisedPrompt`, and, when MiniMax reports them, `outputFormat` and `requestId`. The first entry also carries the raw request `usage`. A provider error keeps MiniMax's message.

## Async API keys for Fal, Replicate, Fish Audio, Cartesia, and xAI

`ApiKey` of `FalProvider`, `ReplicateProvider` (the Replicate API token), `FishAudioProvider`, `CartesiaProvider`, and `XaiProvider` is a `Resolvable<string>`: a string, a `Task<string>`, or a callback made with `Resolvable<string>.From(...)`. A callback is called before each request and may be asynchronous. Use it to supply refreshed credentials when keys rotate. A `Task<string>` resolves to the same key each time. Caching and refreshing the credentials is the callback's job. A string and an environment variable work as before.

```csharp
using Vercel.AI.OpenAICompatible;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Xai;

var xai = XaiProvider.Create(
    new XaiOptions { ApiKey = Resolvable<string>.From(async ct => await tokenStore.GetKeyAsync(ct)) }
);
```

xAI files, batches, and the realtime and image models resolve the key the same way, for each request. The header functions of `XaiFiles`, `XaiImageModelConfig`, and `XaiRealtimeModelConfig` take a `CancellationToken` and return a `Task` of the headers.

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

## xAI Batch

`XaiProvider.ExperimentalBatch()` (upstream `experimental_batch()`) returns the xAI batch API, which `Batch.StartBatchAsync` and the other batch functions use. Its provider id is `xai.batch`. The language and Responses models are not batch APIs.

- `DoStartAsync` accepts `text` and `image` requests and throws `UnsupportedFunctionalityException` (`batch request type: <type>`) for any other type before it sends a request. It writes one JSONL line per request (`custom_id`, `method`, `url`, `body`) to `batch.jsonl`, uploads it to `/files`, and posts `{ name: "ai-sdk-text-batch", input_file_id }` to `/batches`. A text request goes to `/v1/responses` with the same body as `ResponsesModel`, so different text models can share a batch. An image request goes to `/v1/images/generations`, or to `/v1/images/edits` when it has reference files. Its `size`, `seed`, and `mask` add `unsupported` warnings. Set `providerOptions.xai.inputFileExpiresAfter` (3600 to 2592000 seconds) to expire the uploaded input file. It is sent as the first form field, because xAI rejects it after the file. The start result has `ProviderMetadata.xai.inputFileId` and, when xAI reports it, `inputFileExpiresAt`. A `webhookUrl` adds an `unsupported` warning.
- `DoGetStatusAsync` maps xAI's counters to `RequestCounts`. `Failed` is `num_error + num_cancelled`, and counts that do not add up are omitted. A batch is `completed` when no request is pending, and `failed` with `ErrorCode` `batch_cancelled` or `batch_expired` when it was cancelled or has expired.
- `DoCancelAsync` posts to `/batches/{id}:cancel`. `DoListAsync` takes `Limit` and `Cursor` (sent as `pagination_token`) and returns `NextCursor` when xAI sends one.
- `DoGetResultsAsync` throws `InvalidArgumentException` for `batchId` while the batch is pending. It then pages through `/batches/{id}/results` (1000 per page), one page at a time. A text item has the generated text, reasoning, `url` sources, tool calls, `FinishReason`, `Usage`, and `ProviderMetadata.xai` (`costInUsdTicks`, `serviceTier`). Tool calls that xAI ran itself come back as provider-executed, dynamic calls followed by their tool results. An image item has base64 images (a URL-only image is downloaded from a trusted origin) and `ProviderMetadata.xai` (`images[].revisedPrompt`, `costInUsdTicks`). An item with an error is `failed`, or `cancelled` for cancellation codes. A moderated image is `failed` with a content policy message. An item that does not match the expected shape is `failed` with `ErrorCode` `invalid_response`, and the remaining items still arrive.
- Every xAI batch call maps a failed response with the xAI error handler (`XaiError.FailedResponseHandler`), as upstream's `xaiFailedResponseHandler` does.

## xAI video models

Create a model with `xai.VideoModel("grok-imagine-video-1.5")` or its alias `xai.Video("grok-imagine-video-1.5")`. The provider id is `xai.video`. `XaiVideoModel` implements `IVideoCaller`, so pass it as the `Model` of `GenerateVideoRequest`. It supports standard generation from a text prompt or an image, plus explicit video editing, video extension, and reference-to-video (R2V). It implements `DoStartAsync` and `DoStatusAsync`, and `GenerateVideo` polls for the result. xAI has no webhooks, so a `Webhook` option is ignored with a warning and the SDK polls. Any other model id is sent as is. Runnable examples are in `examples/GenerateVideoXai*`.

```csharp
using Vercel.AI.Operations;
using Vercel.AI.Xai;

var xai = XaiProvider.Create();
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
    Prompt = new VideoPrompt("A yorkie among dandelions at Crissy Field in San Francisco."),
    AspectRatio = "16:9",
    Duration = 5,
    ProviderOptions = OperationJson.Parse("""{ "xai": { "user": "user-123", "pollTimeoutMs": 600000 } }"""),
});
```

An image start frame works as a prompt image, `new VideoPrompt(text, image)`, or as a `FrameImages` entry with the `first_frame` role. `grok-imagine-video-1.5` also accepts a `last_frame`. Other models ignore it with a warning, so use `extend-video` to continue an existing video. A `first_frame` is sent as xAI's `image` input.

### Editing, extension, and reference-to-video

Select the operation with `mode` in the `xai` provider options. Each mode is exclusive. Omitting `mode` while setting `videoUrl` behaves like `edit-video`, and omitting it while setting `referenceImageUrls` or passing image or audio `InputReferences` behaves like `reference-to-video`.

| `mode` | Needs | Endpoint |
| --- | --- | --- |
| none | a prompt, optionally an image | `POST /videos/generations` |
| `edit-video` | `videoUrl` | `POST /videos/edits` |
| `extend-video` | `videoUrl` | `POST /videos/extensions` |
| `reference-to-video` | `referenceImageUrls` or `InputReferences` | `POST /videos/generations` |

Editing accepts videos up to 8.7 seconds. `Duration`, `AspectRatio`, and `Resolution` are not supported for editing, and each one is ignored with a warning: the output matches the input video, capped at 720p. Extension continues from the last frame. `Duration` is the length of the extension only. `AspectRatio` and `Resolution` are ignored with a warning, because the output inherits them from the source video.

The xAI-hosted video URL is in `ProviderMetadata` at `xai.videoUrl`. Use it to chain edits, or to branch into concurrent edits with `Task.WhenAll`.

Reference images guide the style and content of the video, and are not used as the first frame. Refer to them in the prompt as `<IMAGE_1>`, `<IMAGE_2>`, and so on. `referenceImageUrls` holds 1 to 7 URLs or base64 data URIs. The top-level `InputReferences` option takes precedence and selects reference-to-video. It accepts image and audio references, and an audio reference must declare an audio media type. Audio goes to `reference_audios`, alone or together with images. A reference with no media type is an image. A video reference is ignored with a warning. `referenceVoiceIds` holds up to 3 xAI preset voice ids that give the subject a voice. Refer to audio in the prompt as `<AUDIO_0>`, `<AUDIO_1>`, and `<AUDIO_2>`, in the order passed. Ids are case-insensitive, and an unknown id returns a `400` that lists the available voices. More than 3 audio inputs are cut to the first 3 with a warning. `referenceVoiceIds` is ignored with a warning outside reference-to-video. Reference-to-video is capped at `720p`: a `1080p` request is downgraded with a warning.

### Provider options

Pass these under the `xai` key of `ProviderOptions`. `XaiVideoModelOptions.Parse` reads them. An option of the wrong type throws `InvalidArgumentException` before any request. Options that the model does not know are sent to the API unchanged.

- **pollIntervalMs** _number_ and **pollTimeoutMs** _number_: positive numbers. The poll settings of the `GenerateVideoRequest` control polling, as upstream.
- **resolution** _string_: `480p`, `720p`, or `1080p`. The standard `Resolution` option maps `1920x1080` to `1080p`, `1280x720` to `720p`, and `854x480` and `640x480` to `480p`. Any other value is ignored with a warning. `1080p` needs `grok-imagine-video-1.5`. With `grok-imagine-video` the request is sent with `1080p` and a warning. The provider option wins over the standard option.
- **output** _object_: delivers the completed video to storage you manage. Set **output.uploadUrl** to a signed HTTPS PUT URL, for example an S3 or GCS presigned URL or an Azure Blob SAS URL. It works for generation, editing, and extension. The URL must allow `Content-Type: video/mp4`, and Azure Blob hosts also receive `x-ms-blob-type: BlockBlob`. Allow enough time for generation and upload before the signature expires. A URL with user info or a fragment, a URL that is not HTTPS, and any other `output` shape are rejected before the request starts. xAI performs the upload and the SDK keeps polling. It does not download the destination, even when xAI also returns a video URL. The result has `UploadedVideos` set to 1 and no `Videos`, so use `UploadedVideos` and do not read `Video`. Without `output`, the video is downloaded as usual. Completion means xAI reported the operation as done, not that the storage object exists, so confirm the object before relying on it.
- **storageOptions** _object_: persists a copy in the xAI Files API. Set `filename` and optionally `expiresAfter` (a positive integer up to 30 days, in seconds) and `publicUrl` (`true`, or `{ "expiresAfter": 3600 }` with a lifetime from 3,600 to 2,592,000 seconds). It is independent of `output`, and both are sent when set together. Files details and errors are in `ProviderMetadata` at `xai.fileOutput` and `xai.storageError`. Files URLs are not downloaded in upload mode.
- **user** _string_: an opaque end-user identifier for abuse monitoring. It is sent for generation, reference-to-video, and editing, and omitted for extension.
- **mode**, **videoUrl**, **referenceImageUrls**, **referenceVoiceIds**: as above.
- **keyframes** _array_: up to four mid-video image anchors, `{ "imageUrl": "...", "timestampSeconds": 1.5 }`, each with a positive timestamp. Keyframes need `grok-imagine-video-1.5`. For other models, and for editing and extension, they are ignored with a warning.

`GenerateAudio` is sent as `generate_audio` for generation. Editing and extension ignore it with a warning. `Fps` and `Seed` are ignored with a warning, and so is `N` above 1: xAI makes one video per call.

Signed upload URLs are credentials. An invalid `output` throws an `InvalidArgumentException` with no value and no inner exception. The operation keeps only a `hasUploadUrl` flag, never the URL. A failed upload start request throws an `ApiException` with the message `xAI video upload request failed.`, the original status code and retry flag, and `output.upload_url` set to `[REDACTED]` in `RequestBodyValues`. A failed upload status request throws one with `xAI video upload status request failed.` and no response body. A failed upload does not echo xAI's failure message. Avoid logging provider options, custom HTTP handlers, or raw provider diagnostics.

```csharp
var result = await GenerateVideo.GenerateVideoAsync(new GenerateVideoRequest
{
    Model = (IVideoCaller)xai.Video("grok-imagine-video-1.5"),
    Prompt = new VideoPrompt("A blue ball rolls across a white surface."),
    ProviderOptions = OperationJson.Parse(
        new JsonObject { ["xai"] = new JsonObject { ["output"] = new JsonObject { ["uploadUrl"] = signedPutUrl } } }.ToJsonString()),
});
Console.WriteLine("Videos delivered by xAI: " + result.UploadedVideos);
```

### Status and metadata

A status of `done` with a video URL completes the call. Other outcomes:

- `failed` is an error: `Video generation failed: <message>`. In upload mode the message is left out: `Video generation failed.`.
- `expired` is an error: `Video generation request expired.`.
- A video with `respect_moderation` set to `false` is an error: `Video generation was blocked due to a content policy violation.`.
- `done` with no video URL is an error: `Video generation completed but no video URL was returned.`. Upload mode needs no URL.
- A `202` response, with or without a body, means the video is still pending.

`ProviderMetadata` at `xai` holds `requestId`, and when xAI returns them `videoUrl` (not in upload mode), `duration`, `costInUsdTicks`, `progress`, `fileOutput` (`fileId`, `filename`, `expiresAt`, `publicUrl`, `publicUrlError`, `publicUrlExpiresAt`), and `storageError`. A Files `publicUrl` is the video URL when xAI returns no other one. The request id is encoded as one URL path segment, so a request id with a slash or dots stays in one segment.

### Video model capabilities

| Model | Duration | Resolution | Image-to-video | Editing | Extension | R2V |
| --- | --- | --- | --- | --- | --- | --- |
| `grok-imagine-video` | 1-15s | `480p`, `720p` | yes | yes | yes | yes |
| `grok-imagine-video-1.5` | 1-15s | `480p`, `720p`, `1080p` | yes | yes | yes | yes |

Aspect ratios are `1:1`, `16:9`, `9:16`, `4:3`, `3:4`, `3:2`, and `2:3`. The default aspect ratio is `16:9` and the default resolution is `480p`. Native `1080p` applies to text-to-video and image-to-video. Image-to-video keeps the aspect ratio of the input image unless you set `AspectRatio`, which stretches it. Generated video URLs are ephemeral, so download them promptly. Video generation can take several minutes.

## DeepSeek Responses API

`DeepSeekProvider.ResponsesModel(modelId)` creates a language model for DeepSeek's [Responses API](https://api-docs.deepseek.com/guides/responses_api) (`POST {base}/responses`), like `deepSeek.responses(modelId)` upstream. The provider id is `deepseek.responses`. `LanguageModel(modelId)` still returns the Chat Completions model. The Responses model supports `deepseek-flash` and `deepseek-v4-pro`, streaming, function tools, `JsonSchema` structured output (`JsonSchemaName` is the schema name, `response` when not set) and `JsonResponse`, `ToolChoice` modes, and DeepSeek's server-side web search.

```csharp
using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;

var deepSeek = DeepSeekProvider.Create(new OpenAICompatibleOptions { ApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") });
var model = deepSeek.ResponsesModel("deepseek-v4-pro");
```

The Responses API is stateless. A request sends the full conversation history, including reasoning and previous web search calls, on every call.

### Provider options

Pass these under the `deepseek` key of `ProviderOptions`.

- **thinking** _object_: set `type` to `disabled` to turn off thinking mode. It is sent as `reasoning.effort: "none"`. `type` is `enabled` or `disabled`; thinking is on by default.
- **reasoningEffort** _string_: `low`, `high`, or `max`. It controls the thinking strength when thinking is on. The top-level `Reasoning` option maps to the same values: `minimal` and `low` become `low`, `medium` and `high` become `high`, and `xhigh` and `max` become `max`. `none` is sent as `none`. A level that changes adds a `compatibility` warning. A `reasoningEffort` option wins and adds no warning.
- **topLogprobs** _number_: the number of most likely tokens to return at each token position (0 to 20). The log probabilities come back in `providerMetadata.deepseek.logprobs`.
- **userId** _string_: an opaque end-user identifier, sent as `user`. It must match `^[a-zA-Z0-9_-]+$` and has at most 512 characters.

`DeepSeekLanguageModelResponsesOptions` (upstream `DeepSeekLanguageModelResponsesOptions`) is the typed form of these options. Set `Thinking`, `ReasoningEffort`, `TopLogprobs`, and `UserId`, then pass `options.ToProviderOptions()` as `ProviderOptions`. `ToProviderOptions` is .NET-only: it validates the values and builds the `deepseek` entry. See `examples/DeepSeekResponses`.

An invalid option throws an `InvalidArgumentError`.

An image file part accepts the provider option `deepseek.imageDetail` (`low`, `high`, `original`, or `auto`) as `FileContentPart.ProviderOptions`. A file part with a provider reference sends `file_id`. The `fileData` option is not supported with the Responses API and throws an `UnsupportedFunctionalityException`. Image URLs are at most 8192 characters. Inline images must be JPEG, PNG, GIF, or WebP. A user file part that is not an image is left out with an `unsupported` warning (`user message part type: file`).

The Responses API does not support `StopSequences`, `FrequencyPenalty`, `PresencePenalty`, `TopK`, `Seed`, or strict function tools. The provider omits them and adds an `unsupported` warning.

DeepSeek rejects `ToolChoice.Required` and forced tool choices while thinking is on, which is the default. Set `thinking.type` to `disabled` to use them. `ToolChoice.Auto` and `ToolChoice.None` work in both modes.

### Web search

DeepSeek can search the web and read pages on the server while it generates a response. Add `DeepSeekTools.WebSearch()` (`deepseek.web_search`) to a `deepseek-v4-pro` Responses model. DeepSeek ignores the tool on `deepseek-flash`.

```csharp
var result = await AiClient.GenerateTextAsync(new GenerateTextOptions
{
    Model = deepSeek.ResponsesModel("deepseek-v4-pro"),
    Prompt = "What is the official Python website?",
    Tools = new[] { DeepSeekTools.WebSearch() },
});

Console.WriteLine(result.Text);
```

`DeepSeekTools.WebSearch(name)` takes the name the tool goes by in the call and its result. It defaults to `web_search`.

Each server-side search comes back as a `GeneratedToolCall` with `ProviderExecuted` set and a `GeneratedToolResult` named after the tool. The result is `{ status, action }`, where `action` is the search DeepSeek ran, such as `{ type: "search", queries: [...] }`. The raw DeepSeek `web_search_call` item stays in the tool call's provider metadata (`deepseek.webSearchCall`). A later turn sends it back, so DeepSeek restores its search results. A client tool that happens to be named `web_search` is still sent as a function call. A provider-executed call without that metadata is left out of the next request. Another provider-defined tool id is left out with an `unsupported` warning (`provider-defined tool <id>`). A `ToolChoice.Tool(name)` that selects a provider-defined tool is not sent and adds an `unsupported` warning (`toolChoice for provider-defined tool <id>`).

DeepSeek bills successful searches and page reads per operation. The counts are in `providerMetadata.deepseek.serverToolUse` (`toolSearch` and `toolFetch`) and in `Usage.Raw` (`server_tool_use`). See [DeepSeek pricing](https://api-docs.deepseek.com/quick_start/pricing) for the rates.

### Provider metadata

`providerMetadata.deepseek` holds `responseId`, `serverToolUse` when a server-side tool ran, and `logprobs` when `topLogprobs` is set. A reasoning part carries the DeepSeek reasoning item id in `providerMetadata.deepseek.itemId`. The stream sends it on `reasoning-start` and `reasoning-end`.

A failed response (`status: "failed"` or an `error` object) throws an `ApiException` with the DeepSeek message. Its status code comes from the error code: `rate_limit_exceeded` and `rate_limit_error` are 429 and retryable, `insufficient_quota` is 429, `authentication_error` and `invalid_api_key` are 401, `permission_error` is 403, `bad_request`, `context_length_exceeded`, and `invalid_request_error` are 400, `timeout` and `timeout_error` are 504 and retryable, and every other code is 500 and retryable. A stream reports the same failure as an `error` part and ends with the `error` finish reason.

## Gateway fetcher config

`GatewayConfig` (upstream `GatewayConfig`) configures the Gateway fetchers (`GatewayFetchMetadata`, `GatewaySpendReport`, `GatewayGenerationInfoFetcher`). `GatewayConfig.Headers` (upstream `headers`) is an asynchronous resolver: `Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>`. It is called on each request. When it is `null`, the fetcher sends no extra headers.

Breaking change: `GatewayConfig.HeadersAsync` is removed. Set `Headers` instead. A synchronous factory such as `Headers = () => dictionary` must become `Headers = _ => Task.FromResult(dictionary)`. Upstream's `Resolvable` also accepts a plain value or a promise. The port accepts only the async function form.

## Gateway transcription

`GatewayProvider.TranscriptionModel(id)` posts `{ audio, mediaType, providerOptions }` to `{base}/transcription-model` with the `ai-transcription-model-specification-version: 4` and `ai-model-id` headers. The response carries `text`, `segments`, `words`, `language`, `durationInSeconds`, `warnings`, `usage`, and `providerMetadata`. `TranscribeResult.Words` is the Gateway's `words` when it sends them, and an empty list stays an empty list. The Gateway adapter does not forward the shared `TimestampGranularities`, so a non-empty list adds an `unsupported` warning on `timestampGranularities` and leaves the request body as it is. For `xai/` and `spacexai/` models, `providerOptions.xai.diarize` or `providerOptions.spacexai.diarize` adds an `unsupported` warning on `providerOptions.xai.diarize` when the response has no `speaker` anywhere. Gateway error responses map to the `Gateway*Error` types.

### Streaming

`GatewayProvider.ExperimentalTranscription(id)` (or `TranscriptionModel(id)`, cast to `ITranscriptionCaller`) streams live audio over the `/transcription-model` WebSocket. Pass the model to `StreamTranscribe.Start`. The socket URL is the base URL upgraded to `ws(s)`, with the model id in `?ai-model-id=`. Auth rides the `Sec-WebSocket-Protocol` handshake (`ai-gateway-transcription.v1`, `ai-gateway-auth.<token>`, and `ai-gateway-team.<base64url team>` when a team is set), because a browser `WebSocket` cannot send headers. The resolved request headers are also passed to header-capable WebSocket implementations; set `GatewayOptions.WebSocket` to a `WebSocketFactory` to replace the default `ClientWebSocket`.

The model follows the transcription-stream envelope (`TranscriptionStreamEnvelope`). It sends one `transcription-stream.start` text frame (`inputAudioFormat`, plus `providerOptions` and `includeRawChunks` when set), binary audio frames of at most 64 KiB, and a `transcription-stream.audio-done` text frame. `ProviderRequest.Body` is the start frame. Every server text frame is one JSON stream part. Malformed frames and unknown part types are ignored, `response-metadata.timestamp` is revived to a date, and the stream ends after the `finish` part with close code 1000. An `error` part stops the audio and reaches the caller as a part. When the server then closes the socket, the stream fails with a Gateway error. A `type` of `authentication_error`, `invalid_request_error`, `rate_limit_exceeded`, and the other Gateway error types maps to the matching `Gateway*Error`; any other payload fails with `AI Gateway transcription stream failed: <message>`. A close without a `finish` part fails with `AI Gateway transcription stream closed before a finish part was received`, and a socket error fails with `Connection error on AI Gateway transcription stream`. A cancelled token fails the stream with the abort reason. An audio stream that fails with a cancellation or a `TimeoutError` passes that error through unchanged.

### Client secrets

`GatewayProvider.ExperimentalTranscriptionGetTokenAsync(model, expiresAfterSeconds)` mints a transcription-bound short-lived client secret (`vcst_`) through `/v1/realtime/client-secrets` with `routeKind: "transcription"`, and returns `GatewayTranscriptionFactoryGetTokenResult` (`Token`, `Url`, `ExpiresAt`). Call it on the server. A browser then connects with `GatewayProvider.Create(new GatewayOptions { ApiKey = token }).ExperimentalTranscription(model)`, without holding the Gateway credential. `ExpiresAt` stays `null` when the endpoint sends none.

`GatewayProvider.ExperimentalSpeechGetTokenAsync(model, expiresAfterSeconds)` does the same for streaming speech (`routeKind: "speech"`) and returns `GatewaySpeechFactoryGetTokenResult` (`Token`, `Url` such as `wss://ai-gateway.vercel.sh/v4/ai/speech-model?ai-model-id=elevenlabs%2Feleven-flash-v2.5`, `ExpiresAt`). The Gateway streaming speech model itself is not ported yet (#752).

Minting needs the Gateway credential, so it must run on a server. In a browser runtime (Blazor WebAssembly, where `OperatingSystem.IsBrowser()` is true) `ExperimentalRealtimeGetTokenAsync`, `ExperimentalTranscriptionGetTokenAsync` and `ExperimentalSpeechGetTokenAsync` throw an `AiSdkException` ("must be minted server-side"). Building the realtime model with `RealtimeModel(id)` touches no credential and works in a browser, so a client can drive the transport with a token minted on the server.

### Live models

`GatewayProvider.ExperimentalRealtime(id, options)` (upstream `experimental_realtime(id, options)`) returns a `GatewayRealtimeModelLive` (provider id `gateway.live`) for the known Live ids in `GatewayKnownLiveModelId.GatewayLiveModelIds` (`openai/gpt-live-1`, `openai/gpt-live-1.1`), and a `GatewayRealtimeModel` for every other id, including `google/gemini-3.8-live`. `RealtimeModel(id)` does the same without options. Pass `new GatewayRealtimeOptions { Api = GatewayRealtimeApi.Live }` for a Live id the SDK does not know yet, or `GatewayRealtimeApi.Realtime` to force the realtime route. An `Api` value outside the enum throws `InvalidArgumentException`. Upstream's typed overloads (which narrow the return type from the literal `api`) have no C# equivalent, so the method returns `IRealtimeModel`; cast it.

A Live model holds one continuous conversation. `Capabilities` are `continuous`, transport `websocket`, connections `client-secret-websocket` and `server-websocket`, `Startup` `session-start` and `Finalization` `session-close`. It speaks the Gateway's normalized Live protocol on `{BaseUrl}/live-model?ai-model-id=`, so `ParseServerEvent` passes events through. A Gateway-originated `error` frame keeps itself as `Raw`, and a value that is not an event with a string `type` becomes an `invalid_server_event` error. `ParseServerEvent` accepts a `RealtimeModelV4ServerEvent` or the `JsonElement` of a wire frame.

`SerializeClientEvent` returns the JSON the Gateway expects: `session-start` (with `modelId` and the validated `config`), `session-close`, `input-audio-mute`, `input-audio-unmute`, `input-audio-append` and `context-append`. Any other event throws `UnsupportedFunctionalityException`. `BuildSessionConfig` accepts `Instructions`, `Voice`, the input and output audio formats and `ProviderOptions`; any other setting throws `UnsupportedFunctionalityException` (`AI Gateway Live session setting: <name>`). Audio is `audio/pcm` at 16000 or 24000 Hz, or `audio/pcma` or `audio/pcmu` at 8000 Hz, with the same format in both directions. `ProviderOptions` takes `gateway` (open) and `openai` with `delegation` (`{ "type": "client" }` or null), `input` (at most 128 messages), `store` and `voice` (`{ "id": "..." }`); a malformed value throws `InvalidArgumentException`. `context-append` accepts only `openai.channel` of `commentary`, `instructions` or `thinking`. A non-conversation intent throws.

`ExperimentalRealtimeGetTokenAsync(model, expiresAfterSeconds, cancellationToken, options)` mints a Live-bound secret (`routeKind: "live"`) for a Live model and returns the `/live-model` URL. Realtime mints are unchanged and omit `routeKind`. `GetWebSocketConfig(token, url)` returns the subprotocols that carry the token, and `GetServerWebSocketConfigAsync` returns the URL and the defined auth headers. The `GatewayRealtimeLive` example drives a server WebSocket session. The browser session that upstream's `ai` package adds (`api.token` for continuous sessions) has no .NET counterpart, because the port has no browser realtime session.

## Gladia transcription options

`GladiaProvider.Transcription()` uses the default Gladia model. Pass a model id to pick another one: `provider.TranscriptionModel("solaria-3")`. The id is forwarded as `model` in the pre-recorded request, kept in `Response.ModelId`, and kept through `Serialize()` and `Deserialize()`. The `default` id sends no `model` field, so Gladia chooses its own default (`solaria-1`). The provider also sends an `x-gladia-version: ai-sdk/<version>` header on every request. A header you set yourself wins.

Set the Gladia pre-recorded options on `GladiaTranscriptionRequest.ProviderOptions["gladia"]`, with camelCase keys like upstream. The port maps them to the snake_case fields of the Gladia v2 API. Invalid options throw `InvalidArgumentError` (`invalid gladia provider options`) before the audio is uploaded.

```csharp
var options = JsonDocument.Parse("""
    {
      "languageConfig": { "languages": ["en"], "codeSwitching": false },
      "piiRedaction": true,
      "piiRedactionConfig": { "processedTextType": "MARKER" }
    }
    """).RootElement;

var model = (GladiaProvider.GladiaTranscriptionModel)provider.TranscriptionModel("solaria-3");
var result = await model.TranscribeAsync(
    audio,
    new GladiaTranscriptionRequest { ProviderOptions = new Dictionary<string, JsonElement> { ["gladia"] = options } },
    cancellationToken);
```

- `languageConfig` (`languages`, `codeSwitching`) sets the language. `piiRedaction`, `piiRedactionConfig` (`entityTypes`, `processedTextType` of `MARKER` or `MASK`), the `batch` translation model with `lipsync`, `contextAdaptation`, `context`, and `informal`, and `audioToLlmConfig.model` are sent as given.
- `language`, `detectLanguage`, `enableCodeSwitching`, and `codeSwitchingConfig` still work. They are translated to `languageConfig` and add a `DeprecatedWarning`. When `languageConfig` is set it wins, and the warning says so.
- A `customVocabulary` array is translated to `customVocabulary: true` plus `customVocabularyConfig.vocabulary` with a `DeprecatedWarning`. Entries that are not a string or an object with a string `value` and correctly typed optional fields are dropped with a warning. An explicit `customVocabularyConfig.vocabulary` wins over the array.
- `contextPrompt`, `moderation`, `chapterization`, `nameConsistency`, `structuredDataExtraction`, `structuredDataExtractionConfig`, `displayMode`, and `diarizationConfig.enhanced` are no longer in the Gladia v2 API. They are not sent and add an `UnsupportedWarning`.
- `piiRedactionConfig` without `piiRedaction: true` adds a warning. The `solaria-3` model supports one language and no code switching, so other settings add a warning. The request is not changed.
- All warnings are in `GladiaTranscriptionResult.Warnings`. `GladiaTranscriptionRequest.Body` (snake_case fields) remains a .NET-only way to add raw fields. It is merged over the fields built from the options.

## Gladia transcription polling

`GladiaProvider.Transcription()` uploads the audio, starts a pre-recorded job, and polls its result URL until the job reports `done`. The polling budget defaults to 10 minutes (`600000` ms) and the interval between status checks to 1 second (`1000` ms). Long audio files and audio intelligence add-ons can need more than a minute, so raise the budget when you need to.

```csharp
var provider = GladiaProvider.Create(new GladiaOptions
{
    ApiKey = apiKey,
    PollIntervalMs = 2000,
    PollTimeoutMs = 20 * 60 * 1000,
});

// Per-call values win over the provider settings.
var result = await provider.Transcription().TranscribeAsync(
    audio,
    new GladiaTranscriptionRequest { PollTimeoutMs = 30 * 60 * 1000 },
    cancellationToken);
```

- `GladiaOptions.PollIntervalMs` and `GladiaOptions.PollTimeoutMs` set the provider defaults. `GladiaTranscriptionRequest.PollIntervalMs` and `GladiaTranscriptionRequest.PollTimeoutMs` override them for one call. They are never sent to the Gladia API.

When Gladia reports a failed job, the call throws an `AiSdkError` named `TranscriptionJobFailed`. Its `Cause` is a `GladiaTranscriptionJobResultException` with the parsed `Result` and the `ErrorCode`.
- Each value must be a positive integer. Anything else throws `InvalidArgumentError`: the provider settings when the provider is created, the per-call values when the call starts.
- When the budget runs out, the call throws an `AiSdkError` named `TranscriptionJobPollingTimedOut` with the message `Transcription job polling timed out after <timeout> ms`.
- The `CancellationToken` stops the loop before the next poll and cancels the wait between polls. A cancel surfaces as an `OperationCanceledException`.

`GladiaTranscriptionModel.Serialize()` returns the model id and config for a workflow boundary (provider id, base URL, headers, and the two polling settings). It leaves out the API key and any header whose name contains `authorization`, `key`, `token`, `secret`, or `cookie`. `GladiaTranscriptionModel.Deserialize(serialized, apiKey)` restores the model with its polling settings; without `apiKey` it reads `GLADIA_API_KEY`.

## QuiverAI language models

`QuiverAIProvider.LanguageModel(modelId)` creates an Arrow language model (`arrow-2` or `arrow-2-telos`) for QuiverAI's stateless `POST /v1/responses` endpoint. The provider id is `quiverai.responses`. The model reuses the Open Responses transport and applies QuiverAI's request policy.

```csharp
using Vercel.AI.QuiverAI;

var provider = QuiverAIProvider.Create(new OpenAICompatibleOptions { ApiKey = apiKey });
var model = provider.LanguageModel("arrow-2");
```

### Reasoning

Set reasoning with `LanguageModelCallOptions.Reasoning` or with the `quiverai` provider options `reasoningEffort` (`low`, `medium`, `high`, or `xhigh`) and `reasoningSummary` (`auto`). The model validates the provider options first and throws an `InvalidArgumentException` for an unsupported value. A native `reasoningEffort` wins over `Reasoning`.

QuiverAI supports four efforts, so a top-level `Reasoning` value that is not one of them maps to the nearest supported effort and adds a `compatibility` warning. `none` and `minimal` map to `low`, and `max` maps to `xhigh`.

The provider does not request encrypted reasoning. When it replays the conversation in a tool loop, it sends the opaque reasoning item ids and the safe summaries, in order, without the private reasoning text or the encrypted reasoning state.

### Tools and limits

- Function tools run in your application. `provider.Tools.CustomTool(...)` creates a QuiverAI custom tool that returns a raw string and also runs in your application. Hosted tools are not supported.
- Requests are stateless. Structured output settings are omitted and reported as an `unsupported` `responseFormat` warning.
- Streaming tool calls are assembled completely before they run.
- `LanguageModelUsage` reports input, cache-read, cache-write, text-output, and reasoning token counts when QuiverAI returns them.
- Errors use QuiverAI's flat `{ status, code, message, request_id }` body, and a failed response keeps the status code from its `error.status_code`.

`QuiverAILanguageModel.Serialize()` returns the model id and config for a workflow boundary (provider id, base URL, headers). It leaves out the API key and any header whose name contains `authorization`, `key`, `token`, `secret`, or `cookie`. `QuiverAILanguageModel.Deserialize(serialized, apiKey)` restores the model with QuiverAI's request policy and error handling; without `apiKey` it reads `QUIVERAI_API_KEY`.

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

An OpenAI-compatible chat model also accepts inline text file data. `FileContentPart.FromText("text/plain", "Résumé: café, 日本語")` is sent as a `{ "type": "text", "text": ... }` part, the same as text supplied as bytes. Properties from the part's `openaiCompatible` provider options, such as `cache_control`, are copied onto it. Inline text with a media type that is not `text/*` throws `UnsupportedFunctionalityException`.

Anthropic and Google read the same reference. Anthropic sends the `anthropic` entry as a Files API `file_id`. Google sends the `google` entry as the `fileUri`. `AnthropicSkills.UploadSkillAsync` returns its skill id as `ProviderReference["anthropic"]`.

`AnthropicProvider.Files()` returns an `AnthropicFiles` client for the Files API. It implements the operations `IFileStore`, so `UploadFileAsync(UploadFileCall, ...)` takes tagged upload data: `data` (bytes or base64) and `text` are sent, and `stream` data throws `UnsupportedFunctionalityException` and cancels the `UploadStream` before any request. The result carries the provider reference, media type, filename, byte size, timestamps and provider metadata. Besides `UploadAsync`, it has `GetFileMetadataAsync`, `DownloadFileAsync`, and `DeleteFileAsync`. Each takes a provider reference with an `anthropic` entry, and throws `ArgumentException` when the entry is missing or blank. The file id is encoded as one URL path segment. Each call sends `anthropic-beta: files-api-2025-04-14` and accepts per-call headers, which win over the provider headers and the default beta header. `AnthropicOptions.AuthTokenProvider` (upstream's function form of `authToken`) is called on every request, so `Files()` calls and Messages calls send a refreshed `Authorization: Bearer` token each time, and no `x-api-key`. It takes precedence over `AuthToken`, and setting it together with `ApiKey` throws `InvalidOperationException`.

- `UploadAsync` and `GetFileMetadataAsync` return `ProviderReference`, `FileName`, `MediaType`, `ByteSize`, `CreatedAt`, `ExpiresAt`, and `ProviderMetadata`. `ProviderMetadata["anthropic"]` holds `filename`, `mimeType`, `sizeBytes`, `createdAt`, `downloadable`, and `expiresAt`. A field the response omits or sets to null is left out.
- `DownloadFileAsync` returns the `Content` stream (the response stays open until you dispose it) and a `MediaType` taken from the response `Content-Type` without its parameters. `MediaType` is null when the response has none.
- `DeleteFileAsync` returns `Deleted` and the `ProviderReference` of the deleted file.

```csharp
var files = anthropic.Files();
var uploaded = await files.UploadAsync("notes.txt", Encoding.UTF8.GetBytes("Notes for Claude."), "text/plain", null, ct);
try
{
    var metadata = await files.GetFileMetadataAsync(uploaded.ProviderReference.ToDictionary(p => p.Key, p => (string?)p.Value), ct);
    Console.WriteLine($"{metadata.FileName} {metadata.MediaType} {metadata.ByteSize}");
}
finally
{
    await files.DeleteFileAsync(uploaded.ProviderReference.ToDictionary(p => p.Key, p => (string?)p.Value), ct);
}
```

For a file generated by code execution or a skill, pass its id as `{ ["anthropic"] = id }`. Anthropic only allows a download when `ProviderMetadata["anthropic"]["downloadable"]` is `true`. Downloading a file you uploaded returns an API error. Metadata and deletion work for both. Per-call headers can override the default beta header. Anthropic's current Files API also supports requests without that beta, which may return additional metadata such as expiration.

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

`PrepareStepContext` also exposes `Messages`, the prompt plus the responses of the earlier steps, and `Model`, the model the step is about to use. `Messages` does not hold the instructions. `Instructions` is what the step is about to send, `InitialInstructions` and `InitialMessages` are what the call started with, and `ResponseMessages` are the messages the earlier steps added. Return `PrepareStepUpdate.Instructions` to replace the instructions, or `PrepareStepUpdate.Messages` to replace the messages. Each override carries forward to later steps, and replacing one keeps the other. `PrepareStepContext.RuntimeContext` and `ToolsContext` hold the contexts the step is about to use (`GenerateTextOptions.RuntimeContext` and `ToolsContext`). Return `PrepareStepUpdate.RuntimeContext` or `ToolsContext` to replace them for this step and all later steps. Return `PrepareStepUpdate.Model` or `ActiveTools` to change the model or the active tools for that step only. Return `MaxOutputTokens`, `Temperature`, `TopP`, `TopK`, `PresencePenalty`, `FrequencyPenalty`, `StopSequences`, `Seed` or `Reasoning` to change that call setting for that step only; a null value keeps the call's setting, and an empty `StopSequences` clears them. `PrepareStepContext.Model` is always the model of the call. The redo of an interrupted step runs `PrepareStep` again with the contexts that the step started with. Like upstream, the results of all tool calls in one step are a single `ToolModelMessage`; read them from `ToolModelMessage.Results` (a single-result message lists itself).

## Anthropic message threads

[Anthropic message threads](https://platform.claude.com/docs/en/build-with-claude/message-threads) reduce request upload size and time to first token in long conversations. The first request sends the full conversation, and later requests send only the new messages. Token usage, pricing, and rate limits stay the same. Prompt caching is separate.

Set `thread` in the `anthropic` provider options. `{ "type": "create" }` starts a thread. `{ "type": "continue", "previousMessageId": "..." }` continues it. `previousMessageId` is sent as `previous_message_id`, and the `message-threads-2026-08-12` beta header is added for you. `AnthropicProvider` and `AnthropicAwsProvider` support threads. A thread option on `AmazonBedrockAnthropicProvider`, `GoogleVertexAnthropicProvider`, or another Messages-compatible provider throws `UnsupportedFunctionalityException`, and a `thread` that is neither of the two shapes above throws `InvalidArgumentException`. Both throw before any request is sent. With SigV4 on `AnthropicAwsProvider`, sign every request of a thread with the same IAM role or IAM user.

```csharp
var model = anthropic.LanguageModel("claude-sonnet-4-6");

var first = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    System = "Keep replies brief.",
    Prompt = "Remember that my favorite color is cobalt blue.",
    ProviderOptions = new Dictionary<string, JsonElement>
    {
        ["anthropic"] = JsonSerializer.SerializeToElement(new { thread = new { type = "create" } }),
    },
});

var second = await client.GenerateTextAsync(new GenerateTextOptions
{
    Model = model,
    Prompt = "What is my favorite color?",
    ProviderOptions = new Dictionary<string, JsonElement>
    {
        ["anthropic"] = JsonSerializer.SerializeToElement(new
        {
            thread = new { type = "continue", previousMessageId = first.Response.Id },
        }),
    },
});
```

On a continuation the request omits `system` and the tool definitions, even when you pass them, because Anthropic keeps them from the create request. Keep passing identical tools so tool names, tool choice, and tool beta headers still work. The request keeps `tool_choice` and the other per-request settings. A `compatibility` warning on `providerOptions.anthropic.thread` says that supplied instructions or tools were left out. To change them, start a new thread.

### Computer toolset factory

`AnthropicTools.ComputerToolset20260801(configs, name)` declares `anthropic.computer_toolset_20260801` with a local input schema. `configs` maps a member name (see `AnthropicTools.ComputerToolset20260801Members`) to an `AnthropicComputerToolsetMemberConfig` with `Enabled` (default `true`) and `DeferLoading` (default `false`). Members you omit keep their defaults, including `zoom`. The input schema drops every member with `Enabled = false` from the `action` enum, so a call to it fails input validation. An unknown member name throws `ArgumentException`.

```csharp
var computer = AnthropicTools.ComputerToolset20260801(new Dictionary<string, AnthropicComputerToolsetMemberConfig>
{
    ["wait"] = new() { Enabled = false },
    ["zoom"] = new() { DeferLoading = true },
});
```

### Computer toolset members

`anthropic.computer_toolset_20260801` (`LanguageModelTool.ProviderDefined("anthropic.computer_toolset_20260801", name, args)`) returns each action as its own `tool_use` block with `toolset_name: "computer"`. The provider maps every action to one tool call named like your tool, with the member name as `action`, and keeps that returned member name as `action` even when the member input carries a different `action`.

It rejects a member before it emits any tool call, in `DoGenerateAsync` and `DoStreamAsync`, with `InvalidResponseDataException` when the toolset name is not `computer`, the member is unknown, the toolset is not among the prepared tools, or `configs` disables the member with `enabled: false`. On a thread continuation the check uses the tool definitions you pass, even though the request omits them. Streamed member input must be a JSON object; malformed JSON or a non-object value also throws `InvalidResponseDataException`.

Conversation management is yours. The provider keeps no history, does not remove earlier messages, and does not replay a request. Keep the full conversation locally. Pass only the new messages on a continuation (after a tool call, only the new tool results). Passing the whole history can duplicate messages or fail. Setting `thread` on an agent does not continue its thread by itself. Use `PrepareStep` to set the latest response id and to pick only the new messages.

Content-dependent beta headers, such as the one for PDFs, must be the same on every turn of a thread. Pin them with `anthropicBeta` on the create request and on every continuation, including text-only ones. When message content enables a beta that you did not pin (through `anthropicBeta` or an `anthropic-beta` header), the request adds a `compatibility` warning that lists it.

A thread expires five minutes after its last request and can become unavailable earlier. Read `error_code` from the failed response body with `AnthropicErrorData.Parse(exception.ResponseBody).ErrorCode`. On `thread_not_found`, `thread_already_continued`, or `thread_continue_in_flight`, replay the full conversation once with `{ "type": "create" }`, including the original system prompt and tools, and remove cache breakpoints from older replayed messages. Do not treat every 404 as an expired thread. `AnthropicErrorData.Details` keeps `error_code` and `unsupported`, drops unknown fields, and ignores a `details` value of an unknown shape, so a new Anthropic detail shape never hides the error message. Continue only from the latest response id, which can be continued once. With a streamed call, read the whole stream and check that the finish reason is not `FinishReason.Interrupted` before you continue.

Start a new thread with the full history to change the system prompt, tools, model, thinking, tool choice, output format, context management, top-level `cacheControl`, or beta headers, and after a fallback model answered or a refusal. Other fixed fields must keep the same values on every request. Send MCP configuration and container ids on each request that needs them. Top-level `cacheControl` counts toward the four cache breakpoints. Threads do not support `compaction` or `contextManagement`, except a `clear_thinking_20251015` edit with `keep: "all"`; anything else throws `InvalidArgumentException`. An empty continuation prompt is accepted, which resumes a `pause_turn` response at the provider level. Message Batches do not accept threads. `provider.ExperimentalBatch().DoStartAsync` throws `UnsupportedFunctionalityException` ("Anthropic Message Batches do not support message threads (request \"<id>\").") before it sends a request.

Web tools that run through code execution (`web_search_20260209`, `web_search_20260318`, `web_fetch_20260209`, `web_fetch_20260318`) can return `code_execution` calls even when you did not pass the code execution tool. These calls are marked `Dynamic`, also on a thread continuation that left the tool definitions out.

## Decision state

`DecideRequest.State` is a `DecisionState`: a string, a JSON object, or a list of parts. A string converts to it implicitly. `DecisionState.FromJson(element)` takes a JSON object, and a raw JSON array is not state. Wrap it in an object or in a `DecisionJsonPart`. A list of parts is an array of `DecisionTextPart`, `DecisionJsonPart`, and `DecisionFilePart` (media type, `FilePartInput` data, optional `FileName` and `ProviderOptions`). `Decide.DecideAsync` normalizes the state with `DecisionState.PrepareDecisionStateAsync` before the model call. A string becomes one text part, an object becomes one json part, and a file part gets a full media type. A URL file is downloaded with `DecideRequest.Download` (`experimental_download` upstream). Without that hook a URL file throws `DownloadError` before any download or model call, so the runtime's default transport is never used for state. Pass `Download.CreateDownload(maxBytes, fetch)` with a `DownloadFetch` that validates DNS at connection time for untrusted URLs. Inline bytes, base64 content, and data URLs need no hook. A failed download throws `DownloadError` before any model call. `DecisionModelCall.State` and `DecisionModelEvent.State` hold the normalized parts as `DecisionModelTextPart`, `DecisionModelJsonPart`, and `DecisionModelFilePart`. `DecideEvent.State` keeps the public value. `OpenAIDecisionModel` sends text as `input_text`. A JSON object or a nonempty array of objects is sent as native `input_json` when it fits the API limits: 32 container levels, 10,000 values, and 256 KiB of compact UTF-8 JSON. Larger JSON state and other JSON values are serialized as JSON `input_text`. Inline PNG, JPEG, WebP, or GIF files are sent as `input_image`; other files throw `UnsupportedFunctionalityException`. `GatewayDecisionModel` posts `{ stateParts, questions, providerOptions }` to `/decision-model`. `stateParts` holds the ordered text, json, and file parts. File bytes go out as base64, and a file with URL, reference, or text data throws `UnsupportedFunctionalityException` before any request. `providerOptions` is sent whenever the call has it, even when it is empty.

Set `providerOptions.openai.promptCacheKey` to partition prompt cache reuse across OpenAI Decisions requests that share state:

```csharp
ProviderOptions = OperationJson.Parse("""
    {
      "openai": {
        "safetyIdentifier": "user-123",
        "promptCacheKey": "tenant-42"
      }
    }
    """),
```

## Decision questions

`new DecisionQuestion(type, instructions, criteria)` takes `DecisionInput?` instructions: a string (implicit), or a JSON object or array through `DecisionInput.FromJson(element)` or `DecisionInput.Parse(json)`. Any other JSON value throws `InvalidArgumentException`. For `choice`, `score`, and `boolean`, the instructions may be null when the provider supports it and the criteria define the judgment. A question without instructions leaves the field out of the request. Set `HasInstructions = true` on a question with null instructions to send an explicit `null`, as upstream does for `instructions: null`. `PerplexityDecisionModel` and `GatewayDecisionModel` honor it. A `boolean` question without instructions must describe at least one of `True` or `False` with a non-null value, otherwise `Decide.DecideAsync` throws `InvalidArgumentException`. A `box` always requires instructions. Criteria are typed by question type. A `choice` takes `DecisionChoiceCriteria` (a nonempty map of option names to `DecisionInput?`), a `score` takes `DecisionScoreCriteria` (at least one level; a provider can require more), and a `boolean` takes an optional `DecisionBooleanCriteria` with `True` and `False`. A side that is never set is left out of the request. A side set to null is sent as null. A `box` takes no criteria. Descriptions may be null in the shared types, but providers can impose stricter limits. `OpenAIDecisionModel` and `GatewayDecisionModel` send a question without instructions as is, and `DecisionLanguageModel` leaves `instructions` out of the rubric.

## Decision language model

`new DecisionLanguageModel(languageModel, provider?)` wraps a V4 `ILanguageModel` so `Decide.DecideAsync` can use it for `choice`, `score`, and `boolean` questions. It does not support `box` questions. The provider id defaults to the wrapped model's provider plus `.decision`. It sends one structured-output request with reasoning set to `none`. Question ids and choice labels are replaced by internal keys (`q0`, `c0`, ...) in the schema and mapped back in the answers. State parts keep their order. Text and image file parts pass through, and a json part is sent as compact JSON text. Any non-image file part throws `UnsupportedFunctionalityException` before the model is called. A wrong finish reason, invalid JSON, a missing or extra answer, an unknown choice, a score outside its rubric, or a probability outside 0 to 1 throws `InvalidResponseDataException`. A choice needs at least one option and a score at least two levels, otherwise `InvalidArgumentException` is thrown. A file part's `providerOptions` are forwarded as the `FileContentPart.ProviderOptions` of the image. `DecisionLanguageModel.Serialize(model)` returns a `DecisionLanguageModelOptions` with the wrapped `Model` and the `Provider` id for a workflow boundary, and `DecisionLanguageModel.Deserialize(options)` restores the model. The wrapped model is not copied, so a workflow runtime must serialize it with its own hooks.

## Extraction

`Extract.ExtractAsync(ExtractRequest)` is `experimental_extract`. It pulls typed values out of one shared state, such as an invoice number, a total, a due date, or a list of names, and sits next to `Decide.DecideAsync`. `ExtractRequest` takes `Model` (an `IExtractionCaller` instance) or `ModelId` (a string id resolved by the default provider, which is the Gateway unless `DecisionModels.DefaultProvider` is set; see [Gateway extraction models](#gateway-extraction-models)), `State` (a `DecisionState`, handled as in "Decision state"), `Fields` (a nonempty map of field ids to `ExtractionField`), and the optional `ReferenceDate`, `MaxOutputTokens`, `Reasoning`, `Download`, `Headers`, `ProviderOptions`, `MaxRetries`, `RuntimeContext`, `OnStart`, `OnEnd`, and `Telemetry`. Retries, cancellation, and the `ai/` user-agent suffix work as in `Decide.DecideAsync`.

`new ExtractionField(type, instructions)` has seven types: `text`, `number`, `date`, `time`, `datetime`, `duration`, and `list`. `Unit` names the unit of a `number` (any string) or a `duration` (`seconds`, `minutes`, `hours`, `days`, `weeks`, `months`, or `years`; default `seconds`). `Integer` and `Decimals` (a `double?` holding an integer from 0 to 6) constrain `number`, `duration`, and `list` of numbers. A `list` sets `Items` to `text`, `number`, or `date`. `ReferenceDate` is a `YYYY-MM-DD` date that anchors relative dates and dates without a year.

`ExtractionResult.Answers` holds one `ExtractionAnswer` per field. The upstream `value` is split by type: `Text` for `text`, `date`, `time`, and `datetime`, `Number` for `number` and `duration`, and `TextItems` or `NumberItems` for a `list`. It is null when the state has nothing for the field. A model can also return `GuessText`, `GuessNumber`, `PNull`, `Confidence`, and a `Source`. A source is `new ExtractionSource("text", part, start, end)`, where the offsets are UTF-16 code units, so `state[(int)part].Text.Substring((int)start, (int)(end - start))` is the found text. `part`, `start`, and `end` are `double`, like upstream's `number`: a model may return a non-integer, and validation rejects it, or `new ExtractionSource("file", part)`. A list answer carries `Items` (one `ExtractionItem` with `Confidence` and `Source` per value) instead of a source.

Validation is strict and returns no partial results. `Extract.ExtractAsync` throws `InvalidArgumentException` before any model call for a state or field that is not valid, a `ReferenceDate` that is not a calendar date, an empty field map, `Decimals` that is not an integer from 0 to 6, an instruction containing a non-finite number, an unknown duration unit, or an unknown list item type. A field type the model does not list in `SupportedFieldTypes` throws `ExtractionUnsupportedFieldTypeError` before any provider I/O. After the call, an answer with the wrong type, a value that is not a calendar date, an `HH:MM[:SS]` time, an ISO 8601 datetime, an integer when `Integer` is set, a number with more than `Decimals` places, or a negative duration, a probability outside [0, 1], or a source outside the state throws `InvalidResponseDataException`. A refused field throws `ExtractionRefusalError` and is not retried.

`ExtractionResult.Answers` normalizes an empty answer: a `Text` of `""` or an empty `TextItems` or `NumberItems` becomes `null`, and a list answer drops its `Items`. The model's own answers are not changed.

### Extraction telemetry and lifecycle callbacks

`ExtractRequest.OnStart` and `OnEnd` receive an `ExtractStartEvent` and an `ExtractEndEvent` (`ai.extract`) with the full `RuntimeContext`, `Headers`, and `ProviderOptions`. `ExtractRequest.Telemetry` is a `TelemetryOptions`, the same type `GenerateTextOptions.Telemetry` takes. Its `Integrations` are `ITelemetry` objects with `OnExtractStart`, `OnExtractionModelCallStart`, `OnExtractionModelCallEnd` (events `ExtractionModelCallStartEvent` and `ExtractionModelCallEndEvent`, operation id `ai.extract.doExtract`), `OnExtractEnd`, and `OnError` (a `TelemetryErrorEvent` with `CallId` and `Error`). These callbacks are kept apart from the stable `OnEnd`, as upstream does. The order is `OnStart`, `OnExtractStart`, `OnExtractionModelCallStart`, `OnExtractionModelCallEnd`, `OnEnd`, `OnExtractEnd`, the same as `Decide.DecideAsync`. Integrations get events with `AiVersion`, `RecordInputs`, `RecordOutputs`, and `FunctionId`. `IncludeRuntimeContext` picks the `RuntimeContext` keys they see. With `RecordInputs = false` they get no state, fields, or reference date, and with `RecordOutputs = false` they get no answers. Both flags also drop the provider metadata. Request headers and provider options never reach integrations, because they can carry credentials. File URLs and references in the state are replaced with `[redacted]`, and `OnStart` and `OnEnd` still get the originals. A `DownloadError` reaches `OnError` with its URL redacted, because the URL can be a signed link. `IsEnabled = false` turns the callbacks off. `ExtractTelemetry` is gone. Pass a `TelemetryOptions` with an `ITelemetry` instead. `Extract.ExtractAsync` and `Decide.DecideAsync` also publish an `experimental_extract` and an `experimental_decide` span on the telemetry tracing channel (see "Telemetry tracing channel" below).

### OpenTelemetry integration

`Vercel.AI.OpenTelemetry.OpenTelemetry` (upstream's `OpenTelemetry` class in `@ai-sdk/otel`) is an `ITelemetry` that records spans on an `ActivitySource`. Pass `new OpenTelemetry(new OpenTelemetryOptions { ... })` in `TelemetryOptions.Integrations`, or register it with `TelemetryRegistry.RegisterTelemetry`, and add the source to your `TracerProvider`. `OpenTelemetryOptions.Tracer` (upstream's `tracer`) is the `ActivitySource`. It defaults to a source named `gen_ai`. `EnrichSpan` adds custom attributes to a span when it is created (an exception is ignored), and `AgentSpanName` (`ModelId`, `FunctionId`, or `AgentSpanName.From(callback)`) names the root `invoke_agent` span of text generation. The namespace has a type named `OpenTelemetry`, so import it with an alias when you also use the namespace.

Text generation gets a root `invoke_agent {modelId}` span, a `step N` span, a `chat {modelId}` span for each model call, and an `execute_tool {name}` span for each tool. `Extract` gets an `extract {modelId}` operation span and a child model call span with the response id, response model, and usage. `Decide` gets the `decide {modelId}` equivalents. The supplemental options are off by default: `Usage`, `ProviderMetadata`, `RuntimeContext`, `Headers`, `ExperimentalDecision` (`ai.decision.*`, with the deprecated alias `ExperimentalEvaluation`), and `ExperimentalExtraction` (`ai.extraction.state`, `ai.extraction.fields`, `ai.extraction.referenceDate`, and `ai.extraction.answers`). `RecordInputs = false` and `RecordOutputs = false` drop the input and output attributes. Extraction spans never carry file URLs or provider metadata.

The port does not record message and tool definition attributes, embed, rerank, speech, transcription, object generation, durable runs, tool choice, or inference duration metrics yet.

- **No events before the call:** invalid input and an unsupported field type throw before any event. A refused field calls `OnError` and no end callback. The model call end event holds the normalized answers that are validated and returned.
- **Runtime context:** telemetry gets only the keys of `IncludeRuntimeContext` that are `true`, and an empty context when it is null.
- **Recording flags:** `RecordInputs = false` removes the state, the fields, and the reference date from telemetry events. They go into the model prompt. `RecordOutputs = false` removes the answers. `FunctionId`, `RecordInputs`, and `RecordOutputs` are copied onto telemetry events.
- **Credentials:** telemetry events never carry `Headers` or `ProviderOptions`, because they can hold auth headers or provider API keys. `OnStart`, `OnEnd`, and the provider call still get them.
- **File locations:** telemetry replaces a file URL or provider reference in the state with `[redacted]`, because it can be a signed link. The download and `OnStart` and `OnEnd` use the original. A `DownloadError` reaches `OnError` as `[redacted]` without the URL, while the caller's exception keeps it.

`OpenAIProvider.ExtractionModel(id)` (Responses API, provider id `openai.extraction`), `AnthropicProvider.ExtractionModel(id)` (Messages, `anthropic.extraction`), and `GoogleProvider.ExtractionModel(id)` (Gemini, `google.extraction`) return an `ExtractionLanguageModel`. It wraps a V4 `ILanguageModel` and sends one structured-output request with reasoning `none`. Each field gets a nullable schema (`anyOf` the typed value and `null`) under an internal key (`f0`, `f1`, and so on), and the answers are mapped back to the field ids. Field options and `ReferenceDate` go in the prompt. An empty string or list becomes `null`. The adapter returns `null` for missing values and never returns `PNull`, `Confidence`, `Guess`, or `Source`. Image and PDF files pass through, and any other file media type throws `UnsupportedFunctionalityException` before the model is called. A finish reason of `content-filter` returns a refusal for each field. Any other finish reason except `stop`, invalid JSON, and a missing or extra value throw `InvalidResponseDataException`. `new ExtractionLanguageModel(model, provider, maxFieldsPerCall)` (a positive integer, or `double.PositiveInfinity` for no limit, the default) splits larger field maps into parallel calls of at most `maxFieldsPerCall` fields, because Anthropic caps structured output at 16 union parameters (`AnthropicProvider.ExtractionModel` sets 16). The answers are merged, usage is summed, and the response and provider metadata come from the first call. When one call fails, the whole extraction fails and the other calls are cancelled, and a caller cancel reaches every call. `ExtractionLanguageModel.Serialize(model)` and `Deserialize(options)` work like their `DecisionLanguageModel` counterparts.

## Gateway reranking models

`GatewayProvider.RerankingModel(id)` returns an `IRerankingModel`. `Reranking(id)` (upstream `rerankingModel` and `reranking`) returns the concrete `GatewayRerankingModel`, which `RerankRequest.Model` accepts. It posts `{ documents, query, topN, providerOptions }` to `{base}/reranking-model` with the `ai-reranking-model-specification-version: 4` and `ai-model-id` headers, and leaves out `topN` and `providerOptions` when you did not set them. The response `warnings`, `providerMetadata`, and headers come back on the `RerankModelResponse`. Reranking models re-score candidate documents after an initial retrieval step, which helps retrieval-augmented generation pipelines.

```csharp
var result = await Rerank.RerankAsync(new RerankRequest
{
    Model = GatewayProvider.Create().Reranking("cohere/rerank-v3.5"),
    Query = "What is the capital of France?",
    Documents = ["Paris is the capital of France.", "Berlin is the capital of Germany.", "Madrid is the capital of Spain."],
    TopN = 2,
});
```

## Gateway extraction models

`GatewayProvider.ExtractionModel(id)` (alias `Extraction(id)`, upstream `extractionModel` and `extraction`) returns a `GatewayExtractionModel`. It posts `{ stateParts, fields, referenceDate, maxOutputTokens, reasoning, providerOptions }` to `{base}/extraction-model` with the `ai-extraction-model-specification-version: 4` and `ai-model-id` headers, and leaves out each option you did not set. The `fields` are the `ExtractionField` values as given.

```csharp
var result = await Extract.ExtractAsync(new ExtractRequest
{
    Model = GatewayProvider.Create().ExtractionModel("scaledown/extract-1"),
    State = "Invoice INV-2041 from Acme Corp. Contacts: Ana, Bo.",
    Fields = new Dictionary<string, ExtractionField>
    {
        ["invoiceNumber"] = new("text", DecisionInput.From("The invoice number")),
        ["contacts"] = new("list", DecisionInput.From("Contact names")) { Items = "text" },
    },
});
```

- **Field types:** `SupportedFieldTypes` lists every type, because support depends on the model behind the Gateway. The Gateway rejects a field the model cannot extract with a 400, and the model rethrows it as `ExtractionUnsupportedFieldTypeError` with its `FieldId` and `FieldType`, after the request is sent. Any other error maps to a `Gateway*Error`.
- **Files:** send file state inline, as bytes or base64. A file with URL or text data throws `UnsupportedFunctionalityException` before any request. Pass `ExtractRequest.Download` to have `Extract.ExtractAsync` download URL files first. A file with `reference` data is sent unchanged, and the Gateway rejects it for now with a 400.
- **Response:** optional answer fields that are `null` are left out, so answers match the specification. A refusal answer reaches `Extract.ExtractAsync`, which throws `ExtractionRefusalError`. A warning this client does not know is kept as an `other` warning instead of failing a successful response.
- **Provider options:** Gateway provider options such as `user` and `tags` go under `providerOptions.gateway`.
- **Model discovery:** `extraction` is a known model type, so `GetAvailableModelsAsync()` keeps extraction models.

`ExtractRequest.ModelId` resolves a string id the same way as `DecideRequest.ModelId`: through `DecisionModels.DefaultProvider`, which must implement `IExtractionProvider`, or through the Gateway when it is null (`AI_GATEWAY_API_KEY`). `DecisionModels.ResolveExtractionModel(model, modelId)` throws `InvalidOperationException` ("The default provider does not support extraction models.") when the default provider has no `ExtractionModel`, `NoSuchModelError` (`extractionModel`) when it returns no model, and `UnsupportedModelVersionException` for a model that is not `v4`. `GatewayProvider` implements `IExtractionProvider`. Extraction models in `CustomProvider` and `ProviderRegistry` are not ported yet (#1372).

### Extraction models in registries and custom providers

A provider exposes extraction models by implementing `IExtractionProvider.ExtractionModel(id)`. `OpenAIProvider`, `AnthropicProvider`, `GoogleProvider`, and `CustomProvider` do. `ProviderRegistry.ExtractionModel("providerId:modelId")` returns the extraction model of the provider registered under `providerId`. It throws `NoSuchProviderError` for an unknown provider and `NoSuchModelError` (`ModelType` `extractionModel`) when the provider has no extraction factory or the factory returns null. Registry middleware does not wrap extraction models.

`CustomProvider.AddExtractionModel(alias, model)` registers an `IExtractionCaller`, and `AddExtractionModel(alias, "model-id")` registers a string alias that goes through `DecisionModels.DefaultProvider` when `ExtractionModel(alias)` is called. An alias wins over `FallbackProvider`. An id that is not an alias goes to the fallback provider's extraction factory, or throws `NoSuchModelError`. Ids such as `toString` or `__proto__` never resolve to an alias.

`ExtractRequest.ModelId` resolves a string id as described under Gateway extraction models: through `DecisionModels.DefaultProvider`, or the Gateway when it is null. `Model` wins when both are set. A model that is not `v4` throws `UnsupportedModelVersionException` on every path.

```csharp
var registry = new ProviderRegistry();
registry.Register("invoices", new CustomProvider
{
    FallbackProvider = openai,
}
    .AddExtractionModel("fast", openai.ExtractionModel("gpt-6.1-sol"))
    .AddExtractionModel("careful", openai.ExtractionModel("gpt-6-luna")));

DecisionModels.DefaultProvider = new CustomProvider()
    .AddExtractionModel("careful", registry.ExtractionModel("invoices:careful"));

var result = await Extract.ExtractAsync(new ExtractRequest
{
    ModelId = "careful",
    State = "Invoice INV-2041, total due $1,250.00 by March 3.",
    Fields = new Dictionary<string, ExtractionField> { ["total"] = new("number") { Decimals = 2 } },
});
```

## Gateway decision fallbacks

`providerOptions.gateway.models` for a decision request may start with one conditional entry, `{ "model": "...", "when": { ... } }`, followed by plain model ids. `when` is `confidenceBelow`, `probabilityBetween: [min, max]`, or an `any`, `all`, or `atLeast` group of conditions, nested at most 5 levels with 1 to 20 conditions per list. `GatewayDecisionModel` rejects an invalid shape with an `InvalidArgumentException` before any request. Other gateway options pass through unchanged.

## Anthropic Message Batches

`provider.ExperimentalBatch()` on the Anthropic provider starts, inspects, cancels, and lists Message Batches, and reads their results.

- `DoStartAsync` sends one Messages request per batch request. Request IDs must match `^[A-Za-z0-9_-]{1,64}$` and be unique, or it throws `InvalidArgumentException` for `requests`. Only `text` requests are accepted. Set `anthropicBeta` (an array of strings) in `providerOptions.anthropic` of the batch call. Per-request `anthropicBeta` throws `UnsupportedFunctionalityException`, and so do `speed`, `fallbacks[].speed`, message threads, a JSON-tool structured-output fallback, and a provider tool with a custom name. All of them fail before a request is sent. An `anthropicBeta` that is not an array of strings throws `InvalidArgumentException` for `providerOptions`. The `anthropic-beta` header holds the batch betas plus the betas the requests need. A `webhookUrl` adds an `unsupported` warning.
- `DoGetStatusAsync` maps `ended` to `completed`. Every other status (`in_progress`, `canceling`, or a new one) is `pending`. `RequestCounts` sums `processing`, `succeeded`, `errored`, `canceled`, and `expired` for the total. `Failed` is `errored + canceled + expired`. `ProviderMetadata.anthropic` holds `archivedAt`, `cancelInitiatedAt`, `endedAt`, `requestCounts`, and `resultsUrl`.
- `DoCancelAsync` posts to the cancel endpoint. `DoListAsync` takes `Limit` and `Cursor` (sent as `after_id`) and returns `NextCursor` only when Anthropic reports more batches.
- `DoGetResultsAsync` streams the JSON Lines results as each line arrives. A batch that is not complete throws `InvalidArgumentException` for `batchId`. An archived batch throws `InvalidArgumentException` for `batchId`. A completed batch without a results URL throws `AiSdkException` (`completed without batch output`). A succeeded item has the same content as a generated message: text with `Citations` and the raw citations in `ProviderMetadata.anthropic.citations`, reasoning, compaction text, tool calls and results (client, server, MCP, web search, web fetch, code execution, tool search, and advisor), `url` sources, and `custom` parts (`anthropic.container_upload`, `anthropic.fallback`). Code execution calls are dynamic, because the request tools are not available when the results are read. `FinishReason`, `RawFinishReason`, `Usage`, `ResponseId`, `ModelId`, and `ProviderMetadata.anthropic` (`usage`, `stopSequence`, `serviceTier`, `stopDetails`, `inputTransformations`, `safeguardResults`, `iterations`, `container`, `contextManagement`) are set. An errored item is `failed` with `ErrorMessage`, `ErrorType`, and, when Anthropic sends one, `ProviderMetadata.anthropic.requestId`. Canceled and expired items are `cancelled` and `expired`. An item that is not valid (an unknown result type, a message without the required fields, or a known content block with missing fields) is `failed` with `ErrorCode` `invalid_response` and the message `Anthropic returned an invalid Message batch result.`. The remaining items still arrive. Unknown content block types are skipped. `AnthropicOptions.GenerateId` creates the ids of the `url` sources (upstream `generateId`). It also applies to generated messages.

## Message prompts in batches

A `text` `BatchRequest` takes either `Prompt` (a string) or `Messages` (a `ModelMessage` list), plus `Instructions` or `System`, like `generateText`. Combining `Prompt` and `Messages` throws `InvalidPromptException`. `Batch.StartBatchAsync` passes the batch API's `SupportedUrls` to `LanguageModelPrompts.ConvertToLanguageModelPromptAsync` (upstream `supportedUrls` of `convertToLanguageModelPrompt`). File parts whose URL matches a supported pattern stay URLs, and the others are downloaded with the default download function. OpenAI and xAI batches convert file parts the same way as their Responses models.

## Structured output in batches

`StartBatchOptions.Output` (upstream `output` of `startBatch`) takes an `OutputSpec`, as `GenerateTextOptions.Output` does. `Batch.StartBatchAsync` sends its response format (`responseFormat`) with every `text` request in the batch. `image` requests ignore it. It applies to the whole batch, not to one request. To use different schemas, start separate batches.

`Batch.StartBatchAsync` takes `StartBatchOptions` (upstream `StartBatchOptions`), which has no `MaxRetries`: starting a batch never retries. The other batch functions take `BatchRequestOptions`. `BatchStartResult` and `ListedBatch` are `BatchReference` values (upstream `Batch = BatchReference & BatchStatus`), so `BatchStatus` derives from `BatchReference`.

Results are retrieved separately, so pass the same `Output` to `Batch.GetBatchResults` (or `GetAllBatchResultsAsync`) to parse and validate each successful text item. An item then has either `Output` (the parsed `JsonElement`) or `OutputError` (a `BatchError` with `Message` and `Type`). A parsing or validation failure does not fail the item: `Text`, `Usage`, and the other results stay available, so you can resubmit only the requests whose response was not valid. `OutputError.Type` is `AI_NoObjectGeneratedError` when the text is not valid JSON or does not match the schema, and `AI_NoOutputGeneratedError` when the item did not finish with a response (for example, a tool call). A JSON output parses `BatchTextGeneration.StructuredOutputText` when a provider sets it, and the joined text otherwise. Failed items and image items get neither field, and without `Output` the raw JSON is in `Text` (the joined text content parts; `StructuredOutputText` is not copied into `Text`).

To get a typed output, build the specification with a type argument, for example `OutputSpec.Object<Translation>(schema)` (or `OutputSpec.Array<Item>(elementSchema)`), and call `Batch.GetBatchResults<TOutput>(request, output)`. Each item is a `BatchItemResult<TOutput>` whose `Output` is a `TOutput?` (property names match the JSON case-insensitively, camelCase by default) and whose `OutputError` is set instead when parsing or deserializing failed. `StartBatchOptions.Output` still takes any `OutputSpec`, so a typed specification can be passed to `StartBatchAsync` as is. `OutputSpec.Text()` and `OutputSpec.Choice(options)` return an `OutputSpec<string>`, and `OutputSpec.Json()` returns an `OutputSpec<JsonElement>`, so `GetBatchResults<string>(request, OutputSpec.Choice(options))` gives a typed `Output`. For `GenerateTextAsync` and `StreamTextAsync`, see [Typed output](streaming.md#typed-output).

The Google Batch API does not reliably enforce JSON schemas, so `GoogleBatchApi.DoStartAsync` throws `UnsupportedFunctionalityException` (`Functionality` is `structured output schemas in Google batches`) for a `text` request whose response format is JSON with a schema, before it sends a request. `Output` without a schema (JSON mode) is allowed and sends `responseMimeType: application/json`. To send a schema-less request while keeping a schema for validation, set `providerOptions.google.structuredOutputs` to `false`: the schema is not sent, and you validate the results with `Batch.GetBatchResults` and the same `Output`.

## Google Batch

`GoogleProvider.ExperimentalBatch()` (upstream `experimental_batch()`) returns the Gemini Batch API. Its provider id is `google.generative-ai.batch`.

- `DoStartAsync` accepts `text` and `image` requests. Every request in a batch must use the same model, because the model is part of the endpoint (`models/<id>:batchGenerateContent`). Other types throw `UnsupportedFunctionalityException` (`batch request type: <type>`) before a request is sent. The batch display name is `ai-sdk-batch-<id>`, with the id from `GoogleOptions.GenerateId`. Requests are sent inline while the creation body stays under 20,000,000 bytes. Beyond that, the requests are uploaded as a JSONL file with the resumable upload protocol and the batch references it (`inputConfig.fileName`). A file over 2 GB throws `InvalidArgumentException`. The start result then has `ProviderMetadata.google.inputFileId` and, when Google reports it, `inputFileExpiresAt`. A `webhookUrl` is sent as `webhookConfig.uris`. Request warnings keep their request id.
- `DoGetStatusAsync` maps `metadata.state` (`BATCH_STATE_` or `JOB_STATE_` prefix) to `pending`, `completed`, or `failed`. Without a state, `done` and `error` decide. Int64 counts arrive as strings and are normalized, and an omitted pending, successful, or failed count is `0`. A top-level `error` becomes `ErrorMessage` and `ErrorCode`.
- `DoCancelAsync` posts to `<batch id>:cancel`. `DoListAsync` reads `batches` with `pageSize` (`Limit`) and `pageToken` (`Cursor`), and returns `nextPageToken` as `NextCursor`.
- Mask-based image edits and more than one image in a single request throw `UnsupportedFunctionalityException` (`mask-based image editing in Google batches`, `multiple images per Google batch request`) before a request is sent. A `size` on an image request adds an `unsupported` warning; use `aspectRatio`.

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

### OpenAI API key

`OpenAIOptions.ApiKey` is a `Resolvable<string>`: a string, a `Task<string>`, or `Resolvable<string>.From(...)` with a synchronous or asynchronous callback (`Func<string>`, `Func<Task<string>>`, or `Func<CancellationToken, Task<string>>`). A callback is called before each request, so it can supply a refreshed key; a `Task<string>` resolves to the same key each time. The key still defaults to `OPENAI_API_KEY`. Chat, Responses, completion, embedding, image, speech, transcription, speech translation, decision, file, skill, batch, realtime and live calls all resolve it per request. `OpenAIProvider.CreateOpenAIHeadersAsync(callHeaders, cancellationToken)` replaces the synchronous `CreateOpenAIHeaders`.

### OpenAI prompt caching

For GPT-5.6 and later models, `providerOptions.openai.promptCacheOptions` configures prompt caching on the Responses API. `mode` is `implicit` or `explicit`. `ttl` sets the minimum cache lifetime: `30m` (the default) or `12h`. 12-hour writes are billed at a higher rate than 30-minute writes. Any other `ttl` throws `InvalidArgumentException`. Set `prewarm` to `true` to write the prompt to the cache without generating output; the result then has no content. Chat Completions accepts `30m` only.

```csharp
var options = new LanguageModelCallOptions
{
    Prompt = prompt,
    ProviderOptions = new Dictionary<string, JsonElement>
    {
        ["openai"] = JsonDocument.Parse("{\"promptCacheOptions\":{\"mode\":\"implicit\",\"ttl\":\"12h\",\"prewarm\":true}}").RootElement.Clone(),
    },
};
```

In explicit mode, only content marked with `promptCacheBreakpoint` is cached. Set `{"mode":"explicit","ttl":"12h"}` under `openai` in the `ProviderOptions` of a system message, a `TextContentPart`, or a `FileContentPart`; the Responses request carries it as `prompt_cache_breakpoint`. `Usage.CacheWriteTokens` reports the tokens written, and `Usage.Raw` holds `input_tokens_details.cache_write_tokens_details` with `ttl_30m_tokens` and `ttl_12h_tokens`.

### OpenAI message names

Chat Completions accepts an optional participant name on each system, developer, user, or assistant message. Set `{"name":"alice"}` under `openai` in the `ProviderOptions` of the `SystemModelMessage`, `UserModelMessage`, or `AssistantModelMessage` to help the model tell participants with the same role apart. Use pseudonyms instead of personal identifiers when possible; names are sent to the provider with the message.

```csharp
var alice = new UserModelMessage("My favorite color is blue.")
{
    ProviderOptions = new Dictionary<string, JsonElement>
    {
        ["openai"] = JsonDocument.Parse("{\"name\":\"alice\"}").RootElement.Clone(),
    },
};
```

Set the name on the message, not on a content part or in the call-level provider options. A name that is not a string throws `InvalidArgumentException` (`invalid openai provider options`). `ToolModelMessage` does not support names: a name in its `MessageProviderOptions` is omitted with an `unsupported` warning (`message name on tool messages`). The option applies to Chat Completions (`ChatModel`), including Azure chat models. The Responses model does not support message names. A name is separate from the call-level `user` and `safetyIdentifier` options.

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

`JsonStreams.ReadJsonAsync` (upstream `createJsonResponseHandler`) takes `experimentalBinaryFields`, a list of paths of string fields that carry large base64 data, such as `images[*]` or `content[*].data.data`. A path is a sequence of plain object keys (letters, digits, `_`, and `-`) joined by `.`, with `[*]` for every element of an array. An invalid path throws `InvalidArgumentError` before the body is read. With paths, the body is read in chunks (`ResponseBodies.ReadResponseWithSizeLimitAsync` passes each chunk to its `onChunk` callback instead of collecting it) and `JsonBinaryFieldScanner` (upstream `json-binary-field-scanner.ts`) decodes the strings at those paths while they arrive. Only canonical standard-alphabet base64 that decodes to at least 64 KiB (`JsonBinaryFieldScanner.DefaultBinaryFieldMinBytes`) is extracted. `JsonBody.Value` and `JsonBody.RawValue` hold a placeholder string in its place, the parse error text holds the placeholder too, and `JsonBody.Binaries` maps each placeholder to its `byte[]`. A `JsonElement` cannot hold bytes, so `ReadJsonAsync` differs from upstream, where `value` holds the `Uint8Array`. `JsonBinaryFieldScanner.ReplaceBinaryFieldPlaceholders` turns a parsed value into a tree where the placeholders are replaced with their bytes.

`ResponseHandlers.CreateJsonResponseHandler<T>(validate, experimentalBinaryFields)` is the typed form. It returns a `JsonResponseHandler<T>` and rejects an invalid path when it is created. `validate` receives the parsed body as `bool`, `double`, `string`, `List<object?>`, `Dictionary<string, object?>`, or `null`, with the decoded `byte[]` in place of each placeholder, and returns the `T`. A `null` `validate` returns that value as it is. The result's `Value` holds the bytes and `RawValue` holds the placeholders. A body that is not JSON, or a validator that throws, throws an `ApiException` ("Invalid JSON response") whose cause is a `JsonParseException` or a `TypeValidationException`; both, and `ResponseBody`, hold the placeholders, never the bytes. Upstream also makes `JSON.stringify` of the decoded arrays print the placeholder while an error can refer to them (`withBinaryFields`, `maskBinariesInJson`). A `byte[]` has no such hook, so a validator must not print the bytes into its own error message.

`ApiException` now also carries `Url`, `RequestBodyValues`, `ResponseHeaders`, and `IsRetryable` (408, 409, 429, and 500 and above by default) and `ErrorData`, the provider error parsed from the response body (upstream `APICallError.data`). It is a `JsonElement` when `CreateJsonErrorResponseAsync` parsed the body, and `null` otherwise. It is not called `Data` because `Exception.Data` already exists.

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

The Gateway image model sends `seed`, `files`, `mask`, `providerOptions`, and call headers to `/image-model`. Byte files are encoded as base64, base64 files pass unchanged, and URL files stay URLs. Its `ImageModelResult` carries the response `Images` as the base64 strings the server returned, `Warnings` (`unsupported`, `compatibility`, `deprecated`, and `other`), `ProviderMetadata`, `IsRetryable`, `Response` (model id, timestamp, headers), and `Usage` (`InputTokens`, `OutputTokens`, `TotalTokens`, each null when the server omitted it). `MaxImagesPerCall` is `int.MaxValue`, so requests are never split client-side.

`IEmbeddingModel.DoEmbedAsync` takes `int? dimensions = null` and `IReadOnlyDictionary<string, string?>? headers = null` after the cancellation token, and `EmbeddingModelCall` carries `Dimensions`. `IRerankingModel.DoRerankAsync` takes the same `headers` parameter. Call headers override the provider headers. `EmbedOptions.Headers` and `RerankOptions.Headers` pass them from `AiClient`. Custom models must add the parameters.

`ImageCallOptions.Files` and `Mask` are `ImageModelFile` values, the same file-or-URL union as upstream's `ImageModelV4File`. Create one with `ImageModelFile.FromFile(bytes, mediaType)`, `ImageModelFile.FromBase64(base64, mediaType)` (base64 text that the Gateway passes unchanged), or `ImageModelFile.FromUrl(url)`, and set `ProviderOptions` on it when a provider reads per-file options. `OpenAICompatibleImageFile` is gone. Use `ImageModelFile` instead.

`ImageGenerationResult` also carries `ProviderMetadata` and `IsRetryable` (`false` when the provider filtered the content, so an empty result is not retried).

### Google image models

`GoogleImageModel` runs Gemini image generation (`gemini-*` ids only) through the Gemini language model with the `IMAGE` response modality. The `google` entry of `ImageCallOptions.ProviderOptions` passes to that call, so safety settings, thinking, and the other [Google language model options](https://ai-sdk.dev/providers/ai-sdk-providers/google-generative-ai) apply. `Seed`, `AspectRatio` (merged into `imageConfig`), and call `Headers` are sent too. `Size` adds an `unsupported` warning. Use `AspectRatio` instead.

Pass `{ "google": { "googleSearch": { } } }` to ground the image with Google Search. The value is the args of the `google.google_search` tool, for example `{ "searchTypes": { "imageSearch": {} } }`.

`Files` are sent as inline images for editing. A URL file throws because its media type `image/*` is not passed as inline bytes. A `Mask` throws `AiSdkException`. `SupportsFileInputs` is `true` and `SupportsMaskInputs` is `false` for the Gemini image models that take input images, and both are `null` otherwise.

The result carries `Usage`, `Response` (timestamp, model id, headers), and `ProviderMetadata` under `google`: the language model metadata (`promptFeedback`, `groundingMetadata`, `usageMetadata`, and so on) plus `finishReason` (the raw candidate finish reason) and `images` (one object per image). A blocked prompt returns no images with `promptFeedback` in the metadata, and `IsRetryable` is `false` when the finish reason is a content filter. `GoogleImageResult` is gone. Use `ImageGenerationResult.CallWarnings`.

### Decide and the deprecated Evaluate names

`Decide.DecideAsync(DecideRequest)` is `experimental_decide`. It answers typed questions against one shared state. A decision model implements `IDecisionCaller.DoDecideAsync(DecisionModelCall)` and returns a `DecisionModelResult`. Operation ids are `ai.decide` and `ai.decide.doDecide`. `DecideRequest.Telemetry` is a `TelemetryOptions`. Its `ITelemetry` integrations get `OnDecideStart`, `OnDecideEnd`, `OnDecisionModelCallStart`, `OnDecisionModelCallEnd`, and `OnError`, with `AiVersion`, the recording flags, and `FunctionId` on the events. With `RecordInputs = false` they get no state or questions, and with `RecordOutputs = false` no answers. `DecideTelemetry` is gone. Lifecycle events are `DecideEvent` and `DecisionModelEvent`. The refused-question and unsupported-type errors are `DecisionRefusalError`, `DecisionUnsupportedQuestionTypeError` (marker `vercel.ai.error.AI_DecisionUnsupportedQuestionTypeError`), and `DecisionUnsupportedQuestionTypeException`.

`DecideRequest.MaxOutputTokens` (an integer of at least 1, otherwise `InvalidArgumentError` is thrown before any model call) and `DecideRequest.Reasoning` (`provider-default`, `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, or `max`) reach the model as `DecisionModelCall.MaxOutputTokens` and `DecisionModelCall.Reasoning`, and `DecideEvent` reports both in `OnStart` and `OnEnd`. `DecisionLanguageModel` forwards them to the wrapped language model and uses reasoning `none` when `Reasoning` is null. Set `Reasoning = "provider-default"` to use the wrapped model's own default effort instead. `GatewayDecisionModel` adds `maxOutputTokens` and `reasoning` to the request body when set. `OpenAIDecisionModel`, `PerplexityDecisionModel`, and `TypeSafeAiDecisionModel` do not send either setting and report an `unsupported` warning for `maxOutputTokens` or `reasoning` when it is set.

The old names still compile and are marked `[Obsolete]`: `Evaluate.EvaluateAsync`, `EvaluateRequest`, `EvaluateResult`, `EvaluateEvent`, `EvaluateModelEvent`, `IEvaluationCaller.DoEvaluateAsync`, `EvaluationQuestion`, `EvaluationAnswer`, `EvaluationRounding`, `EvaluationModelCall`, `EvaluationModelResult`, and the two `EvaluationUnsupportedQuestionType*` types. C# cannot make two names one type, so each alias derives from its decision type, and `Evaluate.EvaluateAsync` forwards to `Decide.DecideAsync`. A model that implements only `IEvaluationCaller` still runs through `Evaluate.EvaluateAsync`. To pass it to `Decide.DecideAsync`, wrap it with `DecisionCaller.FromLegacy(model)`. Both `Decide.DecideAsync` and `DecisionLanguageModel` throw the unsupported-type error as `EvaluationUnsupportedQuestionTypeException`, so a `catch` of the old or the new type matches, and `DecisionUnsupportedQuestionTypeError.IsInstance` recognizes it. Deprecated telemetry callbacks (`ITelemetry.OnEvaluateStart`, `OnEvaluationModelCallStart`, `OnEvaluationModelCallEnd`, and `OnEvaluateEnd`) run once and receive the decision operation ids. When both a decision callback and its deprecated alias are set, only the decision callback runs. The rubric API is a different feature. See "Rubric models" below.

#### Decision models in registries and custom providers

A provider exposes decision models by implementing `IDecisionProvider.DecisionModel(id)`. `GatewayProvider` does. `ProviderRegistry.DecisionModel("providerId:modelId")` returns the decision model of a registered provider. Custom separators and colons inside the model id work as they do for other model types. Language model middleware does not wrap decision models. `CustomProvider.AddDecisionModel(alias, model)` registers an alias, and `CustomProvider.DecisionModel(alias)` returns it. An unknown alias goes to the fallback provider's decision factory when it has one. A provider without a decision factory, or one that returns null, throws `NoSuchModelError` with `ModelType` `decisionModel`. An unknown registry provider throws `NoSuchProviderError`. A model whose `SpecificationVersion` is not `v4` throws `UnsupportedModelVersionException`.

The deprecated factory `IEvaluationProvider.EvaluationModel(id)` and the deprecated alias map `CustomProvider.AddEvaluationModel(alias, IEvaluationCaller)` still resolve through `DecisionModel`. A provider that implements both factories is asked through `IDecisionProvider` only. The deprecated alias map is ignored once any alias is added with `AddDecisionModel`. `ProviderRegistry.EvaluationModel(id)` and `CustomProvider.EvaluationModel(id)` are deprecated aliases of `DecisionModel(id)`. They return the same decision model as an `IEvaluationDecisionCaller`, which also has the deprecated `DoEvaluateAsync`. A model that already implements `IEvaluationDecisionCaller` is returned unchanged. Any other model is wrapped. The wrapper's `DoEvaluateAsync` calls the model's own `IEvaluationCaller.DoEvaluateAsync` when it has one, and `DoDecideAsync` otherwise.

#### String model ids and the default provider

`DecideRequest` takes the model as `Model` (an `IDecisionCaller`) or as `ModelId` (a string). `Model` wins when both are set, and `AiSdkException` ("Model or ModelId is required.") is thrown when neither is. A model id is resolved through `DecisionModels.DefaultProvider` (`AI_SDK_DEFAULT_PROVIDER` upstream), which must implement `IDecisionProvider` or the deprecated `IEvaluationProvider`. Otherwise `DecisionModels.Resolve` throws `InvalidOperationException` ("The default provider does not support decision models."). When `DefaultProvider` is null, the id goes through `GatewayProvider` and its `AI_GATEWAY_API_KEY` key. `DecisionModels.GatewayFactory` replaces that provider. A provider that returns no model throws `NoSuchModelError` (`decisionModel`), and errors thrown by the provider pass through. The `v4` check applies whether the model comes from an instance, an alias, a registry, or the default provider. `CustomProvider.AddDecisionModel(alias, "provider-model-id")` registers a string alias. `CustomProvider.DecisionModel(alias)` resolves it through the default provider.

#### Rubric models

`AiClient.EvaluateAsync(EvaluateOptions)` scores a candidate against a rubric and returns a `RubricResult`. Upstream has no such API, so the port names it after the rubric. A model that scores directly implements `IRubricModel` and is set on `EvaluateOptions.RubricModel`. Without one, a language model scores the candidate. `ProviderBase.RubricModel(id)`, `ProviderRegistry.RubricModel("providerId:modelId")`, `CustomProvider.RubricModel(id)`, and `CustomProvider.AddRubricModel(id, model)` return and register rubric models. `AnthropicProvider.RubricModel(id)` returns an `AnthropicEvaluationModel`.

These names replace the old rubric names, because `EvaluationModel` now means the deprecated decision accessor. `IEvaluationModel`, `EvaluationResult`, `EvaluateOptions.EvaluationModel`, and `CustomProvider.AddEvaluationModel(id, IEvaluationModel)` still compile and are marked `[Obsolete]`. `RubricModels.FromLegacy(model)` wraps an `IEvaluationModel` as an `IRubricModel`. `ProviderBase.EvaluationModel` is gone; override `RubricModel` instead. `AnthropicProvider.EvaluationModel(id)` now returns the decision model; call `RubricModel(id)` for the rubric model.
- Provider options namespaces: `ProviderOptionsResolver.Resolve` (maps to `resolveProviderOptions`) reads options that a model accepts under several `providerOptions` keys. With `ProviderOptionsResolution.First` the keys are aliases and the first present one wins as a whole (for example `amazonBedrock` then `bedrock`, or `googleVertex` then `vertex`). With `ProviderOptionsResolution.Merge` every present key is parsed and merged in order, so later keys win key by key (for example `openai` then `azure` for Azure chat). A key that is missing or `null` is skipped, duplicate keys are read once, and the `parse` callback you pass validates each present key and throws for an invalid one. The result carries the options and the keys that contributed. Claude on Google Vertex (`GoogleVertexAnthropicProvider`) reads the legacy `vertex` key at request level when `googleVertex` is absent. `googleVertex` wins when both are set, and provider metadata is written under `googleVertex` only when that key was used.

## Gladia transcription metadata

`GladiaProvider.Transcription().TranscribeAsync` returns the parsed Gladia response on `GladiaTranscriptionResult.ProviderMetadata` under `gladia`. The port reads only what it needs for the result (`result.metadata.audio_duration`, and `result.transcription.full_transcript`, `languages`, and `utterances` with `start`, `end`, and `text`). Everything else in the response passes through as Gladia returns it, so an unexpected add-on payload never fails a successful transcription.

These values are available under `gladia`:

- `status`: `done`. A failed job throws instead.
- `result.metadata`: `audio_duration`, `number_of_distinct_channels`, `billing_time`, and `transcription_time`.
- `result.transcription`: `full_transcript`, `languages`, and `utterances`, plus `sentences` and `subtitles` when the matching option is enabled.
- `result.translation`, `result.summarization`, `result.named_entity_recognition`, `result.sentiment_analysis`, `result.audio_to_llm`, and `result.sentences`: the add-on results, when the matching option is enabled. Gladia deprecated `result.sentences`. The same content is in `result.transcription.sentences`.

Each add-on reports `success`, `is_empty`, `exec_time`, and `error` next to its `results`. Check them before you read the results. `audio_to_llm` items keep their own `success`, `is_empty`, `exec_time`, and `error`.

When Gladia reports `status: "error"`, `TranscribeAsync` throws `AiSdkException`. If the response has `error_code` (the HTTP status code of the failed request), the message is `Transcription job failed with error code <code>.`, otherwise `Transcription job failed.`

## Async provider credentials

`OpenAICompatibleOptions.ApiKey` is a `Resolvable<string>?` (upstream `apiKey?: Resolvable<string>`). It accepts a string, a `Task<string>`, or `Resolvable<string>.From(...)` with a synchronous callback (`Func<string>`), an asynchronous callback (`Func<Task<string>>`), or a callback that takes the call's `CancellationToken` (`Func<CancellationToken, Task<string>>`). A callback runs before each request, so it can return a refreshed credential. A callback is responsible for its own caching and refreshing. A `Task<string>` resolves to the same key every time. This applies to every provider built on `OpenAICompatibleOptions`, including OpenAI-compatible, Alibaba, AssemblyAI, Baseten, Black Forest Labs, ByteDance, Cerebras, DeepInfra, DeepSeek, Deepgram, ElevenLabs, Fireworks, Gladia, GMI Cloud, Groq, HeyGen, Hugging Face, Hume, Liquid, Mistral (including realtime sessions), Moonshot, Perplexity, Prodia, QuiverAI, Rev.ai, Together AI, and Voyage.

```csharp
var provider = FireworksProvider.Create(
    new OpenAICompatibleOptions
    {
        ApiKey = Resolvable<string>.From(async ct => await tokenCache.GetAsync(ct)),
    }
);
```

`OpenAICompatibleProvider.CreateHeadersAsync(callHeaders, cancellationToken)` (upstream `getHeaders`) resolves the key and builds the request headers. It replaces the synchronous `CreateHeaders`, and `ResolveApiKeyAsync(cancellationToken)` replaces `ResolveApiKey`. A key that resolves to an empty value is treated as missing, so the environment variable still applies. `DeepgramOptions.ApiKeyProvider` is removed: set `ApiKey = Resolvable<string>.From(...)` instead.

## Cartesia streaming transcription

`CartesiaProvider.TranscriptionModel(id)` streams live audio with Ink 2 over Cartesia's WebSocket API (`/stt/turns/websocket`, or `/stt/websocket` when turn detection is off). Pass the model to `StreamTranscribe.Start`. Ink Whisper (and dated snapshots such as `ink-whisper-2025-06-04`) is batch-only: streaming it throws `UnsupportedFunctionalityException`. Any other id streams, and calling `DoGenerateAsync` on a streaming id throws `UnsupportedFunctionalityException`. `CartesiaModelCatalog.GetCartesiaTranscriptionModelEntry(id).Streaming` reports which.

```csharp
var provider = CartesiaProvider.Create(new CartesiaOptions { ApiKey = key });
var audio = new AudioChunkStream();
var result = StreamTranscribe.Start(new StreamTranscribeRequest
{
    Model = (ITranscriptionCaller)provider.TranscriptionModel("ink-2"),
    Audio = audio,
    InputAudioFormat = new AudioFormat("audio/pcm", 24000),
    ProviderOptions = OperationJson.Parse(@"{""cartesia"":{""language"":""en"",""streaming"":{""keyterms"":[""Cartesia"",""Sonic""],""turnEndTimeoutMs"":800}}}"),
});
```

- **Access token.** The provider creates a short-lived, STT-scoped access token (`POST /access-token`) before it opens the socket, so the API key is not in the WebSocket URL. `ProviderRequest.Body` is the socket URL without the token. Send audio at about its playback rate.
- **Options.** Ink 2 supports English only (`language` other than `en` throws `InvalidArgumentException`). `timestampGranularities` warns. `streaming.turnDetection` (default true) uses Cartesia's native turn detection; with `false` the transcript is finalized when the audio stream ends. `streaming.keyterms` takes up to 100 terms. `turnStartThreshold`, `turnEagerEndThreshold`, `turnEndThreshold`, and `turnEndTimeoutMs` (an integer) tune turn detection and warn when `turnDetection` is `false`. Invalid options throw `InvalidArgumentException` before any request.
- **Encoding.** `audio/pcm`, `audio/pcmu`, and `audio/pcma` map to `pcm_s16le`, `pcm_mulaw`, and `pcm_alaw`; any other type throws. `streaming.encoding` (`pcm_s16le`, `pcm_s32le`, `pcm_f16le`, `pcm_f32le`, `pcm_mulaw`, `pcm_alaw`) overrides it. Widening `audio/pcm` to another linear encoding is silent; any other override that contradicts the declared type adds an `other` warning.
- **Settings.** `CartesiaOptions.Version` sets the `Cartesia-Version` header and the socket's `cartesia_version` (default `2026-03-01`). `CartesiaOptions.WebSocket` takes a `WebSocketFactory`.
- **Parts.** Turn updates are `transcript-partial` parts and turn ends are `transcript-final` parts, both with `Id` set to Cartesia's `request_id`. `finish` joins the final text (with a space when turn detection is on) and sums the reported duration.

## Deepgram streaming transcription

`DeepgramProvider.TranscriptionModel(id)` also streams live audio over Deepgram's [`/v1/listen` WebSocket API](https://developers.deepgram.com/reference/speech-to-text/listen-streaming). Pass the model to `StreamTranscribe.Start`. Nova models stream. Flux models use a separate `/v2/listen` protocol, and `DeepgramModelCatalog.GetDeepgramTranscriptionModelEntry(id).Streaming` is `false` for them, so streaming one throws `UnsupportedFunctionalityException`.

```csharp
var provider = DeepgramProvider.Create(new DeepgramOptions { ApiKey = key });
var audio = new AudioChunkStream();
var result = StreamTranscribe.Start(new StreamTranscribeRequest
{
    Model = (ITranscriptionCaller)provider.TranscriptionModel("nova-3"),
    Audio = audio,
    InputAudioFormat = new AudioFormat("audio/pcm", 16000),
    ProviderOptions = OperationJson.Parse(@"{""deepgram"":{""language"":""en"",""streaming"":{""interimResults"":true,""endpointing"":300}}}"),
});

audio.Write(pcmBytes); // or audio.Write(base64String)
audio.Complete();      // flushes the remaining transcript

await foreach (var part in result.FullStream)
{
    if (part.Type is "transcript-partial" or "transcript-final")
    {
        Console.WriteLine(part.Type + " " + part.Text);
    }
}

Console.WriteLine(await result.Text);
```

- **Input audio.** `AudioFormat` maps to `SharedV4AudioFormat`: a media type and a sample rate. Chunks are raw 16-bit little-endian PCM (`audio/pcm`), G.711 μ-law (`audio/pcmu`), or G.711 A-law (`audio/pcma`). `Rate` must be a positive integer. A string given to `AudioChunkStream.Write` is base64 audio. Empty chunks are skipped. Completing the stream flushes the remaining transcript before the result finishes. Idle connections send keepalive messages, and cancellation closes the socket and cancels the audio source (`AudioChunkStream(onCancel)`). Reading pauses while more than 1 MiB is queued on the socket.
- **Authentication.** The provider sends the API key as WebSocket subprotocols (`token`, key), so no custom header is needed. Never put a key in the URL. To use a short-lived token, set an `Authorization: Bearer <token>` header on the provider or on the call. Any other scheme throws `InvalidArgumentException`. `TranscriptionStreamStart.Request.Body` holds the URL without credentials.
- **API key.** `ApiKey` is a `Resolvable<string>`: a string, a `Task<string>`, or `Resolvable<string>.From(...)` with a sync or async callback. The provider resolves it on every call (batch and streaming), like upstream's resolvable `apiKey`. An `Authorization` header in `Headers` still wins.
- **Sample rate.** `AudioFormat.Rate` is a `double?`, like upstream's `rate: number`. Deepgram streaming accepts only positive integer rates and rejects others such as `1.5`.
- **Custom WebSocket.** `DeepgramOptions.WebSocket` takes a `WebSocketFactory` that returns an `IWebSocketLike`. It defaults to `ClientWebSocket`. `ConnectToWebSocket.Connect`, `WebSocketUtils.ToWebSocketUrl`, and `WebSocketUtils.WaitForWebSocketBufferDrainAsync` are the shared transport helpers that providers build on.
- **Options.** Live transcription supports `language`, `smartFormat`, `punctuate`, `diarize`, `fillerWords`, `redact`, `replace`, `search`, and `keyterm`. Batch-only options produce `unsupported` warnings, unless they are `false`. The `providerOptions.deepgram.streaming` object accepts `interimResults` (bool), `endpointing` (a number of milliseconds, or `false`), `channels` (integer), `multichannel` (bool), `vadEvents` (bool), and `utteranceEndMs` (integer, at least 1000). Batch calls (`DoGenerateAsync`) warn about `streaming`.
- **Parts.** Revisable results are `transcript-partial` parts (`StartSecond`, `DurationInSeconds`, `ChannelIndex`). Finalized results are `transcript-final` parts (`StartSecond`, `EndSecond`, `ChannelIndex`). Both carry `ProviderMetadata.deepgram.speechFinal`. `finish` joins the finalized text and word timings in chronological order across channels, reports Deepgram's duration as `DurationInSeconds`, and reports `Usage.Seconds` when Deepgram sends `Metadata.duration`. With `IncludeRawChunks`, every provider event is also emitted as a `raw` part.

## OpenAI transcription

`OpenAIProvider.TranscriptionModel(id)` returns an `OpenAITranscriptionModel`. It implements `ITranscriptionModel` and `ITranscriptionCaller`, so it works with `Transcribe` and `StreamTranscribe`. What a model can do comes from `OpenAIModelCatalog.GetOpenAITranscriptionModelEntry`, which resolves a dated snapshot such as `gpt-realtime-whisper-2026-01-01` to its base model:

| Model | Transcription | Streaming | Delay | Language option |
| --- | --- | --- | --- | --- |
| `whisper-1`, `gpt-4o-mini-transcribe`, `gpt-4o-transcribe`, `gpt-4o-transcribe-diarize` | yes | no | no | `language` |
| `gpt-transcribe` | yes | yes | no | `languages` |
| `gpt-live-transcribe` | no | yes | yes | `languages` |
| `gpt-realtime-whisper` | no | yes | yes | `language` |

Calling `TranscribeAsync` on a model without transcription, or streaming a model without streaming, throws `UnsupportedFunctionalityException`. Unknown model ids are tried against the API.

For `gpt-transcribe`, use `languages` for the expected languages and `keywords` for names, acronyms and domain vocabulary. `include`, `languages` and `keywords` are sent as repeated `include[]`, `languages[]` and `keywords[]` multipart fields. Empty arrays are not sent. Use `languages` instead of `language` with `gpt-transcribe`; do not send both.

```csharp
var result = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = (ITranscriptionCaller)provider.TranscriptionModel("gpt-transcribe"),
    Audio = wavBytes,
    ProviderOptions = OperationJson.Parse(@"{""openai"":{""languages"":[""en"",""fr""],""keywords"":[""premium plan"",""AC-42""],""prompt"":""A customer support call.""}}"),
});
```

### Streaming

Streaming transcription runs over the OpenAI Realtime WebSocket `{BaseUrl}/realtime?intent=transcription`. It is used with `StreamTranscribe.Start`:

```csharp
var audio = new AudioChunkStream();
var result = StreamTranscribe.Start(new StreamTranscribeRequest
{
    Model = (ITranscriptionCaller)provider.TranscriptionModel("gpt-realtime-whisper"),
    Audio = audio,
    InputAudioFormat = new AudioFormat("audio/pcm", 24000),
    ProviderOptions = OperationJson.Parse(@"{""openai"":{""language"":""en"",""streaming"":{""delay"":""low""}}}"),
});
```

- **Session.** The stream sends `session.update` when the socket opens, then `input_audio_buffer.append` for each audio chunk, and `input_audio_buffer.commit` when the audio ends. `TranscriptionStreamStart.Request.Body` holds the `session.update` message. The `language` option is sent as `language`, or as a one-entry `languages` list for models whose language field is `languages`. `streaming.delay` is sent for models with a delay setting, and `streaming.include` is sent as the session `include`.
- **Parts.** `conversation.item.input_audio_transcription.delta` becomes a `transcript-delta` part. `conversation.item.input_audio_transcription.completed` becomes a `transcript-final` part and then `finish`, with no segments and the `language` option as `Language`. An `error` event ends the stream with an `AiSdkException` that carries the server message. A close before the transcript completes ends the stream without an error. With `IncludeRawChunks`, every event is also emitted as a `raw` part.
- **Warnings.** The REST-only options `include`, `languages`, `keywords`, `prompt`, `temperature` and `timestampGranularities` produce `unsupported` warnings named `providerOptions.openai.{option}`. `streaming.delay` on a model without a delay setting, such as `gpt-transcribe`, produces `providerOptions.openai.streaming.delay`.
- **Authentication.** A native WebSocket cannot send headers, so the bearer token goes in the `openai-insecure-api-key.{token}` subprotocol next to `realtime`, and the `Authorization` header is removed because OpenAI rejects handshakes that send both. Other headers are passed on. The scheme is case-insensitive, and a per-call header overrides the provider header whatever its casing. Without an `Authorization` header only `realtime` is requested.
- **Custom socket.** `OpenAIOptions.WebSocket` sets the `WebSocketFactory`. It defaults to `ClientWebSocket`. Backpressure pauses audio reads while the socket buffer is full, and cancelling the stream closes the socket and cancels the audio (`AudioChunkStream(onCancel)`).

## OpenAI speech translation

Speech translation is experimental. `OpenAIProvider.SpeechTranslationModel(id)` returns an `OpenAISpeechTranslationModel`. It implements `ISpeechTranslationCaller`, so it works with `StreamTranslate`. It translates live speech over the OpenAI Realtime translations WebSocket (`wss://.../realtime/translations?model={id}`) and is streaming-only. The one model is `gpt-realtime-translate`, which produces translated audio, translated text, and a source transcript.

```csharp
var audio = new AudioChunkStream();
var result = StreamTranslate.Start(new StreamTranslateRequest
{
    Model = (ISpeechTranslationCaller)provider.SpeechTranslationModel("gpt-realtime-translate"),
    Audio = audio,
    InputAudioFormat = new AudioFormat("audio/pcm", 24000),
    TargetLanguage = "es",
});
```

- **Session.** The stream sends `session.update` with the `TargetLanguage` as the output language when the socket opens, then `session.input_audio_buffer.append` for each audio chunk, and `session.close` when the audio ends. `TranslationStreamStart.Request.Body` holds the `session.update` message. `TargetLanguage` is required.
- **Parts.** `session.input_transcript.delta` becomes a `source-transcript-delta` part and `session.output_transcript.delta` an `output-text-delta` part. `session.output_audio.delta` becomes an `audio` part with the decoded bytes. An empty audio delta is skipped. `session.closed` emits `source-transcript-final` and `output-text-final` for the accumulated text (each only when not empty), then `finish` with `SourceText` and `OutputText`. A server `error` event is emitted as an `error` part and the stream goes on. A close or a socket error before `session.closed` ends the stream with an `AiSdkException` that carries the close code and reason. With `IncludeRawChunks`, every event is also emitted as a `raw` part.
- **Input audio.** Only 24kHz 16-bit PCM input is accepted (`audio/pcm`, rate 24000 or none). Another format throws an `InvalidArgumentException` for `inputAudioFormat`. The source language is auto-detected and the output is always 24kHz 16-bit PCM, so `SourceLanguage` and `OutputAudioFormat` produce `unsupported` warnings.
- **Authentication and socket.** The same as [OpenAI transcription](#openai-transcription): the bearer token goes in the `openai-insecure-api-key.{token}` subprotocol, the `Authorization` header is removed, and `OpenAIOptions.WebSocket` sets the `WebSocketFactory`. Cancelling the stream closes the socket and cancels the audio. When `StreamTranslateRequest`'s cancellation token fires, the stream fails with the request's abort reason.

## OpenAI realtime

`OpenAIProvider.RealtimeModel(id)` returns an `OpenAIRealtimeModel` (OpenAI Realtime, provider id `openai.realtime`) or an `OpenAIRealtimeModelLive` (OpenAI Live, provider id `openai.live`). Both implement `IServerWebSocketRealtimeModel`. The known `gpt-live-1` id uses Live and every other id uses Realtime. `RealtimeModel(id, new OpenAIRealtimeOptions { Api = "live" })` selects the API explicitly, for an early-access model id. Realtime is experimental.

Trusted server-owned connections:

- `GetServerWebSocketConfigAsync(intent, cancellationToken)` returns a `RealtimeModelV4ServerConnection` with the URL and the headers (the provider API key, organization, project and custom headers; headers without a value are dropped). Live resolves the API key when you await the call, so an asynchronous key callback works there too. Realtime builds `{BaseUrl}/realtime?model={id}` for `RealtimeModelV4SessionIntent.Conversation` (the default) and `{BaseUrl}/realtime?intent=transcription` for `Transcription`, where the model goes in `session.update`. An `http:` base URL uses `ws:`. Live builds `{BaseUrl}/live/sessions` and rejects a transcription intent. Any other intent throws.
- `SerializeClientEvent(event, intent)`, `BuildSessionConfig(config, intent)` and `ParseServerEvent(raw)` map the normalized events to and from the wire format. Use the same intent for the connection and for serialization. A transcription session accepts only `InputAudioFormat*`, `InputAudioTranscription`, `TurnDetection` and `ProviderOptions`. Setting `Instructions`, `Voice`, `OutputModalities`, `OutputAudioFormat*`, `OutputAudioTranscription` or `Tools` throws.
- Realtime `response-done` events carry `RealtimeModelV4Usage` with gross input and output tokens, the cached portion and the modality splits. `input-transcription-completed` events carry `Seconds` for duration-billed transcription or token counts for token-billed transcription.
- `CreateClientSecretAsync(sessionConfig, expiresAfterSeconds, cancellationToken)` mints a short-lived Realtime token. Live has no client secret.
- `OpenAIProvider.ExperimentalRealtimeGetTokenAsync(model, options, sessionConfig, expiresAfterSeconds, cancellationToken)` mints that token from the provider, on the server. It resolves the API like `RealtimeModel` does, so `OpenAIRealtimeOptions.Api` applies. A known Live model, or `Api = "live"`, throws `UnsupportedFunctionalityException` before any request, and an invalid `Api` throws `InvalidArgumentException`. A failed Realtime request does not fall back to Live. The result has the `Token`, the `Url` (`wss://{host}/v1/realtime?model={id}`) and `ExpiresAt`.
- `OpenAIRealtimeModel.GetWebSocketConfig(token, url)` returns the URL and the subprotocols (`realtime` and `openai-insecure-api-key.{token}`) a browser opens a WebSocket with. Live has no such member: it connects from a trusted server.

Live options go in `RealtimeModelV4SessionConfig.ProviderOptions["openai"]`. `OpenAIRealtimeModelLiveOptions.ToJsonObject()` builds that value. It supports `ClientDelegation` (or `DisableDelegation`), `Input` (at most 128 text messages), `VoiceId` (exclusive with `Voice`), `Store` and `Client` (the WebRTC data channel policy). Unknown keys throw. Live startup settings are immutable: send `session-start` once, and `session-update` throws `UnsupportedFunctionalityException`. Use `input-audio-mute`, `input-audio-unmute` and `context-append` (`ProviderOptions["openai"]["channel"]` is `instructions`, `thinking` or `commentary`). `CreateWebRTCSessionAsync(sdp, sessionConfig, cancellationToken)` exchanges a WebRTC offer for the answer on a trusted server; it rejects fixed audio formats and Responses delegation before any request.

## Word and segment timings

`Transcribe.TranscribeAsync` returns `Segments` and an optional `Words` list on `TranscribeResult`. `Words` holds the word-level timings that the provider returned, whether you asked for them or the provider sends them by default. It is `null` when the provider returns none, and an empty list is kept as an empty list. `Segments` keeps the provider's own behavior and may hold individual words. The SDK does not build caption segments from word timings.

Ask for timings with `TranscribeRequest.TimestampGranularities` (`"word"`, `"segment"`). Omitting it keeps the provider defaults. An empty list asks for no extra timing detail and does not suppress timings the provider returns by default. An explicit provider option wins over the shared request, and the adapter warns when a requested granularity is unavailable or disabled by a provider option.

```csharp
var transcript = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = model,
    Audio = audio,
    TimestampGranularities = new[] { "word", "segment" },
});

var words = transcript.Words;
var segments = transcript.Segments;
```

Vertex Gemini transcription (`gemini-3.5-transcribe`) returns word timings only. They appear in both `Words` and `Segments`. A request for `"word"` sends `audioTranscriptionConfig.wordTimestamp = true` unless `wordTimestamp` is set. If `wordTimestamp` is `false`, the adapter warns that the option overrides the request. A request for `"segment"` warns that no separate caption segments exist. A non-empty transcript without word timings warns that the response omitted them. Azure Speech fills `Words` from the phrase words.

Vertex Gemini live models (`gemini-3.5-transcribe-live`) stream over the Vertex Live API WebSocket (`wss://{location}-aiplatform.googleapis.com/ws/google.cloud.aiplatform.v1.LlmBidiService/BidiGenerateContent`). Pass the model to `StreamTranscribe.Start`. The socket sends the same OAuth bearer header as the HTTP calls, so set `VertexOptions.WebSocket` to a `WebSocketFactory` if you need to replace the default `ClientWebSocket`. Input audio must be 16 kHz 16-bit PCM (`audio/pcm`), and audio is sent only after the server's `setupComplete`. The stream emits `transcript-partial` (interim), `transcript-delta`, and `transcript-final` parts with ids `google-segment-N`. If no final segment arrives, the latest interim text becomes the final one. The stream finishes on `interactionStatus` `IDLE` after the audio ended, after a quiet grace window, or when the socket closes after the audio ended. A close before the audio ended is an error. Streaming a unary id, or calling `DoGenerateAsync` on a live id, throws an `InvalidArgumentException`. `finish.Usage` carries the `promptTokenCount`, `candidatesTokenCount`, and `totalTokenCount` of `usageMetadata`, and `ProviderMetadata` `google.usageMetadata` carries all of it.

OpenAI transcription sends the shared request as `timestamp_granularities[]`. `providerOptions.openai.timestampGranularities` wins over it, and each requested granularity it leaves out is reported as an `unsupported` warning on `timestampGranularities`. Only the `verbose_json` response format accepts timestamp granularities, so `whisper-1` (which defaults to it) sends `segment` when you ask for nothing. For `gpt-4o-transcribe`, `gpt-4o-mini-transcribe`, `gpt-transcribe` and an explicit `json` response format the field is omitted and each requested granularity gets an `unsupported` warning. `gpt-4o-transcribe-diarize` returns speaker segments without a timestamp control and warns only about `word`. `Words` is filled from the response `words` even when the response also has `segments`.

The catalog entry also drives the request defaults: `gpt-4o-transcribe-diarize` requests the `diarized_json` response format and the `auto` chunking strategy, and `whisper-1` requests `verbose_json`. `providerOptions.openai.responseFormat` and `chunkingStrategy` override them. The raw response `usage` object is returned as `TranscriptionModelResult.Usage`. When the response has no `duration`, `DurationInSeconds` is read from a `{ "type": "duration", "seconds": n }` usage. Token usage is never read as a duration.

## Groq transcription

`GroqProvider.TranscriptionModel(id)` returns a `GroqTranscriptionModel` with provider id `groq.transcription`. It implements `ITranscriptionModel`, `ITranscriptionCaller`, and `ITranscriptionUrlCaller`, so it works with `Transcribe`. It does not stream. Read the options from `providerOptions.groq`: `language`, `prompt`, `responseFormat`, `temperature` (0 to 1), and `timestampGranularities`. With `responseFormat` set to `text`, the response body is the transcript. When a response has no `segments`, the result falls back to its `words`.

`ITranscriptionUrlCaller` is the optional native URL capability that upstream calls `doGenerateFromUrl`. `netstandard2.0` has no default interface members, so it is a separate interface. A model that implements it receives HTTP(S) URLs without a local download. When `TranscribeRequest.AudioUrl` is an HTTP(S) URL and `TranscribeRequest.Download` is not set, `Transcribe.TranscribeAsync` calls `DoGenerateFromUrlAsync`. Groq sends its `url` multipart field and no `file`, which avoids its attachment upload limit. Groq's overall audio size and duration limits still apply. The SDK does not download or inspect the audio.

```csharp
var result = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = (ITranscriptionCaller)groq.TranscriptionModel("whisper-large-v3-turbo"),
    AudioUrl = "https://example.com/large-audio.mp3",
});
```

- The URL must be reachable by Groq. Request headers configure the Groq API request and are not sent to the audio host.
- Groq strips the URL fragment, and rejects non-HTTP(S) URLs and URLs with user info with `InvalidArgumentException` before any request.
- Groq does not follow redirects. When it reports a `301`, `302`, `303`, `307`, or `308` fetch failure, the model returns `null` from `DoGenerateFromUrlAsync`. Core then downloads the audio with the secure downloader and uploads the bytes, so the attachment limit applies. The media type of the file part is the downloaded `Content-Type` when it is a full `audio/*` type (parameters removed, lowercased), then the type detected from the bytes, then `audio/wav`. Retries after that repeat only the file call. Every other Groq error is thrown unchanged.
- Setting `TranscribeRequest.Download` always downloads locally, even for models with native URL support. Bytes passed as `Audio` are always uploaded as a file.

## Google realtime

`GoogleProvider.RealtimeModel(id)` returns a `GoogleRealtimeModel` for Gemini Live. Realtime is experimental. `GetWebSocketConfig(token, url)` returns a `RealtimeModelV4WebSocketConfig` with the URL (`?access_token=` carries the token) and `Protocols` set to `null`, because Google does not authenticate through subprotocols.

Typed provider options are `GoogleRealtimeModelOptions`. Call `ToJsonObject()` and put the result under the `google` key of `RealtimeModelV4SessionConfig.ProviderOptions`:

- `TranslationConfig` (`TargetLanguageCode`, `EchoTargetLanguage`) configures `gemini-3.5-live-translate-preview`. `TargetLanguageCode` is a BCP-47 code and defaults to `en`. With `EchoTargetLanguage` set to `true`, input audio already in the target language is echoed instead of producing silence.
- `ThinkingConfig` (`ThinkingLevel` of `low`, `medium` or `high`, `ThinkingBudget`, `IncludeThoughts`) configures background-reasoning models such as `gemini-3.8-live-extended-thinking`. They need exactly one of `ThinkingLevel` or `ThinkingBudget`; the provider sends `thinkingLevel: "low"` when you set neither.
- `DefaultToolBehavior` (`BLOCKING` or `NON_BLOCKING`) is stamped onto every function declaration. Gemini 3.8 Live models default to `NON_BLOCKING`. Set `BLOCKING` on `gemini-3.8-live` to wait for the tool result. Background-reasoning models accept only `NON_BLOCKING`.

```csharp
var options = new GoogleRealtimeModelOptions
{
    TranslationConfig = new GoogleRealtimeTranslationConfig { TargetLanguageCode = "pl", EchoTargetLanguage = true },
};
var config = new RealtimeModelV4SessionConfig
{
    ProviderOptions = new JsonObject { ["google"] = options.ToJsonObject() },
};
```

## xAI speech

`XaiProvider.SpeechModel()` creates the model for the [xAI Text to Speech API](https://docs.x.ai/developers/model-capabilities/audio/text-to-speech) (`XaiSpeechModel`, provider id `xai.speech`). The endpoint needs no model id, so the model id is empty. `XaiProvider.SpeechModel(string)` is the `ISpeechModel` factory and uses the id you pass. The model implements `ISpeechModel` and `ISpeechCaller`, so it works with `GenerateSpeech`. A request posts JSON to `{base}/tts`.

```csharp
using Vercel.AI.Xai;

var xai = XaiProvider.Create(new XaiOptions { ApiKey = Environment.GetEnvironmentVariable("XAI_API_KEY") });
var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest
{
    Model = xai.SpeechModel(),
    Text = "Hello from the AI SDK!",
    Voice = "ara",
    Language = "en",
    OutputFormat = "mp3",
    Speed = 1.1,
});
```

- **Text**: text to synthesize. It can contain xAI speech tags such as `[pause]`, `[laugh]`, or `<whisper>...</whisper>`.
- **Voice**: the xAI voice id. Defaults to `eve`. Built-in ids are `eve`, `ara`, `rex`, `sal`, and `leo`. Custom voice ids are accepted too.
- **Language**: a BCP-47 code or `auto` (the default).
- **Speed**: speech rate multiplier from 0.7 to 1.5.
- **OutputFormat**: `mp3` (the default), `wav`, `pcm`, `mulaw`, or `alaw`. Another value falls back to `mp3` with an `unsupported` warning on `outputFormat`.
- **Instructions**: xAI has no such field. It adds an `unsupported` warning. Use speech tags in `Text` instead.

Pass xAI controls under the `xai` key of `ProviderOptions` (`XaiSpeechModelOptions`). An invalid value throws `InvalidArgumentException` before any request.

- **sampleRate** _int_: 8000, 16000, 22050, 24000, 44100, or 48000 Hz.
- **bitRate** _int_: 32000, 64000, 96000, 128000, or 192000 bits per second. It applies to `mp3` only. For another format it is ignored with an `unsupported` warning on `providerOptions`.
- **optimizeStreamingLatency** _int_: 0, 1, or 2. A higher value reduces the time to the first audio chunk, trading some quality.
- **textNormalization** _bool_: normalizes written-form text into spoken form before synthesis.
- **withTimestamps** _bool_: returns character-level timings with the audio. The API then answers with a JSON envelope, which the model decodes.
- **replace** _map of string to string_: phrases and their spoken substitutions, such as respellings or IPA phonetics, applied before synthesis.

`ProviderMetadata.xai` holds the `traceId` from the `x-trace-id` response header, and with `withTimestamps` also `duration` (seconds), `contentType`, and `audioTimestamps` (`graphChars` and `graphTimes`, where `graphTimes[i]` is the `[start, end]` interval of the character `graphChars[i]`, in seconds).

## xAI image

`XaiProvider.ImageModel(modelId)` (alias `XaiProvider.Image(modelId)`) creates an image model (`XaiImageModel`, provider id `xai.image`). It implements `IImageModel` and `IImageCaller`, so it works with `GenerateImage`. A call without input images posts to `{base}/images/generations`. A call with input images posts to `{base}/images/edits`, with one image as `image` and several as `images`. A URL image is sent as it is and file bytes as a base64 data URI. The model asks for `b64_json`, and when the response carries only URLs, it downloads them.

```csharp
using Vercel.AI.Xai;

var xai = XaiProvider.Create(new XaiOptions { ApiKey = Environment.GetEnvironmentVariable("XAI_API_KEY") });
var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest
{
    Model = xai.ImageModel("grok-imagine-image"),
    Text = "A futuristic cityscape at sunset",
    AspectRatio = "16:9",
});
```

To build the model without a provider, pass an `XaiImageModelConfig` (provider id, base URL, a `Headers` function, and an `HttpClient`) to the `XaiImageModel` constructor. Like upstream, the model adds no headers of its own: it sends the config headers and the call headers.

The catalog decides `MaxImagesPerCall` (3), `SupportsFileInputs`, and `SupportsMaskInputs`. For `grok-imagine-image`, `grok-imagine-image-pro`, and `grok-imagine-image-2.0` they are `true` and `false`. For another model id both are `null`. xAI does not support `Size`, so use `AspectRatio` (`1:1`, `16:9`, `9:16`, `4:3`, `3:4`, `3:2`, `2:3`, `2:1`, `1:2`, `19.5:9`, `9:19.5`, `20:9`, `9:20`, and `auto`). `Size`, `Seed`, and a mask each add an `unsupported` warning. A response with `respect_moderation: false` throws an `AiSdkException`, because xAI replaced the image with a placeholder.

Pass xAI controls under the `xai` key of `ProviderOptions`. An invalid value throws `InvalidArgumentException` before any request.

- **resolution** _string_: `1k`, `1.5k`, or `2k`. `grok-imagine-image` does not support `1.5k`.
- **quality** _string_: `low`, `medium`, `high`, or `auto`. `grok-imagine-image-2.0` accepts `low`, `medium`, and `auto`.
- **aspect_ratio** _string_: used when the call sets no `AspectRatio`.
- **output_format** _string_, **sync_mode** _bool_, and **user** _string_ (a unique end-user identifier).

`ProviderMetadata.xai.images` has one entry per image, with `revisedPrompt` when xAI revised the prompt. `costInUsdTicks` is the cost that xAI reports.

## xAI realtime

`XaiProvider.RealtimeModel(id)` returns an `XaiRealtimeModel`. `GetWebSocketConfig(token, url)` returns a `RealtimeModelV4WebSocketConfig` with the URL and the `xai-client-secret.{token}` subprotocol. `XaiRealtimeModelOptions` has no members yet, like upstream's empty options type.

## xAI transcription

`XaiProvider.TranscriptionModel(modelId)` creates a model for the [xAI Speech to Text API](https://docs.x.ai/developers/model-capabilities/audio/speech-to-text) (`XaiTranscriptionModel`, provider id `xai.transcription`). `XaiProvider.TranscriptionModel()` uses xAI's default transcription model. Pass `grok-voice-transcribe-2.0` to pin one. A request posts a multipart form to `{base}/stt` with the `file` field last, and `doStream` opens `wss://{host}/v1/stt`.

```csharp
using Vercel.AI.Xai;

var xai = XaiProvider.Create(new XaiOptions { ApiKey = Environment.GetEnvironmentVariable("XAI_API_KEY") });
var result = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = xai.TranscriptionModel(),
    Audio = audio,
    TimestampGranularities = new[] { "word" },
});
```

The word timings that xAI returns are in `TranscribeResult.Words`, even when you set no timing option, and `Segments` holds the same words. A request for `"word"` needs no xAI control. xAI has no caption segments, so a request for `"segment"` adds an `unsupported` warning on `timestampGranularities`, and a non-empty transcript without word timings adds an `other` warning when you asked for `"word"`. When a word has a `speaker`, the raw words come back in `providerMetadata.xai.words`.

Pass xAI controls under the `xai` key of `ProviderOptions` (upstream `XaiTranscriptionModelOptions`, here `XaiTranscriptionModelOptions`):

- **audioFormat** _string_: `pcm`, `mulaw`, `alaw`, or `opus`, for raw, headerless audio. Do not set it for container formats such as Ogg/Opus. `opus` is for streaming only, with one raw Opus packet per chunk, and an `audio/opus` input format maps to it.
- **sampleRate** _number_: 8000, 16000, 22050, 24000, 44100, or 48000 Hz.
- **language** _string_: language code for inverse text normalization.
- **format** _bool_: enables inverse text normalization and requires `language`. Streaming ignores it with an `unsupported` warning.
- **multichannel** _bool_ and **channels** _int_ (2 to 8): per-channel transcription. Streaming with `multichannel` requires `channels` and throws `InvalidArgumentException` otherwise.
- **diarize** _bool_, **keyterm** _string or string[]_, **fillerWords** _bool_.
- **vadThreshold** _number_ (0 to 1): defaults to 0.5 for `transcribe` and 0.08 for streaming. 0 disables the filter.
- **streaming** _object_: **interimResults** _bool_, **endpointing** _int_ (0 to 5000 ms), **smartTurn** _number_ (0 to 1), and **smartTurnTimeout** _int_ (1 to 5000 ms).

Streaming sends the audio as binary frames and `audio.done` at the end. It emits `transcript-partial` parts, a `transcript-final` for each `speech_final` utterance, and one `finish`. `transcript.done` has an empty text on the live API, so the finish text joins the finalized utterances of each channel, with the latest unfinalized text at the end, and channels are joined with a newline. Streaming does not return per-word timings, speakers, or segments. An unrecognized `InputAudioFormat.Type` falls back to `pcm` with an `other` warning. xAI needs the `Authorization` header on the WebSocket, so set `XaiOptions.WebSocket` on runtimes where the default socket cannot send it. `XaiTranscriptionModel.Serialize()` and `Deserialize(...)` keep the model id across a workflow boundary, without the key.

The model catalog of upstream (`getXaiTranscriptionModelEntry`) is not ported. The request sends the model id you pass as `model`, and nothing when it is empty.

## Azure transcription

`AzureOpenAIProvider.Transcription(deploymentId)` (alias `TranscriptionModel(deploymentId)`) returns an `AzureTranscriptionModel` with provider id `azure.transcription`. It implements `ITranscriptionModel` and `ITranscriptionCaller`, so it works with `Transcribe` and `StreamTranscribe`. The API is resolved for each call from the model ID and `providerOptions.azure.api`:

- `speech`: the default for the MAI-Transcribe models in the Azure model catalog (`mai-transcribe-2`, `mai-transcribe-1.5`, matched case-insensitively). It posts the audio and a model definition to `{SpeechBaseUrl}/speechtotext/transcriptions:transcribe?api-version=2025-10-15` with the `Ocp-Apim-Subscription-Key` header. `SpeechBaseUrl` defaults to `https://{resourceName}.cognitiveservices.azure.com`. Speech requests ignore `BaseUrl`, `ApiVersion` and `UseDeploymentBasedUrls`. Speech does not stream, so `DoStreamAsync` throws `UnsupportedFunctionalityException`.
- `mai`: the default for `mai-transcribe-2-streaming`. It streams over the WebSocket `{MaiBaseUrl}/realtime?intent=transcription`. `MaiBaseUrl` defaults to `https://{resourceName}.services.ai.azure.com/mai/v1`. File transcription throws `UnsupportedFunctionalityException`. Input audio must be 16-bit mono PCM (`audio/pcm`) at 16000 or 24000 Hz (24000 when the rate is omitted), otherwise the call throws `InvalidArgumentException`. The stream sends `session.update`, waits for `session.updated`, sends the audio, commits once when the input ends, and reports `transcript-delta`, `transcript-partial` and `transcript-final` parts and a `finish` part. Use `api: "mai"` for a custom deployment name.
- `openai`: every other deployment. It calls `/openai/v1/audio/transcriptions` (or the deployment route with `UseDeploymentBasedUrls`) like `OpenAITranscriptionModel`. `DoStreamAsync` delegates to the OpenAI model unchanged, so models that stream (see [OpenAI transcription](#openai-transcription)) open the OpenAI realtime WebSocket on `{BaseUrl}/v1/realtime?intent=transcription&api-version=...`. Other deployment names throw `UnsupportedFunctionalityException`. The OpenAI options `include`, `languages` and `keywords` are sent on this route and `AzureOpenAIOptions.NativeWebSocket` replaces the default `ClientWebSocket`.

```csharp
var provider = AzureOpenAIProvider.Create(new AzureOpenAIOptions { ResourceName = "my-resource", ApiKey = key });
var result = await Transcribe.TranscribeAsync(new TranscribeRequest
{
    Model = (ITranscriptionCaller)provider.Transcription("mai-transcribe-2"),
    Audio = wavBytes,
    ProviderOptions = OperationJson.Parse(@"{""azure"":{""timestamps"":""word"",""diarization"":{""enabled"":true}}}"),
});
```

- **Options.** `providerOptions.azure` is validated before any request. Unknown keys and invalid values throw `InvalidArgumentException`. Speech options: `timestamps` (`word`, `segment` or `none`; Azure's own default is `none`, the SDK sends `segment` unless the model cannot return timings), `transcribeStyle` (`verbatim` or `clean`), `locales` (exactly one locale), `diarization` (`{ "enabled": bool }`) and `phraseList` (`{ "phrases": [...] }`). MAI option: `language`, a hint for streaming. MAI-Transcribe-1.5 cannot return word or segment timings.
- **OpenAI options under `azure`.** The OpenAI transcription options (`language`, `prompt`, `temperature`, `timestampGranularities`, `responseFormat`, `chunkingStrategy`, ...) are accepted under `azure` as well as `openai`. They are merged into `openai` for the OpenAI API and `azure` wins when both set the same option.
- **Warnings.** An option that the resolved API does not use is reported as an `unsupported` warning named `providerOptions.azure.{key}`: Speech-only options on the OpenAI and MAI APIs, OpenAI-only options on the Speech and MAI APIs, and `language` on the Speech API (it is the MAI hint, and an OpenAI option on the OpenAI API). `TranscriptionModelCall.TimestampGranularities` (`word`, `segment`) maps onto `timestamps` when `timestamps` is not set. An explicit `timestamps` that gives less than requested, or a model that cannot return timings, adds a `timestampGranularities` warning.
- **Results.** Speech results carry `Segments` for phrases and `Words` for words that have an offset and a duration, a two-letter `Language` only when every phrase reports the same locale, `DurationInSeconds`, and the phrases (text, offsets, locale, speaker, confidence, words) under `ProviderMetadata.azure.phrases`. A response that does not match the Speech shape throws `ApiException` (`Invalid JSON response`). HTTP errors keep the message (`error.message` or the top-level `message`), the status, the response headers and the body.
- **Authentication.** The Speech and MAI APIs use `Ocp-Apim-Subscription-Key` and `api-key`. With `TokenProvider`, Speech requests send a fresh `Authorization: Bearer` token for each request unless the call sets its own `Authorization` header. MAI streaming sends the key as the `api-key` query parameter without `AzureOpenAIOptions.WebSocket`, and as a header with it. A `TokenProvider` or an `Authorization` header needs `WebSocket`, a `WebSocketFactory` such as the one the other streaming providers take, and throws `InvalidArgumentException` without it.
- **Workflows.** `AzureTranscriptionModel`, `AzureSpeechModel` and `AzureImageModel` each have `Serialize()` and `Deserialize()`. For example `AzureTranscriptionModel.Serialize()` keeps the model ID and the base URLs, the API version and the headers, but not the key or the token provider. `Deserialize(serialized, apiKey)` restores the model and resolves the API for each call again.


## Telemetry tracing channel

Upstream traces telemetry events on the Node.js `ai:telemetry` tracing channel (`AI_SDK_TELEMETRY_TRACING_CHANNEL`). The port publishes on a `System.Diagnostics.DiagnosticListener` with the same name, `TracingChannel.AiSdkTelemetryTracingChannel`. Subscribe with `DiagnosticListener.AllListeners` and pick the listener by name. Each tracing-channel sub-channel is one event name: `start`, `end`, `asyncStart`, `asyncEnd`, and `error`. The value of each event is a `TelemetryTracingChannelMessage` with `Type` (one of the `TelemetryTracingEventType` constants), `Event` (the telemetry event of the span), `Result` (set before `asyncEnd`), and `Error` (set before `error`). `bindStore` has no .NET counterpart, so there is no `bindStart`; use `AsyncLocal` values set in the subscriber of `start` if you need to carry context.

Only `Extract.ExtractAsync` (`experimental_extract`) and `Decide.DecideAsync` (`experimental_decide`) publish spans so far. The other span types are declared, because upstream declares them, but nothing publishes them yet. A span follows the per-call `TelemetryOptions`:

- `IsEnabled = false` publishes nothing for the call.
- The event has `AiVersion`, `RecordInputs`, `RecordOutputs`, and `FunctionId`, and `RuntimeContext` holds only the keys of `IncludeRuntimeContext`.
- An extract span has no `Headers` and an empty `ProviderOptions`, file URLs and references in its state are replaced with `[redacted]`, and a failed download reaches `error` without its URL.
- With `RecordInputs = false` the event has no state, fields, or reference date.
- `Result` is set only when both `RecordInputs` and `RecordOutputs` are not `false`. Otherwise `Error` is a generic `AI SDK operation failed` exception.

A subscriber that throws never affects the call, and a call with no subscriber runs without tracing.
