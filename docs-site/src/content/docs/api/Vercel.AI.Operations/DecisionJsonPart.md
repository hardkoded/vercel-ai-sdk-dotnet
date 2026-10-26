---
title: "DecisionJsonPart"
description: "A JSON part of decision state."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionJsonPart"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionJsonPart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionJsonPart : DecisionStatePart
```


A JSON part of decision state.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionStatePart](/api/Vercel.AI.Operations/DecisionStatePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L64)

## Constructors

<a id="Vercel_AI_Operations_DecisionJsonPart__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionJsonPart__ctor_System_Text_Json_JsonElement_"></a>

### DecisionJsonPart(JsonElement)


```csharp
public DecisionJsonPart(JsonElement value)
```


Creates a JSON part. Any JSON value is valid.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L67)

## Properties

<a id="Vercel_AI_Operations_DecisionJsonPart_Value_"></a>
<a id="Vercel_AI_Operations_DecisionJsonPart_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


The JSON value.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionState.cs#L79)
