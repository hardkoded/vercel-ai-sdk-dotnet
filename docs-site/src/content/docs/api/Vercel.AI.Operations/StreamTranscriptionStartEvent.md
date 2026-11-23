---
title: "StreamTranscriptionStartEvent"
description: "Event passed to telemetry when CancellationToken) begins. Maps to StreamTranscriptionStartEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranscriptionStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamTranscriptionStartEvent
```


Event passed to telemetry when `CancellationToken)` begins. Maps to `StreamTranscriptionStartEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L568)

## Constructors

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent__ctor_System_String_System_String_System_String_Vercel_AI_Operations_TranscriptionAudioInfo_Vercel_AI_Operations_AudioFormat_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_"></a>

### `StreamTranscriptionStartEvent(string, string, string, TranscriptionAudioInfo, AudioFormat?, int?, IReadOnlyDictionary<string, string>?, JsonElement)`


```csharp
public StreamTranscriptionStartEvent(string callId, string provider, string modelId, TranscriptionAudioInfo audio, AudioFormat? inputAudioFormat, int? maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `audio` | [TranscriptionAudioInfo](/api/Vercel.AI.Operations/TranscriptionAudioInfo/) |  |
| `inputAudioFormat` | [AudioFormat](/api/Vercel.AI.Operations/AudioFormat/) |  |
| `maxRetries` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L571)

## Properties

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L629)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_Audio_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_Audio"></a>

### Audio


```csharp
public TranscriptionAudioInfo Audio { get; }
```


Input audio metadata.

**Value:** [TranscriptionAudioInfo](/api/Vercel.AI.Operations/TranscriptionAudioInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L605)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L593)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L626)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_Headers_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Caller headers, without the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L614)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_InputAudioFormat_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_InputAudioFormat"></a>

### InputAudioFormat


```csharp
public AudioFormat? InputAudioFormat { get; }
```


Raw input format of the audio stream.

**Value:** [AudioFormat](/api/Vercel.AI.Operations/AudioFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L608)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; }
```


Resolved retry limit.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L611)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L602)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.streamTranscribe`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L596)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L599)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L617)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L620)

<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L623)
