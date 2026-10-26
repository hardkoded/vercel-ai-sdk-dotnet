---
title: "DeepgramProvider.DeepgramSpeechModel"
description: "Deepgram Aura text-to-speech model."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramProvider.DeepgramSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramProvider.DeepgramSpeechModel : ISpeechModel
```


Deepgram Aura text-to-speech model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L332)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel__ctor_Vercel_AI_Deepgram_DeepgramProvider_System_String_"></a>

### DeepgramSpeechModel(DeepgramProvider, string)


```csharp
public DeepgramSpeechModel(DeepgramProvider provider, string modelId)
```


Creates a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [DeepgramProvider](/api/Vercel.AI.Deepgram/DeepgramProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L342)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L352)

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_Provider_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L349)

## Methods

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L355)

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramSpeechModel_GenerateAsync_Vercel_AI_Deepgram_DeepgramSpeechRequest_System_Threading_CancellationToken_"></a>

### GenerateAsync(DeepgramSpeechRequest, CancellationToken)


```csharp
public Task<DeepgramSpeechResult> GenerateAsync(DeepgramSpeechRequest request, CancellationToken cancellationToken)
```


Synthesizes speech with `/v1/speak`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [DeepgramSpeechRequest](/api/Vercel.AI.Deepgram/DeepgramSpeechRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DeepgramSpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L363)
