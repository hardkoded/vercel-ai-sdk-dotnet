---
title: "LumaImageModel"
description: "Luma Dream Machine image model. It submits a generation, polls it, and downloads the image."
editUrl: false
prev: false
next: false
sidebar:
  label: "LumaImageModel"
  hidden: true
---

<a id="Vercel_AI_Luma_LumaImageModel"></a>

Namespace: [Vercel.AI.Luma](/api/Vercel.AI.Luma/)


```csharp
public sealed class LumaImageModel : IImageModel
```


Luma Dream Machine image model. It submits a generation, polls it, and downloads the image.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L91)

## Constructors

<a id="Vercel_AI_Luma_LumaImageModel__ctor_"></a>
<a id="Vercel_AI_Luma_LumaImageModel__ctor_Vercel_AI_Luma_LumaProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `LumaImageModel(LumaProvider, string, Func<DateTimeOffset>?)`


```csharp
public LumaImageModel(LumaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates an image model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [LumaProvider](/api/Vercel.AI.Luma/LumaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L100)

## Properties

<a id="Vercel_AI_Luma_LumaImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_Luma_LumaImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int MaxImagesPerCall { get; }
```


Luma returns one image per call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L114)

<a id="Vercel_AI_Luma_LumaImageModel_ModelId_"></a>
<a id="Vercel_AI_Luma_LumaImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L111)

<a id="Vercel_AI_Luma_LumaImageModel_Provider_"></a>
<a id="Vercel_AI_Luma_LumaImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L108)

## Methods

<a id="Vercel_AI_Luma_LumaImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Luma_LumaImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(ImageCallOptions, CancellationToken)


```csharp
public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
```


Generates images. Maps to `doGenerate` on an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ImageCallOptions](/api/Vercel.AI.Provider/ImageCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ImageGenerationResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L117)

<a id="Vercel_AI_Luma_LumaImageModel_GenerateAsync_"></a>
<a id="Vercel_AI_Luma_LumaImageModel_GenerateAsync_Vercel_AI_Luma_LumaImageRequest_System_Threading_CancellationToken_"></a>

### GenerateAsync(LumaImageRequest, CancellationToken)


```csharp
public Task<LumaImageGeneration> GenerateAsync(LumaImageRequest request, CancellationToken cancellationToken)
```


Generates one image.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [LumaImageRequest](/api/Vercel.AI.Luma/LumaImageRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LumaImageGeneration&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaImageModel.cs#L133)
