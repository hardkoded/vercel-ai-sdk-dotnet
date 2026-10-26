---
title: "Decide"
description: "Decides typed questions. Maps to experimentaldecide."
editUrl: false
prev: false
next: false
sidebar:
  label: "Decide"
  hidden: true
---

<a id="Vercel_AI_Operations_Decide"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class Decide
```


Decides typed questions. Maps to `experimental_decide`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L496)

## Methods

<a id="Vercel_AI_Operations_Decide_DecideAsync_"></a>
<a id="Vercel_AI_Operations_Decide_DecideAsync_Vercel_AI_Operations_DecideRequest_System_Threading_CancellationToken_"></a>

### DecideAsync(DecideRequest, CancellationToken)


```csharp
public static Task<DecisionResult> DecideAsync(DecideRequest request, CancellationToken cancellationToken = default)
```


Decides every question in one model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [DecideRequest](/api/Vercel.AI.Operations/DecideRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DecisionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L501)

<a id="Vercel_AI_Operations_Decide_ValidateAnswers_"></a>
<a id="Vercel_AI_Operations_Decide_ValidateAnswers_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionQuestion__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__Vercel_AI_Operations_DecisionRounding_"></a>

### `ValidateAnswers(IReadOnlyDictionary<string, DecisionQuestion>, IReadOnlyDictionary<string, DecisionAnswer>?, DecisionRounding?)`


```csharp
public static void ValidateAnswers(IReadOnlyDictionary<string, DecisionQuestion> questions, IReadOnlyDictionary<string, DecisionAnswer>? answers, DecisionRounding? rounding)
```


Validates answers without rewriting provider numbers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questions` | [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `answers` | [IReadOnlyDictionary&lt;string, DecisionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `rounding` | [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L660)

<a id="Vercel_AI_Operations_Decide_ValidateInput_"></a>
<a id="Vercel_AI_Operations_Decide_ValidateInput_Vercel_AI_Operations_DecisionState_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionQuestion__"></a>

### `ValidateInput(DecisionState?, IReadOnlyDictionary<string, DecisionQuestion>?)`


```csharp
public static void ValidateInput(DecisionState? state, IReadOnlyDictionary<string, DecisionQuestion>? questions)
```


Validates state and questions before any model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `state` | [DecisionState](/api/Vercel.AI.Operations/DecisionState/) |  |
| `questions` | [IReadOnlyDictionary&lt;string, DecisionQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L601)
