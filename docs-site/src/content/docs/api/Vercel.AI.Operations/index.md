---
title: "Vercel.AI.Operations"
description: "Speech, image, video, embeddings, transcription, and other operations."
editUrl: false
sidebar:
  label: "Vercel.AI.Operations"
---

[API reference](/api/)

Speech, image, video, embeddings, transcription, and other operations.

## Classes

| Type | Summary |
| --- | --- |
| [AiSdkVersion](/api/Vercel.AI.Operations/AiSdkVersion/) | Package version used in the ai/ user-agent suffix. |
| [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/) | Audio the model may take ownership of. |
| [Batch](/api/Vercel.AI.Operations/Batch/) | Batch operations. Maps to startBatch, getBatchStatus, getBatchResults, listBatches, and cancelBatch. |
| [BatchCancelResult](/api/Vercel.AI.Operations/BatchCancelResult/) | Cancellation result. |
| [BatchContentPart](/api/Vercel.AI.Operations/BatchContentPart/) | One content part inside a batch text result. |
| [BatchImageGeneration](/api/Vercel.AI.Operations/BatchImageGeneration/) | A provider image generation inside a batch item. |
| [BatchItem](/api/Vercel.AI.Operations/BatchItem/) | One raw batch item. |
| [BatchItemResult](/api/Vercel.AI.Operations/BatchItemResult/) | Normalized batch item. |
| [BatchListResult](/api/Vercel.AI.Operations/BatchListResult/) | A page of batches. |
| [BatchModels](/api/Vercel.AI.Operations/BatchModels/) | Default provider used when a batch call omits one. |
| [BatchModelUsage](/api/Vercel.AI.Operations/BatchModelUsage/) | Provider token usage before it is reshaped. |
| [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) | Arguments for a batch operation. |
| [BatchReference](/api/Vercel.AI.Operations/BatchReference/) | A version-2 batch reference. |
| [BatchRequest](/api/Vercel.AI.Operations/BatchRequest/) | One request in a batch. |
| [BatchRequestCounts](/api/Vercel.AI.Operations/BatchRequestCounts/) | Request counts. |
| [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/) | Options for batch operations. |
| [BatchRequestWarning](/api/Vercel.AI.Operations/BatchRequestWarning/) | A warning tied to a request. |
| [BatchStartResult](/api/Vercel.AI.Operations/BatchStartResult/) | Start acknowledgement. |
| [BatchStatus](/api/Vercel.AI.Operations/BatchStatus/) | Status fields shared by start, list, and status calls. |
| [BatchTextGeneration](/api/Vercel.AI.Operations/BatchTextGeneration/) | A provider text generation inside a batch item. |
| [BatchTool](/api/Vercel.AI.Operations/BatchTool/) | A tool definition forwarded to a batch without being executed. |
| [BatchUsage](/api/Vercel.AI.Operations/BatchUsage/) | Usage returned to batch callers. |
| [Decide](/api/Vercel.AI.Operations/Decide/) | Decides typed questions. Maps to experimentaldecide. |
| [DecideEvent](/api/Vercel.AI.Operations/DecideEvent/) | Lifecycle event for decide. |
| [DecideRequest](/api/Vercel.AI.Operations/DecideRequest/) | Options for CancellationToken). |
| [DecideTelemetry](/api/Vercel.AI.Operations/DecideTelemetry/) | Telemetry callbacks. Generic onStart and onEnd are not invoked. |
| [DecisionAnswer](/api/Vercel.AI.Operations/DecisionAnswer/) | One model answer. Numbers are returned unchanged. |
| [DecisionBooleanCriteria](/api/Vercel.AI.Operations/DecisionBooleanCriteria/) | Optional descriptions of the true and false outcomes of a boolean question. A side that is never set is left out of the wire request. A side set to null is sent as null. |
| [DecisionBox](/api/Vercel.AI.Operations/DecisionBox/) | Edges in [0, 1], measured from the image's top-left corner. |
| [DecisionCaller](/api/Vercel.AI.Operations/DecisionCaller/) | Helpers for IDecisionCaller. |
| [DecisionChoiceCriteria](/api/Vercel.AI.Operations/DecisionChoiceCriteria/) | Criteria of a choice question: a nonempty map of option names to descriptions. A null description means none. |
| [DecisionCriteria](/api/Vercel.AI.Operations/DecisionCriteria/) | Criteria of a decision question. Maps to the criteria member of DecisionModelV4Question. |
| [DecisionFilePart](/api/Vercel.AI.Operations/DecisionFilePart/) | A file part of decision state. A URL is downloaded before the model call. |
| [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/) | Content for decision instructions and criteria descriptions: a string, a JSON object, or a JSON array. Maps to DecisionModelV4Input. |
| [DecisionJsonPart](/api/Vercel.AI.Operations/DecisionJsonPart/) | A JSON part of decision state. |
| [DecisionLanguageModel](/api/Vercel.AI.Operations/DecisionLanguageModel/) | Adapts structured language-model output to Choice, Score, and Boolean decisions. Maps to DecisionLanguageModel. |
| [DecisionLanguageModelOptions](/api/Vercel.AI.Operations/DecisionLanguageModelOptions/) | The wrapped model and provider id that cross a workflow boundary for DecisionLanguageModel. |
| [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/) | Arguments for doDecide. |
| [DecisionModelEvent](/api/Vercel.AI.Operations/DecisionModelEvent/) | Model-call telemetry event. |
| [DecisionModelFilePart](/api/Vercel.AI.Operations/DecisionModelFilePart/) | A file part the model receives. Maps to LanguageModelV4FilePart. |
| [DecisionModelJsonPart](/api/Vercel.AI.Operations/DecisionModelJsonPart/) | A JSON part the model receives. |
| [DecisionModelResult](/api/Vercel.AI.Operations/DecisionModelResult/) | Model response for doDecide. |
| [DecisionModels](/api/Vercel.AI.Operations/DecisionModels/) | Resolves decision models, including string model ids. Maps to resolveDecisionModel. |
| [DecisionModelStatePart](/api/Vercel.AI.Operations/DecisionModelStatePart/) | A part of decision state as the model receives it. Maps to DecisionModelV4StatePart. |
| [DecisionModelTextPart](/api/Vercel.AI.Operations/DecisionModelTextPart/) | A text part the model receives. |
| [DecisionQuestion](/api/Vercel.AI.Operations/DecisionQuestion/) | One decision question. Maps to DecisionModelV4Question. |
| [DecisionResult](/api/Vercel.AI.Operations/DecisionResult/) | Decision result. |
| [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/) | Declared rounding precision. |
| [DecisionScoreCriteria](/api/Vercel.AI.Operations/DecisionScoreCriteria/) | Criteria of a score question: at least two ordered levels, indexed from zero. A null description means none. |
| [DecisionState](/api/Vercel.AI.Operations/DecisionState/) | Shared state of a decision: text, a JSON object, or ordered parts. Maps to DecisionState. A JSON array is not valid state. Wrap it in an object or in a DecisionJsonPart. CancellationToken) maps to prepareDecisionState. |
| [DecisionStatePart](/api/Vercel.AI.Operations/DecisionStatePart/) | A part of decision state as the caller passes it. Maps to DecisionStatePart. |
| [DecisionTextPart](/api/Vercel.AI.Operations/DecisionTextPart/) | A text part of decision state. |
| [DecisionUnsupportedQuestionTypeException](/api/Vercel.AI.Operations/DecisionUnsupportedQuestionTypeException/) | The decision model does not support a question type. It carries the ErrorMarker marker, so IsInstance(object) recognizes it. |
| [DownloadedMedia](/api/Vercel.AI.Operations/DownloadedMedia/) | Downloaded media. |
| [DownloadException](/api/Vercel.AI.Operations/DownloadException/) | Downloading generated media failed. |
| [Embed](/api/Vercel.AI.Operations/Embed/) | Embeds one value. Maps to embed. |
| [EmbeddingModelCall](/api/Vercel.AI.Operations/EmbeddingModelCall/) | Arguments for one doEmbed call. |
| [EmbeddingModelCallEvent](/api/Vercel.AI.Operations/EmbeddingModelCallEvent/) | One inner doEmbed telemetry event. |
| [EmbeddingModelResponse](/api/Vercel.AI.Operations/EmbeddingModelResponse/) | One embedding returned by a model call. |
| [EmbedEndEvent](/api/Vercel.AI.Operations/EmbedEndEvent/) | End event for embed and embedMany. |
| [EmbedMany](/api/Vercel.AI.Operations/EmbedMany/) | Embeds many values, splitting on count and UTF-8 byte limits. Maps to embedMany. |
| [EmbedManyRequest](/api/Vercel.AI.Operations/EmbedManyRequest/) | Options for CancellationToken). |
| [EmbedManyResult](/api/Vercel.AI.Operations/EmbedManyResult/) | Result of CancellationToken). |
| [EmbedRequest](/api/Vercel.AI.Operations/EmbedRequest/) | Options for CancellationToken). |
| [EmbedResult](/api/Vercel.AI.Operations/EmbedResult/) | Result of CancellationToken). |
| [EmbedStartEvent](/api/Vercel.AI.Operations/EmbedStartEvent/) | Start event for embed and embedMany. |
| [EmbedTelemetry](/api/Vercel.AI.Operations/EmbedTelemetry/) | Telemetry callbacks for an embedding operation. |
| [Evaluate](/api/Vercel.AI.Operations/Evaluate/) | Deprecated alias of Decide. |
| [EvaluateEvent](/api/Vercel.AI.Operations/EvaluateEvent/) | Deprecated alias of DecideEvent. |
| [EvaluateModelEvent](/api/Vercel.AI.Operations/EvaluateModelEvent/) | Deprecated alias of DecisionModelEvent. |
| [EvaluateRequest](/api/Vercel.AI.Operations/EvaluateRequest/) | Deprecated alias of DecideRequest. |
| [EvaluateResult](/api/Vercel.AI.Operations/EvaluateResult/) | Deprecated alias of DecisionResult. |
| [EvaluateTelemetry](/api/Vercel.AI.Operations/EvaluateTelemetry/) | Deprecated alias of DecideTelemetry. |
| [EvaluationAnswer](/api/Vercel.AI.Operations/EvaluationAnswer/) | Deprecated alias of DecisionAnswer. |
| [EvaluationModelCall](/api/Vercel.AI.Operations/EvaluationModelCall/) | Deprecated alias of DecisionModelCall. |
| [EvaluationModelResult](/api/Vercel.AI.Operations/EvaluationModelResult/) | Deprecated alias of DecisionModelResult. |
| [EvaluationQuestion](/api/Vercel.AI.Operations/EvaluationQuestion/) | Deprecated alias of DecisionQuestion. |
| [EvaluationRounding](/api/Vercel.AI.Operations/EvaluationRounding/) | Deprecated alias of DecisionRounding. |
| [EvaluationUnsupportedQuestionTypeException](/api/Vercel.AI.Operations/EvaluationUnsupportedQuestionTypeException/) | Deprecated alias of DecisionUnsupportedQuestionTypeException. |
| [GeneratedAudio](/api/Vercel.AI.Operations/GeneratedAudio/) | Generated speech audio. |
| [GeneratedImage](/api/Vercel.AI.Operations/GeneratedImage/) | A generated image file. |
| [GeneratedVideo](/api/Vercel.AI.Operations/GeneratedVideo/) | A generated video file. |
| [GenerateImage](/api/Vercel.AI.Operations/GenerateImage/) | Generates images. Maps to generateImage. |
| [GenerateImageRequest](/api/Vercel.AI.Operations/GenerateImageRequest/) | Options for CancellationToken). |
| [GenerateImageResult](/api/Vercel.AI.Operations/GenerateImageResult/) | Result of CancellationToken). |
| [GenerateSpeech](/api/Vercel.AI.Operations/GenerateSpeech/) | Generates speech. Maps to generateSpeech. |
| [GenerateSpeechRequest](/api/Vercel.AI.Operations/GenerateSpeechRequest/) | Options for CancellationToken). |
| [GenerateSpeechResult](/api/Vercel.AI.Operations/GenerateSpeechResult/) | Result of CancellationToken). |
| [GenerateVideo](/api/Vercel.AI.Operations/GenerateVideo/) | Generates videos. Maps to experimentalgenerateVideo. |
| [GenerateVideoRequest](/api/Vercel.AI.Operations/GenerateVideoRequest/) | Options shared by generate and start. |
| [GenerateVideoResult](/api/Vercel.AI.Operations/GenerateVideoResult/) | Result of CancellationToken). |
| [GetVideoStatus](/api/Vercel.AI.Operations/GetVideoStatus/) | Reads video operation status. Maps to experimentalgetVideoStatus. |
| [GetVideoStatusRequest](/api/Vercel.AI.Operations/GetVideoStatusRequest/) | Options for CancellationToken). |
| [GoogleMultimodalEmbedding](/api/Vercel.AI.Operations/GoogleMultimodalEmbedding/) | Gemini embedding caller used by EmbedMany. Splits multimodal google.content to the chunk that is about to be sent and posts embedContent or batchEmbedContents. |
| [ImageCallResult](/api/Vercel.AI.Operations/ImageCallResult/) | One provider call preserved on the result. |
| [ImageModelCall](/api/Vercel.AI.Operations/ImageModelCall/) | Arguments for one image model call. |
| [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/) | An image sent to doGenerate as a file or URL. |
| [ImageModelResult](/api/Vercel.AI.Operations/ImageModelResult/) | One doGenerate result. |
| [ImagePrompt](/api/Vercel.AI.Operations/ImagePrompt/) | A string prompt or an image edit prompt. |
| [ImagePromptParts](/api/Vercel.AI.Operations/ImagePromptParts/) | Normalized image prompt fields. |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | Invalid caller argument. Maps to InvalidArgumentError. |
| [InvalidResponseDataException](/api/Vercel.AI.Operations/InvalidResponseDataException/) | The provider response did not match the expected shape. |
| [ListedBatch](/api/Vercel.AI.Operations/ListedBatch/) | One listed batch. |
| [MediaTypeDetector](/api/Vercel.AI.Operations/MediaTypeDetector/) | Detects media types from file signatures. |
| [ModelPartStream](/api/Vercel.AI.Operations/ModelPartStream/) | Model stream that stays open until Cancel() when Completes is false. |
| [ModelStreamPart](/api/Vercel.AI.Operations/ModelStreamPart/) | A part emitted by a transcription or translation model stream. |
| [NoImageGeneratedException](/api/Vercel.AI.Operations/NoImageGeneratedException/) | No image was produced after retries. |
| [NormalizedBatchRequest](/api/Vercel.AI.Operations/NormalizedBatchRequest/) | Normalized request sent to doStartBatch. |
| [NoSpeechGeneratedException](/api/Vercel.AI.Operations/NoSpeechGeneratedException/) | No speech audio was produced. |
| [NoTranscriptGeneratedException](/api/Vercel.AI.Operations/NoTranscriptGeneratedException/) | No transcript text was produced. |
| [NoTranslationGeneratedException](/api/Vercel.AI.Operations/NoTranslationGeneratedException/) | No translation text or audio was produced. |
| [NoVideoGeneratedException](/api/Vercel.AI.Operations/NoVideoGeneratedException/) | No video was produced. |
| [OperationCallbacks](/api/Vercel.AI.Operations/OperationCallbacks/) | Invokes callbacks and swallows their exceptions. |
| [OperationFullStream&lt;T&gt;](/api/Vercel.AI.Operations/OperationFullStream-1/) | A single-consumer stream with an explicit Cancel. |
| [OperationHeaders](/api/Vercel.AI.Operations/OperationHeaders/) | Header and retry helpers shared by the operation types. |
| [OperationJson](/api/Vercel.AI.Operations/OperationJson/) | JSON helpers used while normalizing provider options. |
| [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/) | Shared request fields for an operation call. |
| [OperationRetry](/api/Vercel.AI.Operations/OperationRetry/) | Retries a provider call the way prepareRetries does. |
| [OperationRetryException](/api/Vercel.AI.Operations/OperationRetryException/) | Retries were exhausted. LastError is the final attempt. |
| [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) | Token counts for an image or evaluation call. |
| [OperationWarning](/api/Vercel.AI.Operations/OperationWarning/) | A warning returned by a model call. |
| [PartQueue&lt;T&gt;](/api/Vercel.AI.Operations/PartQueue-1/) | Async queue used to hand stream parts to one consumer. |
| [PendingPromise&lt;T&gt;](/api/Vercel.AI.Operations/PendingPromise-1/) | A promise that can be resolved once. |
| [ProviderFileReference](/api/Vercel.AI.Operations/ProviderFileReference/) | A provider file reference. |
| [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) | Headers and body returned by one provider call. |
| [RankedDocument](/api/Vercel.AI.Operations/RankedDocument/) | One document in the reranked order. |
| [Rerank](/api/Vercel.AI.Operations/Rerank/) | Reranks documents. Maps to rerank. |
| [RerankEndEvent](/api/Vercel.AI.Operations/RerankEndEvent/) | End event for rerank. |
| [RerankModelCall](/api/Vercel.AI.Operations/RerankModelCall/) | Arguments for doRerank. |
| [RerankModelCallEvent](/api/Vercel.AI.Operations/RerankModelCallEvent/) | Inner rerank telemetry event. |
| [RerankModelDocuments](/api/Vercel.AI.Operations/RerankModelDocuments/) | Documents sent to a reranking model. |
| [RerankModelRank](/api/Vercel.AI.Operations/RerankModelRank/) | One ranked index from the model. |
| [RerankModelResponse](/api/Vercel.AI.Operations/RerankModelResponse/) | Model response for doRerank. |
| [RerankRequest](/api/Vercel.AI.Operations/RerankRequest/) | Options for CancellationToken). |
| [RerankResponseInfo](/api/Vercel.AI.Operations/RerankResponseInfo/) | Response metadata for a rerank call. |
| [RerankResult](/api/Vercel.AI.Operations/RerankResult/) | Result of CancellationToken). |
| [RerankStartEvent](/api/Vercel.AI.Operations/RerankStartEvent/) | Start event for rerank. |
| [RerankTelemetry](/api/Vercel.AI.Operations/RerankTelemetry/) | Telemetry callbacks for rerank. |
| [RetryableCallException](/api/Vercel.AI.Operations/RetryableCallException/) | A provider HTTP failure that may be retried. |
| [SearchableTool](/api/Vercel.AI.Operations/SearchableTool/) | A tool the search step can hide until it is discovered. |
| [SkillFile](/api/Vercel.AI.Operations/SkillFile/) | One file inside a skill upload. |
| [SkillFilePayload](/api/Vercel.AI.Operations/SkillFilePayload/) | Normalized skill file passed to the provider. |
| [SpeechAudioFormat](/api/Vercel.AI.Operations/SpeechAudioFormat/) | Audio format of a speech stream, such as audio/pcm at 24000 Hz or audio/mpeg. |
| [SpeechModelCall](/api/Vercel.AI.Operations/SpeechModelCall/) | Arguments for doGenerate on a speech model. |
| [SpeechModelResult](/api/Vercel.AI.Operations/SpeechModelResult/) | Speech model response. |
| [SpeechModels](/api/Vercel.AI.Operations/SpeechModels/) | Resolves string speech model ids. |
| [SpeechModelStreamCall](/api/Vercel.AI.Operations/SpeechModelStreamCall/) | Arguments for doStream on a speech model. Maps to SpeechModelV4StreamOptions. |
| [SpeechModelStreamPart](/api/Vercel.AI.Operations/SpeechModelStreamPart/) | A part emitted by a streaming speech model. Maps to SpeechModelV4StreamPart. |
| [SpeechModelStreamResponse](/api/Vercel.AI.Operations/SpeechModelStreamResponse/) | Response data returned by doStream. |
| [SpeechModelStreamResult](/api/Vercel.AI.Operations/SpeechModelStreamResult/) | The result of a speech model doStream call. Maps to SpeechModelV4StreamResult. |
| [SpeechStreamPart](/api/Vercel.AI.Operations/SpeechStreamPart/) | A part of FullStream. |
| [SpeechStreamUsage](/api/Vercel.AI.Operations/SpeechStreamUsage/) | Usage of a speech stream call. Every field is optional because providers bill speech in different units. |
| [SpeechTextPart](/api/Vercel.AI.Operations/SpeechTextPart/) | One piece of streamed input text. Consecutive parts are synthesized in order as one continuous script. Maps to SpeechTextPart and SpeechModelV4StreamTextPart. |
| [SpeechTextStream](/api/Vercel.AI.Operations/SpeechTextStream/) | The text a speech model reads. The provider owns it once doStream is called: it must enumerate it to the end or dispose its enumerator on every terminal path. |
| [SpeechTranslationModels](/api/Vercel.AI.Operations/SpeechTranslationModels/) | Resolves string speech-translation model ids. |
| [StartVideo](/api/Vercel.AI.Operations/StartVideo/) | Starts a video generation. Maps to experimentalstartVideo. |
| [StartVideoResult](/api/Vercel.AI.Operations/StartVideoResult/) | Result of CancellationToken). |
| [StreamSpeech](/api/Vercel.AI.Operations/StreamSpeech/) | Streams speech audio. Maps to experimentalstreamSpeech. |
| [StreamSpeechRequest](/api/Vercel.AI.Operations/StreamSpeechRequest/) | Options for CancellationToken). |
| [StreamSpeechResult](/api/Vercel.AI.Operations/StreamSpeechResult/) | Result of CancellationToken). Maps to StreamSpeechResult. |
| [StreamTranscribe](/api/Vercel.AI.Operations/StreamTranscribe/) | Streams a transcript. Maps to streamTranscribe. |
| [StreamTranscribeRequest](/api/Vercel.AI.Operations/StreamTranscribeRequest/) | Options for CancellationToken). |
| [StreamTranscribeResult](/api/Vercel.AI.Operations/StreamTranscribeResult/) | Streaming transcription result. |
| [StreamTranslate](/api/Vercel.AI.Operations/StreamTranslate/) | Streams a speech translation. Maps to streamTranslate. |
| [StreamTranslateRequest](/api/Vercel.AI.Operations/StreamTranslateRequest/) | Options for CancellationToken). |
| [StreamTranslateResult](/api/Vercel.AI.Operations/StreamTranslateResult/) | Streaming translation result. |
| [ToolCallerBinding](/api/Vercel.AI.Operations/ToolCallerBinding/) | How a tool may be called. |
| [ToolCallers](/api/Vercel.AI.Operations/ToolCallers/) | Direct tool invocation. An omitted routing entry permits this caller. |
| [ToolSearch](/api/Vercel.AI.Operations/ToolSearch/) | Creates the search tool. Maps to toolSearch. |
| [ToolSearchCall](/api/Vercel.AI.Operations/ToolSearchCall/) | A planned model tool call. |
| [ToolSearchGeneration](/api/Vercel.AI.Operations/ToolSearchGeneration/) | Runs the tool-search step loop used by generateText, streamText, and the tool-loop agent. The model tool list, execution timing, and catalog messages match across those modes. |
| [ToolSearchMatch](/api/Vercel.AI.Operations/ToolSearchMatch/) | One discovered tool. Schemas are not included. |
| [ToolSearchMatchList](/api/Vercel.AI.Operations/ToolSearchMatchList/) | Tools returned by one search. |
| [ToolSearchRun](/api/Vercel.AI.Operations/ToolSearchRun/) | The outcome of a tool-search generation. |
| [ToolSearchState](/api/Vercel.AI.Operations/ToolSearchState/) | Per-generation discovery state. Maps to createToolSearchState. |
| [ToolSearchStepItem](/api/Vercel.AI.Operations/ToolSearchStepItem/) | A tool result or tool error from one step. |
| [ToolSearchStepRecord](/api/Vercel.AI.Operations/ToolSearchStepRecord/) | One model step produced by ToolSearchGeneration. |
| [Transcribe](/api/Vercel.AI.Operations/Transcribe/) | Transcribes audio. Maps to transcribe. |
| [TranscribeRequest](/api/Vercel.AI.Operations/TranscribeRequest/) | Options for CancellationToken). |
| [TranscribeResult](/api/Vercel.AI.Operations/TranscribeResult/) | Transcription result. |
| [TranscriptionModelCall](/api/Vercel.AI.Operations/TranscriptionModelCall/) | Arguments for a transcription doGenerate. |
| [TranscriptionModelResult](/api/Vercel.AI.Operations/TranscriptionModelResult/) | Transcription model response. |
| [TranscriptionModels](/api/Vercel.AI.Operations/TranscriptionModels/) | Resolves string transcription model ids. |
| [TranscriptionStreamCall](/api/Vercel.AI.Operations/TranscriptionStreamCall/) | Arguments for streaming transcription. |
| [TranscriptionStreamStart](/api/Vercel.AI.Operations/TranscriptionStreamStart/) | A started transcription stream. |
| [TranscriptSegment](/api/Vercel.AI.Operations/TranscriptSegment/) | One timed transcript segment. |
| [TranslationStreamCall](/api/Vercel.AI.Operations/TranslationStreamCall/) | Arguments for a speech-translation stream. |
| [TranslationStreamStart](/api/Vercel.AI.Operations/TranslationStreamStart/) | A started translation stream. |
| [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/) | The provider does not implement the requested operation. |
| [UnsupportedModelVersionException](/api/Vercel.AI.Operations/UnsupportedModelVersionException/) | The model specification version is not supported. |
| [UploadData](/api/Vercel.AI.Operations/UploadData/) | Tagged file bytes, text, or a stream. |
| [UploadFile](/api/Vercel.AI.Operations/UploadFile/) | Uploads a file. Maps to uploadFile. |
| [UploadFileCall](/api/Vercel.AI.Operations/UploadFileCall/) | Arguments forwarded to a files API. |
| [UploadFileModelResult](/api/Vercel.AI.Operations/UploadFileModelResult/) | Provider upload result. |
| [UploadFileRequest](/api/Vercel.AI.Operations/UploadFileRequest/) | Options for CancellationToken). |
| [UploadFileResult](/api/Vercel.AI.Operations/UploadFileResult/) | Result of CancellationToken). |
| [UploadSkill](/api/Vercel.AI.Operations/UploadSkill/) | Uploads a skill. Maps to uploadSkill. |
| [UploadSkillCall](/api/Vercel.AI.Operations/UploadSkillCall/) | Arguments for a skill upload. |
| [UploadSkillRequest](/api/Vercel.AI.Operations/UploadSkillRequest/) | Options for CancellationToken). |
| [UploadSkillResult](/api/Vercel.AI.Operations/UploadSkillResult/) | Skill upload result. |
| [UploadStream](/api/Vercel.AI.Operations/UploadStream/) | A stream the provider may take ownership of. |
| [VideoCallInputs](/api/Vercel.AI.Operations/VideoCallInputs/) | Normalized prompt, frames, and references. |
| [VideoFrameFile](/api/Vercel.AI.Operations/VideoFrameFile/) | A normalized frame image. |
| [VideoFrameImage](/api/Vercel.AI.Operations/VideoFrameImage/) | A role-tagged frame image. |
| [VideoModelCall](/api/Vercel.AI.Operations/VideoModelCall/) | Arguments shared by generate, start, and status. |
| [VideoModelFile](/api/Vercel.AI.Operations/VideoModelFile/) | A file or URL passed to a video model. |
| [VideoModelResult](/api/Vercel.AI.Operations/VideoModelResult/) | Synchronous doGenerate result or a completed status payload. |
| [VideoOutput](/api/Vercel.AI.Operations/VideoOutput/) | One generated video payload from the model. |
| [VideoPrompt](/api/Vercel.AI.Operations/VideoPrompt/) | Text prompt or a prompt with a start image. |
| [VideoReference](/api/Vercel.AI.Operations/VideoReference/) | Reference media, either raw content or an object with a media type. |
| [VideoStartResult](/api/Vercel.AI.Operations/VideoStartResult/) | Result of doStart. |
| [VideoStatusResult](/api/Vercel.AI.Operations/VideoStatusResult/) | Result of doStatus. |
| [VideoWebhook](/api/Vercel.AI.Operations/VideoWebhook/) | Webhook notification. |
| [WarningLog](/api/Vercel.AI.Operations/WarningLog/) | Warning logger. Observer sees every call, including empty warning lists. Logger maps to globalThis.AISDKLOGWARNINGS. |
| [WarningLogContext](/api/Vercel.AI.Operations/WarningLogContext/) | Arguments passed to the warning logger. |

