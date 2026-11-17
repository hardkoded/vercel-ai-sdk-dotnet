---
title: "CerebrasMetadataExtractor"
description: "Exposes the service tier Cerebras applied as providerMetadata.cerebras.serviceTier."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasMetadataExtractor"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasMetadataExtractor"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public sealed class CerebrasMetadataExtractor : OpenAICompatibleMetadataExtractor
```


Exposes the service tier Cerebras applied as `providerMetadata.cerebras.serviceTier`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleMetadataExtractor](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleMetadataExtractor/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasMetadataExtractor.cs#L12)

## Fields

<a id="Vercel_AI_Cerebras_CerebrasMetadataExtractor_Instance"></a>

### Instance


```csharp
public static readonly CerebrasMetadataExtractor Instance
```


The shared instance.

**Value:** [CerebrasMetadataExtractor](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasMetadataExtractor.cs#L15)

## Methods

<a id="Vercel_AI_Cerebras_CerebrasMetadataExtractor_CreateStreamExtractor_"></a>
<a id="Vercel_AI_Cerebras_CerebrasMetadataExtractor_CreateStreamExtractor"></a>

### CreateStreamExtractor()


```csharp
public override OpenAICompatibleStreamMetadataExtractor CreateStreamExtractor()
```


Creates an extractor for one stream. It sees every chunk, then builds the metadata at the end.

**Returns:** [OpenAICompatibleStreamMetadataExtractor](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleStreamMetadataExtractor/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasMetadataExtractor.cs#L24)

<a id="Vercel_AI_Cerebras_CerebrasMetadataExtractor_ExtractMetadata_"></a>
<a id="Vercel_AI_Cerebras_CerebrasMetadataExtractor_ExtractMetadata_System_Text_Json_JsonElement_"></a>

### ExtractMetadata(JsonElement)


```csharp
public override JsonObject? ExtractMetadata(JsonElement parsedBody)
```


Extracts provider metadata from a complete response body. Returns an object keyed by provider name, or null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parsedBody` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasMetadataExtractor.cs#L18)
