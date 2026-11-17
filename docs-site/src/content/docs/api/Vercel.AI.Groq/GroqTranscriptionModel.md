---
title: "GroqTranscriptionModel"
description: "Groq speech-to-text model. Maps to GroqTranscriptionModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "GroqTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Groq_GroqTranscriptionModel"></a>

Namespace: [Vercel.AI.Groq](/api/Vercel.AI.Groq/)


```csharp
public sealed class GroqTranscriptionModel : ITranscriptionModel, ITranscriptionUrlCaller, ITranscriptionCaller
```


Groq speech-to-text model. Maps to `GroqTranscriptionModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionUrlCaller](/api/Vercel.AI.Operations/ITranscriptionUrlCaller/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L18)

## Constructors

<a id="Vercel_AI_Groq_GroqTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel__ctor_Vercel_AI_Groq_GroqProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `GroqTranscriptionModel(GroqProvider, string, Func<DateTimeOffset>?)`


```csharp
public GroqTranscriptionModel(GroqProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a transcription model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and defaults to UTC now.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GroqProvider](/api/Vercel.AI.Groq/GroqProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L33)

## Properties

<a id="Vercel_AI_Groq_GroqTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L54)

<a id="Vercel_AI_Groq_GroqTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L48)

<a id="Vercel_AI_Groq_GroqTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L45)

<a id="Vercel_AI_Groq_GroqTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L51)

## Methods

<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(TranscriptionModelCall, CancellationToken)


```csharp
public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
```


Transcribes audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionModelCall](/api/Vercel.AI.Operations/TranscriptionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L78)

<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoGenerateFromUrlAsync_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoGenerateFromUrlAsync_Vercel_AI_Operations_TranscriptionModelUrlCall_System_Threading_CancellationToken_"></a>

### DoGenerateFromUrlAsync(TranscriptionModelUrlCall, CancellationToken)


```csharp
public Task<TranscriptionModelResult?> DoGenerateFromUrlAsync(TranscriptionModelUrlCall call, CancellationToken cancellationToken)
```


Transcribes audio the provider fetches from [Audio](/api/Vercel.AI.Operations/TranscriptionModelUrlCall/#Vercel_AI_Operations_TranscriptionModelUrlCall_Audio). Returns &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt;
when the provider cannot fetch this URL and the SDK should download it locally and call `CancellationToken)`.
Throws for other errors, such as authentication or invalid audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionModelUrlCall](/api/Vercel.AI.Operations/TranscriptionModelUrlCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L95)

<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

### DoStreamAsync(TranscriptionStreamCall, CancellationToken)


```csharp
public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
```


Starts a stream, or null when streaming is unsupported.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionStreamCall](/api/Vercel.AI.Operations/TranscriptionStreamCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionStreamStart&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L123)

<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Groq_GroqTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

### DoTranscribeAsync(AudioInput, CancellationToken)


```csharp
public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
```


Transcribes audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqTranscriptionModel.cs#L57)
