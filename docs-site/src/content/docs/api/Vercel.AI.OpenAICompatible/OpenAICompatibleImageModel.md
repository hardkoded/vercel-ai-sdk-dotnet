---
title: "OpenAICompatibleImageModel"
description: "OpenAI-compatible image model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleImageModel"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatibleImageModel : IImageModel
```


OpenAI-compatible image model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L759)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `OpenAICompatibleImageModel(OpenAICompatibleProvider, string, Func<DateTimeOffset>?)`


```csharp
public OpenAICompatibleImageModel(OpenAICompatibleProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L765)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel_ModelId_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L776)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel_Provider_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L773)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleImageModel_DoGenerateAsync_Vercel_AI_Provider_ImageCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L779)
