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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L61)

## Constructors

<a id="Vercel_AI_Operations_ExtractionSource__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionSource__ctor_System_String_System_Double_System_Nullable_System_Double__System_Nullable_System_Double__"></a>

### ExtractionSource(string, double, double?, double?)


```csharp
public ExtractionSource(string type, double part, double? start = null, double? end = null)
```


Creates a source.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) | `text` or `file`. |
| `part` | [double](https://learn.microsoft.com/dotnet/api/system.double) | Index of the part in the state. |
| `start` | [double?](https://learn.microsoft.com/dotnet/api/system.double) | Start offset in UTF-16 code units, inclusive. Text sources only. |
| `end` | [double?](https://learn.microsoft.com/dotnet/api/system.double) | End offset in UTF-16 code units, exclusive. Text sources only. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L68)

## Properties

<a id="Vercel_AI_Operations_ExtractionSource_End_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_End"></a>

### End


```csharp
public double? End { get; }
```


End offset in UTF-16 code units, exclusive. Set for a text source.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L86)

<a id="Vercel_AI_Operations_ExtractionSource_Part_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_Part"></a>

### Part


```csharp
public double Part { get; }
```


Index of the text or file part in the state.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L80)

<a id="Vercel_AI_Operations_ExtractionSource_Start_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_Start"></a>

### Start


```csharp
public double? Start { get; }
```


Start offset in UTF-16 code units, inclusive. Set for a text source.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L83)

<a id="Vercel_AI_Operations_ExtractionSource_Type_"></a>
<a id="Vercel_AI_Operations_ExtractionSource_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text` or `file`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L77)
