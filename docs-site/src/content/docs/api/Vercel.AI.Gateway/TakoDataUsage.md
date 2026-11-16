---
title: "TakoDataUsage"
description: "Data cost of a Tako search. Maps to TakoSearchResponse['usage']['data']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoDataUsage"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoDataUsage"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoDataUsage
```


Data cost of a Tako search. Maps to `TakoSearchResponse['usage']['data']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1079)

## Properties

<a id="Vercel_AI_Gateway_TakoDataUsage_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoDataUsage_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1094)

<a id="Vercel_AI_Gateway_TakoDataUsage_CostUsd_"></a>
<a id="Vercel_AI_Gateway_TakoDataUsage_CostUsd"></a>

### CostUsd


```csharp
[JsonPropertyName("cost_usd")]
public double CostUsd { get; set; }
```


Cost in USD.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1082)

<a id="Vercel_AI_Gateway_TakoDataUsage_DataPoints_"></a>
<a id="Vercel_AI_Gateway_TakoDataUsage_DataPoints"></a>

### DataPoints


```csharp
[JsonPropertyName("data_points")]
public double? DataPoints { get; set; }
```


Data points.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1090)

<a id="Vercel_AI_Gateway_TakoDataUsage_Datasets_"></a>
<a id="Vercel_AI_Gateway_TakoDataUsage_Datasets"></a>

### Datasets


```csharp
[JsonPropertyName("datasets")]
public double Datasets { get; set; }
```


Datasets.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1086)
