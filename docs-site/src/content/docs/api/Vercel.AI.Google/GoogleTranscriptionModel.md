---
title: "GoogleTranscriptionModel"
description: "Gemini transcription. Unary models post to the Interactions API."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleTranscriptionModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


Gemini transcription. Unary models post to the Interactions API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L14)

## Constructors

<a id="Vercel_AI_Google_GoogleTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel__ctor_Vercel_AI_Google_GoogleProvider_System_String_"></a>

### GoogleTranscriptionModel(GoogleProvider, string)


```csharp
public GoogleTranscriptionModel(GoogleProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L19)

## Properties

<a id="Vercel_AI_Google_GoogleTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


False. The Gemini Live WebSocket is not opened by this port.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L47)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L32)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L26)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L41)

## Methods

<a id="Vercel_AI_Google_GoogleTranscriptionModel_BuildLiveSetup_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_BuildLiveSetup_System_String_"></a>

### BuildLiveSetup(string)


```csharp
public static JsonObject BuildLiveSetup(string modelId)
```


Builds the Live API setup message. Live models reject a unary call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L169)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_BuildRequest_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_BuildRequest_System_String_Vercel_AI_Provider_AudioInput_System_Text_Json_Nodes_JsonObject_"></a>

### BuildRequest(string, AudioInput, JsonObject?)


```csharp
public static JsonObject BuildRequest(string modelId, AudioInput audio, JsonObject? transcriptionConfig)
```


Builds the Interactions transcription body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `transcriptionConfig` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L145)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(TranscriptionModelCall, CancellationToken)


```csharp
public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
```


Transcribes audio through the Interactions API. `google` provider options become the
`transcription_config`, and `word_info` annotations become word segments.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionModelCall](/api/Vercel.AI.Operations/TranscriptionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L64)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

### DoStreamAsync(TranscriptionStreamCall, CancellationToken)


```csharp
public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
```


Returns null. The Gemini Live WebSocket is not opened by this port.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionStreamCall](/api/Vercel.AI.Operations/TranscriptionStreamCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionStreamStart&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L139)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L53)

<a id="Vercel_AI_Google_GoogleTranscriptionModel_IsLive_"></a>
<a id="Vercel_AI_Google_GoogleTranscriptionModel_IsLive_System_String_"></a>

### IsLive(string)


```csharp
public static bool IsLive(string modelId)
```


True when the model id is a Live API transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L35)
