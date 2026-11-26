---
title: "GenerateSpeechStartEvent"
description: "Event passed to telemetry integrations when speech generation begins. Maps to GenerateSpeechStartEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateSpeechStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GenerateSpeechStartEvent : IOperationStartEvent
```


Event passed to telemetry integrations when speech generation begins. Maps to `GenerateSpeechStartEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IOperationStartEvent](/api/Vercel.AI/IOperationStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L207)

## Constructors

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent__ctor_System_String_System_String_System_String_System_String_System_String_System_String_System_String_System_Nullable_System_Double__System_String_System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_"></a>

### `GenerateSpeechStartEvent(string, string, string, string, string?, string?, string?, double?, string?, int, IReadOnlyDictionary<string, string>?, JsonElement)`


```csharp
public GenerateSpeechStartEvent(string callId, string provider, string modelId, string text, string? voice, string? outputFormat, string? instructions, double? speed, string? language, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `voice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputFormat` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `speed` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L210)

## Properties

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L279)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this speech generation call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L240)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L288)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Headers_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Additional HTTP headers sent with the request.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L273)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Instructions_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; }
```


Additional speech instructions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L261)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Language_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Language"></a>

### Language


```csharp
public string? Language { get; }
```


The requested language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L267)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; }
```


Maximum number of retries for failed requests.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L270)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


The specific model identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L249)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.generateSpeech`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L243)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_OutputFormat_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; }
```


The requested output format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L258)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


The provider identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L246)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Additional provider-specific options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L276)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L282)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L285)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Speed_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Speed"></a>

### Speed


```csharp
public double? Speed { get; }
```


The requested speech speed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L264)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Text_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Text"></a>

### Text


```csharp
public string Text { get; }
```


The text to convert to speech.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L252)

<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Voice_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechStartEvent_Voice"></a>

### Voice


```csharp
public string? Voice { get; }
```


The requested voice.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L255)
