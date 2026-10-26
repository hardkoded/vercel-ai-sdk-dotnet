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
| [FileStreamPart](/api/Vercel.AI.Google/FileStreamPart/) | A streamed file part. The shared stream model has no file delta, so this carries the bytes. |
| [GoogleAssistantPart](/api/Vercel.AI.Google/GoogleAssistantPart/) | An assistant content part. |
| [GoogleAssistantTurn](/api/Vercel.AI.Google/GoogleAssistantTurn/) | An assistant turn. |
| [GoogleBatchApi](/api/Vercel.AI.Google/GoogleBatchApi/) | Gemini Batch API. Downloads the file-based JSONL results of a finished batch. |
| [GoogleEmbeddingModel](/api/Vercel.AI.Google/GoogleEmbeddingModel/) | Gemini embedding model. One value uses :embedContent; several values use :batchEmbedContents. |
| [GoogleFileAssistantPart](/api/Vercel.AI.Google/GoogleFileAssistantPart/) | An assistant file or reasoning file. |
| [GoogleFileUrls](/api/Vercel.AI.Google/GoogleFileUrls/) | URLs Gemini can read without downloading the bytes. |
| [GoogleFileUserPart](/api/Vercel.AI.Google/GoogleFileUserPart/) | A user file. Kind is url, data, text, or reference. |
| [GoogleFinishReason](/api/Vercel.AI.Google/GoogleFinishReason/) | Maps Gemini finish reasons onto the shared finish-reason enum. |
| [GoogleGeneratedFile](/api/Vercel.AI.Google/GoogleGeneratedFile/) | A generated file, including thought images. |
| [GoogleImageModel](/api/Vercel.AI.Google/GoogleImageModel/) | Gemini image generation. The call runs through GoogleLanguageModel with the IMAGE response modality, so the google provider options apply, and googleSearch enables Google Search grounding. |
| [GoogleInteractionsAgent](/api/Vercel.AI.Google/GoogleInteractionsAgent/) | An agent to call through the Interactions API: { agent: name } or { managedAgent: name } upstream. |
| [GoogleInteractionsFinish](/api/Vercel.AI.Google/GoogleInteractionsFinish/) | Maps an Interactions API status onto a finish reason. |
| [GoogleInteractionsInput](/api/Vercel.AI.Google/GoogleInteractionsInput/) | Converts a prompt into an Interactions input array. |
| [GoogleInteractionsModel](/api/Vercel.AI.Google/GoogleInteractionsModel/) | Language model that posts to the Gemini Interactions API. |
| [GoogleInteractionsOutput](/api/Vercel.AI.Google/GoogleInteractionsOutput/) | Reads text and function calls from an Interactions response. |
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
| [GoogleRealtime](/api/Vercel.AI.Google/GoogleRealtime/) | Maps Gemini Live WebSocket messages. No socket is opened, so this compiles on netstandard2.0. |
| [GoogleRealtimeClientSecret](/api/Vercel.AI.Google/GoogleRealtimeClientSecret/) | An ephemeral Gemini Live auth token and the WebSocket URL it opens. |
| [GoogleRealtimeEvent](/api/Vercel.AI.Google/GoogleRealtimeEvent/) | One normalized Gemini Live event. |
| [GoogleRealtimeModel](/api/Vercel.AI.Google/GoogleRealtimeModel/) | Realtime model. Event mapping is local; opening the Live socket is not available on this runtime. |
| [GoogleRealtimeSession](/api/Vercel.AI.Google/GoogleRealtimeSession/) | Session fields accepted by GoogleRealtime. |
| [GoogleRealtimeTool](/api/Vercel.AI.Google/GoogleRealtimeTool/) | A realtime function tool. |
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
| [GoogleVertexAnthropicProvider](/api/Vercel.AI.Google/GoogleVertexAnthropicProvider/) | Anthropic on Vertex. Sends the Anthropic Messages body to rawPredict with anthropicversion in the body. Vertex does not accept URL sources, the native output format, or strict tools. |
| [GoogleVertexAuth](/api/Vercel.AI.Google/GoogleVertexAuth/) | Header construction for Vertex. Live Application Default Credentials are not contacted. |
| [GoogleVertexCloudSpeechModel](/api/Vercel.AI.Google/GoogleVertexCloudSpeechModel/) | Cloud Text-to-Speech text:synthesize for Chirp 3 HD voices. |
| [GoogleVertexEndpoints](/api/Vercel.AI.Google/GoogleVertexEndpoints/) | Vertex host and URL construction. Locations are single DNS labels so they cannot rewrite the host. |
| [GoogleVertexGeminiTranscriptionModel](/api/Vercel.AI.Google/GoogleVertexGeminiTranscriptionModel/) | Vertex Gemini transcription through generateContent. |
| [GoogleVertexMaasProvider](/api/Vercel.AI.Google/GoogleVertexMaasProvider/) | Vertex MaaS OpenAI-compatible endpoint. |
| [GoogleVertexPartnerOptions](/api/Vercel.AI.Google/GoogleVertexPartnerOptions/) | Settings for the OpenAI-compatible MaaS and xAI endpoints on Vertex. |
| [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/) | Google Vertex AI provider. |
| [GoogleVertexSpeechTranscriptionModel](/api/Vercel.AI.Google/GoogleVertexSpeechTranscriptionModel/) | Cloud Speech-to-Text recognize. |
| [GoogleVertexXaiProvider](/api/Vercel.AI.Google/GoogleVertexXaiProvider/) | xAI Grok models hosted on Vertex. |
| [GoogleWarning](/api/Vercel.AI.Google/GoogleWarning/) | A warning produced while preparing a Gemini request. |
| [VertexOptions](/api/Vercel.AI.Google/VertexOptions/) | Vertex settings. The default route stays compatible with the regional v1 publisher URL. |
