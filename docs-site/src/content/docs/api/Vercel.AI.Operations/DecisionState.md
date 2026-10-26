---
title: "DecisionState"
description: "Shared state of a decision: text, a JSON object, or ordered parts. Maps to DecisionState. A JSON array is not valid state. Wrap it in an object or in a Decis..."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionState"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionState"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionState
```


Shared state of a decision: text, a JSON object, or ordered parts. Maps to `DecisionState`.
A JSON array is not valid state. Wrap it in an object or in a [DecisionJsonPart](/api/Vercel.AI.Operations/DecisionJsonPart/).
`CancellationToken)` maps to `prepareDecisionState`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L151)

## Properties

<a id="Vercel_AI_Operations_DecisionState_Json_"></a>
<a id="Vercel_AI_Operations_DecisionState_Json"></a>

### Json


```csharp
public JsonElement? Json { get; }
```


The JSON object, when the state is an object.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L164)

<a id="Vercel_AI_Operations_DecisionState_Parts_"></a>
<a id="Vercel_AI_Operations_DecisionState_Parts"></a>

### Parts


```csharp
public IReadOnlyList<DecisionStatePart>? Parts { get; }
```


The parts, when the state is a list of parts.

**Value:** [IReadOnlyList&lt;DecisionStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L167)

<a id="Vercel_AI_Operations_DecisionState_Text_"></a>
<a id="Vercel_AI_Operations_DecisionState_Text"></a>

### Text


```csharp
public string? Text { get; }
```


The text, when the state is a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L161)

## Methods

<a id="Vercel_AI_Operations_DecisionState_From_"></a>
<a id="Vercel_AI_Operations_DecisionState_From_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionStatePart__"></a>

### `From(IReadOnlyList<DecisionStatePart>)`


```csharp
public static DecisionState From(IReadOnlyList<DecisionStatePart> parts)
```


Creates state from ordered parts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parts` | [IReadOnlyList&lt;DecisionStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [DecisionState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L187)

<a id="Vercel_AI_Operations_DecisionState_From_System_String_"></a>

### From(string)


```csharp
public static DecisionState From(string text)
```


Creates state from text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L170)

<a id="Vercel_AI_Operations_DecisionState_FromJson_"></a>
<a id="Vercel_AI_Operations_DecisionState_FromJson_System_Text_Json_JsonElement_"></a>

### FromJson(JsonElement)


```csharp
public static DecisionState FromJson(JsonElement value)
```


Creates state from a JSON object. Any other JSON value throws [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [DecisionState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L176)

<a id="Vercel_AI_Operations_DecisionState_PrepareDecisionStateAsync_"></a>
<a id="Vercel_AI_Operations_DecisionState_PrepareDecisionStateAsync_Vercel_AI_Operations_DecisionState_System_Threading_CancellationToken_"></a>

### PrepareDecisionStateAsync(DecisionState, CancellationToken)


```csharp
public static Task<IReadOnlyList<DecisionModelStatePart>> PrepareDecisionStateAsync(DecisionState state, CancellationToken cancellationToken = default)
```


Turns public state into the list of parts the model receives.
URL files are downloaded. File media types are resolved to a full type.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `state` | [DecisionState](./) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;DecisionModelStatePart&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L210)

## Operators

<a id="Vercel_AI_Operations_DecisionState_op_Implicit_"></a>
<a id="Vercel_AI_Operations_DecisionState_op_Implicit_System_String__Vercel_AI_Operations_DecisionState"></a>

### implicit operator DecisionState(string)


```csharp
public static implicit operator DecisionState(string text)
```


Converts text to state.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L193)

<a id="Vercel_AI_Operations_DecisionState_op_Implicit_Vercel_AI_Operations_DecisionStatePart____Vercel_AI_Operations_DecisionState"></a>

### implicit operator DecisionState(DecisionStatePart[])


```csharp
public static implicit operator DecisionState(DecisionStatePart[] parts)
```


Converts an array of parts to state.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parts` | `DecisionStatePart\[\]` |  |

**Returns:** [DecisionState](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L199)
