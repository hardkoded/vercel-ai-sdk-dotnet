---
title: "ExtractionField"
description: "A typed value to extract from the shared state. Maps to ExtractionModelV4Field."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionField"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionField"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionField
```


A typed value to extract from the shared state. Maps to `ExtractionModelV4Field`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L11)

## Constructors

<a id="Vercel_AI_Operations_ExtractionField__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionField__ctor_System_String_Vercel_AI_Operations_DecisionInput_"></a>

### ExtractionField(string, DecisionInput?)


```csharp
public ExtractionField(string type, DecisionInput? instructions = null)
```


Creates a field.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) | `text`, `number`, `date`, `time`, `datetime`, `duration`, or `list`. |
| `instructions` | [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/) | Instructions as a string, JSON object, or JSON array. Optional. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L19)

## Fields

<a id="Vercel_AI_Operations_ExtractionField_Types"></a>

### Types


```csharp
public static readonly IReadOnlyList<string> Types
```


Field types an extraction model can support.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L14)

## Properties

<a id="Vercel_AI_Operations_ExtractionField_Decimals_"></a>
<a id="Vercel_AI_Operations_ExtractionField_Decimals"></a>

### Decimals


```csharp
public int? Decimals { get; init; }
```


Maximum decimal places, an integer from 0 to 6. Applies to `number`, `duration`, and `list` of numbers.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L38)

<a id="Vercel_AI_Operations_ExtractionField_DurationUnits_"></a>
<a id="Vercel_AI_Operations_ExtractionField_DurationUnits"></a>

### DurationUnits


```csharp
public static IReadOnlyList<string> DurationUnits { get; }
```


Valid duration units. Maps to `ExtractionModelV4DurationUnit`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L44)

<a id="Vercel_AI_Operations_ExtractionField_Instructions_"></a>
<a id="Vercel_AI_Operations_ExtractionField_Instructions"></a>

### Instructions


```csharp
public DecisionInput? Instructions { get; }
```


Instructions. A string, JSON object, or JSON array.

**Value:** [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L29)

<a id="Vercel_AI_Operations_ExtractionField_Integer_"></a>
<a id="Vercel_AI_Operations_ExtractionField_Integer"></a>

### Integer


```csharp
public bool? Integer { get; init; }
```


Require an integer value. Applies to `number`, `duration`, and `list` of numbers.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L35)

<a id="Vercel_AI_Operations_ExtractionField_Items_"></a>
<a id="Vercel_AI_Operations_ExtractionField_Items"></a>

### Items


```csharp
public string? Items { get; init; }
```


Item type of a `list` field: `text`, `number`, or `date`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L41)

<a id="Vercel_AI_Operations_ExtractionField_ListItems_"></a>
<a id="Vercel_AI_Operations_ExtractionField_ListItems"></a>

### ListItems


```csharp
public static IReadOnlyList<string> ListItems { get; }
```


Valid item types of a list field.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L47)

<a id="Vercel_AI_Operations_ExtractionField_Type_"></a>
<a id="Vercel_AI_Operations_ExtractionField_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text`, `number`, `date`, `time`, `datetime`, `duration`, or `list`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L26)

<a id="Vercel_AI_Operations_ExtractionField_Unit_"></a>
<a id="Vercel_AI_Operations_ExtractionField_Unit"></a>

### Unit


```csharp
public string? Unit { get; init; }
```


Unit the value should be expressed in. A free string for `number`. For `duration` it is one of [DurationUnits](#Vercel_AI_Operations_ExtractionField_DurationUnits), default `seconds`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L32)
