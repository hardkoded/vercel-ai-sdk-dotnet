---
title: "EmbedEventValue<TOne, TMany>"
description: "A single item for embed or a list for embedMany, as carried by embed events."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedEventValue<TOne, TMany>"
  hidden: true
---

<a id="Vercel_AI_EmbedEventValue_2"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class EmbedEventValue<TOne, TMany>
```


A single item for `embed` or a list for `embedMany`, as carried by embed events.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOne` | The single item type. |
| `TMany` | The list type. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L42)

## Properties

<a id="Vercel_AI_EmbedEventValue_2_IsMany_"></a>
<a id="Vercel_AI_EmbedEventValue_2_IsMany"></a>

### IsMany


```csharp
public bool IsMany { get; }
```


True for `embedMany`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L52)

<a id="Vercel_AI_EmbedEventValue_2_Many_"></a>
<a id="Vercel_AI_EmbedEventValue_2_Many"></a>

### Many


```csharp
public TMany? Many { get; }
```


The list. Default when `IsMany` is false.

**Value:** `TMany`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L58)

<a id="Vercel_AI_EmbedEventValue_2_One_"></a>
<a id="Vercel_AI_EmbedEventValue_2_One"></a>

### One


```csharp
public TOne? One { get; }
```


The single item. Default when `IsMany` is true.

**Value:** `TOne`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L55)

## Methods

<a id="Vercel_AI_EmbedEventValue_2_FromMany_"></a>
<a id="Vercel_AI_EmbedEventValue_2_FromMany__1_"></a>

### FromMany(TMany)


```csharp
public static EmbedEventValue<TOne, TMany> FromMany(TMany values)
```


Wraps the `embedMany` list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | `TMany` |  |

**Returns:** [EmbedEventValue&lt;TOne, TMany&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L67)

<a id="Vercel_AI_EmbedEventValue_2_FromOne_"></a>
<a id="Vercel_AI_EmbedEventValue_2_FromOne__0_"></a>

### FromOne(TOne)


```csharp
public static EmbedEventValue<TOne, TMany> FromOne(TOne value)
```


Wraps the single `embed` item.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `TOne` |  |

**Returns:** [EmbedEventValue&lt;TOne, TMany&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationUnions.cs#L61)
