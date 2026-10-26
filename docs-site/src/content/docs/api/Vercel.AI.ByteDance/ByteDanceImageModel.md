---
title: "ByteDanceImageModel"
description: "ByteDance Seedream image model. Input files turn a generation into an edit."
editUrl: false
prev: false
next: false
sidebar:
  label: "ByteDanceImageModel"
  hidden: true
---

<a id="Vercel_AI_ByteDance_ByteDanceImageModel"></a>

Namespace: [Vercel.AI.ByteDance](/api/Vercel.AI.ByteDance/)


```csharp
public sealed class ByteDanceImageModel : IImageModel, IImageCaller
```


ByteDance Seedream image model. Input files turn a generation into an edit.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L15)

## Constructors

<a id="Vercel_AI_ByteDance_ByteDanceImageModel__ctor_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceImageModel__ctor_Vercel_AI_ByteDance_ByteDanceProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `ByteDanceImageModel(ByteDanceProvider, string, Func<DateTimeOffset>?)`


```csharp
public ByteDanceImageModel(ByteDanceProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ByteDanceProvider](/api/Vercel.AI.ByteDance/ByteDanceProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L26)

## Properties

<a id="Vercel_AI_ByteDance_ByteDanceImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


The API has no output count, so each call returns one image.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L40)

<a id="Vercel_AI_ByteDance_ByteDanceImageModel_ModelId_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L37)

<a id="Vercel_AI_ByteDance_ByteDanceImageModel_Provider_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L34)

## Methods

<a id="Vercel_AI_ByteDance_ByteDanceImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L55)

<a id="Vercel_AI_ByteDance_ByteDanceImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L49)

<a id="Vercel_AI_ByteDance_ByteDanceImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceImageModel.cs#L43)
