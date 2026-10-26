---
title: "Vercel.AI.Provider"
description: "Language model, embedding, image, and other model contracts."
editUrl: false
sidebar:
  label: "Vercel.AI.Provider"
---

[API reference](/api/)

Language model, embedding, image, and other model contracts.

## Classes

| Type | Summary |
| --- | --- |
| [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) | Base exception for the community .NET port of the Vercel AI SDK. |
| [ApiConnectionException](/api/Vercel.AI.Provider/ApiConnectionException/) | The connection to the provider failed. |
| [ApiException](/api/Vercel.AI.Provider/ApiException/) | The provider returned a non-success HTTP status after retries. |
| [ApiTimeoutException](/api/Vercel.AI.Provider/ApiTimeoutException/) | The provider call timed out. |
| [ApiUserAbortException](/api/Vercel.AI.Provider/ApiUserAbortException/) | The call was cancelled through a CancellationToken. It derives from OperationCanceledException, so catch (OperationCanceledException) handles every caller cancel. It is not an AiSdkException. |
| [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/) | One part of an assistant message, kept in the order the model produced it. |
| [AssistantCustomPart](/api/Vercel.AI.Provider/AssistantCustomPart/) | A provider-specific part, identified by Kind. |
| [AssistantFilePart](/api/Vercel.AI.Provider/AssistantFilePart/) | A file in an assistant message. Maps to the assistant file part. |
| [AssistantModelMessage](/api/Vercel.AI.Provider/AssistantModelMessage/) | An assistant message. |
| [AssistantReasoningFilePart](/api/Vercel.AI.Provider/AssistantReasoningFilePart/) | A file the model produced as part of its reasoning. Maps to the assistant reasoning-file part. |
| [AssistantReasoningPart](/api/Vercel.AI.Provider/AssistantReasoningPart/) | Reasoning in an assistant message. |
| [AssistantTextPart](/api/Vercel.AI.Provider/AssistantTextPart/) | Text in an assistant message. |
| [AssistantToolApprovalRequestPart](/api/Vercel.AI.Provider/AssistantToolApprovalRequestPart/) | A request to approve a tool call, kept in an assistant message. |
| [AssistantToolCallPart](/api/Vercel.AI.Provider/AssistantToolCallPart/) | A tool call in an assistant message. |
| [AssistantToolResultPart](/api/Vercel.AI.Provider/AssistantToolResultPart/) | A provider-executed tool result in an assistant message. |
| [AudioInput](/api/Vercel.AI.Provider/AudioInput/) | Audio sent to a transcription or translation model. |
| [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/) | Numeric usage from a speech or transcription call. A count stays unset when the provider does not report that unit. |
| [AuthenticationException](/api/Vercel.AI.Provider/AuthenticationException/) | HTTP 401. |
| [BadRequestException](/api/Vercel.AI.Provider/BadRequestException/) | HTTP 400. |
| [BatchJob](/api/Vercel.AI.Provider/BatchJob/) | A submitted batch job. |
| [CallWarning](/api/Vercel.AI.Provider/CallWarning/) | A warning produced while calling a model. |
| [Citation](/api/Vercel.AI.Provider/Citation/) | A reference that supports generated text: the source it points at, plus the optional range of the text it supports and the cited passage. It is not a second copy of the retrieved sources. |
| [CustomStreamPart](/api/Vercel.AI.Provider/CustomStreamPart/) | A provider-specific part, identified by Kind. |
| [DocumentSourceStreamPart](/api/Vercel.AI.Provider/DocumentSourceStreamPart/) | A retrieved document, such as a file a search tool returned. |
| [EmbeddingCallOptions](/api/Vercel.AI.Provider/EmbeddingCallOptions/) | Per-call embedding settings. Maps to the options of doEmbed. |
| [EmbeddingResponseInfo](/api/Vercel.AI.Provider/EmbeddingResponseInfo/) | Raw HTTP response of an embedding call. Maps to response of doEmbed. |
| [EmbeddingResult](/api/Vercel.AI.Provider/EmbeddingResult/) | Embedding vector result. |
| [ErrorMessage](/api/Vercel.AI.Provider/ErrorMessage/) | Formats an unknown thrown value the way getErrorMessage does. |
| [ErrorStreamPart](/api/Vercel.AI.Provider/ErrorStreamPart/) | A stream error from the provider. |
| [EvaluationResult](/api/Vercel.AI.Provider/EvaluationResult/) | Deprecated name of RubricResult. |
| [FileContentPart](/api/Vercel.AI.Provider/FileContentPart/) | A file or image in a user message. |
| [FinishReasons](/api/Vercel.AI.Provider/FinishReasons/) | Maps provider finish-reason strings onto FinishReason. |
| [FinishStreamPart](/api/Vercel.AI.Provider/FinishStreamPart/) | The stream finished. |
| [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) | Content the model generated. |
| [GeneratedCustom](/api/Vercel.AI.Provider/GeneratedCustom/) | A provider-specific content part, identified by Kind. It carries no data of its own; the provider keeps it in ProviderMetadata. |
| [GeneratedDocumentSource](/api/Vercel.AI.Provider/GeneratedDocumentSource/) | A document source, such as a retrieved file or a cited document. |
| [GeneratedFile](/api/Vercel.AI.Provider/GeneratedFile/) | A generated file. |
| [GeneratedImage](/api/Vercel.AI.Provider/GeneratedImage/) | One generated image. |
| [GeneratedReasoning](/api/Vercel.AI.Provider/GeneratedReasoning/) | Reasoning text kept separate from the answer. |
| [GeneratedSource](/api/Vercel.AI.Provider/GeneratedSource/) | A URL source cited by the model. |
| [GeneratedText](/api/Vercel.AI.Provider/GeneratedText/) | Generated text. |
| [GeneratedToolApprovalRequest](/api/Vercel.AI.Provider/GeneratedToolApprovalRequest/) | A request to approve a tool call, such as one a provider-executed MCP tool needs. A model returns it with ToolCallId only; the generation loop fills in ToolCall. |
| [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) | A function tool call. |
| [GeneratedToolResult](/api/Vercel.AI.Provider/GeneratedToolResult/) | A tool result: one the provider produced itself, such as a web or file search, or one the generation loop produced by running a client tool. |
| [ImageCallOptions](/api/Vercel.AI.Provider/ImageCallOptions/) | Image call settings. |
| [ImageGenerationResult](/api/Vercel.AI.Provider/ImageGenerationResult/) | Image generation result. |
| [ImageResponseInfo](/api/Vercel.AI.Provider/ImageResponseInfo/) | Response metadata of an image call. Maps to response of doGenerate on an image model. |
| [InternalServerException](/api/Vercel.AI.Provider/InternalServerException/) | HTTP 500-599. |
| [JsError](/api/Vercel.AI.Provider/JsError/) | JavaScript Error analog whose ToString() omits the stack. |
| [JsRangeError](/api/Vercel.AI.Provider/JsRangeError/) | JavaScript RangeError analog. |
| [JsTypeError](/api/Vercel.AI.Provider/JsTypeError/) | JavaScript TypeError analog. |
| [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) | Settings for one language-model call. Property names follow the V4 call options. |
| [LanguageModelGenerateResult](/api/Vercel.AI.Provider/LanguageModelGenerateResult/) | Result of CancellationToken). |
| [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/) | One part of a language-model stream. |
| [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/) | Request info for a stream call. Maps to the request field of doStream. |
| [LanguageModelStreamResponse](/api/Vercel.AI.Provider/LanguageModelStreamResponse/) | Response info for a stream call. Maps to the response field of doStream. |
| [LanguageModelStreamResult](/api/Vercel.AI.Provider/LanguageModelStreamResult/) | What CancellationToken) returns: the stream plus the request and response info. Maps to the { stream, request, response } object returned by doStream. Enumerating the result enumerates Stream. A provider opens the HTTP response in DoStreamAsync. Dispose the result to release that response when you stop without enumerating the stream, or while you enumerate it. Disposing is safe to repeat, and it does nothing after the stream has ended. |
| [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/) | A function tool definition sent to the model. |
| [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) | Token counts for a model call. |
| [MessageConversionException](/api/Vercel.AI.Provider/MessageConversionException/) | A UI message could not be converted to a model message. Maps to MessageConversionError. |
| [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/) | A prompt message. |
| [NotFoundException](/api/Vercel.AI.Provider/NotFoundException/) | HTTP 404. |
| [PermissionDeniedException](/api/Vercel.AI.Provider/PermissionDeniedException/) | HTTP 403. |
| [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) | Provider factory. Maps to ProviderV4. Modalities a provider does not implement throw AiSdkException. |
| [RateLimitException](/api/Vercel.AI.Provider/RateLimitException/) | HTTP 429. |
| [RawStreamPart](/api/Vercel.AI.Provider/RawStreamPart/) | One raw provider stream event. |
| [ReasoningDeltaStreamPart](/api/Vercel.AI.Provider/ReasoningDeltaStreamPart/) | A reasoning delta. |
| [ReasoningEndStreamPart](/api/Vercel.AI.Provider/ReasoningEndStreamPart/) | Marks the end of one reasoning block. |
| [ReasoningStartStreamPart](/api/Vercel.AI.Provider/ReasoningStartStreamPart/) | Marks the start of one reasoning block. |
| [RerankItem](/api/Vercel.AI.Provider/RerankItem/) | One reranked document. |
| [RerankResult](/api/Vercel.AI.Provider/RerankResult/) | Rerank result. |
| [ResponseMetadataStreamPart](/api/Vercel.AI.Provider/ResponseMetadataStreamPart/) | Identifies the provider response. |
| [RubricModels](/api/Vercel.AI.Provider/RubricModels/) | Helpers for IRubricModel. |
| [RubricResult](/api/Vercel.AI.Provider/RubricResult/) | Score of a candidate against a rubric. Returned by IRubricModel. |
| [SourceStreamPart](/api/Vercel.AI.Provider/SourceStreamPart/) | A cited URL. |
| [SpeechCallOptions](/api/Vercel.AI.Provider/SpeechCallOptions/) | Speech call settings. |
| [SpeechResult](/api/Vercel.AI.Provider/SpeechResult/) | Speech synthesis result. |
| [StreamStartStreamPart](/api/Vercel.AI.Provider/StreamStartStreamPart/) | Marks the start of a stream and carries call warnings. |
| [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/) | System instructions. |
| [TextContentPart](/api/Vercel.AI.Provider/TextContentPart/) | Text in a user message. |
| [TextDeltaStreamPart](/api/Vercel.AI.Provider/TextDeltaStreamPart/) | A text delta. |
| [TextEndStreamPart](/api/Vercel.AI.Provider/TextEndStreamPart/) | Marks the end of one text block. |
| [TextStartStreamPart](/api/Vercel.AI.Provider/TextStartStreamPart/) | Marks the start of one text block. |
| [ToolApprovalRequestStreamPart](/api/Vercel.AI.Provider/ToolApprovalRequestStreamPart/) | A request to approve a tool call. |
| [ToolApprovalResponseModelMessage](/api/Vercel.AI.Provider/ToolApprovalResponseModelMessage/) | The caller's answer to a GeneratedToolApprovalRequest. Only a response with ProviderExecuted reaches the model; the generation loop drops the others from the prompt. |
| [ToolCallStreamPart](/api/Vercel.AI.Provider/ToolCallStreamPart/) | A completed tool call. |
| [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) | How the model should choose tools. Maps to toolChoice. |
| [ToolChoice.NamedChoice](/api/Vercel.AI.Provider/ToolChoice.NamedChoice/) | A named tool choice. |
| [ToolInputDeltaStreamPart](/api/Vercel.AI.Provider/ToolInputDeltaStreamPart/) | A streamed tool-argument fragment. |
| [ToolInputEndStreamPart](/api/Vercel.AI.Provider/ToolInputEndStreamPart/) | Marks the end of streamed tool input. |
| [ToolInputStartStreamPart](/api/Vercel.AI.Provider/ToolInputStartStreamPart/) | Marks the start of streamed tool input. |
| [ToolModelMessage](/api/Vercel.AI.Provider/ToolModelMessage/) | A tool result fed back to the model. |
| [ToolResultStreamPart](/api/Vercel.AI.Provider/ToolResultStreamPart/) | A tool result the provider reported. |
| [TranscriptionResult](/api/Vercel.AI.Provider/TranscriptionResult/) | Transcription result. |
| [UnprocessableEntityException](/api/Vercel.AI.Provider/UnprocessableEntityException/) | HTTP 422. |
| [UploadedFile](/api/Vercel.AI.Provider/UploadedFile/) | An uploaded file. |
| [UploadedSkill](/api/Vercel.AI.Provider/UploadedSkill/) | An uploaded skill. |
| [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/) | A user content part. |
| [UserModelMessage](/api/Vercel.AI.Provider/UserModelMessage/) | A user message. |
| [VideoCallOptions](/api/Vercel.AI.Provider/VideoCallOptions/) | Video call settings. |
| [VideoResult](/api/Vercel.AI.Provider/VideoResult/) | Video generation result. |

