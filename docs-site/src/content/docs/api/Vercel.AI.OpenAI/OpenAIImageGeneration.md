---
title: "OpenAIImageGeneration"
description: "Image generation result, including usage and warnings."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIImageGeneration"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIImageGeneration
```


Image generation result, including usage and warnings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L71)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L120)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Images_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Images"></a>

### Images


```csharp
public IReadOnlyList<GeneratedImage> Images { get; }
```


Generated images.

**Value:** [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L96)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_InputTokens_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_InputTokens"></a>

### InputTokens


```csharp
public int? InputTokens { get; }
```


Input tokens, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L102)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id used for the call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L117)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_OutputTokens_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_OutputTokens"></a>

### OutputTokens


```csharp
public int? OutputTokens { get; }
```


Output tokens, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L105)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_ProviderMetadata_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Provider metadata, including per-image token details.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L111)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Timestamp_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp recorded for the response.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L114)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_TotalTokens_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_TotalTokens"></a>

### TotalTokens


```csharp
public int? TotalTokens { get; }
```


Total tokens, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L108)

<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGeneration_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing the request.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L99)
