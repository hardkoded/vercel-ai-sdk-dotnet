---
title: "FalImageModel"
description: "Fal image model. Posts to {baseURL}/{modelId} and downloads each returned image."
editUrl: false
prev: false
next: false
sidebar:
  label: "FalImageModel"
  hidden: true
---

<a id="Vercel_AI_Fal_FalImageModel"></a>

Namespace: [Vercel.AI.Fal](/api/Vercel.AI.Fal/)


```csharp
public sealed class FalImageModel : IImageModel, IImageCaller
```


Fal image model. Posts to `{baseURL}/{modelId}` and downloads each returned image.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L16)

## Constructors

<a id="Vercel_AI_Fal_FalImageModel__ctor_"></a>
<a id="Vercel_AI_Fal_FalImageModel__ctor_Vercel_AI_Fal_FalProvider_System_String_System_Func_System_DateTime__"></a>

### `FalImageModel(FalProvider, string, Func<DateTime>?)`


```csharp
public FalImageModel(FalProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates an image model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [FalProvider](/api/Vercel.AI.Fal/FalProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L39)

## Properties

<a id="Vercel_AI_Fal_FalImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_Fal_FalImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


Fal returns one image per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L56)

<a id="Vercel_AI_Fal_FalImageModel_ModelId_"></a>
<a id="Vercel_AI_Fal_FalImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L50)

<a id="Vercel_AI_Fal_FalImageModel_Provider_"></a>
<a id="Vercel_AI_Fal_FalImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L47)

<a id="Vercel_AI_Fal_FalImageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Fal_FalImageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L53)

<a id="Vercel_AI_Fal_FalImageModel_SupportsFileInputs_"></a>
<a id="Vercel_AI_Fal_FalImageModel_SupportsFileInputs"></a>

### SupportsFileInputs


```csharp
public bool? SupportsFileInputs { get; }
```


True when the model takes input images, false when it does not, null when unknown. Maps to `supportsFileInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L59)

<a id="Vercel_AI_Fal_FalImageModel_SupportsMaskInputs_"></a>
<a id="Vercel_AI_Fal_FalImageModel_SupportsMaskInputs"></a>

### SupportsMaskInputs


```csharp
public bool? SupportsMaskInputs { get; }
```


True when the model takes a mask, false when it takes input images but no mask, null when unknown. Maps to `supportsMaskInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L90)

## Methods

<a id="Vercel_AI_Fal_FalImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Fal_FalImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L116)

<a id="Vercel_AI_Fal_FalImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L110)

<a id="Vercel_AI_Fal_FalImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_Fal_FalImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalImageModel.cs#L104)
