---
title: "OpenAICompatibleMetadataExtractor"
description: "Extracts provider-specific metadata from a chat response. Maps to MetadataExtractor."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleMetadataExtractor"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleMetadataExtractor"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public abstract class OpenAICompatibleMetadataExtractor
```


Extracts provider-specific metadata from a chat response. Maps to `MetadataExtractor`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [CerebrasMetadataExtractor](/api/Vercel.AI.Cerebras/CerebrasMetadataExtractor/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleMetadataExtractor.cs#L11)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleMetadataExtractor_CreateStreamExtractor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleMetadataExtractor_CreateStreamExtractor"></a>

### CreateStreamExtractor()


```csharp
public abstract OpenAICompatibleStreamMetadataExtractor CreateStreamExtractor()
```


Creates an extractor for one stream. It sees every chunk, then builds the metadata at the end.

**Returns:** [OpenAICompatibleStreamMetadataExtractor](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleStreamMetadataExtractor/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleMetadataExtractor.cs#L19)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleMetadataExtractor_ExtractMetadata_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleMetadataExtractor_ExtractMetadata_System_Text_Json_JsonElement_"></a>

### ExtractMetadata(JsonElement)


```csharp
public abstract JsonObject? ExtractMetadata(JsonElement parsedBody)
```


Extracts provider metadata from a complete response body. Returns an object keyed by provider name, or null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parsedBody` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleMetadataExtractor.cs#L16)
