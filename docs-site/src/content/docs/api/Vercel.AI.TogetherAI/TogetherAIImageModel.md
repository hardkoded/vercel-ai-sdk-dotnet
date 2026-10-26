---
title: "TogetherAIImageModel"
description: "Together image generation. Posts /images/generations with width, height, and base64 output."
editUrl: false
prev: false
next: false
sidebar:
  label: "TogetherAIImageModel"
  hidden: true
---

<a id="Vercel_AI_TogetherAI_TogetherAIImageModel"></a>

Namespace: [Vercel.AI.TogetherAI](/api/Vercel.AI.TogetherAI/)


```csharp
public sealed class TogetherAIImageModel : IImageModel
```


Together image generation. Posts `/images/generations` with width, height, and base64 output.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIImageModel.cs#L15)

## Constructors

<a id="Vercel_AI_TogetherAI_TogetherAIImageModel__ctor_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIImageModel__ctor_Vercel_AI_TogetherAI_TogetherAIProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `TogetherAIImageModel(TogetherAIProvider, string, Func<DateTimeOffset>?)`


```csharp
public TogetherAIImageModel(TogetherAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates an image model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and defaults to UTC now.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [TogetherAIProvider](/api/Vercel.AI.TogetherAI/TogetherAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIImageModel.cs#L26)

## Properties

<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int MaxImagesPerCall { get; }
```


Together accepts one image per call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIImageModel.cs#L40)

<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_ModelId_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIImageModel.cs#L37)

<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_Provider_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIImageModel.cs#L34)

## Methods

<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIImageModel.cs#L43)
