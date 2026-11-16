---
title: "TakoCard"
description: "A card of a Tako search response. Maps to TakoCard."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoCard"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoCard"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoCard
```


A card of a Tako search response. Maps to `TakoCard`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L951)

## Properties

<a id="Vercel_AI_Gateway_TakoCard_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoCard_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1030)

<a id="Vercel_AI_Gateway_TakoCard_CardId_"></a>
<a id="Vercel_AI_Gateway_TakoCard_CardId"></a>

### CardId


```csharp
[JsonPropertyName("card_id")]
public string? CardId { get; set; }
```


Card id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L954)

<a id="Vercel_AI_Gateway_TakoCard_CardType_"></a>
<a id="Vercel_AI_Gateway_TakoCard_CardType"></a>

### CardType


```csharp
[JsonPropertyName("card_type")]
public string? CardType { get; set; }
```


Card type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L994)

<a id="Vercel_AI_Gateway_TakoCard_Content_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Content"></a>

### Content


```csharp
[JsonPropertyName("content")]
public TakoResultContent? Content { get; set; }
```


Content.

**Value:** [TakoResultContent](/api/Vercel.AI.Gateway/TakoResultContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1006)

<a id="Vercel_AI_Gateway_TakoCard_DataFreshness_"></a>
<a id="Vercel_AI_Gateway_TakoCard_DataFreshness"></a>

### DataFreshness


```csharp
[JsonPropertyName("data_freshness")]
public TakoDataFreshness? DataFreshness { get; set; }
```


Data freshness.

**Value:** [TakoDataFreshness](/api/Vercel.AI.Gateway/TakoDataFreshness/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1022)

<a id="Vercel_AI_Gateway_TakoCard_Description_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Description"></a>

### Description


```csharp
[JsonPropertyName("description")]
public string? Description { get; set; }
```


Description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L962)

<a id="Vercel_AI_Gateway_TakoCard_EmbedUrl_"></a>
<a id="Vercel_AI_Gateway_TakoCard_EmbedUrl"></a>

### EmbedUrl


```csharp
[JsonPropertyName("embed_url")]
public string? EmbedUrl { get; set; }
```


Embed url.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L978)

<a id="Vercel_AI_Gateway_TakoCard_Exportable_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Exportable"></a>

### Exportable


```csharp
[JsonPropertyName("exportable")]
public bool? Exportable { get; set; }
```


Whether the card is exportable.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1010)

<a id="Vercel_AI_Gateway_TakoCard_ImageUrl_"></a>
<a id="Vercel_AI_Gateway_TakoCard_ImageUrl"></a>

### ImageUrl


```csharp
[JsonPropertyName("image_url")]
public string? ImageUrl { get; set; }
```


Image url.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L974)

<a id="Vercel_AI_Gateway_TakoCard_Methodologies_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Methodologies"></a>

### Methodologies


```csharp
[JsonPropertyName("methodologies")]
public IList<TakoMethodology>? Methodologies { get; set; }
```


Methodologies.

**Value:** [IList&lt;TakoMethodology&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L986)

<a id="Vercel_AI_Gateway_TakoCard_MetricDefinitions_"></a>
<a id="Vercel_AI_Gateway_TakoCard_MetricDefinitions"></a>

### MetricDefinitions


```csharp
[JsonPropertyName("metric_definitions")]
public IList<TakoMetricDefinition>? MetricDefinitions { get; set; }
```


Metric definitions.

**Value:** [IList&lt;TakoMetricDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1018)

<a id="Vercel_AI_Gateway_TakoCard_Nodes_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Nodes"></a>

### Nodes


```csharp
[JsonPropertyName("nodes")]
public IList<TakoNode>? Nodes { get; set; }
```


Data Graph nodes.

**Value:** [IList&lt;TakoNode&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1014)

<a id="Vercel_AI_Gateway_TakoCard_Placeholder_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Placeholder"></a>

### Placeholder


```csharp
[JsonPropertyName("placeholder")]
public TakoPlaceholder? Placeholder { get; set; }
```


Set when Tako withholds the card's data.

**Value:** [TakoPlaceholder](/api/Vercel.AI.Gateway/TakoPlaceholder/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1026)

<a id="Vercel_AI_Gateway_TakoCard_Relevance_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Relevance"></a>

### Relevance


```csharp
[JsonPropertyName("relevance")]
public string? Relevance { get; set; }
```


`High`, `Low` or `Medium`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L998)

<a id="Vercel_AI_Gateway_TakoCard_RelevanceScore_"></a>
<a id="Vercel_AI_Gateway_TakoCard_RelevanceScore"></a>

### RelevanceScore


```csharp
[JsonPropertyName("relevance_score")]
public double? RelevanceScore { get; set; }
```


Relevance to the query, 1.0-5.0. Null unless the account is entitled.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1002)

<a id="Vercel_AI_Gateway_TakoCard_SemanticDescription_"></a>
<a id="Vercel_AI_Gateway_TakoCard_SemanticDescription"></a>

### SemanticDescription


```csharp
[JsonPropertyName("semantic_description")]
public string? SemanticDescription { get; set; }
```


Semantic description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L966)

<a id="Vercel_AI_Gateway_TakoCard_SourceIndexes_"></a>
<a id="Vercel_AI_Gateway_TakoCard_SourceIndexes"></a>

### SourceIndexes


```csharp
[JsonPropertyName("source_indexes")]
public IList<string>? SourceIndexes { get; set; }
```


Source indexes, `data` or `web`.

**Value:** [IList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L990)

<a id="Vercel_AI_Gateway_TakoCard_Sources_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Sources"></a>

### Sources


```csharp
[JsonPropertyName("sources")]
public IList<TakoCardSource>? Sources { get; set; }
```


Sources.

**Value:** [IList&lt;TakoCardSource&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L982)

<a id="Vercel_AI_Gateway_TakoCard_Title_"></a>
<a id="Vercel_AI_Gateway_TakoCard_Title"></a>

### Title


```csharp
[JsonPropertyName("title")]
public string? Title { get; set; }
```


Title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L958)

<a id="Vercel_AI_Gateway_TakoCard_WebpageUrl_"></a>
<a id="Vercel_AI_Gateway_TakoCard_WebpageUrl"></a>

### WebpageUrl


```csharp
[JsonPropertyName("webpage_url")]
public string? WebpageUrl { get; set; }
```


Webpage url.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L970)
