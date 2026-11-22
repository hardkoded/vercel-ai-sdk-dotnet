---
title: "ExtractionModelCallStartEvent"
description: "Event fired when the extraction model call begins. Maps to ExtractionModelCallStartEvent. The logical model call includes any provider retries."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionModelCallStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class ExtractionModelCallStartEvent
```


Event fired when the extraction model call begins. Maps to `ExtractionModelCallStartEvent`.
The logical model call includes any provider retries.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ExtractionModelCallEndEvent](/api/Vercel.AI.Operations/ExtractionModelCallEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L160)

## Constructors

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent__ctor_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionModelStatePart__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionField__System_String_System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_"></a>

### `ExtractionModelCallStartEvent(string, string, string, IReadOnlyList<DecisionModelStatePart>, IReadOnlyDictionary<string, ExtractionField>, string?, bool?, bool?, string?)`


```csharp
public ExtractionModelCallStartEvent(string callId, string provider, string modelId, IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, ExtractionField> fields, string? referenceDate, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `state` | [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `fields` | [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `referenceDate` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `recordInputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `recordOutputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L163)

## Properties

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for the outer extraction call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L187)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_Fields_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_Fields"></a>

### Fields


```csharp
public IReadOnlyDictionary<string, ExtractionField> Fields { get; }
```


The fields to extract from the shared state.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L202)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; }
```


Present only on telemetry events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L214)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


The extraction model identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L196)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


Identifies the inner operation: `ai.extract.doExtract`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L190)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


The provider identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L193)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; }
```


Present only on telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L208)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; }
```


Present only on telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L211)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_ReferenceDate_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_ReferenceDate"></a>

### ReferenceDate


```csharp
public string? ReferenceDate { get; }
```


`YYYY-MM-DD` date that anchors relative dates and dates without a year.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L205)

<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_State_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallStartEvent_State"></a>

### State


```csharp
public IReadOnlyList<DecisionModelStatePart> State { get; }
```


The normalized list of state parts the model receives.

**Value:** [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L199)
