---
title: "FishAudioProvider.FishSpeechModel"
description: "Fish Audio speech model. Posts to /v1/tts and selects the model with a model header."
editUrl: false
prev: false
next: false
sidebar:
  label: "FishAudioProvider.FishSpeechModel"
  hidden: true
---

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel"></a>

Namespace: [Vercel.AI.FishAudio](/api/Vercel.AI.FishAudio/)


```csharp
public sealed class FishAudioProvider.FishSpeechModel : ISpeechModel, ISpeechCaller
```


Fish Audio speech model. Posts to `/v1/tts` and selects the model with a `model` header.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/), [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L118)

## Constructors

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel__ctor_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel__ctor_Vercel_AI_FishAudio_FishAudioProvider_System_String_System_Func_System_DateTime__"></a>

### `FishSpeechModel(FishAudioProvider, string, Func<DateTime>?)`


```csharp
public FishSpeechModel(FishAudioProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a speech model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [FishAudioProvider](/api/Vercel.AI.FishAudio/FishAudioProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L143)

## Properties

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_ModelId_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L157)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_Provider_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L154)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_SpecificationVersion_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L151)

## Methods

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_DoGenerateAsync_Vercel_AI_Operations_SpeechModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L168)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L160)
