---
title: "QuiverAIImageModel"
description: "QuiverAI SVG model. providerOptions.quiverai.operation selects generate (default), vectorize, animate, or edit. Invalid requests throw before any HTTP call."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIImageModel"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAIImageModel : IImageModel, IImageCaller
```


QuiverAI SVG model. `providerOptions.quiverai.operation` selects `generate` (default),
`vectorize`, `animate`, or `edit`. Invalid requests throw before any HTTP call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L20)

## Constructors

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel__ctor_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModel__ctor_Vercel_AI_QuiverAI_QuiverAIProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `QuiverAIImageModel(QuiverAIProvider, string, Func<DateTimeOffset>?)`


```csharp
public QuiverAIImageModel(QuiverAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the timestamp when the response has none.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [QuiverAIProvider](/api/Vercel.AI.QuiverAI/QuiverAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L34)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


At most 16 images per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L48)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_ModelId_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L45)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_Provider_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L42)

## Methods

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L66)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L57)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModel.cs#L51)
