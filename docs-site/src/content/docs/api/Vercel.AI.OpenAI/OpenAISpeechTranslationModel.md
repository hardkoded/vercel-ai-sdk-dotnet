---
title: "OpenAISpeechTranslationModel"
description: "OpenAI speech translation model. REST calls use /audio/translations. Realtime streaming uses the /realtime/translations WebSocket."
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
public sealed class OpenAISpeechTranslationModel : ISpeechTranslationModel, ISpeechTranslationCaller
```


OpenAI speech translation model. REST calls use `/audio/translations`. Realtime streaming uses the
`/realtime/translations` WebSocket.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechTranslationModel](/api/Vercel.AI.Provider/ISpeechTranslationModel/), [ISpeechTranslationCaller](/api/Vercel.AI.Operations/ISpeechTranslationCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L17)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L31)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L95)

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L92)

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. String ids are resolved before this is checked.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L98)

## Methods

<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechTranslationModel_DoStreamAsync_Vercel_AI_Operations_TranslationStreamCall_System_Threading_CancellationToken_"></a>

### DoStreamAsync(TranslationStreamCall, CancellationToken)


```csharp
public Task<TranslationStreamStart> DoStreamAsync(TranslationStreamCall call, CancellationToken cancellationToken)
```


Starts a realtime speech translation over the OpenAI Realtime translations WebSocket. Maps to `doStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranslationStreamCall](/api/Vercel.AI.Operations/TranslationStreamCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranslationStreamStart&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L110)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/SpeechTranslation/OpenAISpeechTranslationModel.cs#L101)
