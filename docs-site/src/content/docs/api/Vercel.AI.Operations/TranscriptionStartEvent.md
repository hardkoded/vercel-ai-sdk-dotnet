---
title: "TranscriptionStartEvent"
description: "Event passed to telemetry when CancellationToken) begins. Maps to TranscriptionStartEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionStartEvent
```


Event passed to telemetry when `CancellationToken)` begins. Maps to `TranscriptionStartEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L304)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent__ctor_System_String_System_String_System_String_Vercel_AI_Operations_TranscriptionAudioInfo_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyList_System_String__Vercel_AI_Operations_AudioFormat_System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_"></a>

### `TranscriptionStartEvent(string, string, string, TranscriptionAudioInfo, int?, IReadOnlyDictionary<string, string>?, JsonElement, IReadOnlyList<string>?, AudioFormat?, bool?, bool?, string?)`


```csharp
public TranscriptionStartEvent(string callId, string provider, string modelId, TranscriptionAudioInfo audio, int? maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, IReadOnlyList<string>? timestampGranularities, AudioFormat? inputAudioFormat = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `audio` | [TranscriptionAudioInfo](/api/Vercel.AI.Operations/TranscriptionAudioInfo/) |  |
| `maxRetries` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `timestampGranularities` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `inputAudioFormat` | [AudioFormat](/api/Vercel.AI.Operations/AudioFormat/) |  |
| `recordInputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `recordOutputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L307)

## Properties

<a id="Vercel_AI_Operations_TranscriptionStartEvent_Audio_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_Audio"></a>

### Audio


```csharp
public TranscriptionAudioInfo Audio { get; }
```


Input audio metadata.

**Value:** [TranscriptionAudioInfo](/api/Vercel.AI.Operations/TranscriptionAudioInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L336)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L324)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; }
```


Copied from [FunctionId](/api/Vercel.AI.Operations/TranscribeTelemetry/#Vercel_AI_Operations_TranscribeTelemetry_FunctionId).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L360)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_Headers_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Caller headers, without the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L345)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_InputAudioFormat_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_InputAudioFormat"></a>

### InputAudioFormat


```csharp
public AudioFormat? InputAudioFormat { get; }
```


Raw input format. Always &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; for non-streaming transcription.

**Value:** [AudioFormat](/api/Vercel.AI.Operations/AudioFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L339)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; }
```


Resolved retry limit.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L342)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L333)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.transcribe`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L327)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L330)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L348)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; }
```


Copied from [RecordInputs](/api/Vercel.AI.Operations/TranscribeTelemetry/#Vercel_AI_Operations_TranscribeTelemetry_RecordInputs).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L354)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; }
```


Copied from [RecordOutputs](/api/Vercel.AI.Operations/TranscribeTelemetry/#Vercel_AI_Operations_TranscribeTelemetry_RecordOutputs).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L357)

<a id="Vercel_AI_Operations_TranscriptionStartEvent_TimestampGranularities_"></a>
<a id="Vercel_AI_Operations_TranscriptionStartEvent_TimestampGranularities"></a>

### TimestampGranularities


```csharp
public IReadOnlyList<string>? TimestampGranularities { get; }
```


Requested shared timing granularities, when specified.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L351)
