---
title: "AzureTranscriptionModel"
description: "Azure transcription model. It resolves the API for each call, because providerOptions also reach this model through the gateway: openai for Azure OpenAI depl..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureTranscriptionModel"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


Azure transcription model. It resolves the API for each call, because providerOptions also reach this model through
the gateway: `openai` for Azure OpenAI deployments, `speech` for the MAI-Transcribe Speech API and `mai`
for MAI streaming transcription. Maps to `AzureTranscriptionModel` in `azure-openai-provider.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L19)

## Properties

<a id="Vercel_AI_Azure_AzureTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L47)

<a id="Vercel_AI_Azure_AzureTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L41)

<a id="Vercel_AI_Azure_AzureTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L38)

<a id="Vercel_AI_Azure_AzureTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L44)

## Methods

<a id="Vercel_AI_Azure_AzureTranscriptionModel_Deserialize_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_String_System_Net_Http_HttpMessageHandler_"></a>

### Deserialize(SerializedModelOptions, string?, HttpMessageHandler?)


```csharp
public static AzureTranscriptionModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
```


Restores a model from [Serialize()](#Vercel_AI_Azure_AzureTranscriptionModel_Serialize), resolving the API for each call again. Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `serialized` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | The serialized model id and config. |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) | API key. Defaults to the `AZURE_API_KEY` environment variable. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | HTTP handler. Pass one from tests. |

**Returns:** [AzureTranscriptionModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L79)

<a id="Vercel_AI_Azure_AzureTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L130)

<a id="Vercel_AI_Azure_AzureTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L167)

<a id="Vercel_AI_Azure_AzureTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L117)

<a id="Vercel_AI_Azure_AzureTranscriptionModel_Serialize_"></a>
<a id="Vercel_AI_Azure_AzureTranscriptionModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and the JSON-serializable provider settings for a workflow boundary. Maps to
`[WORKFLOW_SERIALIZE]`. The API key and the token provider are not part of the serialized config. Pass the key
to `HttpMessageHandler)`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureTranscriptionModel.cs#L54)
