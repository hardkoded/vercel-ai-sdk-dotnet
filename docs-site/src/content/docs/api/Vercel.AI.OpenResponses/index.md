---
title: "Vercel.AI.OpenResponses"
description: "Public types in the `Vercel.AI.OpenResponses` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.OpenResponses"
---

[API reference](/api/)

Public types in the `Vercel.AI.OpenResponses` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [OpenResponsesDecodeEventContext](/api/Vercel.AI.OpenResponses/OpenResponsesDecodeEventContext/) | Input of DecodeEvent. |
| [OpenResponsesDecodeItemContext](/api/Vercel.AI.OpenResponses/OpenResponsesDecodeItemContext/) | Input of DecodeItem. |
| [OpenResponsesEncodeInputItemContext](/api/Vercel.AI.OpenResponses/OpenResponsesEncodeInputItemContext/) | Input of EncodeInputItem. |
| [OpenResponsesErrorMetadata](/api/Vercel.AI.OpenResponses/OpenResponsesErrorMetadata/) | Status and retry metadata read from an endpoint-specific response error. |
| [OpenResponsesExtension](/api/Vercel.AI.OpenResponses/OpenResponsesExtension/) | One Open Responses extension. Tool, item, and event capabilities are registered independently. |
| [OpenResponsesExtensionRegistry](/api/Vercel.AI.OpenResponses/OpenResponsesExtensionRegistry/) | Indexes extensions by id and by wire type. |
| [OpenResponsesExtensions](/api/Vercel.AI.OpenResponses/OpenResponsesExtensions/) | Builds an OpenResponsesExtensionRegistry. |
| [OpenResponsesLanguageModel](/api/Vercel.AI.OpenResponses/OpenResponsesLanguageModel/) | Posts to /responses. |
| [OpenResponsesOptions](/api/Vercel.AI.OpenResponses/OpenResponsesOptions/) | Open Responses settings. The default origin is the AI Gateway Open Responses route. |
| [OpenResponsesProvider](/api/Vercel.AI.OpenResponses/OpenResponsesProvider/) | Open Responses provider. |
| [OpenResponsesServiceCollectionExtensions](/api/Vercel.AI.OpenResponses/OpenResponsesServiceCollectionExtensions/) | Registers Open Responses. |
| [OpenResponsesToolContext](/api/Vercel.AI.OpenResponses/OpenResponsesToolContext/) | Input of EncodeTool and EncodeToolChoice. |

## Enums

| Type | Summary |
| --- | --- |
| [OpenResponsesItemMode](/api/Vercel.AI.OpenResponses/OpenResponsesItemMode/) | Whether an item is decoded from a generate response or from a stream. |
