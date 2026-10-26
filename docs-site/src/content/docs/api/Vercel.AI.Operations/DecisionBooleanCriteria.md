---
title: "DecisionBooleanCriteria"
description: "Optional descriptions of the true and false outcomes of a boolean question. A side that is never set is left out of the wire request. A side set to null is s..."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionBooleanCriteria"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionBooleanCriteria"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionBooleanCriteria : DecisionCriteria
```


Optional descriptions of the true and false outcomes of a boolean question. A side that is never set is left out of the wire
request. A side set to null is sent as null.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionCriteria](/api/Vercel.AI.Operations/DecisionCriteria/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L148)

## Properties

<a id="Vercel_AI_Operations_DecisionBooleanCriteria_False_"></a>
<a id="Vercel_AI_Operations_DecisionBooleanCriteria_False"></a>

### False


```csharp
public DecisionInput? False { get; init; }
```


Description of the false outcome.

**Value:** [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L165)

<a id="Vercel_AI_Operations_DecisionBooleanCriteria_HasFalse_"></a>
<a id="Vercel_AI_Operations_DecisionBooleanCriteria_HasFalse"></a>

### HasFalse


```csharp
public bool HasFalse { get; }
```


True when [False](#Vercel_AI_Operations_DecisionBooleanCriteria_False) was set, including to null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L179)

<a id="Vercel_AI_Operations_DecisionBooleanCriteria_HasTrue_"></a>
<a id="Vercel_AI_Operations_DecisionBooleanCriteria_HasTrue"></a>

### HasTrue


```csharp
public bool HasTrue { get; }
```


True when [True](#Vercel_AI_Operations_DecisionBooleanCriteria_True) was set, including to null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L176)

<a id="Vercel_AI_Operations_DecisionBooleanCriteria_True_"></a>
<a id="Vercel_AI_Operations_DecisionBooleanCriteria_True"></a>

### True


```csharp
public DecisionInput? True { get; init; }
```


Description of the true outcome.

**Value:** [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L154)
