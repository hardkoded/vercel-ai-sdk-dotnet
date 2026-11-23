---
title: "StreamTranscriptionEndEvent"
description: "Event passed to telemetry when CancellationToken) completes. Maps to StreamTranscriptionEndEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranscriptionEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamTranscriptionEndEvent
```


Event passed to telemetry when `CancellationToken)` completes. Maps to `StreamTranscriptionEndEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L633)

## Constructors

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent__ctor_System_String_System_String_System_String_Vercel_AI_Operations_TranscriptionAudioInfo_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_TranscriptSegment__System_String_System_Nullable_System_Double__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `StreamTranscriptionEndEvent(string, string, string, TranscriptionAudioInfo, string, IReadOnlyList<TranscriptSegment>, string?, double?, JsonElement?, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse)`


```csharp
public StreamTranscriptionEndEvent(string callId, string provider, string modelId, TranscriptionAudioInfo audio, string text, IReadOnlyList<TranscriptSegment> segments, string? language, double? durationInSeconds, JsonElement? usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `audio` | [TranscriptionAudioInfo](/api/Vercel.AI.Operations/TranscriptionAudioInfo/) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `segments` | [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `durationInSeconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `usage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L636)

## Properties

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L714)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Audio_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Audio"></a>

### Audio


```csharp
public TranscriptionAudioInfo Audio { get; }
```


Input audio metadata.

**Value:** [TranscriptionAudioInfo](/api/Vercel.AI.Operations/TranscriptionAudioInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L678)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_CallId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L666)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_DurationInSeconds_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Duration of the input audio, when available.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L690)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L711)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Language_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Detected language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L687)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L675)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.streamTranscribe`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L669)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Provider_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L672)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata. Hidden when inputs or outputs are not recorded.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L699)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L705)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L708)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L702)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Segments_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment> Segments { get; }
```


Segments with timing information.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L684)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Text_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Text"></a>

### Text


```csharp
public string Text { get; }
```


The complete transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L681)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Usage_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Usage"></a>

### Usage


```csharp
public JsonElement? Usage { get; }
```


Usage reported by the provider, when available.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L693)

<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_StreamTranscriptionEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from the transcription model.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L696)
