---
title: "EvaluateModelEvent"
description: "Deprecated alias of DecisionModelEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluateModelEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluateModelEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionModelEvent instead.")]
public sealed class EvaluateModelEvent : DecisionModelEvent
```


Deprecated alias of [DecisionModelEvent](/api/Vercel.AI.Operations/DecisionModelEvent/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionModelEvent](/api/Vercel.AI.Operations/DecisionModelEvent/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L154)

## Constructors

<a id="Vercel_AI_Operations_EvaluateModelEvent__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluateModelEvent__ctor_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionModelStatePart__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionQuestion__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionAnswer__Vercel_AI_Operations_OperationUsage_System_Nullable_System_Boolean__System_Nullable_System_Boolean__System_String_"></a>

### `EvaluateModelEvent(string, string, string, string, IReadOnlyList<DecisionModelStatePart>?, IReadOnlyDictionary<string, DecisionQuestion>, IReadOnlyDictionary<string, DecisionAnswer>?, OperationUsage?, bool?, bool?, string?)`


```csharp
public EvaluateModelEvent(string callId, string operationId, string provider, string modelId, IReadOnlyList<DecisionModelStatePart>? state, IReadOnlyDictionary<string, DecisionQuestion> questions, IReadOnlyDictionary<string, DecisionAnswer>? answers = null, OperationUsage? usage = null, bool? recordInputs = null, bool? recordOutputs = null, string? functionId = null)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L158)
