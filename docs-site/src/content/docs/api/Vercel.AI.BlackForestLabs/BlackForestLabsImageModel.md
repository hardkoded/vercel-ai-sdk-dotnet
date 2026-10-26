---
title: "BlackForestLabsImageModel"
description: "Black Forest Labs image model. It submits the request, polls pollingurl until the task is Ready, then downloads the sample. Model ids pick the legacy, FLUX 3..."
editUrl: false
prev: false
next: false
sidebar:
  label: "BlackForestLabsImageModel"
  hidden: true
---

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel"></a>

Namespace: [Vercel.AI.BlackForestLabs](/api/Vercel.AI.BlackForestLabs/)


```csharp
public sealed class BlackForestLabsImageModel : IImageModel, IImageCaller
```


Black Forest Labs image model. It submits the request, polls `polling_url` until the task is `Ready`,
then downloads the sample. Model ids pick the legacy, FLUX 3, or latest image API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L22)

## Constructors

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel__ctor_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel__ctor_Vercel_AI_BlackForestLabs_BlackForestLabsProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `BlackForestLabsImageModel(BlackForestLabsProvider, string, Func<DateTimeOffset>?)`


```csharp
public BlackForestLabsImageModel(BlackForestLabsProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [BlackForestLabsProvider](/api/Vercel.AI.BlackForestLabs/BlackForestLabsProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L53)

## Properties

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


Black Forest Labs returns one image per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L73)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_ModelId_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L67)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_Provider_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L64)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L70)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_SupportsFileInputs_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_SupportsFileInputs"></a>

### SupportsFileInputs


```csharp
public bool? SupportsFileInputs { get; }
```


True when the model takes input images, false when it does not, null when unknown.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L76)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_SupportsMaskInputs_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_SupportsMaskInputs"></a>

### SupportsMaskInputs


```csharp
public bool? SupportsMaskInputs { get; }
```


True when the model takes a mask, false when it does not, null when unknown.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L96)

## Methods

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L151)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L138)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsImageModel.cs#L132)
