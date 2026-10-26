---
title: "EvaluationAnswer"
description: "Deprecated alias of DecisionAnswer."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationAnswer"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluationAnswer"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionAnswer instead.")]
public sealed class EvaluationAnswer : DecisionAnswer
```


Deprecated alias of [DecisionAnswer](/api/Vercel.AI.Operations/DecisionAnswer/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionAnswer](/api/Vercel.AI.Operations/DecisionAnswer/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L22)

## Constructors

<a id="Vercel_AI_Operations_EvaluationAnswer__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluationAnswer__ctor_System_String_System_String_System_Nullable_System_Double__System_Nullable_System_Double__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Double__"></a>

### `EvaluationAnswer(string, string?, double?, double?, IReadOnlyDictionary<string, double>?)`


```csharp
public EvaluationAnswer(string type, string? choice = null, double? score = null, double? probability = null, IReadOnlyDictionary<string, double>? probabilities = null)
```


Creates an answer.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `choice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `score` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `probability` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `probabilities` | [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L26)
