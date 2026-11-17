---
title: "GladiaProvider.GladiaTranscriptionModel"
description: "Uploads audio, starts a pre-recorded job, and reads the result."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaProvider.GladiaTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaProvider.GladiaTranscriptionModel : ITranscriptionModel
```


Uploads audio, starts a pre-recorded job, and reads the result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L333)

## Constructors

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel__ctor_Vercel_AI_Gladia_GladiaProvider_System_String_"></a>

### GladiaTranscriptionModel(GladiaProvider, string)


```csharp
public GladiaTranscriptionModel(GladiaProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GladiaProvider](/api/Vercel.AI.Gladia/GladiaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L338)

## Properties

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L351)

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L345)

## Methods

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Deserialize_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_String_System_Net_Http_HttpMessageHandler_"></a>

### Deserialize(SerializedModelOptions, string?, HttpMessageHandler?)


```csharp
public static GladiaProvider.GladiaTranscriptionModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
```


Restores a model from [Serialize()](#Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serialized` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | The serialized model id and config. |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) | API key. Defaults to the `GLADIA_API_KEY` environment variable. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | HTTP handler. Pass one from tests. |

**Returns:** [GladiaProvider.GladiaTranscriptionModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L384)

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L417)

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Serialize_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and config for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`.
The API key is not part of the serialized config. Pass it to `HttpMessageHandler)`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L357)

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_TranscribeAsync_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_TranscribeAsync_Vercel_AI_Provider_AudioInput_Vercel_AI_Gladia_GladiaTranscriptionRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(AudioInput, GladiaTranscriptionRequest?, CancellationToken)


```csharp
public Task<GladiaTranscriptionResult> TranscribeAsync(AudioInput audio, GladiaTranscriptionRequest? request, CancellationToken cancellationToken)
```


Transcribes audio through upload, pre-recorded init, and result polling.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `request` | [GladiaTranscriptionRequest](/api/Vercel.AI.Gladia/GladiaTranscriptionRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GladiaTranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L428)
