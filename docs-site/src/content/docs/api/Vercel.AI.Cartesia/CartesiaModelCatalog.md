---
title: "CartesiaModelCatalog"
description: "Cartesia model catalog. Maps to cartesia-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "CartesiaModelCatalog"
  hidden: true
---

<a id="Vercel_AI_Cartesia_CartesiaModelCatalog"></a>

Namespace: [Vercel.AI.Cartesia](/api/Vercel.AI.Cartesia/)


```csharp
public static class CartesiaModelCatalog
```


Cartesia model catalog. Maps to `cartesia-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaStreamingTranscription.cs#L26)

## Methods

<a id="Vercel_AI_Cartesia_CartesiaModelCatalog_GetCartesiaTranscriptionModelEntry_"></a>
<a id="Vercel_AI_Cartesia_CartesiaModelCatalog_GetCartesiaTranscriptionModelEntry_System_String_"></a>

### GetCartesiaTranscriptionModelEntry(string)


```csharp
public static CartesiaTranscriptionCapabilities GetCartesiaTranscriptionModelEntry(string modelId)
```


Returns the capabilities of a transcription model id. Future ids inherit the streaming protocol.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [CartesiaTranscriptionCapabilities](/api/Vercel.AI.Cartesia/CartesiaTranscriptionCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaStreamingTranscription.cs#L29)
