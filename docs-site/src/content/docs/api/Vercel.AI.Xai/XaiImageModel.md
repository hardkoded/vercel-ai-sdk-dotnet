---
title: "XaiImageModel"
description: "xAI image model, over /images/generations and /images/edits. Maps to XaiImageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiImageModel"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiImageModel"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiImageModel : IImageModel, IImageCaller
```


xAI image model, over `/images/generations` and `/images/edits`. Maps to `XaiImageModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L16)

## Constructors

<a id="Vercel_AI_Xai_XaiImageModel__ctor_"></a>
<a id="Vercel_AI_Xai_XaiImageModel__ctor_Vercel_AI_Xai_XaiProvider_System_String_System_Func_System_DateTime__"></a>

### `XaiImageModel(XaiProvider, string, Func<DateTime>?)`


```csharp
public XaiImageModel(XaiProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and defaults to UTC now. Maps to `_internal.currentDate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L22)

## Properties

<a id="Vercel_AI_Xai_XaiImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


Maximum images in one call. Maps to `maxImagesPerCall`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L43)

<a id="Vercel_AI_Xai_XaiImageModel_ModelId_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L40)

<a id="Vercel_AI_Xai_XaiImageModel_Provider_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L37)

<a id="Vercel_AI_Xai_XaiImageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L34)

<a id="Vercel_AI_Xai_XaiImageModel_SupportsFileInputs_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_SupportsFileInputs"></a>

### SupportsFileInputs


```csharp
public bool? SupportsFileInputs { get; }
```


Whether the model takes input images. Null when the catalog does not say. Maps to `supportsFileInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L46)

<a id="Vercel_AI_Xai_XaiImageModel_SupportsMaskInputs_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_SupportsMaskInputs"></a>

### SupportsMaskInputs


```csharp
public bool? SupportsMaskInputs { get; }
```


Whether the model takes a mask. Null when the catalog does not say. Maps to `supportsMaskInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L50)

## Methods

<a id="Vercel_AI_Xai_XaiImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L88)

<a id="Vercel_AI_Xai_XaiImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L60)

<a id="Vercel_AI_Xai_XaiImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_Xai_XaiImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiImageModel.cs#L54)
