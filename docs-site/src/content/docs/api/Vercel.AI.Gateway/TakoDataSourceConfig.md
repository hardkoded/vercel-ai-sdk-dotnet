---
title: "TakoDataSourceConfig"
description: "Options of the curated data source. Maps to TakoDataSourceConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoDataSourceConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoDataSourceConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoDataSourceConfig
```


Options of the curated data source. Maps to `TakoDataSourceConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L481)

## Properties

<a id="Vercel_AI_Gateway_TakoDataSourceConfig_ContentFormat_"></a>
<a id="Vercel_AI_Gateway_TakoDataSourceConfig_ContentFormat"></a>

### ContentFormat


```csharp
public string? ContentFormat { get; set; }
```


Serialization for inlined card data: `card_json`, `csv`, `json_compact` or `json_records`. Maps to `contentFormat`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L497)

<a id="Vercel_AI_Gateway_TakoDataSourceConfig_Count_"></a>
<a id="Vercel_AI_Gateway_TakoDataSourceConfig_Count"></a>

### Count


```csharp
public int? Count { get; set; }
```


Maximum number of data results to return (1-20). When [IncludeContents](#Vercel_AI_Gateway_TakoDataSourceConfig_IncludeContents) is true, each additional result adds its own data surcharge.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L484)

<a id="Vercel_AI_Gateway_TakoDataSourceConfig_IncludeContents_"></a>
<a id="Vercel_AI_Gateway_TakoDataSourceConfig_IncludeContents"></a>

### IncludeContents


```csharp
public bool? IncludeContents { get; set; }
```


Inline rows for each data result. This adds a data surcharge based on row count and dataset source. To estimate cost,
search with this disabled and inspect `cards.content.export_pricing`. This applies to every returned card; limit
[Count](#Vercel_AI_Gateway_TakoDataSourceConfig_Count) and [MaxRows](#Vercel_AI_Gateway_TakoDataSourceConfig_MaxRows) to control cost.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L491)

<a id="Vercel_AI_Gateway_TakoDataSourceConfig_MaxRows_"></a>
<a id="Vercel_AI_Gateway_TakoDataSourceConfig_MaxRows"></a>

### MaxRows


```csharp
public int? MaxRows { get; set; }
```


Maximum rows to inline per result. Omit to use the allowance in `cards.content.export_pricing`. A data surcharge applies per 1,000 exported rows; lower values reduce cost.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L500)

<a id="Vercel_AI_Gateway_TakoDataSourceConfig_Mode_"></a>
<a id="Vercel_AI_Gateway_TakoDataSourceConfig_Mode"></a>

### Mode


```csharp
public string? Mode { get; set; }
```


Requested delivery mode for card data, `inline` or `url`. Search cards are always inlined.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L494)

<a id="Vercel_AI_Gateway_TakoDataSourceConfig_NodeIds_"></a>
<a id="Vercel_AI_Gateway_TakoDataSourceConfig_NodeIds"></a>

### NodeIds


```csharp
public IList<string>? NodeIds { get; set; }
```


Data Graph node IDs to prioritize. Maps to `nodeIds`.

**Value:** [IList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L503)

<a id="Vercel_AI_Gateway_TakoDataSourceConfig_Strict_"></a>
<a id="Vercel_AI_Gateway_TakoDataSourceConfig_Strict"></a>

### Strict


```csharp
public bool? Strict { get; set; }
```


Return only cards matching [NodeIds](#Vercel_AI_Gateway_TakoDataSourceConfig_NodeIds). Requires at least one node ID.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L506)
