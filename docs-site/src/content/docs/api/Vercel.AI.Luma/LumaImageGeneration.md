---
title: "LumaImageGeneration"
description: "Luma image result with warnings and response metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "LumaImageGeneration"
  hidden: true
---

<a id="Vercel_AI_Luma_LumaImageGeneration"></a>

Namespace: [Vercel.AI.Luma](/api/Vercel.AI.Luma/)


```csharp
public sealed class LumaImageGeneration : ImageGenerationResult
```


Luma image result with warnings and response metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ImageGenerationResult](/api/Vercel.AI.Provider/ImageGenerationResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L54)

## Properties

<a id="Vercel_AI_Luma_LumaImageGeneration_Headers_"></a>
<a id="Vercel_AI_Luma_LumaImageGeneration_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers of the generation response.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L87)

<a id="Vercel_AI_Luma_LumaImageGeneration_ModelId_"></a>
<a id="Vercel_AI_Luma_LumaImageGeneration_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L84)

<a id="Vercel_AI_Luma_LumaImageGeneration_Timestamp_"></a>
<a id="Vercel_AI_Luma_LumaImageGeneration_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Time the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L81)

<a id="Vercel_AI_Luma_LumaImageGeneration_Warnings_"></a>
<a id="Vercel_AI_Luma_LumaImageGeneration_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings for settings Luma does not support.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L78)
