---
title: "TakoPlaceholder"
description: "Set when Tako withholds a card's data. Maps to TakoCard['placeholder']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoPlaceholder"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoPlaceholder"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoPlaceholder
```


Set when Tako withholds a card's data. Maps to `TakoCard['placeholder']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L961)

## Properties

<a id="Vercel_AI_Gateway_TakoPlaceholder_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoPlaceholder_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L972)

<a id="Vercel_AI_Gateway_TakoPlaceholder_Reason_"></a>
<a id="Vercel_AI_Gateway_TakoPlaceholder_Reason"></a>

### Reason


```csharp
[JsonPropertyName("reason")]
public string? Reason { get; set; }
```


`not_served` (variable cost off or not served) or `over_cap` (over `MaxVariableCostUsd`).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L968)

<a id="Vercel_AI_Gateway_TakoPlaceholder_WithheldSource_"></a>
<a id="Vercel_AI_Gateway_TakoPlaceholder_WithheldSource"></a>

### WithheldSource


```csharp
[JsonPropertyName("withheld_source")]
public string WithheldSource { get; set; }
```


Name of the withheld source.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L964)
