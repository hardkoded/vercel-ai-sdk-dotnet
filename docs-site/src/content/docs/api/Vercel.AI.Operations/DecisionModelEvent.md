---
title: "DecisionModelEvent"
description: "Model-call telemetry event."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionModelEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionModelEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionModelEvent
```


Model-call telemetry event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluateModelEvent](/api/Vercel.AI.Operations/EvaluateModelEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L309)

## Constructors

<a id="Vercel_AI_Operations_DecisionModelEvent__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent__ctor_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionModelStatePart__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionQuestion__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__Vercel_AI_Operations_OperationUsage_System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_"></a>

### `DecisionModelEvent(string, string, string, string, IReadOnlyList<DecisionModelStatePart>?, IReadOnlyDictionary<string, DecisionQuestion>, IReadOnlyDictionary<string, DecisionAnswer>?, OperationUsage?, bool?, bool?, string?)`


```csharp
public DecisionModelEvent(string callId, string operationId, string provider, string modelId, IReadOnlyList<DecisionModelStatePart>? state, IReadOnlyDictionary<string, DecisionQuestion> questions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
```


Creates a model event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `state` | [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `questions` | [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `answers` | [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `recordInputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `recordOutputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L312)

## Properties

<a id="Vercel_AI_Operations_DecisionModelEvent_Answers_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, DecisionAnswer>? Answers { get; }
```


Answers after the model returns.

**Value:** [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L346)

<a id="Vercel_AI_Operations_DecisionModelEvent_CallId_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L328)

<a id="Vercel_AI_Operations_DecisionModelEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; }
```


Telemetry function id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L358)

<a id="Vercel_AI_Operations_DecisionModelEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L337)

<a id="Vercel_AI_Operations_DecisionModelEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.decide.doDecide`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L331)

<a id="Vercel_AI_Operations_DecisionModelEvent_Provider_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L334)

<a id="Vercel_AI_Operations_DecisionModelEvent_Questions_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_Questions"></a>

### Questions


```csharp
public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }
```


Questions.

**Value:** [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L343)

<a id="Vercel_AI_Operations_DecisionModelEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; }
```


Telemetry input recording flag.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L352)

<a id="Vercel_AI_Operations_DecisionModelEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; }
```


Telemetry output recording flag.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L355)

<a id="Vercel_AI_Operations_DecisionModelEvent_State_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_State"></a>

### State


```csharp
public IReadOnlyList<DecisionModelStatePart>? State { get; }
```


The normalized list of state parts the model receives.

**Value:** [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L340)

<a id="Vercel_AI_Operations_DecisionModelEvent_Usage_"></a>
<a id="Vercel_AI_Operations_DecisionModelEvent_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Provider usage before total tokens are derived.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L349)
