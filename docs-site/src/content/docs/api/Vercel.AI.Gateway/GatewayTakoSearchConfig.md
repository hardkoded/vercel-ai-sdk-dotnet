---
title: "GatewayTakoSearchConfig"
description: "Developer-only configuration of the Gateway Tako search tool. These options are not part of the tool input, so the model cannot see or set them. The Gateway..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayTakoSearchConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayTakoSearchConfig
```


Developer-only configuration of the Gateway Tako search tool. These options are not part of the tool input,
so the model cannot see or set them. The Gateway applies the defaults and validates them. Maps to `TakoSearchConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L318)

## Properties

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_CountryCode_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_CountryCode"></a>

### CountryCode


```csharp
public string? CountryCode { get; set; }
```


ISO 3166-1 alpha-2 country code. Maps to `countryCode`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L330)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Effort_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Effort"></a>

### Effort


```csharp
public string? Effort { get; set; }
```


Search effort: `deep`, `fast` or `instant`. `fast` is the default balanced option. Maps to `effort`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L321)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_ExtraBody_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_ExtraBody"></a>

### ExtraBody


```csharp
public IDictionary<string, JsonNode?>? ExtraBody { get; set; }
```


Extra Tako request fields in snake_case, deep-merged into the body last. Can't set fields this tool models;
Tako rejects unknown keys (`api_error`). Maps to `extraBody`.

**Value:** [IDictionary&lt;string, JsonNode&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L368)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_IncludeRelated_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_IncludeRelated"></a>

### IncludeRelated


```csharp
public int? IncludeRelated { get; set; }
```


Maximum related search suggestions to include (1-20). Maps to `includeRelated`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L342)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Locale_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Locale"></a>

### Locale


```csharp
public string? Locale { get; set; }
```


BCP-47 locale. Maps to `locale`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L333)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Location_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Location"></a>

### Location


```csharp
public TakoLocation? Location { get; set; }
```


End-user coordinates for localized results. Maps to `location`.

**Value:** [TakoLocation](/api/Vercel.AI.Gateway/TakoLocation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L327)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_MaxVariableCostUsd_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_MaxVariableCostUsd"></a>

### MaxVariableCostUsd


```csharp
public double? MaxVariableCostUsd { get; set; }
```


Most one call can be charged in variable cost, in USD. The Gateway defaults to 0.25. Cards that don't fit come back
as placeholders with reason `over_cap`. Can't be combined with `VariableCost = false`. To remove the cap
(`maxVariableCostUsd: null`), set [RemoveMaxVariableCostCap](#Vercel_AI_Gateway_GatewayTakoSearchConfig_RemoveMaxVariableCostCap) instead. Maps to `maxVariableCostUsd`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L356)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_OutputSettings_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_OutputSettings"></a>

### OutputSettings


```csharp
public TakoOutputSettings? OutputSettings { get; set; }
```


Settings that control card rendering. Maps to `outputSettings`.

**Value:** [TakoOutputSettings](/api/Vercel.AI.Gateway/TakoOutputSettings/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L339)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_RemoveMaxVariableCostCap_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_RemoveMaxVariableCostCap"></a>

### RemoveMaxVariableCostCap


```csharp
public bool RemoveMaxVariableCostCap { get; set; }
```


Removes the variable-cost cap, which sends `maxVariableCostUsd: null`. Takes precedence over
[MaxVariableCostUsd](#Vercel_AI_Gateway_GatewayTakoSearchConfig_MaxVariableCostUsd).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L362)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Sources_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Sources"></a>

### Sources


```csharp
public TakoSearchSourcesConfig? Sources { get; set; }
```


Sources to search. Omit to search both curated data and the web. Maps to `sources`.

**Value:** [TakoSearchSourcesConfig](/api/Vercel.AI.Gateway/TakoSearchSourcesConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L324)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Timezone_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_Timezone"></a>

### Timezone


```csharp
public string? Timezone { get; set; }
```


IANA timezone. Maps to `timezone`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L336)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_VariableCost_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_VariableCost"></a>

### VariableCost


```csharp
public bool? VariableCost { get; set; }
```


Serve per-unit-priced data, such as web traffic and SEO. Each such card adds a charge, capped by
[MaxVariableCostUsd](#Vercel_AI_Gateway_GatewayTakoSearchConfig_MaxVariableCostUsd). On by default; set false to opt out, and those cards come back as placeholders.
Maps to `variableCost`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L349)
