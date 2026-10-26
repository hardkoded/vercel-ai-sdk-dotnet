---
title: "OpenAIFileSearchFilter"
description: "A file search filter: a comparison (Key, Type, one value) or a compound (and/or over Filters)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIFileSearchFilter"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIFileSearchFilter
```


A file search filter: a comparison ([Key](#Vercel_AI_OpenAI_OpenAIFileSearchFilter_Key), [Type](#Vercel_AI_OpenAI_OpenAIFileSearchFilter_Type), one value) or a compound (`and`/`or` over [Filters](#Vercel_AI_OpenAI_OpenAIFileSearchFilter_Filters)).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L87)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_BooleanValue_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_BooleanValue"></a>

### BooleanValue


```csharp
public bool? BooleanValue { get; set; }
```


Boolean value of a comparison filter.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L102)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_Filters_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_Filters"></a>

### Filters


```csharp
public IReadOnlyList<OpenAIFileSearchFilter>? Filters { get; set; }
```


Nested filters of a compound filter.

**Value:** [IReadOnlyList&lt;OpenAIFileSearchFilter&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L108)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_Key_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_Key"></a>

### Key


```csharp
public string? Key { get; set; }
```


Attribute key of a comparison filter.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L93)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_NumberValue_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_NumberValue"></a>

### NumberValue


```csharp
public double? NumberValue { get; set; }
```


Number value of a comparison filter.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L99)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_StringValue_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_StringValue"></a>

### StringValue


```csharp
public string? StringValue { get; set; }
```


String value of a comparison filter.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L96)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_StringValues_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_StringValues"></a>

### StringValues


```csharp
public IReadOnlyList<string>? StringValues { get; set; }
```


String values of an `in`/`nin` comparison filter.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L105)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchFilter_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


Operator: `eq`, `ne`, `gt`, `gte`, `lt`, `lte`, `in`, `nin`, `and`, or `or`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L90)