## Interfaces

| Type | Summary |
| --- | --- |
| [IBatchModel](/api/Vercel.AI.Provider/IBatchModel/) | Batch model specification. |
| [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/) | Embedding model specification. |
| [IEvaluationModel](/api/Vercel.AI.Provider/IEvaluationModel/) | Deprecated name of IRubricModel. Pass it to EvaluationModel or IEvaluationModel). |
| [IFileStore](/api/Vercel.AI.Provider/IFileStore/) | File store used by uploadFile. |
| [IImageModel](/api/Vercel.AI.Provider/IImageModel/) | Image model specification. |
| [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) | Language model specification V4. doGenerate is CancellationToken) and doStream is CancellationToken). |
| [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/) | Realtime model specification. |
| [IRealtimeSession](/api/Vercel.AI.Provider/IRealtimeSession/) | A realtime session. |
| [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/) | Reranking model specification. |
| [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/) | A model that scores a candidate against a rubric. Used by CancellationToken). |
| [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/) | Skill store used by uploadSkill. |
| [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/) | Speech model specification. |
| [ISpeechTranslationModel](/api/Vercel.AI.Provider/ISpeechTranslationModel/) | Speech translation model specification. |
| [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/) | Transcription model specification. |
| [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/) | Video model specification. |

## Enums

| Type | Summary |
| --- | --- |
| [FinishReason](/api/Vercel.AI.Provider/FinishReason/) | Why a language-model call stopped. |
| [ToolInputEncoding](/api/Vercel.AI.Provider/ToolInputEncoding/) | Encoding of the deltas that follow a tool-input-start part. |
