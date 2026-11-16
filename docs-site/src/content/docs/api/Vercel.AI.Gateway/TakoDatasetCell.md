---
title: "TakoDatasetCell"
description: "A dataset cell: a boolean, number, string or null. Maps to TakoDatasetCell."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoDatasetCell"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoDatasetCell"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
[JsonConverter(typeof(TakoDatasetCellConverter))]
public readonly struct TakoDatasetCell
```


A dataset cell: a boolean, number, string or null. Maps to `TakoDatasetCell`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L593)

## Properties

<a id="Vercel_AI_Gateway_TakoDatasetCell_Boolean_"></a>
<a id="Vercel_AI_Gateway_TakoDatasetCell_Boolean"></a>

### Boolean


```csharp
public bool? Boolean { get; }
```


The value when the cell is a boolean.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L604)

<a id="Vercel_AI_Gateway_TakoDatasetCell_IsNull_"></a>
<a id="Vercel_AI_Gateway_TakoDatasetCell_IsNull"></a>

### IsNull


```csharp
public bool IsNull { get; }
```


True when the cell is null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L613)

<a id="Vercel_AI_Gateway_TakoDatasetCell_Number_"></a>
<a id="Vercel_AI_Gateway_TakoDatasetCell_Number"></a>

### Number


```csharp
public double? Number { get; }
```


The value when the cell is a number.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L607)

<a id="Vercel_AI_Gateway_TakoDatasetCell_String_"></a>
<a id="Vercel_AI_Gateway_TakoDatasetCell_String"></a>

### String


```csharp
public string? String { get; }
```


The value when the cell is a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L610)
