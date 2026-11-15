---
title: "MiniMaxImageModel"
description: "MiniMax image model. It posts to {baseURL}/images/generations, or /images/edits when input images are given, asks for URL output, and downloads each image."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxImageModel"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxImageModel"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxImageModel : IImageModel, IImageCaller
```


MiniMax image model. It posts to `{baseURL}/images/generations`, or `/images/edits` when input images are given,
asks for URL output, and downloads each image.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L48)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxImageModel__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel__ctor_System_String_Vercel_AI_MiniMax_MiniMaxImageModelConfig_"></a>

### MiniMaxImageModel(string, MiniMaxImageModelConfig)


```csharp
public MiniMaxImageModel(string modelId, MiniMaxImageModelConfig config)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [MiniMaxImageModelConfig](/api/Vercel.AI.MiniMax/MiniMaxImageModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L53)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_Config_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_Config"></a>

### Config


```csharp
public MiniMaxImageModelConfig Config { get; }
```


The configuration the provider passed in.

**Value:** [MiniMaxImageModelConfig](/api/Vercel.AI.MiniMax/MiniMaxImageModelConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L78)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


MiniMax creates up to 10 images per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L69)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_ModelId_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L66)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_Provider_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L63)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L60)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_SupportsFileInputs_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_SupportsFileInputs"></a>

### SupportsFileInputs


```csharp
public bool SupportsFileInputs { get; }
```


MiniMax image models accept input images. Maps to `supportsFileInputs`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L72)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_SupportsMaskInputs_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_SupportsMaskInputs"></a>

### SupportsMaskInputs


```csharp
public bool SupportsMaskInputs { get; }
```


MiniMax image models do not accept masks. Maps to `supportsMaskInputs`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L75)

## Methods

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L111)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L87)

<a id="Vercel_AI_MiniMax_MiniMaxImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModel.cs#L81)
