---
title: "DecisionStatePart"
description: "A part of decision state as the caller passes it. Maps to DecisionStatePart."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionStatePart"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionStatePart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public abstract class DecisionStatePart
```


A part of decision state as the caller passes it. Maps to `DecisionStatePart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [DecisionFilePart](/api/Vercel.AI.Operations/DecisionFilePart/), [DecisionJsonPart](/api/Vercel.AI.Operations/DecisionJsonPart/), [DecisionTextPart](/api/Vercel.AI.Operations/DecisionTextPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L14)

## Properties

<a id="Vercel_AI_Operations_DecisionStatePart_Type_"></a>
<a id="Vercel_AI_Operations_DecisionStatePart_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text`, `file`, or `json`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L22)
