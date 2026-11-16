---
title: "TakoResultContent"
description: "Content of a card or web result. Maps to TakoResultContent."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoResultContent"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoResultContent"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoResultContent
```


Content of a card or web result. Maps to `TakoResultContent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L779)

## Properties

<a id="Vercel_AI_Gateway_TakoResultContent_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L846)

<a id="Vercel_AI_Gateway_TakoResultContent_CardData_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_CardData"></a>

### CardData


```csharp
[JsonPropertyName("card_data")]
public IDictionary<string, JsonElement>? CardData { get; set; }
```


Card data.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L802)

<a id="Vercel_AI_Gateway_TakoResultContent_CardDataSchema_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_CardDataSchema"></a>

### CardDataSchema


```csharp
[JsonPropertyName("card_data_schema")]
public IDictionary<string, JsonElement>? CardDataSchema { get; set; }
```


Card data schema.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L806)

<a id="Vercel_AI_Gateway_TakoResultContent_ContentFormat_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_ContentFormat"></a>

### ContentFormat


```csharp
[JsonPropertyName("content_format")]
public string? ContentFormat { get; set; }
```


`card_json`, `csv`, `json_compact` or `json_records`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L782)

<a id="Vercel_AI_Gateway_TakoResultContent_Cost_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_Cost"></a>

### Cost


```csharp
[JsonPropertyName("cost")]
public double? Cost { get; set; }
```


Cost.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L786)

<a id="Vercel_AI_Gateway_TakoResultContent_Data_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_Data"></a>

### Data


```csharp
[JsonPropertyName("data")]
public string? Data { get; set; }
```


Serialized data.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L790)

<a id="Vercel_AI_Gateway_TakoResultContent_Dataset_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_Dataset"></a>

### Dataset


```csharp
[JsonPropertyName("dataset")]
public TakoDataset? Dataset { get; set; }
```


Inline dataset.

**Value:** [TakoDataset](/api/Vercel.AI.Gateway/TakoDataset/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L798)

<a id="Vercel_AI_Gateway_TakoResultContent_ExpiresAt_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_ExpiresAt"></a>

### ExpiresAt


```csharp
[JsonPropertyName("expires_at")]
public string? ExpiresAt { get; set; }
```


Expiry of [Url](#Vercel_AI_Gateway_TakoResultContent_Url).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L814)

<a id="Vercel_AI_Gateway_TakoResultContent_ExportPricing_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_ExportPricing"></a>

### ExportPricing


```csharp
[JsonPropertyName("export_pricing")]
public TakoExportPricing? ExportPricing { get; set; }
```


Export pricing.

**Value:** [TakoExportPricing](/api/Vercel.AI.Gateway/TakoExportPricing/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L838)

<a id="Vercel_AI_Gateway_TakoResultContent_Manifest_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_Manifest"></a>

### Manifest


```csharp
[JsonPropertyName("manifest")]
public IList<TakoManifestColumn>? Manifest { get; set; }
```


Manifest.

**Value:** [IList&lt;TakoManifestColumn&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L842)

<a id="Vercel_AI_Gateway_TakoResultContent_PremiumRows_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_PremiumRows"></a>

### PremiumRows


```csharp
[JsonPropertyName("premium_rows")]
public IDictionary<string, double>? PremiumRows { get; set; }
```


Distinct records per premium class (for example `company_record`) among delivered rows.

**Value:** [IDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L826)

<a id="Vercel_AI_Gateway_TakoResultContent_Records_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_Records"></a>

### Records


```csharp
[JsonPropertyName("records")]
public IList<IDictionary<string, TakoDatasetCell>>? Records { get; set; }
```


Records.

**Value:** [IList&lt;IDictionary&lt;string, TakoDatasetCell&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L794)

<a id="Vercel_AI_Gateway_TakoResultContent_RedactedCells_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_RedactedCells"></a>

### RedactedCells


```csharp
[JsonPropertyName("redacted_cells")]
public IDictionary<string, IList<double>>? RedactedCells { get; set; }
```


Column header to the 0-based delivered-row positions whose premium value was withheld.

**Value:** [IDictionary&lt;string, IList&lt;double&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L830)

<a id="Vercel_AI_Gateway_TakoResultContent_TotalMatching_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_TotalMatching"></a>

### TotalMatching


```csharp
[JsonPropertyName("total_matching")]
public double? TotalMatching { get; set; }
```


Full set size when the card lists only a capped slice of it.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L834)

<a id="Vercel_AI_Gateway_TakoResultContent_TotalRows_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_TotalRows"></a>

### TotalRows


```csharp
[JsonPropertyName("total_rows")]
public double? TotalRows { get; set; }
```


Total rows.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L818)

<a id="Vercel_AI_Gateway_TakoResultContent_Truncated_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_Truncated"></a>

### Truncated


```csharp
[JsonPropertyName("truncated")]
public bool? Truncated { get; set; }
```


Whether the rows were truncated.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L822)

<a id="Vercel_AI_Gateway_TakoResultContent_Url_"></a>
<a id="Vercel_AI_Gateway_TakoResultContent_Url"></a>

### Url


```csharp
[JsonPropertyName("url")]
public string? Url { get; set; }
```


Url of the data.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L810)
