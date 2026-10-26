---
title: "OpenAISpeechTranslationModel"
description: "OpenAI speech translation model. REST calls use /audio/translations."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISpeechTranslationModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISpeechTranslationModel : ISpeechTranslationModel
```


OpenAI speech translation model. REST calls use `/audio/translations`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechTranslationModel](/api/Vercel.AI.Provider/ISpeechTranslationModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L10)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAISpeechTranslationModel(OpenAIProvider, string)


```csharp
public OpenAISpeechTranslationModel(OpenAIProvider provider, string modelId)
```


Creates a speech translation model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L16)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L27)

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L24)

## Methods

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_DoTranslateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_DoTranslateAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

### DoTranslateAsync(AudioInput, CancellationToken)


```csharp
public Task<TranscriptionResult> DoTranslateAsync(AudioInput audio, CancellationToken cancellationToken)
```


Translates speech into text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L30)
