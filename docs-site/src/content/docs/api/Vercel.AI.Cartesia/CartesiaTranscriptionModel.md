---
title: "CartesiaTranscriptionModel"
description: "Cartesia batch transcription model. Posts multipart audio to /stt."
editUrl: false
prev: false
next: false
sidebar:
  label: "CartesiaTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel"></a>

Namespace: [Vercel.AI.Cartesia](/api/Vercel.AI.Cartesia/)


```csharp
public sealed class CartesiaTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


Cartesia batch transcription model. Posts multipart audio to `/stt`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L304)

## Constructors

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel__ctor_Vercel_AI_Cartesia_CartesiaProvider_System_String_System_Func_System_DateTime__"></a>

### `CartesiaTranscriptionModel(CartesiaProvider, string, Func<DateTime>?)`


```csharp
public CartesiaTranscriptionModel(CartesiaProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a transcription model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [CartesiaProvider](/api/Vercel.AI.Cartesia/CartesiaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L310)

## Properties

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L327)

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L321)

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L318)

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L324)

## Methods

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L343)

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L398)

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L330)
