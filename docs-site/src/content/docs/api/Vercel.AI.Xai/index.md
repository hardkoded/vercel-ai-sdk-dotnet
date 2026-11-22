---
title: "Vercel.AI.Xai"
description: "xAI provider."
editUrl: false
sidebar:
  label: "Vercel.AI.Xai"
---

[API reference](/api/)

xAI provider.

## Classes

| Type | Summary |
| --- | --- |
| [XaiBatch](/api/Vercel.AI.Xai/XaiBatch/) | xAI Batch API. Uploads the requests as a JSONL file, creates a file-backed batch, and pages through its results. Maps to XaiBatch. |
| [XaiClientSecret](/api/Vercel.AI.Xai/XaiClientSecret/) | Client secret returned by /realtime/clientsecrets. |
| [XaiFileDeleteResult](/api/Vercel.AI.Xai/XaiFileDeleteResult/) | Result of deleting an xAI file. |
| [XaiFileDownload](/api/Vercel.AI.Xai/XaiFileDownload/) | Downloaded xAI file bytes. |
| [XaiFileMetadata](/api/Vercel.AI.Xai/XaiFileMetadata/) | Metadata stored under providerMetadata.xai. A field is null when the response did not return it. |
| [XaiFileResult](/api/Vercel.AI.Xai/XaiFileResult/) | Result of an xAI file upload or metadata read. |
| [XaiFiles](/api/Vercel.AI.Xai/XaiFiles/) | xAI Files API at {base}/files. |
| [XaiFileSearchOptions](/api/Vercel.AI.Xai/XaiFileSearchOptions/) | Arguments of String). Maps to fileSearchArgsSchema. |
| [XaiFileUpload](/api/Vercel.AI.Xai/XaiFileUpload/) | Bytes, base64, or a stream uploaded to xAI. |
| [XaiImageGenerationOptions](/api/Vercel.AI.Xai/XaiImageGenerationOptions/) | Arguments of String). Maps to imageGenerationArgsSchema. |
| [XaiImageModel](/api/Vercel.AI.Xai/XaiImageModel/) | xAI image model, over /images/generations and /images/edits. Maps to XaiImageModel. |
| [XaiImageModelConfig](/api/Vercel.AI.Xai/XaiImageModelConfig/) | Settings of XaiImageModel. Maps to XaiImageModelConfig. |
| [XaiMcpServerOptions](/api/Vercel.AI.Xai/XaiMcpServerOptions/) | Arguments of String). Maps to mcpServerArgsSchema. |
| [XaiOptions](/api/Vercel.AI.Xai/XaiOptions/) | xAI settings. Maps to XaiProviderSettings. |
| [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/) | xAI provider at https://api.x.ai/v1. LanguageModel(string) returns the Responses API model, as upstream does. |
| [XaiRealtimeEventMapper](/api/Vercel.AI.Xai/XaiRealtimeEventMapper/) | Maps xAI realtime wire events to normalized events and back. Maps to the exported functions of xai-realtime-event-mapper.ts: parseXaiRealtimeServerEvent, serializeXaiRealtimeClientEvent and buildXaiSessionConfig. |
| [XaiRealtimeModel](/api/Vercel.AI.Xai/XaiRealtimeModel/) | xAI realtime voice model. Maps to XaiRealtimeModel. |
| [XaiRealtimeModelConfig](/api/Vercel.AI.Xai/XaiRealtimeModelConfig/) | Settings of XaiRealtimeModel. Maps to XaiRealtimeModelConfig. Headers maps the upstream Resolvable: an object, a promise, a callback or an async callback all fit a function that returns a task. |
| [XaiRealtimeModelOptions](/api/Vercel.AI.Xai/XaiRealtimeModelOptions/) | xAI realtime provider options, read from providerOptions.xai. Upstream types this as Record<string, never>: there are no options yet. Maps to XaiRealtimeModelOptions of xai-realtime-model-options.ts. |
| [XaiReasoning](/api/Vercel.AI.Xai/XaiReasoning/) | Reasoning text that carries the xAI Responses item id and encrypted content. |
| [XaiResponsesLanguageModel](/api/Vercel.AI.Xai/XaiResponsesLanguageModel/) | xAI Responses API language model. Generate and stream requests post to /responses. Pass xAI provider-defined tools with JsonElement%7d) and the ids xai.websearch, xai.xsearch, xai.codeexecution, xai.viewimage, xai.viewxvideo, xai.imagegeneration, xai.filesearch, and xai.mcp. |
| [XaiServiceCollectionExtensions](/api/Vercel.AI.Xai/XaiServiceCollectionExtensions/) | Registers XaiProvider. |
| [XaiSpeechModel](/api/Vercel.AI.Xai/XaiSpeechModel/) | xAI text-to-speech model, over /tts. Maps to XaiSpeechModel. |
| [XaiSpeechModelOptions](/api/Vercel.AI.Xai/XaiSpeechModelOptions/) | Provider options for xAI speech generation, read from providerOptions.xai. Maps to XaiSpeechModelOptions. |
| [XaiTools](/api/Vercel.AI.Xai/XaiTools/) | Factories for the xAI provider-defined tools. Each returns a Tool that xAI runs. Maps to xai.tools. The name is the name the model and the results use. It defaults to the tool's wire name. |
| [XaiTranscriptionModel](/api/Vercel.AI.Xai/XaiTranscriptionModel/) | xAI speech-to-text model, over /stt and its WebSocket. Maps to XaiTranscriptionModel. |
| [XaiTranscriptionModelOptions](/api/Vercel.AI.Xai/XaiTranscriptionModelOptions/) | xAI transcription options, read from providerOptions.xai. Maps to XaiTranscriptionModelOptions of xai-transcription-model-options.ts. |
| [XaiTranscriptionStreamingOptions](/api/Vercel.AI.Xai/XaiTranscriptionStreamingOptions/) | Options of streaming for streaming speech-to-text over WebSocket. |
| [XaiVideoKeyframe](/api/Vercel.AI.Xai/XaiVideoKeyframe/) | A mid-video image anchor. Upstream XaiVideoKeyframe. |
| [XaiVideoModel](/api/Vercel.AI.Xai/XaiVideoModel/) | xAI video model. CancellationToken) creates a request and CancellationToken) reads it. It supports generation, editing, extension, and reference-to-video, and can deliver the video to a signed upload URL. The operation is a JSON object with the requestId and, for upload delivery, hasUploadUrl. |
| [XaiVideoModelOptions](/api/Vercel.AI.Xai/XaiVideoModelOptions/) | Provider options for xAI video generation, read from providerOptions.xai. Upstream XaiVideoModelOptions and XaiParsedVideoModelOptions. |
| [XaiVideoStorageOptions](/api/Vercel.AI.Xai/XaiVideoStorageOptions/) | Store the generated video in the xAI Files API. Upstream XaiVideoStorageOptions. |
| [XaiWebSearchOptions](/api/Vercel.AI.Xai/XaiWebSearchOptions/) | Arguments of String). Maps to webSearchArgsSchema. |
| [XaiXSearchOptions](/api/Vercel.AI.Xai/XaiXSearchOptions/) | Arguments of String). Maps to xSearchArgsSchema. |

## Enums

| Type | Summary |
| --- | --- |
| [XaiImageGenerationAction](/api/Vercel.AI.Xai/XaiImageGenerationAction/) | Restricts what the xai.imagegeneration tool can do. Maps to the action argument. |
