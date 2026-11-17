---
title: "TakoUsage"
description: "Usage of a Tako search. Maps to TakoSearchResponse['usage']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoUsage"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoUsage"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoUsage
```


Usage of a Tako search. Maps to `TakoSearchResponse['usage']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1125)

## Properties

<a id="Vercel_AI_Gateway_TakoUsage_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoUsage_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1144)

<a id="Vercel_AI_Gateway_TakoUsage_Compute_"></a>
<a id="Vercel_AI_Gateway_TakoUsage_Compute"></a>

### Compute


```csharp
[JsonPropertyName("compute")]
public TakoCostUsage? Compute { get; set; }
```


Compute cost.

**Value:** [TakoCostUsage](/api/Vercel.AI.Gateway/TakoCostUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1132)

<a id="Vercel_AI_Gateway_TakoUsage_Data_"></a>
<a id="Vercel_AI_Gateway_TakoUsage_Data"></a>

### Data


```csharp
[JsonPropertyName("data")]
public TakoDataUsage? Data { get; set; }
```


Data cost.

**Value:** [TakoDataUsage](/api/Vercel.AI.Gateway/TakoDataUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1136)

<a id="Vercel_AI_Gateway_TakoUsage_TotalCostUsd_"></a>
<a id="Vercel_AI_Gateway_TakoUsage_TotalCostUsd"></a>

### TotalCostUsd


```csharp
[JsonPropertyName("total_cost_usd")]
public double TotalCostUsd { get; set; }
```


Total cost in USD.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1128)

<a id="Vercel_AI_Gateway_TakoUsage_VariableCost_"></a>
<a id="Vercel_AI_Gateway_TakoUsage_VariableCost"></a>

### VariableCost


```csharp
[JsonPropertyName("variable_cost")]
public TakoCostUsage? VariableCost { get; set; }
```


Variable-cost charge for the call.

**Value:** [TakoCostUsage](/api/Vercel.AI.Gateway/TakoCostUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1140)
