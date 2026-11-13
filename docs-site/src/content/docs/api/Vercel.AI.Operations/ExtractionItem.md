---
title: "ExtractionItem"
description: "One entry of a list answer. Maps to the items member of a list ExtractionModelV4Answer."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionItem"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionItem"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionItem
```


One entry of a list answer. Maps to the `items` member of a list `ExtractionModelV4Answer`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L80)

## Constructors

<a id="Vercel_AI_Operations_ExtractionItem__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionItem__ctor_System_Nullable_System_Double__Vercel_AI_Operations_ExtractionSource_"></a>

### ExtractionItem(double?, ExtractionSource?)


```csharp
public ExtractionItem(double? confidence = null, ExtractionSource? source = null)
```


Creates an item entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `confidence` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `source` | [ExtractionSource](/api/Vercel.AI.Operations/ExtractionSource/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L83)

## Properties

<a id="Vercel_AI_Operations_ExtractionItem_Confidence_"></a>
<a id="Vercel_AI_Operations_ExtractionItem_Confidence"></a>

### Confidence


```csharp
public double? Confidence { get; }
```


Confidence in [0, 1] that the item is correct.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L90)

<a id="Vercel_AI_Operations_ExtractionItem_Source_"></a>
<a id="Vercel_AI_Operations_ExtractionItem_Source"></a>

### Source


```csharp
public ExtractionSource? Source { get; }
```


Where the item was found.

**Value:** [ExtractionSource](/api/Vercel.AI.Operations/ExtractionSource/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L93)
