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
| [OpenAIApplyPatchInput](/api/Vercel.AI.OpenAI/OpenAIApplyPatchInput/) | The input the model sends to the applypatch tool. |
| [OpenAIApplyPatchOperation](/api/Vercel.AI.OpenAI/OpenAIApplyPatchOperation/) | A patch operation of the applypatch tool: createfile, deletefile, or updatefile. |
| [OpenAIApplyPatchOutput](/api/Vercel.AI.OpenAI/OpenAIApplyPatchOutput/) | The output of the applypatch tool, what the application sends back. |
| [OpenAIApplyPatchToolOptions](/api/Vercel.AI.OpenAI/OpenAIApplyPatchToolOptions/) | Options of ApplyPatch(OpenAIApplyPatchToolOptions). |
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
| [OpenAIMcpToolOptions](/api/Vercel.AI.OpenAI/OpenAIMcpToolOptions/) | Options of Mcp(OpenAIMcpToolOptions). One of ServerUrl, ConnectorId, or TunnelId must be set. TunnelId cannot be combined with ServerUrl or ConnectorId. |
| [OpenAIOptions](/api/Vercel.AI.OpenAI/OpenAIOptions/) | OpenAI provider settings. |
| [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) | OpenAI provider: Chat Completions, Responses, embeddings, images, speech, transcription, files, and batches. |
| [OpenAIRealtimeClientSecret](/api/Vercel.AI.OpenAI/OpenAIRealtimeClientSecret/) | A realtime client secret. |
| [OpenAIRealtimeModel](/api/Vercel.AI.OpenAI/OpenAIRealtimeModel/) | OpenAI Realtime session URI builder and WebSocket client. |
| [OpenAIRealtimeModelConfig](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelConfig/) | Settings of OpenAIRealtimeModel. Maps to OpenAIRealtimeModelConfig. Headers maps the upstream Resolvable: an object, a promise, a callback or an async callback all fit a function that returns a task. |
| [OpenAIRealtimeModelLive](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLive/) | OpenAI Live realtime model: a continuous conversation over a server WebSocket or WebRTC. Maps to OpenAIRealtimeModelLive. |
| [OpenAIRealtimeModelLiveDataChannel](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLiveDataChannel/) | Data channel permissions of a Live WebRTC client. |
| [OpenAIRealtimeModelLiveOptions](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLiveOptions/) | Experimental Live options under sessionConfig.providerOptions.openai. Maps to OpenAIRealtimeModelLiveOptions. ToJsonObject() builds the provider options value. |
| [OpenAIRealtimeModelLiveServerEventSelector](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLiveServerEventSelector/) | A server event the Live data channel forwards to the client. |
| [OpenAIRealtimeModelLiveWebRTCConfig](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLiveWebRTCConfig/) | The data channel a Live WebRTC client opens. Maps to the result of getWebRTCConfig. |
| [OpenAIRealtimeModelLiveWebRTCSession](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLiveWebRTCSession/) | The answer of a Live WebRTC session creation. Maps to the result of doCreateWebRTCSession. |
| [OpenAIRealtimeOptions](/api/Vercel.AI.OpenAI/OpenAIRealtimeOptions/) | Options of OpenAIRealtimeOptions). Maps to OpenAIRealtimeOptions. |
| [OpenAIRealtimeWebSocketConfig](/api/Vercel.AI.OpenAI/OpenAIRealtimeWebSocketConfig/) | URL and subprotocols a client opens a WebSocket with. Maps to the result of getWebSocketConfig. |
| [OpenAIResponsesAllowedTools](/api/Vercel.AI.OpenAI/OpenAIResponsesAllowedTools/) | The tools a request may call, sent as toolchoice: { type: "allowedtools" }. Maps to the allowedTools option of the OpenAI Responses provider options. |
| [OpenAIResponsesLanguageModel](/api/Vercel.AI.OpenAI/OpenAIResponsesLanguageModel/) | OpenAI Responses API language model. |
| [OpenAIResponsesPreparedRequest](/api/Vercel.AI.OpenAI/OpenAIResponsesPreparedRequest/) | Responses API request body and the warnings produced while building it. |
| [OpenAIServiceCollectionExtensions](/api/Vercel.AI.OpenAI/OpenAIServiceCollectionExtensions/) | Registers OpenAIProvider. |
| [OpenAIShellAction](/api/Vercel.AI.OpenAI/OpenAIShellAction/) | The shell action of a OpenAIShellInput. Maps to the action of the upstream shell input. |
| [OpenAIShellContainerSkill](/api/Vercel.AI.OpenAI/OpenAIShellContainerSkill/) | A skill of a containerAuto shell environment: OpenAIShellSkillReference or OpenAIShellInlineSkill. |
| [OpenAIShellDomainSecret](/api/Vercel.AI.OpenAI/OpenAIShellDomainSecret/) | A domain secret of an allowlist network policy. |
| [OpenAIShellEnvironment](/api/Vercel.AI.OpenAI/OpenAIShellEnvironment/) | Environment of Shell(OpenAIShellToolOptions): containerAuto, containerReference, or local. |
| [OpenAIShellInlineSkill](/api/Vercel.AI.OpenAI/OpenAIShellInlineSkill/) | An inline skill of a containerAuto shell environment: a base64 encoded zip archive. |
| [OpenAIShellInput](/api/Vercel.AI.OpenAI/OpenAIShellInput/) | The input the model sends to the shell tool. |
| [OpenAIShellLocalSkill](/api/Vercel.AI.OpenAI/OpenAIShellLocalSkill/) | A skill available in a local shell environment. |
| [OpenAIShellOutcome](/api/Vercel.AI.OpenAI/OpenAIShellOutcome/) | The outcome of one shell command: timeout, or exit with ExitCode. |
| [OpenAIShellOutput](/api/Vercel.AI.OpenAI/OpenAIShellOutput/) | The output of the shell tool, what the application sends back. |
| [OpenAIShellOutputItem](/api/Vercel.AI.OpenAI/OpenAIShellOutputItem/) | The output of one shell command. |
| [OpenAIShellSkillReference](/api/Vercel.AI.OpenAI/OpenAIShellSkillReference/) | A skillReference skill of a containerAuto shell environment. |
| [OpenAIShellToolOptions](/api/Vercel.AI.OpenAI/OpenAIShellToolOptions/) | Options of Shell(OpenAIShellToolOptions). |
| [OpenAISkillFile](/api/Vercel.AI.OpenAI/OpenAISkillFile/) | One file uploaded as part of a skill. |
| [OpenAISkillStore](/api/Vercel.AI.OpenAI/OpenAISkillStore/) | OpenAI skill store. |
| [OpenAISkillUpload](/api/Vercel.AI.OpenAI/OpenAISkillUpload/) | Skill upload result. |
| [OpenAISpeechCall](/api/Vercel.AI.OpenAI/OpenAISpeechCall/) | Speech synthesis settings. |
| [OpenAISpeechModel](/api/Vercel.AI.OpenAI/OpenAISpeechModel/) | OpenAI speech model. |
| [OpenAISpeechModelOptions](/api/Vercel.AI.OpenAI/OpenAISpeechModelOptions/) | OpenAI speech model options, read from the openai provider options. Routers that also call the OpenAI speech API (Azure) accept the same options under their own key. Maps to OpenAISpeechModelOptions. |
| [OpenAISpeechPreparation](/api/Vercel.AI.OpenAI/OpenAISpeechPreparation/) | Speech request body and warnings. |
| [OpenAISpeechResult](/api/Vercel.AI.OpenAI/OpenAISpeechResult/) | Speech audio plus the response metadata recorded for the call. |
| [OpenAISpeechTranslationModel](/api/Vercel.AI.OpenAI/OpenAISpeechTranslationModel/) | OpenAI speech translation model. REST calls use /audio/translations. Realtime streaming uses the /realtime/translations WebSocket. |
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
