---
title: "TakoDatasetColumn"
description: "A column of a TakoDataset."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoDatasetColumn"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoDatasetColumn"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoDatasetColumn
```


A column of a [TakoDataset](/api/Vercel.AI.Gateway/TakoDataset/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L659)

## Properties

<a id="Vercel_AI_Gateway_TakoDatasetColumn_Name_"></a>
<a id="Vercel_AI_Gateway_TakoDatasetColumn_Name"></a>

### Name


```csharp
[JsonPropertyName("name")]
public string Name { get; set; }
```


Column name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L662)

<a id="Vercel_AI_Gateway_TakoDatasetColumn_Type_"></a>
<a id="Vercel_AI_Gateway_TakoDatasetColumn_Type"></a>

### Type


```csharp
[JsonPropertyName("type")]
public string Type { get; set; }
```


`boolean`, `date`, `datetime`, `number` or `string`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L666)

<a id="Vercel_AI_Gateway_TakoDatasetColumn_Unit_"></a>
<a id="Vercel_AI_Gateway_TakoDatasetColumn_Unit"></a>

### Unit


```csharp
[JsonPropertyName("unit")]
public string? Unit { get; set; }
```


Unit of the values.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L670)
