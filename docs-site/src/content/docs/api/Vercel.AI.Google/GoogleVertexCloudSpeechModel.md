---
title: "GoogleVertexCloudSpeechModel"
description: "Cloud Text-to-Speech text:synthesize for Chirp 3 HD voices."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexCloudSpeechModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexCloudSpeechModel : ISpeechModel
```


Cloud Text-to-Speech `text:synthesize` for Chirp 3 HD voices.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L15)

## Constructors

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel__ctor_Vercel_AI_Google_GoogleVertexProvider_System_String_"></a>

### GoogleVertexCloudSpeechModel(GoogleVertexProvider, string)


```csharp
public GoogleVertexCloudSpeechModel(GoogleVertexProvider provider, string modelId)
```


Creates a Cloud TTS model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L23)

## Fields

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_SynthesizeUrl"></a>

### SynthesizeUrl


```csharp
public const string SynthesizeUrl = "https://texttospeech.googleapis.com/v1/text:synthesize"
```


Synthesize endpoint. Cloud TTS uses one non-regional host.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L18)

## Properties

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L36)

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L30)

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Warnings from the last call.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L39)

## Methods

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_DoGenerateAsync_Vercel_AI_Provider_SpeechCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L42)

<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_Prepare_"></a>
<a id="Vercel_AI_Google_GoogleVertexCloudSpeechModel_Prepare_System_String_System_String_System_String_System_Nullable_System_Double__System_String_"></a>

### Prepare(string, string?, string?, double?, string?)


```csharp
public static GoogleSpeechPreparation Prepare(string text, string? voice, string? language, double? speed, string? instructions)
```


Builds the synthesize body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `voice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `speed` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GoogleSpeechPreparation](/api/Vercel.AI.Google/GoogleSpeechPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L58)
