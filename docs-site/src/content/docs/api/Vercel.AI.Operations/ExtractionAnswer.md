---
title: "ExtractionAnswer"
description: "One model answer. Maps to ExtractionModelV4Answer. The value member is split by type: Text for text, date, time, and datetime, Number for number and duration..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionAnswer"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionAnswer"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionAnswer
```


One model answer. Maps to `ExtractionModelV4Answer`. The `value` member is split by type: [Text](#Vercel_AI_Operations_ExtractionAnswer_Text) for
`text`, `date`, `time`, and `datetime`, [Number](#Vercel_AI_Operations_ExtractionAnswer_Number) for `number` and `duration`, and
[TextItems](#Vercel_AI_Operations_ExtractionAnswer_TextItems) or [NumberItems](#Vercel_AI_Operations_ExtractionAnswer_NumberItems) for `list`. All are null when the state has no value for the field.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L101)

## Constructors

<a id="Vercel_AI_Operations_ExtractionAnswer__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer__ctor_System_String_System_String_System_Nullable_System_Double__System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_Double__System_String_System_Nullable_System_Double__System_Nullable_System_Double__System_Nullable_System_Double__Vercel_AI_Operations_ExtractionSource_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ExtractionItem__"></a>

### `ExtractionAnswer(string, string?, double?, IReadOnlyList<string>?, IReadOnlyList<double>?, string?, double?, double?, double?, ExtractionSource?, IReadOnlyList<ExtractionItem>?)`


```csharp
public ExtractionAnswer(string type, string? text = null, double? number = null, IReadOnlyList<string>? textItems = null, IReadOnlyList<double>? numberItems = null, string? guessText = null, double? guessNumber = null, double? pNull = null, double? confidence = null, ExtractionSource? source = null, IReadOnlyList<ExtractionItem>? items = null)
```


Creates an answer.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `number` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `textItems` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `numberItems` | [IReadOnlyList&lt;double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `guessText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `guessNumber` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `pNull` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `confidence` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `source` | [ExtractionSource](/api/Vercel.AI.Operations/ExtractionSource/) |  |
| `items` | [IReadOnlyList&lt;ExtractionItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L104)

## Properties

<a id="Vercel_AI_Operations_ExtractionAnswer_Confidence_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_Confidence"></a>

### Confidence


```csharp
public double? Confidence { get; }
```


Confidence in [0, 1] that the value is correct.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L155)

<a id="Vercel_AI_Operations_ExtractionAnswer_GuessNumber_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_GuessNumber"></a>

### GuessNumber


```csharp
public double? GuessNumber { get; }
```


Most likely value for a `number` or `duration` field, also when the value is null.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L149)

<a id="Vercel_AI_Operations_ExtractionAnswer_GuessText_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_GuessText"></a>

### GuessText


```csharp
public string? GuessText { get; }
```


Most likely value for a text-like field, also when the value is null.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L146)

<a id="Vercel_AI_Operations_ExtractionAnswer_Items_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_Items"></a>

### Items


```csharp
public IReadOnlyList<ExtractionItem>? Items { get; }
```


One entry per list item, when available. Only list answers use it.

**Value:** [IReadOnlyList&lt;ExtractionItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L161)

<a id="Vercel_AI_Operations_ExtractionAnswer_Number_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_Number"></a>

### Number


```csharp
public double? Number { get; }
```


Value of a `number` or `duration` field. Null when the state has none.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L137)

<a id="Vercel_AI_Operations_ExtractionAnswer_NumberItems_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_NumberItems"></a>

### NumberItems


```csharp
public IReadOnlyList<double>? NumberItems { get; }
```


Value of a `list` field of number items. Null when the state has none.

**Value:** [IReadOnlyList&lt;double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L143)

<a id="Vercel_AI_Operations_ExtractionAnswer_PNull_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_PNull"></a>

### PNull


```csharp
public double? PNull { get; }
```


Probability in [0, 1] that the state does not contain a value.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L152)

<a id="Vercel_AI_Operations_ExtractionAnswer_Source_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_Source"></a>

### Source


```csharp
public ExtractionSource? Source { get; }
```


Where the value was found. Not used by list answers.

**Value:** [ExtractionSource](/api/Vercel.AI.Operations/ExtractionSource/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L158)

<a id="Vercel_AI_Operations_ExtractionAnswer_Text_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_Text"></a>

### Text


```csharp
public string? Text { get; }
```


Value of a `text`, `date`, `time`, or `datetime` field. Null when the state has none.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L134)

<a id="Vercel_AI_Operations_ExtractionAnswer_TextItems_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_TextItems"></a>

### TextItems


```csharp
public IReadOnlyList<string>? TextItems { get; }
```


Value of a `list` field of text or date items. Null when the state has none.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L140)

<a id="Vercel_AI_Operations_ExtractionAnswer_Type_"></a>
<a id="Vercel_AI_Operations_ExtractionAnswer_Type"></a>

### Type


```csharp
public string Type { get; }
```


Answer type. It is the field type, or `refusal` when the model declined the field.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L131)
