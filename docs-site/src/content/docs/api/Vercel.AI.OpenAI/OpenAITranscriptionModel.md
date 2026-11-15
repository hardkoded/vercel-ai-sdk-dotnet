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
public sealed class OpenAITranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


OpenAI audio transcription model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L104)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel__ctor_Vercel_AI_Azure_AzureOpenAIProvider_System_String_System_Func_System_DateTimeOffset__Vercel_AI_ProviderUtils_WebSocketFactory_"></a>

### `OpenAITranscriptionModel(AzureOpenAIProvider, string, Func<DateTimeOffset>?, WebSocketFactory?)`


```csharp
public OpenAITranscriptionModel(AzureOpenAIProvider provider, string deploymentId, Func<DateTimeOffset>? clock = null, WebSocketFactory? webSocket = null)
```


Creates an Azure OpenAI transcription model for a deployment. Requests go to the deployment's
`audio/transcriptions` route and the provider id is `azure.transcription`. &lt;code class="paramref"&gt;webSocket&lt;/code&gt; is the WebSocket for realtime streaming. It defaults to `ClientWebSocket`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/) |  |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `webSocket` | [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L206)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L179)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L185)

## Properties

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L274)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L268)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L265)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L271)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L349)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L406)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

### DoStreamAsync(TranscriptionStreamCall, CancellationToken)


```csharp
public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
```


Starts a realtime streaming transcription over the OpenAI Realtime WebSocket. Maps to `doStream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionStreamCall](/api/Vercel.AI.Operations/TranscriptionStreamCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionStreamStart&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L427)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L277)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L284)
