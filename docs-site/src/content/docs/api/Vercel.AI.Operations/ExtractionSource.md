---
title: "ExtractionSource"
description: "Where in the state a value was found. Maps to ExtractionModelV4Source."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionSource"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionSource"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionSource
```


Where in the state a value was found. Maps to `ExtractionModelV4Source`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L51)

## Constructors

<a id="Vercel_AI_Operations_ExtractionSource__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionSource__ctor_System_String_System_Int32_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### ExtractionSource(string, int, int?, int?)


```csharp
public ExtractionSource(string type, int part, int? start = null, int? end = null)
```


Creates a source.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) | `text` or `file`. |
| `part` | [int](https://learn.microsoft.com/dotnet/api/system.int32) | Index of the part in the state. |
| `start` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Start offset in UTF-16 code units, inclusive. Text sources only. |
| `end` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | End offset in UTF-16 code units, exclusive. Text sources only. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L58)

## Properties

<a id="Vercel_AI_Operations_ExtractionSource_End_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_End"></a>

### End


```csharp
public int? End { get; }
```


End offset in UTF-16 code units, exclusive. Set for a text source.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L76)

<a id="Vercel_AI_Operations_ExtractionSource_Part_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_Part"></a>

### Part


```csharp
public int Part { get; }
```


Index of the text or file part in the state.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L70)

<a id="Vercel_AI_Operations_ExtractionSource_Start_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_Start"></a>

### Start


```csharp
public int? Start { get; }
```


Start offset in UTF-16 code units, inclusive. Set for a text source.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L73)

<a id="Vercel_AI_Operations_ExtractionSource_Type_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text` or `file`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L67)
