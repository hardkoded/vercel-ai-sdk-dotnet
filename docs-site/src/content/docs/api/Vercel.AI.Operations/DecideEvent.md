---
title: "DecideEvent"
description: "Lifecycle event for decide."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecideEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_DecideEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecideEvent
```


Lifecycle event for decide.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluateEvent](/api/Vercel.AI.Operations/EvaluateEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L228)

## Constructors

<a id="Vercel_AI_Operations_DecideEvent__ctor_"></a>
<a id="Vercel_AI_Operations_DecideEvent__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_Vercel_AI_Operations_DecisionState_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionQuestion__System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_System_Nullable_System_Double__System_String_"></a>

### `DecideEvent(string, string, IReadOnlyDictionary<string, object?>, string, string, DecisionState?, IReadOnlyDictionary<string, DecisionQuestion>, int, IReadOnlyDictionary<string, string>?, JsonElement, IReadOnlyDictionary<string, DecisionAnswer>?, OperationUsage?, IReadOnlyList<OperationWarning>?, bool?, bool?, string?, double?, string?)`


```csharp
public DecideEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, DecisionState? state, IReadOnlyDictionary<string, DecisionQuestion> questions, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement providerOptions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, IReadOnlyList<OperationWarning>? warnings = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null, double? maxOutputTokens = null, string? reasoning = null)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `state` | [DecisionState](/api/Vercel.AI.Operations/DecisionState/) |  |
| `questions` | [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `maxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `answers` | [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `recordInputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `recordOutputs` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `functionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxOutputTokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L231)

## Properties

<a id="Vercel_AI_Operations_DecideEvent_Answers_"></a>
<a id="Vercel_AI_Operations_DecideEvent_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, DecisionAnswer>? Answers { get; }
```


Answers, once the call has finished.

**Value:** [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L290)

<a id="Vercel_AI_Operations_DecideEvent_CallId_"></a>
<a id="Vercel_AI_Operations_DecideEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L254)

<a id="Vercel_AI_Operations_DecideEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_DecideEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; }
```


Present only on telemetry events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L305)

<a id="Vercel_AI_Operations_DecideEvent_Headers_"></a>
<a id="Vercel_AI_Operations_DecideEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Caller headers, without the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L284)

<a id="Vercel_AI_Operations_DecideEvent_MaxOutputTokens_"></a>
<a id="Vercel_AI_Operations_DecideEvent_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public double? MaxOutputTokens { get; }
```


Maximum output tokens requested for the decision.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L275)

<a id="Vercel_AI_Operations_DecideEvent_MaxRetries_"></a>
<a id="Vercel_AI_Operations_DecideEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; }
```


Resolved retry limit.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L281)

<a id="Vercel_AI_Operations_DecideEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_DecideEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L266)

<a id="Vercel_AI_Operations_DecideEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_DecideEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.decide`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L257)

<a id="Vercel_AI_Operations_DecideEvent_Provider_"></a>
<a id="Vercel_AI_Operations_DecideEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L263)

<a id="Vercel_AI_Operations_DecideEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_DecideEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L287)

<a id="Vercel_AI_Operations_DecideEvent_Questions_"></a>
<a id="Vercel_AI_Operations_DecideEvent_Questions"></a>

### Questions


```csharp
public IReadOnlyDictionary<string, DecisionQuestion> Questions { get; }
```


Questions.

**Value:** [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L272)

<a id="Vercel_AI_Operations_DecideEvent_Reasoning_"></a>
<a id="Vercel_AI_Operations_DecideEvent_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; }
```


Reasoning effort requested for the decision.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L278)

<a id="Vercel_AI_Operations_DecideEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_DecideEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; }
```


Present only on telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L299)

<a id="Vercel_AI_Operations_DecideEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_DecideEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; }
```


Present only on telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L302)

<a id="Vercel_AI_Operations_DecideEvent_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_DecideEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; }
```


Runtime context. Telemetry receives the filtered copy.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L260)

<a id="Vercel_AI_Operations_DecideEvent_State_"></a>
<a id="Vercel_AI_Operations_DecideEvent_State"></a>

### State


```csharp
public DecisionState? State { get; }
```


The public state, as the caller passed it.

**Value:** [DecisionState](/api/Vercel.AI.Operations/DecisionState/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L269)

<a id="Vercel_AI_Operations_DecideEvent_Usage_"></a>
<a id="Vercel_AI_Operations_DecideEvent_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Usage, once the call has finished.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L293)

<a id="Vercel_AI_Operations_DecideEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_DecideEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning>? Warnings { get; }
```


Warnings, once the call has finished.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L296)
