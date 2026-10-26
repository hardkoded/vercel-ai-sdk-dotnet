---
title: "HumeProvider.HumeSpeechModel"
description: "Posts utterances to /v0/tts/file."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeProvider.HumeSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeProvider.HumeSpeechModel : ISpeechModel
```


Posts utterances to `/v0/tts/file`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L229)

## Constructors

<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel__ctor_"></a>
<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel__ctor_Vercel_AI_Hume_HumeProvider_"></a>

### HumeSpeechModel(HumeProvider)


```csharp
public HumeSpeechModel(HumeProvider provider)
```


Creates a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [HumeProvider](/api/Vercel.AI.Hume/HumeProvider/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L234)

## Properties

<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L246)

<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_Provider_"></a>
<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L240)

## Methods

<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L252)

<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_Hume_HumeProvider_HumeSpeechModel_GenerateAsync_Vercel_AI_Hume_HumeSpeechRequest_System_Threading_CancellationToken_"></a>

### GenerateAsync(HumeSpeechRequest, CancellationToken)


```csharp
public Task<HumeSpeechResult> GenerateAsync(HumeSpeechRequest request, CancellationToken cancellationToken)
```


Synthesizes speech and returns the audio bytes with response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [HumeSpeechRequest](/api/Vercel.AI.Hume/HumeSpeechRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;HumeSpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L260)
