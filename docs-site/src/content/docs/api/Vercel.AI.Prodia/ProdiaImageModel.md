---
title: "ProdiaImageModel"
description: "Prodia text-to-image model. It submits one job per image and reads the multipart result."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProdiaImageModel"
  hidden: true
---

<a id="Vercel_AI_Prodia_ProdiaImageModel"></a>

Namespace: [Vercel.AI.Prodia](/api/Vercel.AI.Prodia/)


```csharp
public sealed class ProdiaImageModel : IImageModel, IImageCaller
```


Prodia text-to-image model. It submits one job per image and reads the multipart result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L16)

## Constructors

<a id="Vercel_AI_Prodia_ProdiaImageModel__ctor_"></a>
<a id="Vercel_AI_Prodia_ProdiaImageModel__ctor_Vercel_AI_Prodia_ProdiaProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `ProdiaImageModel(ProdiaProvider, string, Func<DateTimeOffset>?)`


```csharp
public ProdiaImageModel(ProdiaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ProdiaProvider](/api/Vercel.AI.Prodia/ProdiaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L28)

## Properties

<a id="Vercel_AI_Prodia_ProdiaImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_Prodia_ProdiaImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


Prodia returns one image per job.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L42)

<a id="Vercel_AI_Prodia_ProdiaImageModel_ModelId_"></a>
<a id="Vercel_AI_Prodia_ProdiaImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L39)

<a id="Vercel_AI_Prodia_ProdiaImageModel_Provider_"></a>
<a id="Vercel_AI_Prodia_ProdiaImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L36)

## Methods

<a id="Vercel_AI_Prodia_ProdiaImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L61)

<a id="Vercel_AI_Prodia_ProdiaImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L51)

<a id="Vercel_AI_Prodia_ProdiaImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_Prodia_ProdiaImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaImageModel.cs#L45)
