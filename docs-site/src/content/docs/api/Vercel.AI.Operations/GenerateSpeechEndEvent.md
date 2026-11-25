---
title: "GenerateSpeechEndEvent"
description: "Event passed to telemetry integrations when speech generation completes. Raw audio is intentionally not included. Integrations receive only its size, media t..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateSpeechEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GenerateSpeechEndEvent : IOperationEndEvent
```


Event passed to telemetry integrations when speech generation completes. Raw audio is intentionally not included. Integrations
receive only its size, media type, and format. Maps to `GenerateSpeechEndEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IOperationEndEvent](/api/Vercel.AI/IOperationEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L295)

## Constructors

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent__ctor_System_String_System_String_System_String_System_String_Vercel_AI_Operations_GenerateSpeechAudioInfo_System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `GenerateSpeechEndEvent(string, string, string, string, GenerateSpeechAudioInfo, JsonElement?, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse)`


```csharp
public GenerateSpeechEndEvent(string callId, string provider, string modelId, string text, GenerateSpeechAudioInfo audio, JsonElement? usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `audio` | [GenerateSpeechAudioInfo](/api/Vercel.AI.Operations/GenerateSpeechAudioInfo/) |  |
| `usage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L298)

## Properties

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L352)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Audio_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Audio"></a>

### Audio


```csharp
public GenerateSpeechAudioInfo Audio { get; }
```


Metadata about the generated audio.

**Value:** [GenerateSpeechAudioInfo](/api/Vercel.AI.Operations/GenerateSpeechAudioInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L337)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_CallId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this speech generation call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L322)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L361)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


The specific model identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L331)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.generateSpeech`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L325)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Provider_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


The provider identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L328)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Optional provider-specific metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L346)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L355)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L358)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata from the provider: timestamp, model id and headers.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L349)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Text_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Text"></a>

### Text


```csharp
public string Text { get; }
```


The text that was converted to speech.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L334)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Usage_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Usage"></a>

### Usage


```csharp
public JsonElement? Usage { get; }
```


Usage reported by the provider, when available.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L340)

<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from the speech model.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L343)
