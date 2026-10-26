---
title: "GoogleSpeechModel"
description: "Gemini text-to-speech. Posts to generateContent with an audio response modality."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleSpeechModel : ISpeechModel
```


Gemini text-to-speech. Posts to `generateContent` with an audio response modality.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L112)

## Constructors

<a id="Vercel_AI_Google_GoogleSpeechModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleSpeechModel__ctor_Vercel_AI_Google_GoogleProvider_System_String_"></a>

### GoogleSpeechModel(GoogleProvider, string)


```csharp
public GoogleSpeechModel(GoogleProvider provider, string modelId)
```


Creates a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L117)

## Properties

<a id="Vercel_AI_Google_GoogleSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L135)

<a id="Vercel_AI_Google_GoogleSpeechModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L124)

<a id="Vercel_AI_Google_GoogleSpeechModel_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleSpeechModel_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Warnings from the last call.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L138)

## Methods

<a id="Vercel_AI_Google_GoogleSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Google_GoogleSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L141)

<a id="Vercel_AI_Google_GoogleSpeechModel_GenerateAsync_"></a>
<a id="Vercel_AI_Google_GoogleSpeechModel_GenerateAsync_Vercel_AI_Google_GoogleSpeechCall_System_Threading_CancellationToken_"></a>

### GenerateAsync(GoogleSpeechCall, CancellationToken)


```csharp
public Task<SpeechResult> GenerateAsync(GoogleSpeechCall call, CancellationToken cancellationToken)
```


Synthesizes speech with Gemini-specific options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [GoogleSpeechCall](/api/Vercel.AI.Google/GoogleSpeechCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;SpeechResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L147)

<a id="Vercel_AI_Google_GoogleSpeechModel_Prepare_"></a>
<a id="Vercel_AI_Google_GoogleSpeechModel_Prepare_System_String_System_String_Vercel_AI_Google_GoogleSpeechCall_"></a>

### Prepare(string, string, GoogleSpeechCall)


```csharp
public static GoogleSpeechPreparation Prepare(string modelId, string provider, GoogleSpeechCall call)
```


Builds the speech request body and warnings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `call` | [GoogleSpeechCall](/api/Vercel.AI.Google/GoogleSpeechCall/) |  |

**Returns:** [GoogleSpeechPreparation](/api/Vercel.AI.Google/GoogleSpeechPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L180)
