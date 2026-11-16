---
title: "GatewayTranscriptionModel"
description: "Gateway transcription model, routed through /transcription-model. Maps to GatewayTranscriptionModel. Streaming transcription is not ported yet, so CanStream..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


Gateway transcription model, routed through `/transcription-model`. Maps to `GatewayTranscriptionModel`.
Streaming transcription is not ported yet, so [CanStream](#Vercel_AI_Gateway_GatewayTranscriptionModel_CanStream) is false.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L17)

## Constructors

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel__ctor_Vercel_AI_Gateway_GatewayProvider_System_String_System_String_"></a>

### GatewayTranscriptionModel(GatewayProvider, string, string?)


```csharp
public GatewayTranscriptionModel(GatewayProvider provider, string modelId, string? providerName = null)
```


Creates a Gateway transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) | Gateway provider that sends the requests. |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Model id. |
| `providerName` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Value of [Provider](#Vercel_AI_Gateway_GatewayTranscriptionModel_Provider) (upstream `GatewayConfig.provider`). Defaults to `gateway`. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L25)

## Properties

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L42)

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L36)

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L33)

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L39)

## Methods

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L53)

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L150)

<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTranscriptionModel.cs#L45)
