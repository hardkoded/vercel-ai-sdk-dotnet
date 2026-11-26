---
title: "ElevenLabsProvider.ElevenLabsSpeechModel"
description: "ElevenLabs text-to-speech model."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsProvider.ElevenLabsSpeechModel"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsProvider.ElevenLabsSpeechModel : ISpeechModel, ISpeechCaller
```


ElevenLabs text-to-speech model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/), [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L815)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel__ctor_Vercel_AI_ElevenLabs_ElevenLabsProvider_System_String_"></a>

### ElevenLabsSpeechModel(ElevenLabsProvider, string)


```csharp
public ElevenLabsSpeechModel(ElevenLabsProvider provider, string modelId)
```


Creates a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ElevenLabsProvider](/api/Vercel.AI.ElevenLabs/ElevenLabsProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L840)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_ModelId_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L850)

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_Provider_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L847)

## Methods

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_DoGenerateAsync_Vercel_AI_Operations_SpeechModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(SpeechModelCall, CancellationToken)


```csharp
public Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
```


Generates speech audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [SpeechModelCall](/api/Vercel.AI.Operations/SpeechModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L883)

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L853)

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsSpeechModel_GenerateAsync_Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_System_Threading_CancellationToken_"></a>

### GenerateAsync(ElevenLabsSpeechRequest, CancellationToken)


```csharp
public Task<ElevenLabsSpeechResult> GenerateAsync(ElevenLabsSpeechRequest request, CancellationToken cancellationToken)
```


Synthesizes speech and returns the audio with warnings and response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [ElevenLabsSpeechRequest](/api/Vercel.AI.ElevenLabs/ElevenLabsSpeechRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ElevenLabsSpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L949)
