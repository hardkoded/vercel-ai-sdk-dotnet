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
| [Agent](/api/Vercel.AI/Agent/) | An agent that calls tools until StopWhen matches. Maps to Agent. |
| [AgentOptions](/api/Vercel.AI/AgentOptions/) | Agent settings. |
| [Ai](/api/Vercel.AI/Ai/) | Static entry points. String model ids use the AI Gateway and AIGATEWAYAPIKEY. |
| [AiClient](/api/Vercel.AI/AiClient/) | Default IAiClient. String model ids are resolved by the AI Gateway. |
| [AiSdkOptions](/api/Vercel.AI/AiSdkOptions/) | Options for AiSdkOptions%7d). |
| [AiSdkServiceCollectionExtensions](/api/Vercel.AI/AiSdkServiceCollectionExtensions/) | Dependency injection for IAiClient. |
| [ApiOrProvider&lt;TApi, TProvider&gt;](/api/Vercel.AI/ApiOrProvider-2/) | An API or a provider that exposes one. Maps to the upstream Api \| ProviderV4 unions. |
| [CustomProvider](/api/Vercel.AI/CustomProvider/) | A provider backed by caller-supplied models. Maps to customProvider. A model id that is not registered goes to FallbackProvider, or throws NoSuchModelError when there is none. |
| [DefaultInstructionsMiddleware](/api/Vercel.AI/DefaultInstructionsMiddleware/) | Prepends system instructions when the prompt has none. Maps to defaultInstructionsMiddleware. |
| [DefaultSettingsMiddleware](/api/Vercel.AI/DefaultSettingsMiddleware/) | Fills sampling settings when the caller left them unset. Maps to defaultSettingsMiddleware. |
| [EmbedEventValue&lt;TOne, TMany&gt;](/api/Vercel.AI/EmbedEventValue-2/) | A single item for embed or a list for embedMany, as carried by embed events. |
| [EmbedOptions](/api/Vercel.AI/EmbedOptions/) | Embedding options. |
| [ErrorPart](/api/Vercel.AI/ErrorPart/) | The stream failed. |
| [EvaluateOptions](/api/Vercel.AI/EvaluateOptions/) | Evaluation options. |
| [ExecutedTool](/api/Vercel.AI/ExecutedTool/) | The result of executing a tool. |
| [ExtractJsonMiddleware](/api/Vercel.AI/ExtractJsonMiddleware/) | Strips markdown JSON fences. Maps to extractJsonMiddleware. |
| [ExtractReasoningMiddleware](/api/Vercel.AI/ExtractReasoningMiddleware/) | Splits delimited reasoning, such as <think>...</think>, out of text. Maps to extractReasoningMiddleware. |
| [FinishPart](/api/Vercel.AI/FinishPart/) | The full stream finished. |
| [GenerateImageOptions](/api/Vercel.AI/GenerateImageOptions/) | Image generation options. |
| [GenerateSpeechOptions](/api/Vercel.AI/GenerateSpeechOptions/) | Speech options. |
| [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/) | Options for CancellationToken). Property names follow generateText. |
| [GenerateTextResult](/api/Vercel.AI/GenerateTextResult/) | Result of generateText. |
| [GenerateVideoOptions](/api/Vercel.AI/GenerateVideoOptions/) | Video options. |
| [LanguageModelCallEndEvent](/api/Vercel.AI/LanguageModelCallEndEvent/) | Passed to OnLanguageModelCallEnd after a provider model call returns. |
| [LanguageModelMiddleware](/api/Vercel.AI/LanguageModelMiddleware/) | Passes both operations through. |
| [LanguageModelMiddlewareExtensions](/api/Vercel.AI/LanguageModelMiddlewareExtensions/) | Wraps a language model with middleware. Maps to wrapLanguageModel. |
| [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/) | Metadata for a language model request. Maps to LanguageModelRequestMetadata. |
| [LanguageModelResponseMetadata](/api/Vercel.AI/LanguageModelResponseMetadata/) | Metadata for a language model response, as the finish-step part carries it. Maps to Omit<LanguageModelResponseMetadata, 'messages' \| 'body'>. |
| [OutputChunkTimingStats](/api/Vercel.AI/OutputChunkTimingStats/) | Timing statistics for the gaps between generated output chunks, in milliseconds. |
| [OutputSpec](/api/Vercel.AI/OutputSpec/) | Structured output selector. Maps to Output. |
| [PrepareStepContext](/api/Vercel.AI/PrepareStepContext/) | Context passed to prepareStep. |
| [PrepareStepUpdate](/api/Vercel.AI/PrepareStepUpdate/) | Optional changes applied before a step. |
| [ProviderRegistry](/api/Vercel.AI/ProviderRegistry/) | Looks up provider models by provider:model ids. Maps to createProviderRegistry. Everything after the first separator is the model id, so model ids may contain the separator. |
| [RawPart](/api/Vercel.AI/RawPart/) | A raw chunk of the provider stream. It appears only when RawChunks is set. |
| [ReasoningDeltaPart](/api/Vercel.AI/ReasoningDeltaPart/) | Reasoning delta. |
| [RerankOptions](/api/Vercel.AI/RerankOptions/) | Rerank options. |
| [SimulateStreamingMiddleware](/api/Vercel.AI/SimulateStreamingMiddleware/) | Turns a non-streaming model into text deltas. Maps to simulateStreamingMiddleware. |
| [SourcePart](/api/Vercel.AI/SourcePart/) | A cited source. |
| [StartPart](/api/Vercel.AI/StartPart/) | The full stream started. It is the first part of the stream. |
| [StartStepPart](/api/Vercel.AI/StartStepPart/) | A step started. It comes before the parts of that step. |
| [StepFinishPart](/api/Vercel.AI/StepFinishPart/) | A step finished. |
| [StepPerformance](/api/Vercel.AI/StepPerformance/) | Performance metrics for one step. Maps to StepResultPerformance. |
| [StepResponse](/api/Vercel.AI/StepResponse/) | Response information of one step. Maps to LanguageModelResponseMetadata & { messages, body? } on StepResult.response. |
| [StepResult](/api/Vercel.AI/StepResult/) | One language-model step, including any tool results from that step. |
| [StopCondition](/api/Vercel.AI/StopCondition/) | Stops the tool loop. The default for generateText is IsStepCount(int) of 1. |
| [StopWhen](/api/Vercel.AI/StopWhen/) | Stop-condition factories. Maps to isStepCount, hasToolCall, and isLoopFinished. |
| [StreamAbortContext](/api/Vercel.AI/StreamAbortContext/) | Passed to OnAbort. |
| [StreamTextInclude](/api/Vercel.AI/StreamTextInclude/) | What streamText keeps of the request and the provider stream. Maps to StreamTextInclude. |
| [StreamTextOptions](/api/Vercel.AI/StreamTextOptions/) | Options for CancellationToken). |
| [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | Result of streamText. Enumerating Stream(CancellationToken) or awaiting Text consumes the same buffered events. |
| [TextDeltaPart](/api/Vercel.AI/TextDeltaPart/) | Text delta. |
| [TextStreamPart](/api/Vercel.AI/TextStreamPart/) | A part of the streamText full stream. |
| [Tool](/api/Vercel.AI/Tool/) | A tool the model can call. Maps to tool(). |
| [ToolApprovalRequest](/api/Vercel.AI/ToolApprovalRequest/) | A tool call the caller may approve before it runs. |
| [ToolApprovalRequestPart](/api/Vercel.AI/ToolApprovalRequestPart/) | A provider-executed tool call needs approval before the provider runs it. |
| [ToolCallPart](/api/Vercel.AI/ToolCallPart/) | A tool call. |
| [ToolErrorPart](/api/Vercel.AI/ToolErrorPart/) | A tool call failed or was denied. A successful call is a ToolResultPart. |
| [ToolExecutionEndEvent](/api/Vercel.AI/ToolExecutionEndEvent/) | Passed to OnToolExecutionEnd after a tool ran. |
| [ToolExecutionOptions](/api/Vercel.AI/ToolExecutionOptions/) | The options a tool receives when it runs. Maps to the second argument of execute. |
| [ToolExecutionStartEvent](/api/Vercel.AI/ToolExecutionStartEvent/) | Passed to OnToolExecutionStart before a tool runs. |
| [ToolInputDeltaPart](/api/Vercel.AI/ToolInputDeltaPart/) | A streamed tool-input fragment. |
| [ToolInputEndPart](/api/Vercel.AI/ToolInputEndPart/) | The model finished streaming tool input. |
| [ToolInputStartPart](/api/Vercel.AI/ToolInputStartPart/) | The model started streaming tool input. |
| [ToolResultPart](/api/Vercel.AI/ToolResultPart/) | A tool result. |
| [TranscribeOptions](/api/Vercel.AI/TranscribeOptions/) | Transcription options. |
| [TranslateOptions](/api/Vercel.AI/TranslateOptions/) | Speech translation options. |

## Interfaces

| Type | Summary |
| --- | --- |
| [IAiClient](/api/Vercel.AI/IAiClient/) | Core AI SDK client. Maps to the ai package functions. |
| [IAiTelemetry](/api/Vercel.AI/IAiTelemetry/) | Telemetry hook invoked around generate and stream calls. |
| [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/) | Wraps doGenerate and doStream. Maps to language-model middleware. |
