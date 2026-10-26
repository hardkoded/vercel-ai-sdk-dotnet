---
title: "OpenAITranscriptionModel"
description: "OpenAI audio transcription model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAITranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAITranscriptionModel : ITranscriptionModel
```


OpenAI audio transcription model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L103)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `OpenAITranscriptionModel(OpenAIProvider, string, Func<DateTimeOffset>?)`


```csharp
public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
```


Creates a transcription model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp and defaults to UTC now.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L172)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_System_String_System_Func_System_DateTimeOffset__"></a>

### `OpenAITranscriptionModel(OpenAIProvider, string, string, Func<DateTimeOffset>?)`


```csharp
public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, string path, Func<DateTimeOffset>? clock = null)
```


Creates a transcription or translation model for &lt;code class="paramref"&gt;path&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `path` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L178)

## Properties

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L190)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L187)

## Methods

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_Build_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_Build_Vercel_AI_OpenAI_OpenAITranscriptionCall_"></a>

### Build(OpenAITranscriptionCall)


```csharp
public MultipartFormDataContent Build(OpenAITranscriptionCall call)
```


Builds the multipart body so tests can inspect field names.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [OpenAITranscriptionCall](/api/Vercel.AI.OpenAI/OpenAITranscriptionCall/) |  |

**Returns:** [MultipartFormDataContent](https://learn.microsoft.com/dotnet/api/system.net.http.multipartformdatacontent)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L269)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L199)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_IsRealtimeWhisper_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_IsRealtimeWhisper_System_String_"></a>

### IsRealtimeWhisper(string)


```csharp
public static bool IsRealtimeWhisper(string modelId)
```


True when &lt;code class="paramref"&gt;modelId&lt;/code&gt; must use the realtime transcription socket.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L193)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_TranscribeAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_TranscribeAsync_Vercel_AI_OpenAI_OpenAITranscriptionCall_System_Threading_CancellationToken_"></a>

### TranscribeAsync(OpenAITranscriptionCall, CancellationToken)


```csharp
public Task<OpenAITranscriptionResult> TranscribeAsync(OpenAITranscriptionCall call, CancellationToken cancellationToken)
```


Transcribes audio and returns segments, language, duration, and response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [OpenAITranscriptionCall](/api/Vercel.AI.OpenAI/OpenAITranscriptionCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAITranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L206)
