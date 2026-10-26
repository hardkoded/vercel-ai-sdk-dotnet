---
title: "EvaluationRounding"
description: "Deprecated alias of DecisionRounding."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationRounding"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluationRounding"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionRounding instead.")]
public sealed class EvaluationRounding : DecisionRounding
```


Deprecated alias of [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionRounding](/api/Vercel.AI.Operations/DecisionRounding/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L33)

## Constructors

<a id="Vercel_AI_Operations_EvaluationRounding__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluationRounding__ctor_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### EvaluationRounding(int?, int?)


```csharp
public EvaluationRounding(int? probabilityDecimals = null, int? scoreDecimals = null)
```


Creates rounding limits.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `probabilityDecimals` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `scoreDecimals` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L37)
