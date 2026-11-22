---
title: "TranscriptionEndEvent"
description: "Event passed to telemetry when CancellationToken) completes. Raw audio is not included. Maps to TranscriptionEndEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionEndEvent : IOperationEndEvent
```


Event passed to telemetry when `CancellationToken)` completes. Raw audio is not included. Maps to `TranscriptionEndEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IOperationEndEvent](/api/Vercel.AI/IOperationEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L472)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent__ctor_System_String_System_String_System_String_Vercel_AI_Operations_TranscriptionAudioInfo_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_TranscriptSegment__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_TranscriptSegment__System_String_System_Nullable_System_Double__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_"></a>

### `TranscriptionEndEvent(string, string, string, TranscriptionAudioInfo, string, IReadOnlyList<TranscriptSegment>, IReadOnlyList<TranscriptSegment>?, string?, double?, JsonElement?, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse, bool?, bool?, string?)`


```csharp
public TranscriptionEndEvent(string callId, string provider, string modelId, TranscriptionAudioInfo audio, string text, IReadOnlyList<TranscriptSegment> segments, IReadOnlyList<TranscriptSegment>? words, string? language, double? durationInSeconds, JsonElement? usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
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
| `words` | [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `durationInSeconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `usage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |
| `recordInputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `recordOutputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L475)

## Properties

<a id="Vercel_AI_Operations_TranscriptionEndEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L564)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Audio_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Audio"></a>

### Audio


```csharp
public TranscriptionAudioInfo Audio { get; }
```


Input audio metadata. Byte length and media type are unknown for native URL transcription.

**Value:** [TranscriptionAudioInfo](/api/Vercel.AI.Operations/TranscriptionAudioInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L525)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_CallId_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L513)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_DurationInSeconds_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Duration of the input audio, when available.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L540)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L561)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Language_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Detected language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L537)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L522)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.transcribe`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L516)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Provider_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L519)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata. Hidden when inputs or outputs are not recorded.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L549)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L555)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L558)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L552)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Segments_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment> Segments { get; }
```


Segments with timing information.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L531)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Text_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Text"></a>

### Text


```csharp
public string Text { get; }
```


The complete transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L528)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Usage_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Usage"></a>

### Usage


```csharp
public JsonElement? Usage { get; }
```


Usage reported by the provider, when available.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L543)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from the transcription model.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L546)

<a id="Vercel_AI_Operations_TranscriptionEndEvent_Words_"></a>
<a id="Vercel_AI_Operations_TranscriptionEndEvent_Words"></a>

### Words


```csharp
public IReadOnlyList<TranscriptSegment>? Words { get; }
```


Word timings returned by the provider, whether requested or returned by default.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L534)
