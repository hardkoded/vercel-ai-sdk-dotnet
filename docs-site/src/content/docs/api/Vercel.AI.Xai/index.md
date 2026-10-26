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
| [XaiClientSecret](/api/Vercel.AI.Xai/XaiClientSecret/) | Client secret returned by /realtime/clientsecrets. |
| [XaiFileDeleteResult](/api/Vercel.AI.Xai/XaiFileDeleteResult/) | Result of deleting an xAI file. |
| [XaiFileDownload](/api/Vercel.AI.Xai/XaiFileDownload/) | Downloaded xAI file bytes. |
| [XaiFileMetadata](/api/Vercel.AI.Xai/XaiFileMetadata/) | Metadata stored under providerMetadata.xai. A field is null when the response did not return it. |
| [XaiFileResult](/api/Vercel.AI.Xai/XaiFileResult/) | Result of an xAI file upload or metadata read. |
| [XaiFiles](/api/Vercel.AI.Xai/XaiFiles/) | xAI Files API at {base}/files. |
| [XaiFileSearchOptions](/api/Vercel.AI.Xai/XaiFileSearchOptions/) | Arguments of String). Maps to fileSearchArgsSchema. |
| [XaiFileUpload](/api/Vercel.AI.Xai/XaiFileUpload/) | Bytes, base64, or a stream uploaded to xAI. |
| [XaiImageGenerationOptions](/api/Vercel.AI.Xai/XaiImageGenerationOptions/) | Arguments of String). Maps to imageGenerationArgsSchema. |
| [XaiMcpServerOptions](/api/Vercel.AI.Xai/XaiMcpServerOptions/) | Arguments of String). Maps to mcpServerArgsSchema. |
| [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/) | xAI provider at https://api.x.ai/v1. LanguageModel(string) returns the Responses API model, as upstream does. |
| [XaiRealtimeModel](/api/Vercel.AI.Xai/XaiRealtimeModel/) | xAI realtime voice model. |
| [XaiReasoning](/api/Vercel.AI.Xai/XaiReasoning/) | Reasoning text that carries the xAI Responses item id and encrypted content. |
| [XaiResponsesLanguageModel](/api/Vercel.AI.Xai/XaiResponsesLanguageModel/) | xAI Responses API language model. Generate and stream requests post to /responses. Pass xAI provider-defined tools with JsonElement) and the ids xai.websearch, xai.xsearch, xai.codeexecution, xai.viewimage, xai.viewxvideo, xai.imagegeneration, xai.filesearch, and xai.mcp. |
| [XaiServiceCollectionExtensions](/api/Vercel.AI.Xai/XaiServiceCollectionExtensions/) | Registers XaiProvider. |
| [XaiTools](/api/Vercel.AI.Xai/XaiTools/) | Factories for the xAI provider-defined tools. Each returns a Tool that xAI runs. Maps to xai.tools. The name is the name the model and the results use. It defaults to the tool's wire name. |
| [XaiWebSearchOptions](/api/Vercel.AI.Xai/XaiWebSearchOptions/) | Arguments of String). Maps to webSearchArgsSchema. |
| [XaiXSearchOptions](/api/Vercel.AI.Xai/XaiXSearchOptions/) | Arguments of String). Maps to xSearchArgsSchema. |

## Enums

| Type | Summary |
| --- | --- |
| [XaiImageGenerationAction](/api/Vercel.AI.Xai/XaiImageGenerationAction/) | Restricts what the xai.imagegeneration tool can do. Maps to the action argument. |
