---
title: "AmazonBedrockImageModel"
description: "Amazon Bedrock InvokeModel image model, such as Nova Canvas. Maps to AmazonBedrockImageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockImageModel"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockImageModel : IImageModel, IImageCaller
```


Amazon Bedrock `InvokeModel` image model, such as Nova Canvas. Maps to `AmazonBedrockImageModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/), [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L17)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel__ctor_Vercel_AI_AmazonBedrock_AmazonBedrockProvider_System_String_System_Func_System_DateTime__"></a>

### `AmazonBedrockImageModel(AmazonBedrockProvider, string, Func<DateTime>?)`


```csharp
public AmazonBedrockImageModel(AmazonBedrockProvider provider, string modelId, Func<DateTime>? currentDate = null)
```


Creates an image model. &lt;code class="paramref"&gt;currentDate&lt;/code&gt; sets the response timestamp. Maps to `_internal.currentDate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `currentDate` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L23)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; }
```


Maximum images in one call. Maps to `maxImagesPerCall`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L44)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_ModelId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L41)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_Provider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L38)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L35)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_SupportsFileInputs_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_SupportsFileInputs"></a>

### SupportsFileInputs


```csharp
public bool? SupportsFileInputs { get; }
```


True when the model takes input images, null when unknown. Maps to `supportsFileInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L48)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_SupportsMaskInputs_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_SupportsMaskInputs"></a>

### SupportsMaskInputs


```csharp
public bool? SupportsMaskInputs { get; }
```


True when the model takes a mask, null when unknown. Maps to `supportsMaskInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L54)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_DoGenerateAsync_Vercel_AI_Operations_ImageModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L97)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L66)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_ResolveMaxImagesPerCallAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockImageModel_ResolveMaxImagesPerCallAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockImageModel.cs#L60)
