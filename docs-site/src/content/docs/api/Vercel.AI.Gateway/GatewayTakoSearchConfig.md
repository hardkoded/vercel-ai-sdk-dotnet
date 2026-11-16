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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L271)

## Properties

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_ExtraBody_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_ExtraBody"></a>

### ExtraBody


```csharp
public IDictionary<string, JsonNode?>? ExtraBody { get; set; }
```


Extra Tako request fields in snake_case, deep-merged into the body last. Can't set fields this tool models;
Tako rejects unknown keys (`api_error`). Maps to `extraBody`.

**Value:** [IDictionary&lt;string, JsonNode&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L297)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L285)

<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_RemoveMaxVariableCostCap_"></a>
<a id="Vercel_AI_Gateway_GatewayTakoSearchConfig_RemoveMaxVariableCostCap"></a>

### RemoveMaxVariableCostCap


```csharp
public bool RemoveMaxVariableCostCap { get; set; }
```


Removes the variable-cost cap, which sends `maxVariableCostUsd: null`. Takes precedence over
[MaxVariableCostUsd](#Vercel_AI_Gateway_GatewayTakoSearchConfig_MaxVariableCostUsd).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L291)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L278)
