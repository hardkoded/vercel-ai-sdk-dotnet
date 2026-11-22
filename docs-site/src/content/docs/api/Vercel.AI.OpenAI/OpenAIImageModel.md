---
title: "OpenAIImageModel"
description: "OpenAI image generation and edit model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIImageModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIImageModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIImageModel : IImageModel
```


OpenAI image generation and edit model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L143)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIImageModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel__ctor_Vercel_AI_Azure_AzureOpenAIProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `OpenAIImageModel(AzureOpenAIProvider, string, Func<DateTimeOffset>?)`


```csharp
public OpenAIImageModel(AzureOpenAIProvider provider, string deploymentId, Func<DateTimeOffset>? clock = null)
```


Creates an Azure OpenAI image model for a deployment. The provider id is `azure.image`. Requests use the
Azure routes and headers of &lt;code class="paramref"&gt;provider&lt;/code&gt;, and file URLs are downloaded without its credentials.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/) |  |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L181)

<a id="Vercel_AI_OpenAI_OpenAIImageModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `OpenAIImageModel(OpenAIProvider, string, Func<DateTimeOffset>?)`


```csharp
public OpenAIImageModel(OpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L163)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int MaxImagesPerCall { get; }
```


Maximum images accepted by one call for this model id.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L202)

<a id="Vercel_AI_OpenAI_OpenAIImageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L199)

<a id="Vercel_AI_OpenAI_OpenAIImageModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L196)

<a id="Vercel_AI_OpenAI_OpenAIImageModel_SupportsFileInputs_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_SupportsFileInputs"></a>

### SupportsFileInputs


```csharp
public bool? SupportsFileInputs { get; }
```


True when the model takes input images, false for `dall-e-3`, null when the model id is unknown. Maps to
`fileInput` of the image capabilities in `openai-model-catalog.ts`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L208)

<a id="Vercel_AI_OpenAI_OpenAIImageModel_SupportsMaskInputs_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_SupportsMaskInputs"></a>

### SupportsMaskInputs


```csharp
public bool? SupportsMaskInputs { get; }
```


True when the model takes a mask, false for `dall-e-3`, null when the model id is unknown. Maps to `maskInput`
of the image capabilities in `openai-model-catalog.ts`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L214)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L248)

<a id="Vercel_AI_OpenAI_OpenAIImageModel_GenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_GenerateAsync_Vercel_AI_OpenAI_OpenAIImageCall_System_Threading_CancellationToken_"></a>

### GenerateAsync(OpenAIImageCall, CancellationToken)


```csharp
public Task<OpenAIImageGeneration> GenerateAsync(OpenAIImageCall call, CancellationToken cancellationToken)
```


Generates or edits images.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [OpenAIImageCall](/api/Vercel.AI.OpenAI/OpenAIImageCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIImageGeneration&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L358)

<a id="Vercel_AI_OpenAI_OpenAIImageModel_MaxImages_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_MaxImages_System_String_"></a>

### MaxImages(string)


```csharp
public static int MaxImages(string modelId)
```


Maximum images for &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L217)

<a id="Vercel_AI_OpenAI_OpenAIImageModel_OmitsResponseFormat_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageModel_OmitsResponseFormat_System_String_"></a>

### OmitsResponseFormat(string)


```csharp
public static bool OmitsResponseFormat(string modelId)
```


True when the model rejects an explicit `response_format`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Image/OpenAIImageModel.cs#L241)
