---
title: "Vercel.AI.Google"
description: "Google Gemini and Vertex provider."
editUrl: false
sidebar:
  label: "Vercel.AI.Google"
---

[API reference](/api/)

Google Gemini and Vertex provider.

## Classes

| Type | Summary |
| --- | --- |
| [GoogleAssistantPart](/api/Vercel.AI.Google/GoogleAssistantPart/) | An assistant content part. |
| [GoogleAssistantTurn](/api/Vercel.AI.Google/GoogleAssistantTurn/) | An assistant turn. |
| [GoogleBatchApi](/api/Vercel.AI.Google/GoogleBatchApi/) | Gemini Batch API. Starts, reads, cancels, and lists batches and downloads their results. |
| [GoogleDecisionModel](/api/Vercel.AI.Google/GoogleDecisionModel/) | Answers questions through the Gemini Decisions API (POST /decisions). Maps to DecisionGoogleModel. |
| [GoogleEmbeddingModel](/api/Vercel.AI.Google/GoogleEmbeddingModel/) | Gemini embedding model. One value uses :embedContent; several values use :batchEmbedContents. |
| [GoogleFileAssistantPart](/api/Vercel.AI.Google/GoogleFileAssistantPart/) | An assistant file or reasoning file. |
| [GoogleFileUrls](/api/Vercel.AI.Google/GoogleFileUrls/) | URLs Gemini can read without downloading the bytes. |
| [GoogleFileUserPart](/api/Vercel.AI.Google/GoogleFileUserPart/) | A user file. Kind is url, data, text, or reference. |
| [GoogleFinishReason](/api/Vercel.AI.Google/GoogleFinishReason/) | Maps Gemini finish reasons onto the shared finish-reason enum. |
| [GoogleGeneratedFile](/api/Vercel.AI.Google/GoogleGeneratedFile/) | A generated file. Files the model marks as thoughts are GeneratedReasoningFile instead. |
| [GoogleImageModel](/api/Vercel.AI.Google/GoogleImageModel/) | Gemini image generation. The call runs through GoogleLanguageModel with the IMAGE response modality, so the google provider options apply, and googleSearch enables Google Search grounding. |
| [GoogleInteractionsAgent](/api/Vercel.AI.Google/GoogleInteractionsAgent/) | An agent to call through the Interactions API: { agent: name } or { managedAgent: name } upstream. |
| [GoogleInteractionsFinish](/api/Vercel.AI.Google/GoogleInteractionsFinish/) | Maps an Interactions API status onto a finish reason. |
| [GoogleInteractionsInput](/api/Vercel.AI.Google/GoogleInteractionsInput/) | Converts a prompt into an Interactions input array. |
| [GoogleInteractionsModel](/api/Vercel.AI.Google/GoogleInteractionsModel/) | Language model that posts to the Gemini Interactions API. |
| [GoogleInteractionsOutput](/api/Vercel.AI.Google/GoogleInteractionsOutput/) | Reads text and function calls from an Interactions response. |
| [GoogleInteractionsTools](/api/Vercel.AI.Google/GoogleInteractionsTools/) | Maps tool definitions and toolChoice onto the Gemini Interactions tools[] and toolchoice request fields. Port of prepare-google-interactions-tools.ts. |
| [GoogleJson](/api/Vercel.AI.Google/GoogleJson/) | JSON settings shared by Gemini request builders. |
| [GoogleJsonAccumulator](/api/Vercel.AI.Google/GoogleJsonAccumulator/) | Builds a JSON object from Gemini streaming partialArgs chunks. Concatenated text deltas plus Finalize() match the serialized object. Keys such as proto are stored as ordinary properties. |
| [GoogleJsonSchema](/api/Vercel.AI.Google/GoogleJsonSchema/) | Rewrites JSON Schema so Gemini responseJsonSchema accepts it. |
| [GoogleLanguageModel](/api/Vercel.AI.Google/GoogleLanguageModel/) | Gemini language model. Posts to models/{id}:generateContent. |
| [GoogleMessageOptions](/api/Vercel.AI.Google/GoogleMessageOptions/) | Settings for converting a prompt into Gemini contents. |
| [GoogleMessages](/api/Vercel.AI.Google/GoogleMessages/) | Converts prompts into Gemini contents. |
| [GoogleModelCapabilities](/api/Vercel.AI.Google/GoogleModelCapabilities/) | Which Gemini request features a model id accepts. |
| [GoogleModelCapability](/api/Vercel.AI.Google/GoogleModelCapability/) | Classifies Gemini model ids. Unknown future Gemini ids inherit the newest behavior. |
| [GoogleModelPath](/api/Vercel.AI.Google/GoogleModelPath/) | Builds the resource path Gemini expects for a model id. |
| [GoogleOptions](/api/Vercel.AI.Google/GoogleOptions/) | Gemini settings. |
| [GoogleParseContext](/api/Vercel.AI.Google/GoogleParseContext/) | Ids and metadata keys used while parsing a Gemini response. |
| [GooglePartialArgument](/api/Vercel.AI.Google/GooglePartialArgument/) | One streamed function-call argument fragment. |
| [GooglePartialArgumentsFinal](/api/Vercel.AI.Google/GooglePartialArgumentsFinal/) | The completed argument JSON and the suffix still needed to close it. |
| [GooglePartialArgumentsUpdate](/api/Vercel.AI.Google/GooglePartialArgumentsUpdate/) | The object and text produced by one GoogleJsonAccumulator update. |
| [GooglePreparedRequest](/api/Vercel.AI.Google/GooglePreparedRequest/) | A generateContent request body plus the warnings and extra headers that go with it. |
| [GooglePrompt](/api/Vercel.AI.Google/GooglePrompt/) | Gemini contents plus an optional system instruction. |
| [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) | Google Gemini provider. |
| [GoogleProviderTool](/api/Vercel.AI.Google/GoogleProviderTool/) | A Gemini provider-defined tool such as Google Search. |
| [GoogleRealtimeClientSecret](/api/Vercel.AI.Google/GoogleRealtimeClientSecret/) | An ephemeral Gemini Live auth token and the WebSocket URL it opens. |
| [GoogleRealtimeEventMapper](/api/Vercel.AI.Google/GoogleRealtimeEventMapper/) | Stateful event mapper for Google's Gemini Live API. Google's events have no response or item ids and a single message can carry several pieces of data, so this class tracks turn state to generate consistent synthetic ids. No socket is opened, so this compiles on netstandard2.0. Maps to GoogleRealtimeEventMapper and buildGoogleSessionConfig in google-realtime-event-mapper.ts. |
| [GoogleRealtimeModel](/api/Vercel.AI.Google/GoogleRealtimeModel/) | Realtime model for Gemini Live. Maps to GoogleRealtimeModel. Event mapping is local; opening the Live socket is left to the caller, which uses CancellationToken) or a client secret. |
| [GoogleRealtimeModelConfig](/api/Vercel.AI.Google/GoogleRealtimeModelConfig/) | Settings of GoogleRealtimeModel. Maps to GoogleRealtimeModelConfig. Headers maps the upstream Resolvable: an object, a promise, a callback or an async callback all fit a function that returns a task. |
| [GoogleRealtimeModelOptions](/api/Vercel.AI.Google/GoogleRealtimeModelOptions/) | Google realtime provider options, read from providerOptions.google. Maps to GoogleRealtimeModelOptions of google-realtime-model-options.ts. Put ToJsonObject() under the google key of ProviderOptions. |
| [GoogleRealtimeThinkingConfig](/api/Vercel.AI.Google/GoogleRealtimeThinkingConfig/) | Gemini Live thinking configuration. Maps to thinkingConfig of GoogleRealtimeModelOptions. |
| [GoogleRealtimeTranslationConfig](/api/Vercel.AI.Google/GoogleRealtimeTranslationConfig/) | Gemini Live Translation configuration. Maps to translationConfig of GoogleRealtimeModelOptions. |
| [GoogleReasoning](/api/Vercel.AI.Google/GoogleReasoning/) | Reasoning text that can carry a thought signature. |
| [GoogleReasoningAssistantPart](/api/Vercel.AI.Google/GoogleReasoningAssistantPart/) | Assistant reasoning. |
| [GoogleRequest](/api/Vercel.AI.Google/GoogleRequest/) | Builds a Gemini generateContent body. |
| [GoogleResponse](/api/Vercel.AI.Google/GoogleResponse/) | Parses a Gemini generateContent response. |
| [GoogleServiceCollectionExtensions](/api/Vercel.AI.Google/GoogleServiceCollectionExtensions/) | Registers Google and Vertex. |
| [GoogleSpeechCall](/api/Vercel.AI.Google/GoogleSpeechCall/) | Speech request settings beyond the shared text and voice fields. |
| [GoogleSpeechInput](/api/Vercel.AI.Google/GoogleSpeechInput/) | Inspects a speech transcript before a model call. |
| [GoogleSpeechInspection](/api/Vercel.AI.Google/GoogleSpeechInspection/) | Transcript text and whether the voice is a custom voice. |
| [GoogleSpeechModel](/api/Vercel.AI.Google/GoogleSpeechModel/) | Gemini text-to-speech. Posts to generateContent with an audio response modality. |
| [GoogleSpeechPreparation](/api/Vercel.AI.Google/GoogleSpeechPreparation/) | A prepared Gemini speech request. |
| [GoogleSpeechResult](/api/Vercel.AI.Google/GoogleSpeechResult/) | Speech audio plus the response metadata recorded for the call. |
| [GoogleSpeechTranslation](/api/Vercel.AI.Google/GoogleSpeechTranslation/) | Speech translation configuration for the Gemini Live API. The socket itself is not opened here. |
| [GoogleSystemTurn](/api/Vercel.AI.Google/GoogleSystemTurn/) | A system turn. |
| [GoogleText](/api/Vercel.AI.Google/GoogleText/) | Generated text that can carry a thought signature. |
| [GoogleTextAssistantPart](/api/Vercel.AI.Google/GoogleTextAssistantPart/) | Assistant text. |
| [GoogleTextUserPart](/api/Vercel.AI.Google/GoogleTextUserPart/) | User text. |
| [GoogleThinking](/api/Vercel.AI.Google/GoogleThinking/) | Maps the shared reasoning option onto Gemini thinking configuration. |
| [GoogleTokenUsage](/api/Vercel.AI.Google/GoogleTokenUsage/) | Token usage converted from Gemini usageMetadata. |
| [GoogleToolCall](/api/Vercel.AI.Google/GoogleToolCall/) | A tool call that records whether the provider executed it. |
| [GoogleToolCallAssistantPart](/api/Vercel.AI.Google/GoogleToolCallAssistantPart/) | An assistant function or server tool call. |
| [GoogleToolContentPart](/api/Vercel.AI.Google/GoogleToolContentPart/) | One multimodal tool-result part. |
| [GoogleToolPreparation](/api/Vercel.AI.Google/GoogleToolPreparation/) | Tools and toolConfig prepared for a generateContent request. |
| [GoogleToolResult](/api/Vercel.AI.Google/GoogleToolResult/) | A provider-executed tool result. |
| [GoogleToolResultPart](/api/Vercel.AI.Google/GoogleToolResultPart/) | One tool result. |
| [GoogleTools](/api/Vercel.AI.Google/GoogleTools/) | Prepares Gemini function and provider-defined tools. |
| [GoogleToolTurn](/api/Vercel.AI.Google/GoogleToolTurn/) | A tool-result turn. |
| [GoogleTranscriptionModel](/api/Vercel.AI.Google/GoogleTranscriptionModel/) | Gemini transcription. Unary models post to the Interactions API. |
| [GoogleTurn](/api/Vercel.AI.Google/GoogleTurn/) | One rich prompt turn used by the message converter. |
| [GoogleUsage](/api/Vercel.AI.Google/GoogleUsage/) | Converts Gemini usageMetadata. |
| [GoogleUserPart](/api/Vercel.AI.Google/GoogleUserPart/) | A user content part. |
| [GoogleUserTurn](/api/Vercel.AI.Google/GoogleUserTurn/) | A user turn. |
| [GoogleVertexAnthropicOptions](/api/Vercel.AI.Google/GoogleVertexAnthropicOptions/) | Settings for Anthropic on Vertex. |
| [GoogleVertexAnthropicProvider](/api/Vercel.AI.Google/GoogleVertexAnthropicProvider/) | Anthropic on Vertex. Sends the Anthropic Messages body to rawPredict with anthropicversion in the body. Vertex does not accept URL sources or strict tools. Native output is opt-in. |
| [GoogleVertexAuth](/api/Vercel.AI.Google/GoogleVertexAuth/) | Header construction for Vertex. Live Application Default Credentials are not contacted. |
| [GoogleVertexCloudSpeechModel](/api/Vercel.AI.Google/GoogleVertexCloudSpeechModel/) | Cloud Text-to-Speech text:synthesize for Chirp 3 HD voices. |
| [GoogleVertexEndpoints](/api/Vercel.AI.Google/GoogleVertexEndpoints/) | Vertex host and URL construction. Locations are single DNS labels so they cannot rewrite the host. |
| [GoogleVertexGeminiTranscriptionModel](/api/Vercel.AI.Google/GoogleVertexGeminiTranscriptionModel/) | Vertex Gemini transcription through generateContent. |
| [GoogleVertexMaasProvider](/api/Vercel.AI.Google/GoogleVertexMaasProvider/) | Vertex MaaS OpenAI-compatible endpoint. |
| [GoogleVertexPartnerOptions](/api/Vercel.AI.Google/GoogleVertexPartnerOptions/) | Settings for the OpenAI-compatible MaaS and xAI endpoints on Vertex. |
| [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/) | Google Vertex AI provider. |
| [GoogleVertexSpeechTranscriptionModel](/api/Vercel.AI.Google/GoogleVertexSpeechTranscriptionModel/) | Cloud Speech-to-Text recognize. |
| [GoogleVertexVideoModel](/api/Vercel.AI.Google/GoogleVertexVideoModel/) | Google Vertex video model (Veo). CancellationToken) starts a long-running prediction and CancellationToken) fetches it. The operation is a JSON object with the operationName. |
| [GoogleVertexVideoModelOptions](/api/Vercel.AI.Google/GoogleVertexVideoModelOptions/) | Port of GoogleVertexVideoModelOptions (google-vertex-video-model-options.ts). Read from providerOptions.googleVertex, or the legacy providerOptions.vertex. |
| [GoogleVertexXaiProvider](/api/Vercel.AI.Google/GoogleVertexXaiProvider/) | xAI Grok models hosted on Vertex. |
| [GoogleVideoMetadata](/api/Vercel.AI.Google/GoogleVideoMetadata/) | Metadata for preprocessing video file parts. |
| [GoogleVideoModel](/api/Vercel.AI.Google/GoogleVideoModel/) | Veo video model. CancellationToken) posts predictLongRunning and CancellationToken) reads the operation. The operation is a JSON object with the operationName. |
| [GoogleVideoModelOptions](/api/Vercel.AI.Google/GoogleVideoModelOptions/) | Provider options for Google video generation, read from providerOptions.google. Upstream GoogleVideoModelOptions. |
| [GoogleVideoModelReferenceImage](/api/Vercel.AI.Google/GoogleVideoModelReferenceImage/) | A reference image for Veo style or asset guidance. Upstream inline type of referenceImages. |
| [GoogleWarning](/api/Vercel.AI.Google/GoogleWarning/) | A warning produced while preparing a Gemini request. |
| [PrepareGoogleInteractionsToolsResult](/api/Vercel.AI.Google/PrepareGoogleInteractionsToolsResult/) | Result of ToolChoice): the tools and toolchoice request fields and the warnings. |
| [ToolResultDownloads](/api/Vercel.AI.Google/ToolResultDownloads/) | Settings for downloading remote files that a tool result returns. Maps to toolResultDownloads. |
| [VertexOptions](/api/Vercel.AI.Google/VertexOptions/) | Vertex settings. The default route stays compatible with the regional v1 publisher URL. |
