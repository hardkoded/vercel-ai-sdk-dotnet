---
title: "TakoExportPricing"
description: "Pricing of exporting a card's rows. Maps to TakoResultContent['exportpricing']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoExportPricing"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoExportPricing"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoExportPricing
```


Pricing of exporting a card's rows. Maps to `TakoResultContent['export_pricing']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L723)

## Properties

<a id="Vercel_AI_Gateway_TakoExportPricing_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoExportPricing_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L750)

<a id="Vercel_AI_Gateway_TakoExportPricing_BaselineUsd_"></a>
<a id="Vercel_AI_Gateway_TakoExportPricing_BaselineUsd"></a>

### BaselineUsd


```csharp
[JsonPropertyName("baseline_usd")]
public double BaselineUsd { get; set; }
```


Baseline charge in USD.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L726)

<a id="Vercel_AI_Gateway_TakoExportPricing_FreeRows_"></a>
<a id="Vercel_AI_Gateway_TakoExportPricing_FreeRows"></a>

### FreeRows


```csharp
[JsonPropertyName("free_rows")]
public double? FreeRows { get; set; }
```


Deprecated. Always 0, and Tako will stop sending it.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L730)

<a id="Vercel_AI_Gateway_TakoExportPricing_MaxRowsCeiling_"></a>
<a id="Vercel_AI_Gateway_TakoExportPricing_MaxRowsCeiling"></a>

### MaxRowsCeiling


```csharp
[JsonPropertyName("max_rows_ceiling")]
public double MaxRowsCeiling { get; set; }
```


Row ceiling.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L734)

<a id="Vercel_AI_Gateway_TakoExportPricing_PremiumRowCpmUsd_"></a>
<a id="Vercel_AI_Gateway_TakoExportPricing_PremiumRowCpmUsd"></a>

### PremiumRowCpmUsd


```csharp
[JsonPropertyName("premium_row_cpm_usd")]
public IDictionary<string, double>? PremiumRowCpmUsd { get; set; }
```


USD per 1,000 premium records by class, added to [RowCpmUsd](#Vercel_AI_Gateway_TakoExportPricing_RowCpmUsd) when premium.

**Value:** [IDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L742)

<a id="Vercel_AI_Gateway_TakoExportPricing_RowCpmUsd_"></a>
<a id="Vercel_AI_Gateway_TakoExportPricing_RowCpmUsd"></a>

### RowCpmUsd


```csharp
[JsonPropertyName("row_cpm_usd")]
public double RowCpmUsd { get; set; }
```


USD per 1,000 rows.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L738)

<a id="Vercel_AI_Gateway_TakoExportPricing_VariableCostMaxUsd_"></a>
<a id="Vercel_AI_Gateway_TakoExportPricing_VariableCostMaxUsd"></a>

### VariableCostMaxUsd


```csharp
[JsonPropertyName("variable_cost_max_usd")]
public double? VariableCostMaxUsd { get; set; }
```


Upper bound on this card's variable-cost charge. Set only for per-unit-priced sources.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L746)