## Interfaces

| Type | Summary |
| --- | --- |
| [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/) | Batch API. |
| [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/) | Provider that exposes experimentalbatch(). |
| [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/) | Decision model used by Decide. |
| [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/) | A provider that returns decision models. ProviderRegistry and CustomProvider resolve decision models through it. Maps to the optional decisionModel factory of a provider. |
| [IEmbeddingCaller](/api/Vercel.AI.Operations/IEmbeddingCaller/) | Embedding model used by Embed and EmbedMany. |
| [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/) | Deprecated decision model contract. Implement IDecisionCaller instead. |
| [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/) | A decision model that also has the deprecated CancellationToken). Returned by the deprecated EvaluationModel accessors of ProviderRegistry and CustomProvider. Both methods decide the same way. |
| [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/) | Deprecated provider factory for decision models. Implement IDecisionProvider instead. A provider that implements both is asked through IDecisionProvider only. |
| [IFileStore](/api/Vercel.AI.Operations/IFileStore/) | Files API. |
| [IFileStoreProvider](/api/Vercel.AI.Operations/IFileStoreProvider/) | Provider that can expose a files API. |
| [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/) | Image model used by GenerateImage. |
| [IRerankCaller](/api/Vercel.AI.Operations/IRerankCaller/) | Reranking model used by Rerank. |
| [ISkillStore](/api/Vercel.AI.Operations/ISkillStore/) | Skills API. |
| [ISkillStoreProvider](/api/Vercel.AI.Operations/ISkillStoreProvider/) | Provider that can expose a skills API. |
| [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/) | Speech model used by GenerateSpeech. |
| [ISpeechTranslationCaller](/api/Vercel.AI.Operations/ISpeechTranslationCaller/) | Speech translation model used by StreamTranslate. |
| [IStreamingSpeechCaller](/api/Vercel.AI.Operations/IStreamingSpeechCaller/) | A speech model that can also stream speech audio for streamed text input. Maps to ExperimentalStreamingSpeechModelV4. |
| [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/) | Transcription model used by Transcribe. |
| [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/) | Video model used by generate, start, and status. |
