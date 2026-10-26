---
title: "DeepInfraImageModel"
description: "DeepInfra image model. Generation posts to /inference/{model}. Edits post multipart to the OpenAI-compatible /openai/images/edits route."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepInfraImageModel"
  hidden: true
---

<a id="Vercel_AI_DeepInfra_DeepInfraImageModel"></a>

Namespace: [Vercel.AI.DeepInfra](/api/Vercel.AI.DeepInfra/)


```csharp
public sealed class DeepInfraImageModel : IImageModel
```


DeepInfra image model. Generation posts to `/inference/{model}`.
Edits post multipart to the OpenAI-compatible `/openai/images/edits` route.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraImageModel.cs#L17)

## Constructors

<a id="Vercel_AI_DeepInfra_DeepInfraImageModel__ctor_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraImageModel__ctor_Vercel_AI_DeepInfra_DeepInfraProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `DeepInfraImageModel(DeepInfraProvider, string, Func<DateTimeOffset>?)`


```csharp
public DeepInfraImageModel(DeepInfraProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates an image model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and defaults to UTC now.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [DeepInfraProvider](/api/Vercel.AI.DeepInfra/DeepInfraProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraImageModel.cs#L23)

## Properties

<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int MaxImagesPerCall { get; }
```


DeepInfra accepts one image per call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraImageModel.cs#L37)

<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_ModelId_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraImageModel.cs#L34)

<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_Provider_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraImageModel.cs#L31)

## Methods

<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraImageModel.cs#L40)

<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_ReadError_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraImageModel_ReadError_System_String_"></a>

### ReadError(string?)


```csharp
public static string? ReadError(string? body)
```


Reads `detail.error`, then the shared OpenAI-compatible error message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraImageModel.cs#L82)
