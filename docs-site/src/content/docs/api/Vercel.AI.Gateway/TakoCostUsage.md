---
title: "TakoCostUsage"
description: "Cost of one part of a Tako search. Maps to TakoSearchResponse['usage']['compute'] and ['variablecost']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoCostUsage"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoCostUsage"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoCostUsage
```


Cost of one part of a Tako search. Maps to `TakoSearchResponse['usage']['compute']` and `['variable_cost']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1067)

## Properties

<a id="Vercel_AI_Gateway_TakoCostUsage_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoCostUsage_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1074)

<a id="Vercel_AI_Gateway_TakoCostUsage_CostUsd_"></a>
<a id="Vercel_AI_Gateway_TakoCostUsage_CostUsd"></a>

### CostUsd


```csharp
[JsonPropertyName("cost_usd")]
public double CostUsd { get; set; }
```


Cost in USD.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1070)
