---
title: "OpenAICompatibleStreamMetadataExtractor"
description: "Accumulates provider-specific metadata from stream chunks."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleStreamMetadataExtractor"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleStreamMetadataExtractor"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public abstract class OpenAICompatibleStreamMetadataExtractor
```


Accumulates provider-specific metadata from stream chunks.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleMetadataExtractor.cs#L23)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleStreamMetadataExtractor_BuildMetadata_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleStreamMetadataExtractor_BuildMetadata"></a>

### BuildMetadata()


```csharp
public abstract JsonObject? BuildMetadata()
```


Builds the provider metadata. Returns an object keyed by provider name, or null.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleMetadataExtractor.cs#L29)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleStreamMetadataExtractor_ProcessChunk_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleStreamMetadataExtractor_ProcessChunk_System_Text_Json_JsonElement_"></a>

### ProcessChunk(JsonElement)


```csharp
public abstract void ProcessChunk(JsonElement parsedChunk)
```


Processes one parsed stream chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parsedChunk` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleMetadataExtractor.cs#L26)
