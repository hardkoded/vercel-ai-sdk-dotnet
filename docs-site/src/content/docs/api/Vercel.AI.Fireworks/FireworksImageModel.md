---
title: "FireworksImageModel"
description: "Fireworks image model. Sync models return bytes. Kontext models poll getresult."
editUrl: false
prev: false
next: false
sidebar:
  label: "FireworksImageModel"
  hidden: true
---

<a id="Vercel_AI_Fireworks_FireworksImageModel"></a>

Namespace: [Vercel.AI.Fireworks](/api/Vercel.AI.Fireworks/)


```csharp
public sealed class FireworksImageModel : IImageModel
```


Fireworks image model. Sync models return bytes. Kontext models poll `get_result`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L17)

## Constructors

<a id="Vercel_AI_Fireworks_FireworksImageModel__ctor_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel__ctor_Vercel_AI_Fireworks_FireworksProvider_System_String_"></a>

### FireworksImageModel(FireworksProvider, string)


```csharp
public FireworksImageModel(FireworksProvider provider, string modelId)
```


Creates an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [FireworksProvider](/api/Vercel.AI.Fireworks/FireworksProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L22)

## Properties

<a id="Vercel_AI_Fireworks_FireworksImageModel_Clock_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for the response timestamp. Defaults to UTC now.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L44)

<a id="Vercel_AI_Fireworks_FireworksImageModel_MaxImagesPerCall_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int MaxImagesPerCall { get; }
```


Fireworks returns one image per call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L35)

<a id="Vercel_AI_Fireworks_FireworksImageModel_ModelId_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L32)

<a id="Vercel_AI_Fireworks_FireworksImageModel_PollInterval_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel_PollInterval"></a>

### PollInterval


```csharp
public TimeSpan PollInterval { get; set; }
```


Delay between async polls.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L38)

<a id="Vercel_AI_Fireworks_FireworksImageModel_PollTimeout_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel_PollTimeout"></a>

### PollTimeout


```csharp
public TimeSpan PollTimeout { get; set; }
```


How long async polling may run.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L41)

<a id="Vercel_AI_Fireworks_FireworksImageModel_Provider_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L29)

## Methods

<a id="Vercel_AI_Fireworks_FireworksImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Fireworks_FireworksImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksImageModel.cs#L47)
