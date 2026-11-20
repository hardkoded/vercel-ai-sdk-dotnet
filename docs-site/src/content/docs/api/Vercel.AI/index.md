---
title: "Vercel.AI"
description: "Client, text generation, streaming, tools, and agents."
editUrl: false
sidebar:
  label: "Vercel.AI"
---

[API reference](/api/)

Client, text generation, streaming, tools, and agents.

## Classes

| Type | Summary |
| --- | --- |
| [AbortPart](/api/Vercel.AI/AbortPart/) | The call was aborted. It is the last part of the stream. |
| [Agent](/api/Vercel.AI/Agent/) | An agent that calls tools until StopWhen matches. Maps to ToolLoopAgent. |
| [Agent&lt;TCallOptions&gt;](/api/Vercel.AI/Agent-1/) | An agent that takes options on each call. Maps to ToolLoopAgent with a CALLOPTIONS type. |
| [AgentOptions](/api/Vercel.AI/AgentOptions/) | Agent settings. |
| [AgentOptions&lt;TCallOptions&gt;](/api/Vercel.AI/AgentOptions-1/) | Agent settings with call options. Maps to ToolLoopAgentSettings with a CALLOPTIONS type. |
| [Ai](/api/Vercel.AI/Ai/) | Static entry points. String model ids use the AI Gateway and AIGATEWAYAPIKEY. |
| [AiClient](/api/Vercel.AI/AiClient/) | Default IAiClient. String model ids are resolved by the AI Gateway. |
| [AiSdkOptions](/api/Vercel.AI/AiSdkOptions/) | Options for AiSdkOptions%7d). |
| [AiSdkServiceCollectionExtensions](/api/Vercel.AI/AiSdkServiceCollectionExtensions/) | Dependency injection for IAiClient. |
| [ApiOrProvider&lt;TApi, TProvider&gt;](/api/Vercel.AI/ApiOrProvider-2/) | An API or a provider that exposes one. Maps to the upstream Api \| ProviderV4 unions. |
| [CustomPart](/api/Vercel.AI/CustomPart/) | Content that only the provider understands, such as a compaction. Maps to the custom stream part. |
| [CustomProvider](/api/Vercel.AI/CustomProvider/) | A provider backed by caller-supplied models. Maps to customProvider. A model id that is not registered goes to FallbackProvider, or throws NoSuchModelError when there is none. |
| [DecisionCapabilities](/api/Vercel.AI/DecisionCapabilities/) | Upstream ModelCapabilities.decision. |
| [DecisionInputCapabilities](/api/Vercel.AI/DecisionInputCapabilities/) | Upstream ModelCapabilities.decision.inputs. |
| [DecisionQuestionTypeCapabilities](/api/Vercel.AI/DecisionQuestionTypeCapabilities/) | Upstream ModelCapabilities.decision.questionTypes. |
| [DefaultInstructionsMiddleware](/api/Vercel.AI/DefaultInstructionsMiddleware/) | Prepends system instructions when the prompt has none. Maps to defaultInstructionsMiddleware. |
| [DefaultSettingsMiddleware](/api/Vercel.AI/DefaultSettingsMiddleware/) | Fills sampling settings when the caller left them unset. Maps to defaultSettingsMiddleware. |
| [DocumentSourcePart](/api/Vercel.AI/DocumentSourcePart/) | A cited document. Maps to the source stream part of source type document. |
| [EmbeddingCapabilities](/api/Vercel.AI/EmbeddingCapabilities/) | Upstream ModelCapabilities.embedding. |
| [EmbedEventValue&lt;TOne, TMany&gt;](/api/Vercel.AI/EmbedEventValue-2/) | A single item for embed or a list for embedMany, as carried by embed events. |
| [EmbedOptions](/api/Vercel.AI/EmbedOptions/) | Embedding options. |
| [ErrorPart](/api/Vercel.AI/ErrorPart/) | The stream failed. |
| [EvaluateOptions](/api/Vercel.AI/EvaluateOptions/) | Evaluation options. |
| [ExecutedTool](/api/Vercel.AI/ExecutedTool/) | The result of executing a tool. |
| [ExtractJsonMiddleware](/api/Vercel.AI/ExtractJsonMiddleware/) | Strips markdown JSON fences. Maps to extractJsonMiddleware. |
| [ExtractReasoningMiddleware](/api/Vercel.AI/ExtractReasoningMiddleware/) | Splits delimited reasoning, such as <think>...</think>, out of text. Maps to extractReasoningMiddleware. |
| [FilePart](/api/Vercel.AI/FilePart/) | A file the model generated. A URL has been downloaded by the time this part is emitted. Maps to the file stream part. |
| [FinishPart](/api/Vercel.AI/FinishPart/) | The full stream finished. |
| [GenerateImageOptions](/api/Vercel.AI/GenerateImageOptions/) | Image generation options. |
| [GenerateSpeechOptions](/api/Vercel.AI/GenerateSpeechOptions/) | Speech options. |
| [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/) | Options for CancellationToken). Property names follow generateText. |
| [GenerateTextResult](/api/Vercel.AI/GenerateTextResult/) | Result of generateText. |
| [GenerateTextStartEvent](/api/Vercel.AI/GenerateTextStartEvent/) | Passed to OnStart when the generation begins, before any model call. Maps to GenerateTextStartEvent. |
| [GenerateTextStepStartEvent](/api/Vercel.AI/GenerateTextStepStartEvent/) | Passed to OnStepStart when a step begins, before the provider is called. Maps to GenerateTextStepStartEvent. |
| [GenerateVideoOptions](/api/Vercel.AI/GenerateVideoOptions/) | Video options. |
| [ImageCapabilities](/api/Vercel.AI/ImageCapabilities/) | Upstream ModelCapabilities.image. |
| [LanguageCapabilities](/api/Vercel.AI/LanguageCapabilities/) | Upstream ModelCapabilities.language. |
| [LanguageInputCapabilities](/api/Vercel.AI/LanguageInputCapabilities/) | Upstream ModelCapabilities.language.inputs. |
| [LanguageModelCallEndEvent](/api/Vercel.AI/LanguageModelCallEndEvent/) | Passed to OnLanguageModelCallEnd after a provider model call returns. |
| [LanguageModelCallStartEvent](/api/Vercel.AI/LanguageModelCallStartEvent/) | Passed to OnLanguageModelCallStart before the model is called. |
| [LanguageModelMiddleware](/api/Vercel.AI/LanguageModelMiddleware/) | Passes both operations through. |
| [LanguageModelMiddlewareExtensions](/api/Vercel.AI/LanguageModelMiddlewareExtensions/) | Wraps a language model with middleware. Maps to wrapLanguageModel. |
| [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/) | Metadata for a language model request. Maps to LanguageModelRequestMetadata. |
| [LanguageModelResponseMetadata](/api/Vercel.AI/LanguageModelResponseMetadata/) | Metadata for a language model response, as the finish-step part carries it. Maps to Omit<LanguageModelResponseMetadata, 'messages' \| 'body'>. |
| [LanguageReasoningCapabilities](/api/Vercel.AI/LanguageReasoningCapabilities/) | Upstream ModelCapabilities.language.reasoning. |
| [LanguageSamplingCapabilities](/api/Vercel.AI/LanguageSamplingCapabilities/) | Upstream ModelCapabilities.language.sampling. |
| [LanguageToolCapabilities](/api/Vercel.AI/LanguageToolCapabilities/) | Upstream ModelCapabilities.language.tools. |
| [ModelCapabilities](/api/Vercel.AI/ModelCapabilities/) | Model metadata shared by provider implementations and generated documentation. Upstream ModelCapabilities. These declarations describe the capabilities exposed by the provider. They do not introduce validation or change request behavior. A null section is undeclared, not explicitly unsupported. |
| [ModelCapability](/api/Vercel.AI/ModelCapability/) | Support declared by a provider's model catalog. Upstream ModelCapability (boolean \| 'unknown' \| { condition: string }). Unknown is distinct from unsupported. A condition describes when support is available; it is documentation, not an expression to execute. |
| [ModelEntry&lt;TRequestConfig&gt;](/api/Vercel.AI/ModelEntry-1/) | One provider-owned model record, which can declare several operations. Upstream ModelEntry<RequestConfig>. <code class="typeparamref">TRequestConfig</code> is a provider-defined type for request encoding and defaults. Feature support belongs in Capabilities, including provider-specific features. |
| [ModelProviderOptions](/api/Vercel.AI/ModelProviderOptions/) | Per-model support for the options in the provider's ProviderOptionDefinitions, keyed by the same sections and option names. Upstream ModelProviderOptions. |
| [OutputChunkTimingStats](/api/Vercel.AI/OutputChunkTimingStats/) | Timing statistics for the gaps between generated output chunks, in milliseconds. |
| [OutputSpec](/api/Vercel.AI/OutputSpec/) | Structured output selector. Maps to Output. |
| [OutputSpec&lt;TOutput&gt;](/api/Vercel.AI/OutputSpec-1/) | Structured output selector whose complete output is deserialized to <code class="typeparamref">TOutput</code>. Maps to Output<OUTPUT>. |
| [PingPart](/api/Vercel.AI/PingPart/) | A transient provider heartbeat. It contains no generated content, does not reset model-output timeouts, and is omitted from UI message streams. Providers may require an explicit opt-in. Maps to TextStreamPingPart. |
| [PrepareStepContext](/api/Vercel.AI/PrepareStepContext/) | Context passed to prepareStep. |
| [PrepareStepUpdate](/api/Vercel.AI/PrepareStepUpdate/) | Optional changes applied before a step. |
| [ProviderOptionDefinition](/api/Vercel.AI/ProviderOptionDefinition/) | A provider option, declared once at the provider level. Upstream ProviderOptionDefinition. |
| [ProviderOptionDefinitions](/api/Vercel.AI/ProviderOptionDefinitions/) | The provider options a provider accepts, grouped by the operation they configure. Upstream ProviderOptionDefinitions. Section names match ModelCapabilities; option names match the keys accepted under providerOptions.<provider> for that operation. |
| [ProviderRegistry](/api/Vercel.AI/ProviderRegistry/) | Looks up provider models by provider:model ids. Maps to createProviderRegistry. Everything after the first separator is the model id, so model ids may contain the separator. |
| [RawPart](/api/Vercel.AI/RawPart/) | A raw chunk of the provider stream. It appears only when RawChunks is set. |
| [RealtimeCapabilities](/api/Vercel.AI/RealtimeCapabilities/) | Upstream ModelCapabilities.realtime. |
| [ReasoningDeltaPart](/api/Vercel.AI/ReasoningDeltaPart/) | Reasoning delta. Maps to reasoning-delta. |
| [ReasoningEndPart](/api/Vercel.AI/ReasoningEndPart/) | A reasoning block ended. Maps to reasoning-end. |
| [ReasoningFilePart](/api/Vercel.AI/ReasoningFilePart/) | A file the model generated as part of its reasoning. A URL has been downloaded by the time this part is emitted. Maps to the reasoning-file stream part. |
| [ReasoningStartPart](/api/Vercel.AI/ReasoningStartPart/) | A reasoning block started. Maps to reasoning-start. |
| [RequestOrigin](/api/Vercel.AI/RequestOrigin/) | Removes credentials, paths, queries, and fragments of a request URL before telemetry. Maps to telemetry/get-request-origin.ts. |
| [RerankingCapabilities](/api/Vercel.AI/RerankingCapabilities/) | Upstream ModelCapabilities.reranking. |
| [RerankingDocumentCapabilities](/api/Vercel.AI/RerankingDocumentCapabilities/) | Upstream ModelCapabilities.reranking.documents. |
| [RerankOptions](/api/Vercel.AI/RerankOptions/) | Rerank options. |
| [SimulateStreamingMiddleware](/api/Vercel.AI/SimulateStreamingMiddleware/) | Turns a non-streaming model into text deltas. Maps to simulateStreamingMiddleware. |
| [SourcePart](/api/Vercel.AI/SourcePart/) | A cited source. |
| [SpeechCapabilities](/api/Vercel.AI/SpeechCapabilities/) | Upstream ModelCapabilities.speech. |
| [SpeechTranslationCapabilities](/api/Vercel.AI/SpeechTranslationCapabilities/) | Upstream ModelCapabilities.speechTranslation. |
| [StartPart](/api/Vercel.AI/StartPart/) | The full stream started. It is the first part of the stream. |
| [StartStepPart](/api/Vercel.AI/StartStepPart/) | A step started. It comes before the parts of that step. |
| [SteeringMessagePart](/api/Vercel.AI/SteeringMessagePart/) | Experimental. A steering message that was added to the run before the step with the given number. Streamed before the output of that step. Maps to the steering-message stream part. StreamText emits it when it delivers a message sent with ExperimentalSteering. |
| [StepFinishPart](/api/Vercel.AI/StepFinishPart/) | A step finished. |
| [StepPerformance](/api/Vercel.AI/StepPerformance/) | Performance metrics for one step. Maps to StepResultPerformance. |
| [StepResponse](/api/Vercel.AI/StepResponse/) | Response information of one step. Maps to LanguageModelResponseMetadata & { messages, body? } on StepResult.response. |
| [StepResult](/api/Vercel.AI/StepResult/) | One language-model step, including any tool results from that step. |
| [StopCondition](/api/Vercel.AI/StopCondition/) | Stops the tool loop. The default for generateText is IsStepCount(int) of 1. |
| [StopWhen](/api/Vercel.AI/StopWhen/) | Stop-condition factories. Maps to isStepCount, hasToolCall, and isLoopFinished. |
| [StreamAbortContext](/api/Vercel.AI/StreamAbortContext/) | Passed to OnAbort. |
| [StreamTextChunkEvent](/api/Vercel.AI/StreamTextChunkEvent/) | Passed to OnChunk. Maps to the event of onChunk. |
| [StreamTextInclude](/api/Vercel.AI/StreamTextInclude/) | What streamText keeps of the request and the provider stream. Maps to StreamTextInclude. |
| [StreamTextOnErrorResult](/api/Vercel.AI/StreamTextOnErrorResult/) | The result of OnErrorRetry. Maps to { retry?: boolean }. |
| [StreamTextOptions](/api/Vercel.AI/StreamTextOptions/) | Options for CancellationToken). |
| [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | Result of streamText. Enumerating Stream(CancellationToken) or awaiting Text consumes the same buffered events. |
| [StreamTextSingleConsumerResult](/api/Vercel.AI/StreamTextSingleConsumerResult/) | A single-consumer view of a text generation, without replay or final content members. Maps to StreamTextSingleConsumerResult. Choose one stream projection. A second one throws. |
| [StreamTextTransformOptions](/api/Vercel.AI/StreamTextTransformOptions/) | The options a StreamTextTransform receives. Maps to the argument of StreamTextTransform. |
| [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/) | Telemetry configuration. Maps to TelemetryOptions in telemetry/telemetry-options.ts. |
| [TextDeltaPart](/api/Vercel.AI/TextDeltaPart/) | Text delta. Maps to text-delta. |
| [TextEndPart](/api/Vercel.AI/TextEndPart/) | A text block ended. Maps to text-end. |
| [TextStartPart](/api/Vercel.AI/TextStartPart/) | A text block started. Maps to text-start. |
| [TextStreamPart](/api/Vercel.AI/TextStreamPart/) | A part of the streamText full stream. |
| [Tool](/api/Vercel.AI/Tool/) | A tool the model can call. Maps to tool(). |
| [ToolApprovalRequestPart](/api/Vercel.AI/ToolApprovalRequestPart/) | A tool call needs approval before it runs. Maps to the tool-approval-request stream part. |
| [ToolApprovalResponsePart](/api/Vercel.AI/ToolApprovalResponsePart/) | A tool approval request was answered. Maps to the tool-approval-response stream part. |
| [ToolCallPart](/api/Vercel.AI/ToolCallPart/) | A tool call. |
| [ToolErrorPart](/api/Vercel.AI/ToolErrorPart/) | A tool call failed or was denied. A successful call is a ToolResultPart. |
| [ToolExecutionEndEvent](/api/Vercel.AI/ToolExecutionEndEvent/) | Passed to OnToolExecutionEnd after a tool ran. |
| [ToolExecutionOptions](/api/Vercel.AI/ToolExecutionOptions/) | The options a tool receives when it runs. Maps to the second argument of execute. |
| [ToolExecutionStartEvent](/api/Vercel.AI/ToolExecutionStartEvent/) | Passed to OnToolExecutionStart before a tool runs. |
| [ToolInputDeltaPart](/api/Vercel.AI/ToolInputDeltaPart/) | A streamed tool-input fragment. |
| [ToolInputEndPart](/api/Vercel.AI/ToolInputEndPart/) | The model finished streaming tool input. |
| [ToolInputStartPart](/api/Vercel.AI/ToolInputStartPart/) | The model started streaming tool input. |
| [ToolOutputDeniedPart](/api/Vercel.AI/ToolOutputDeniedPart/) | A tool call was denied, so its tool did not run. Maps to the tool-output-denied stream part. |
| [ToolResultPart](/api/Vercel.AI/ToolResultPart/) | A tool result. |
| [TranscribeOptions](/api/Vercel.AI/TranscribeOptions/) | Transcription options. |
| [TranscriptionCapabilities](/api/Vercel.AI/TranscriptionCapabilities/) | Upstream ModelCapabilities.transcription. |
| [TranslateOptions](/api/Vercel.AI/TranslateOptions/) | Speech translation options. |
| [VideoCapabilities](/api/Vercel.AI/VideoCapabilities/) | Upstream ModelCapabilities.video. |
| [VideoInputCapabilities](/api/Vercel.AI/VideoInputCapabilities/) | Upstream ModelCapabilities.video.inputs. |

## Interfaces

| Type | Summary |
| --- | --- |
| [IAgent](/api/Vercel.AI/IAgent/) | An agent that streams a reply to model messages. Maps to the Agent interface. |
| [IAgent&lt;TCallOptions&gt;](/api/Vercel.AI/IAgent-1/) | An agent that streams a reply to model messages and takes call options. Maps to the Agent interface with a CALLOPTIONS type. |
| [IAiClient](/api/Vercel.AI/IAiClient/) | Core AI SDK client. Maps to the ai package functions. |
| [IAiTelemetry](/api/Vercel.AI/IAiTelemetry/) | Telemetry hook invoked around generate and stream calls. |
| [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/) | Wraps doGenerate and doStream. Maps to language-model middleware. |

## Delegates

| Type | Summary |
| --- | --- |
| [StreamTextTransform](/api/Vercel.AI/StreamTextTransform/) | A transformation that is applied to the stream of parts. It receives the stream and returns the transformed stream. The transformation must maintain the stream structure for StreamTextAsync to work correctly. Maps to StreamTextTransform. |
