---
title: "OpenAISpeechModel"
description: "OpenAI speech model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISpeechModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISpeechModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISpeechModel : ISpeechModel
```


OpenAI speech model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L95)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAISpeechModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `OpenAISpeechModel(OpenAIProvider, string, Func<DateTimeOffset>?)`


```csharp
public OpenAISpeechModel(OpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L103)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISpeechModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L114)

<a id="Vercel_AI_OpenAI_OpenAISpeechModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L111)

## Methods

<a id="Vercel_AI_OpenAI_OpenAISpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(SpeechCallOptions, CancellationToken)


```csharp
public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
```


Synthesizes speech.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SpeechCallOptions](/api/Vercel.AI.Provider/SpeechCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L177)

<a id="Vercel_AI_OpenAI_OpenAISpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModel_GenerateAsync_Vercel_AI_OpenAI_OpenAISpeechCall_System_Threading_CancellationToken_"></a>

### GenerateAsync(OpenAISpeechCall, CancellationToken)


```csharp
public Task<OpenAISpeechResult> GenerateAsync(OpenAISpeechCall call, CancellationToken cancellationToken)
```


Synthesizes speech and returns the audio bytes, warnings, and response headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [OpenAISpeechCall](/api/Vercel.AI.OpenAI/OpenAISpeechCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAISpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L184)

<a id="Vercel_AI_OpenAI_OpenAISpeechModel_Now_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModel_Now"></a>

### Now()


```csharp
public DateTimeOffset Now()
```


Clock used when a caller records a response timestamp.

**Returns:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L200)

<a id="Vercel_AI_OpenAI_OpenAISpeechModel_Prepare_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModel_Prepare_System_String_Vercel_AI_OpenAI_OpenAISpeechCall_"></a>

### Prepare(string, OpenAISpeechCall)


```csharp
public static OpenAISpeechPreparation Prepare(string modelId, OpenAISpeechCall call)
```


Builds the speech request body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `call` | [OpenAISpeechCall](/api/Vercel.AI.OpenAI/OpenAISpeechCall/) |  |

**Returns:** [OpenAISpeechPreparation](/api/Vercel.AI.OpenAI/OpenAISpeechPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L117)
