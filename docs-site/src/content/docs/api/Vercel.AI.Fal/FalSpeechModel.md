---
title: "FalSpeechModel"
description: "Fal text-to-speech model. Posts to https://fal.run/{modelId} and downloads the returned audio."
editUrl: false
prev: false
next: false
sidebar:
  label: "FalSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Fal_FalSpeechModel"></a>

Namespace: [Vercel.AI.Fal](/api/Vercel.AI.Fal/)


```csharp
public sealed class FalSpeechModel : ISpeechModel, ISpeechCaller
```


Fal text-to-speech model. Posts to `https://fal.run/{modelId}` and downloads the returned audio.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/), [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalSpeechModel.cs#L14)

## Constructors

<a id="Vercel_AI_Fal_FalSpeechModel__ctor_"></a>
<a id="Vercel_AI_Fal_FalSpeechModel__ctor_Vercel_AI_Fal_FalProvider_System_String_System_Func_System_DateTime__"></a>

### `FalSpeechModel(FalProvider, string, Func<DateTime>?)`


```csharp
public FalSpeechModel(FalProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a speech model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [FalProvider](/api/Vercel.AI.Fal/FalProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalSpeechModel.cs#L20)

## Properties

<a id="Vercel_AI_Fal_FalSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Fal_FalSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalSpeechModel.cs#L31)

<a id="Vercel_AI_Fal_FalSpeechModel_Provider_"></a>
<a id="Vercel_AI_Fal_FalSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalSpeechModel.cs#L28)

## Methods

<a id="Vercel_AI_Fal_FalSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Fal_FalSpeechModel_DoGenerateAsync_Vercel_AI_Operations_SpeechModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalSpeechModel.cs#L47)

<a id="Vercel_AI_Fal_FalSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalSpeechModel.cs#L34)
