---
title: "DecisionModelStatePart"
description: "A part of decision state as the model receives it. Maps to DecisionModelV4StatePart."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionModelStatePart"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionModelStatePart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public abstract class DecisionModelStatePart
```


A part of decision state as the model receives it. Maps to `DecisionModelV4StatePart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [DecisionModelFilePart](/api/Vercel.AI.Operations/DecisionModelFilePart/), [DecisionModelJsonPart](/api/Vercel.AI.Operations/DecisionModelJsonPart/), [DecisionModelTextPart](/api/Vercel.AI.Operations/DecisionModelTextPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L83)

## Properties

<a id="Vercel_AI_Operations_DecisionModelStatePart_Type_"></a>
<a id="Vercel_AI_Operations_DecisionModelStatePart_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text`, `file`, or `json`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L91)
