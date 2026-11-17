---
title: "TakoDataset"
description: "Inline dataset of a result content. Maps to TakoResultContent['dataset']."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoDataset"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoDataset"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoDataset
```


Inline dataset of a result content. Maps to `TakoResultContent['dataset']`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L713)

## Properties

<a id="Vercel_AI_Gateway_TakoDataset_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L744)

<a id="Vercel_AI_Gateway_TakoDataset_Columns_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_Columns"></a>

### Columns


```csharp
[JsonPropertyName("columns")]
public IList<TakoDatasetColumn> Columns { get; set; }
```


Columns.

**Value:** [IList&lt;TakoDatasetColumn&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L716)

<a id="Vercel_AI_Gateway_TakoDataset_Provenance_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_Provenance"></a>

### Provenance


```csharp
[JsonPropertyName("provenance")]
public string? Provenance { get; set; }
```


`query` or `web_extraction`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L740)

<a id="Vercel_AI_Gateway_TakoDataset_Ref_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_Ref"></a>

### Ref


```csharp
[JsonPropertyName("ref")]
public string Ref { get; set; }
```


Dataset reference.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L732)

<a id="Vercel_AI_Gateway_TakoDataset_Rows_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_Rows"></a>

### Rows


```csharp
[JsonPropertyName("rows")]
public IList<IList<TakoDatasetCell>> Rows { get; set; }
```


Rows.

**Value:** [IList&lt;IList&lt;TakoDatasetCell&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L720)

<a id="Vercel_AI_Gateway_TakoDataset_Sources_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_Sources"></a>

### Sources


```csharp
[JsonPropertyName("sources")]
public IList<TakoDatasetSource> Sources { get; set; }
```


Sources.

**Value:** [IList&lt;TakoDatasetSource&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L736)

<a id="Vercel_AI_Gateway_TakoDataset_TotalRows_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_TotalRows"></a>

### TotalRows


```csharp
[JsonPropertyName("total_rows")]
public double TotalRows { get; set; }
```


Total rows.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L724)

<a id="Vercel_AI_Gateway_TakoDataset_Truncated_"></a>
<a id="Vercel_AI_Gateway_TakoDataset_Truncated"></a>

### Truncated


```csharp
[JsonPropertyName("truncated")]
public bool Truncated { get; set; }
```


Whether the rows were truncated.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L728)
