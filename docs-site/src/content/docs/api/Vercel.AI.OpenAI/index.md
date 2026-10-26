---
title: "Vercel.AI.OpenAI"
description: "OpenAI Chat Completions, Responses, images, speech, and transcription."
editUrl: false
sidebar:
  label: "Vercel.AI.OpenAI"
---

[API reference](/api/)

OpenAI Chat Completions, Responses, images, speech, and transcription.

## Classes

| Type | Summary |
| --- | --- |
| [OpenAIApproximateLocation](/api/Vercel.AI.OpenAI/OpenAIApproximateLocation/) | An approximate user location for the OpenAI web search tools. |
| [OpenAIBatchApi](/api/Vercel.AI.OpenAI/OpenAIBatchApi/) | OpenAI Batch API. Starts, reads, cancels, and lists Responses batches, and downloads their results. |
| [OpenAICallWarning](/api/Vercel.AI.OpenAI/OpenAICallWarning/) | A warning produced while preparing an OpenAI request. |
| [OpenAIChatLanguageModel](/api/Vercel.AI.OpenAI/OpenAIChatLanguageModel/) | OpenAI Chat Completions language model. |
| [OpenAIChatMessageResult](/api/Vercel.AI.OpenAI/OpenAIChatMessageResult/) | Chat Completions messages and the warnings produced while converting them. |
| [OpenAIChatMessages](/api/Vercel.AI.OpenAI/OpenAIChatMessages/) | Converts prompts into OpenAI Chat Completions messages. |
| [OpenAIChatPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIChatPreparedRequest/) | Chat Completions request body and the warnings produced while building it. |
| [OpenAIChatPromptMessage](/api/Vercel.AI.OpenAI/OpenAIChatPromptMessage/) | One prompt message converted into an OpenAI chat message. |
| [OpenAIChatPromptPart](/api/Vercel.AI.OpenAI/OpenAIChatPromptPart/) | One user or assistant content part. |
| [OpenAIChatToolPreparation](/api/Vercel.AI.OpenAI/OpenAIChatToolPreparation/) | Chat Completions tools and tool choice. |
| [OpenAIChatTools](/api/Vercel.AI.OpenAI/OpenAIChatTools/) | Prepares Chat Completions function tools. |
| [OpenAICodeInterpreterToolOptions](/api/Vercel.AI.OpenAI/OpenAICodeInterpreterToolOptions/) | Options of CodeInterpreter(OpenAICodeInterpreterToolOptions). ContainerId and FileIds are alternatives; with neither the container is created automatically. |
| [OpenAICompletionLanguageModel](/api/Vercel.AI.OpenAI/OpenAICompletionLanguageModel/) | OpenAI legacy completions language model. |
| [OpenAICompletionPreparedRequest](/api/Vercel.AI.OpenAI/OpenAICompletionPreparedRequest/) | Completion request body and warnings. |
| [OpenAICompletionPrompt](/api/Vercel.AI.OpenAI/OpenAICompletionPrompt/) | A completion prompt and its default stop sequences. |
| [OpenAICustomToolOptions](/api/Vercel.AI.OpenAI/OpenAICustomToolOptions/) | Options of CustomTool(OpenAICustomToolOptions). |
| [OpenAIDecisionModel](/api/Vercel.AI.OpenAI/OpenAIDecisionModel/) | Answers questions through the OpenAI Decisions API (POST /decisions). |
| [OpenAIEmbeddingCallResult](/api/Vercel.AI.OpenAI/OpenAIEmbeddingCallResult/) | Embedding vectors plus the raw response. |
| [OpenAIEmbeddingModel](/api/Vercel.AI.OpenAI/OpenAIEmbeddingModel/) | OpenAI embeddings model. |
| [OpenAIFileMetadata](/api/Vercel.AI.OpenAI/OpenAIFileMetadata/) | Metadata returned by the OpenAI Files API. |
| [OpenAIFileReference](/api/Vercel.AI.OpenAI/OpenAIFileReference/) | A file reference that carries an OpenAI file id. |
| [OpenAIFileSearchFilter](/api/Vercel.AI.OpenAI/OpenAIFileSearchFilter/) | A file search filter: a comparison (Key, Type, one value) or a compound (and/or over Filters). |
| [OpenAIFileSearchToolOptions](/api/Vercel.AI.OpenAI/OpenAIFileSearchToolOptions/) | Options of FileSearch(OpenAIFileSearchToolOptions). |
| [OpenAIFileStore](/api/Vercel.AI.OpenAI/OpenAIFileStore/) | OpenAI file store. |
| [OpenAIImageCall](/api/Vercel.AI.OpenAI/OpenAIImageCall/) | Image generation settings beyond the shared prompt and size. |
| [OpenAIImageFile](/api/Vercel.AI.OpenAI/OpenAIImageFile/) | An image file sent to the edits endpoint. |
| [OpenAIImageGeneration](/api/Vercel.AI.OpenAI/OpenAIImageGeneration/) | Image generation result, including usage and warnings. |
| [OpenAIImageGenerationToolOptions](/api/Vercel.AI.OpenAI/OpenAIImageGenerationToolOptions/) | Options of ImageGeneration(OpenAIImageGenerationToolOptions). Maps to the upstream imageGeneration args. |
| [OpenAIImageModel](/api/Vercel.AI.OpenAI/OpenAIImageModel/) | OpenAI image generation and edit model. |
| [OpenAIJsonSchema](/api/Vercel.AI.OpenAI/OpenAIJsonSchema/) | Removes JSON Schema keywords OpenAI does not enforce: string propertyNames and regex lookaround in pattern. |
| [OpenAIJsonSchemaResult](/api/Vercel.AI.OpenAI/OpenAIJsonSchemaResult/) | JSON Schema adjusted for OpenAI structured outputs. |
| [OpenAILanguageModelCapabilities](/api/Vercel.AI.OpenAI/OpenAILanguageModelCapabilities/) | Request limits derived from an OpenAI model id. |
| [OpenAIMcpToolOptions](/api/Vercel.AI.OpenAI/OpenAIMcpToolOptions/) | Options of Mcp(OpenAIMcpToolOptions). One of ServerUrl or ConnectorId must be set. |
| [OpenAIOptions](/api/Vercel.AI.OpenAI/OpenAIOptions/) | OpenAI provider settings. |
| [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) | OpenAI provider: Chat Completions, Responses, embeddings, images, speech, transcription, files, and batches. |
| [OpenAIRealtimeClientSecret](/api/Vercel.AI.OpenAI/OpenAIRealtimeClientSecret/) | A realtime client secret. |
| [OpenAIRealtimeModel](/api/Vercel.AI.OpenAI/OpenAIRealtimeModel/) | OpenAI Realtime session URI builder and WebSocket client. |
| [OpenAIResponsesAllowedTools](/api/Vercel.AI.OpenAI/OpenAIResponsesAllowedTools/) | The tools a request may call, sent as toolchoice: { type: "allowedtools" }. Maps to the allowedTools option of the OpenAI Responses provider options. |
| [OpenAIResponsesLanguageModel](/api/Vercel.AI.OpenAI/OpenAIResponsesLanguageModel/) | OpenAI Responses API language model. |
| [OpenAIResponsesPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIResponsesPreparedRequest/) | Responses API request body and the warnings produced while building it. |
| [OpenAIServiceCollectionExtensions](/api/Vercel.AI.OpenAI/OpenAIServiceCollectionExtensions/) | Registers OpenAIProvider. |
| [OpenAIShellDomainSecret](/api/Vercel.AI.OpenAI/OpenAIShellDomainSecret/) | A domain secret of an allowlist network policy. |
| [OpenAIShellEnvironment](/api/Vercel.AI.OpenAI/OpenAIShellEnvironment/) | Environment of Shell(OpenAIShellToolOptions): containerAuto, containerReference, or local. |
| [OpenAIShellLocalSkill](/api/Vercel.AI.OpenAI/OpenAIShellLocalSkill/) | A skill available in a local shell environment. |
| [OpenAIShellToolOptions](/api/Vercel.AI.OpenAI/OpenAIShellToolOptions/) | Options of Shell(OpenAIShellToolOptions). |
| [OpenAISkillFile](/api/Vercel.AI.OpenAI/OpenAISkillFile/) | One file uploaded as part of a skill. |
| [OpenAISkillStore](/api/Vercel.AI.OpenAI/OpenAISkillStore/) | OpenAI skill store. |
| [OpenAISkillUpload](/api/Vercel.AI.OpenAI/OpenAISkillUpload/) | Skill upload result. |
| [OpenAISpeechCall](/api/Vercel.AI.OpenAI/OpenAISpeechCall/) | Speech synthesis settings. |
| [OpenAISpeechModel](/api/Vercel.AI.OpenAI/OpenAISpeechModel/) | OpenAI speech model. |
| [OpenAISpeechPreparation](/api/Vercel.AI.OpenAI/OpenAISpeechPreparation/) | Speech request body and warnings. |
| [OpenAISpeechResult](/api/Vercel.AI.OpenAI/OpenAISpeechResult/) | Speech audio plus the response metadata recorded for the call. |
| [OpenAISpeechTranslationModel](/api/Vercel.AI.OpenAI/OpenAISpeechTranslationModel/) | OpenAI speech translation model. REST calls use /audio/translations. |
| [OpenAIText](/api/Vercel.AI.OpenAI/OpenAIText/) | Generated text that carries the Responses output item id and its annotations. |
| [OpenAITools](/api/Vercel.AI.OpenAI/OpenAITools/) | Factories for the OpenAI provider-defined tools, the port of upstream openaiTools. Each builds the LanguageModelTool that OpenAI Responses maps to a hosted tool. |
| [OpenAIToolSearchToolOptions](/api/Vercel.AI.OpenAI/OpenAIToolSearchToolOptions/) | Options of ToolSearch(OpenAIToolSearchToolOptions). |
| [OpenAITranscriptionCall](/api/Vercel.AI.OpenAI/OpenAITranscriptionCall/) | Transcription settings beyond the audio payload. |
| [OpenAITranscriptionModel](/api/Vercel.AI.OpenAI/OpenAITranscriptionModel/) | OpenAI audio transcription model. |
| [OpenAITranscriptionResponse](/api/Vercel.AI.OpenAI/OpenAITranscriptionResponse/) | Response metadata recorded for a transcription. |
| [OpenAITranscriptionResult](/api/Vercel.AI.OpenAI/OpenAITranscriptionResult/) | Transcript with timed segments and response metadata. |
| [OpenAIUnsupportedTool](/api/Vercel.AI.OpenAI/OpenAIUnsupportedTool/) | A non-function tool passed to Chat Completions preparation. |
| [OpenAIWebSearchPreviewToolOptions](/api/Vercel.AI.OpenAI/OpenAIWebSearchPreviewToolOptions/) | Options of WebSearchPreview(OpenAIWebSearchPreviewToolOptions). |
| [OpenAIWebSearchToolOptions](/api/Vercel.AI.OpenAI/OpenAIWebSearchToolOptions/) | Options of WebSearch(OpenAIWebSearchToolOptions). |
| [PreparedResponsesTools](/api/Vercel.AI.OpenAI/PreparedResponsesTools/) | Tools prepared for the OpenAI Responses API. |
| [PrepareResponsesToolsOptions](/api/Vercel.AI.OpenAI/PrepareResponsesToolsOptions/) | Options of PrepareResponsesToolsOptions) besides the tools and the tool choice. |
