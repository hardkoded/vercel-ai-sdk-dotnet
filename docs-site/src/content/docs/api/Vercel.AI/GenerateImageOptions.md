---
title: "GenerateImageOptions"
description: "Image generation options."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateImageOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateImageOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class GenerateImageOptions
```


Image generation options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L350)

## Properties

<a id="Vercel_AI_GenerateImageOptions_AspectRatio_"></a>
<a id="Vercel_AI_GenerateImageOptions_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Aspect ratio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L368)

<a id="Vercel_AI_GenerateImageOptions_Count_"></a>
<a id="Vercel_AI_GenerateImageOptions_Count"></a>

### Count


```csharp
public int Count { get; set; }
```


Image count.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L362)

<a id="Vercel_AI_GenerateImageOptions_Files_"></a>
<a id="Vercel_AI_GenerateImageOptions_Files"></a>

### Files


```csharp
public IReadOnlyList<ImageModelFile>? Files { get; set; }
```


Input images for an edit.

**Value:** [IReadOnlyList&lt;ImageModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L374)

<a id="Vercel_AI_GenerateImageOptions_Headers_"></a>
<a id="Vercel_AI_GenerateImageOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Headers for this call. They override the provider headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L383)

<a id="Vercel_AI_GenerateImageOptions_Mask_"></a>
<a id="Vercel_AI_GenerateImageOptions_Mask"></a>

### Mask


```csharp
public ImageModelFile? Mask { get; set; }
```


Edit mask.

**Value:** [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L377)

<a id="Vercel_AI_GenerateImageOptions_Model_"></a>
<a id="Vercel_AI_GenerateImageOptions_Model"></a>

### Model


```csharp
public IImageModel? Model { get; set; }
```


Explicit image model.

**Value:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L353)

<a id="Vercel_AI_GenerateImageOptions_ModelId_"></a>
<a id="Vercel_AI_GenerateImageOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway image model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L356)

<a id="Vercel_AI_GenerateImageOptions_Prompt_"></a>
<a id="Vercel_AI_GenerateImageOptions_Prompt"></a>

### Prompt


```csharp
public string Prompt { get; set; }
```


Prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L359)

<a id="Vercel_AI_GenerateImageOptions_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateImageOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options, keyed by provider id.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L380)

<a id="Vercel_AI_GenerateImageOptions_Seed_"></a>
<a id="Vercel_AI_GenerateImageOptions_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed. Models that do not support it add a warning.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L371)

<a id="Vercel_AI_GenerateImageOptions_Size_"></a>
<a id="Vercel_AI_GenerateImageOptions_Size"></a>

### Size


```csharp
public string? Size { get; set; }
```


Size.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L365)
