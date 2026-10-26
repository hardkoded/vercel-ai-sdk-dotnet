---
title: "ReplicateImageModel"
description: "Replicate image model. It creates a prediction, polls it when the sync wait expires, and downloads each output image. A model:version id posts to /prediction..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReplicateImageModel"
  hidden: true
---

<a id="Vercel_AI_Replicate_ReplicateImageModel"></a>

Namespace: [Vercel.AI.Replicate](/api/Vercel.AI.Replicate/)


```csharp
public sealed class ReplicateImageModel : IImageModel, IImageCaller
```


Replicate image model. It creates a prediction, polls it when the sync wait expires, and downloads each output image.
A `model:version` id posts to `/predictions`; other ids post to `/models/{model}/predictions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L19)

## Constructors

<a id="Vercel_AI_Replicate_ReplicateImageModel__ctor_"></a>
<a id="Vercel_AI_Replicate_ReplicateImageModel__ctor_Vercel_AI_Replicate_ReplicateProvider_System_String_System_Func_System_DateTime__"></a>

### `ReplicateImageModel(ReplicateProvider, string, Func<DateTime>?)`


```csharp
public ReplicateImageModel(ReplicateProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates an image model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ReplicateProvider](/api/Vercel.AI.Replicate/ReplicateProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L31)

## Properties

<a id="Vercel_AI_Replicate_ReplicateImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_Replicate_ReplicateImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


Flux-2 models accept up to 8 input images; other models return one image per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L45)

<a id="Vercel_AI_Replicate_ReplicateImageModel_ModelId_"></a>
<a id="Vercel_AI_Replicate_ReplicateImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L42)

<a id="Vercel_AI_Replicate_ReplicateImageModel_Provider_"></a>
<a id="Vercel_AI_Replicate_ReplicateImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L39)

## Methods

<a id="Vercel_AI_Replicate_ReplicateImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Replicate_ReplicateImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(ImageModelCall, CancellationToken)


```csharp
public Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
```


Generates images.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [ImageModelCall](/api/Vercel.AI.Operations/ImageModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ImageModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L76)

<a id="Vercel_AI_Replicate_ReplicateImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L56)

<a id="Vercel_AI_Replicate_ReplicateImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_Replicate_ReplicateImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

### ResolveMaxImagesPerCallAsync(CancellationToken)


```csharp
public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
```


Resolves a functional per-call limit. The default returns [MaxImagesPerCall](/api/Vercel.AI.Operations/IImageCaller/#Vercel_AI_Operations_IImageCaller_MaxImagesPerCall).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;int?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateImageModel.cs#L50)
