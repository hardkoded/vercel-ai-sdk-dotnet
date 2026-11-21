---
title: "ExtractStartEvent"
description: "Event passed to OnStart for extraction operations. Maps to ExtractStartEvent in extract/extract-events.ts. Called when the operation begins, before the extra..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class ExtractStartEvent
```


Event passed to [OnStart](/api/Vercel.AI.Operations/ExtractRequest/#Vercel_AI_Operations_ExtractRequest_OnStart) for extraction operations. Maps to `ExtractStartEvent` in
`extract/extract-events.ts`. Called when the operation begins, before the extraction model is called.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ExtractEndEvent](/api/Vercel.AI.Operations/ExtractEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L13)

## Constructors

<a id="Vercel_AI_Operations_ExtractStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_Vercel_AI_Operations_DecisionState_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionField__System_String_System_Nullable_System_Double__System_String_System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_"></a>

### `ExtractStartEvent(string, IReadOnlyDictionary<string, object?>, string, string, DecisionState?, IReadOnlyDictionary<string, ExtractionField>, string?, double?, string?, int, IReadOnlyDictionary<string, string>?, JsonElement, bool?, bool?, string?)`


```csharp
public ExtractStartEvent(string callId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, DecisionState? state, IReadOnlyDictionary<string, ExtractionField> fields, string? referenceDate, double? maxOutputTokens, string? reasoning, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `state` | [DecisionState](/api/Vercel.AI.Operations/DecisionState/) |  |
| `fields` | [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `referenceDate` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxOutputTokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `recordInputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `recordOutputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L16)

## Properties

<a id="Vercel_AI_Operations_ExtractStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this extraction call, used to correlate events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L52)

<a id="Vercel_AI_Operations_ExtractStartEvent_Fields_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_Fields"></a>

### Fields


```csharp
public IReadOnlyDictionary<string, ExtractionField> Fields { get; }
```


The fields to extract from the shared state.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L70)

<a id="Vercel_AI_Operations_ExtractStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; }
```


Present only on telemetry events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L97)

<a id="Vercel_AI_Operations_ExtractStartEvent_Headers_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Additional HTTP headers sent with the request.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L85)

<a id="Vercel_AI_Operations_ExtractStartEvent_MaxOutputTokens_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public double? MaxOutputTokens { get; }
```


Maximum output tokens requested for the extraction.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L76)

<a id="Vercel_AI_Operations_ExtractStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; }
```


Maximum number of retries for the extraction model call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L82)

<a id="Vercel_AI_Operations_ExtractStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


The extraction model identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L64)

<a id="Vercel_AI_Operations_ExtractStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


Identifies the operation type: `ai.extract`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L55)

<a id="Vercel_AI_Operations_ExtractStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


The provider identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L61)

<a id="Vercel_AI_Operations_ExtractStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Additional provider-specific options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L88)

<a id="Vercel_AI_Operations_ExtractStartEvent_Reasoning_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; }
```


Reasoning effort requested for the extraction.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L79)

<a id="Vercel_AI_Operations_ExtractStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; }
```


Present only on telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L91)

<a id="Vercel_AI_Operations_ExtractStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; }
```


Present only on telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L94)

<a id="Vercel_AI_Operations_ExtractStartEvent_ReferenceDate_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_ReferenceDate"></a>

### ReferenceDate


```csharp
public string? ReferenceDate { get; }
```


`YYYY-MM-DD` date that anchors relative dates and dates without a year.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L73)

<a id="Vercel_AI_Operations_ExtractStartEvent_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; }
```


User-defined runtime context. Telemetry receives the filtered copy.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L58)

<a id="Vercel_AI_Operations_ExtractStartEvent_State_"></a>
<a id="Vercel_AI_Operations_ExtractStartEvent_State"></a>

### State


```csharp
public DecisionState? State { get; }
```


The shared state to extract field values from.

**Value:** [DecisionState](/api/Vercel.AI.Operations/DecisionState/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L67)
