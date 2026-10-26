---
title: "DecisionModelJsonPart"
description: "A JSON part the model receives."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionModelJsonPart"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionModelJsonPart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionModelJsonPart : DecisionModelStatePart
```


A JSON part the model receives.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionModelStatePart](/api/Vercel.AI.Operations/DecisionModelStatePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L133)

## Constructors

<a id="Vercel_AI_Operations_DecisionModelJsonPart__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionModelJsonPart__ctor_System_Text_Json_JsonElement_"></a>

### DecisionModelJsonPart(JsonElement)


```csharp
public DecisionModelJsonPart(JsonElement value)
```


Creates a JSON part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L136)

## Properties

<a id="Vercel_AI_Operations_DecisionModelJsonPart_Value_"></a>
<a id="Vercel_AI_Operations_DecisionModelJsonPart_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


The JSON value.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L143)
