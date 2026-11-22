---
title: "XaiTranscriptionModel"
description: "xAI speech-to-text model, over /stt and its WebSocket. Maps to XaiTranscriptionModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiTranscriptionModel"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


xAI speech-to-text model, over `/stt` and its WebSocket. Maps to `XaiTranscriptionModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L17)

## Constructors

<a id="Vercel_AI_Xai_XaiTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel__ctor_Vercel_AI_Xai_XaiProvider_System_String_System_Func_System_DateTimeOffset__"></a>

### `XaiTranscriptionModel(XaiProvider, string, Func<DateTimeOffset>?)`


```csharp
public XaiTranscriptionModel(XaiProvider provider, string modelId = "", Func<DateTimeOffset>? clock = null)
```


Creates a transcription model. &lt;code class="paramref"&gt;modelId&lt;/code&gt; is empty for xAI's default model.
&lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp (upstream `_internal.currentDate`) and defaults to UTC now.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L34)

## Properties

<a id="Vercel_AI_Xai_XaiTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L55)

<a id="Vercel_AI_Xai_XaiTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L49)

<a id="Vercel_AI_Xai_XaiTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L46)

<a id="Vercel_AI_Xai_XaiTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L52)

## Methods

<a id="Vercel_AI_Xai_XaiTranscriptionModel_Deserialize_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_String_System_Net_Http_HttpMessageHandler_"></a>

### Deserialize(SerializedModelOptions, string?, HttpMessageHandler?)


```csharp
public static XaiTranscriptionModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
```


Restores a model from [Serialize()](#Vercel_AI_Xai_XaiTranscriptionModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serialized` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | The serialized model id and config. |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) | API key. Defaults to the `XAI_API_KEY` environment variable. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | HTTP handler. Pass one from tests. |

**Returns:** [XaiTranscriptionModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L75)

<a id="Vercel_AI_Xai_XaiTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L127)

<a id="Vercel_AI_Xai_XaiTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L274)

<a id="Vercel_AI_Xai_XaiTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L106)

<a id="Vercel_AI_Xai_XaiTranscriptionModel_Serialize_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and config for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`. The API key and
headers are not part of the serialized config. Pass the key to `HttpMessageHandler)`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModel.cs#L61)
