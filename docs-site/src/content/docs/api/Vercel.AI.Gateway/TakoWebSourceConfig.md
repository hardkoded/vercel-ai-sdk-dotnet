---
title: "TakoWebSourceConfig"
description: "Options of the web source. Maps to TakoWebSourceConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoWebSourceConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoWebSourceConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoWebSourceConfig
```


Options of the web source. Maps to `TakoWebSourceConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L539)

## Properties

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_ArticleContentMaxChars_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_ArticleContentMaxChars"></a>

### ArticleContentMaxChars


```csharp
public int? ArticleContentMaxChars { get; set; }
```


Maximum extracted characters per web page when including contents. Maps to `articleContentMaxChars`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L563)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_Category_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_Category"></a>

### Category


```csharp
public string? Category { get; set; }
```


Optional web-result category filter: `finance`, `news` or `sports`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L548)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_Count_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_Count"></a>

### Count


```csharp
public int? Count { get; set; }
```


Maximum number of web results to return (1-20).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L542)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_ExcludeDomains_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_ExcludeDomains"></a>

### ExcludeDomains


```csharp
public IList<string>? ExcludeDomains { get; set; }
```


Exclude results from these bare domains. Maps to `excludeDomains`.

**Value:** [IList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L554)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_Highlights_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_Highlights"></a>

### Highlights


```csharp
public bool? Highlights { get; set; }
```


Include highlighted passages in web results. Defaults to true in AI Gateway.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L560)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_IncludeContents_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_IncludeContents"></a>

### IncludeContents


```csharp
public bool? IncludeContents { get; set; }
```


Inline extracted web page text. This can add a data charge.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L545)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_IncludeDomains_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_IncludeDomains"></a>

### IncludeDomains


```csharp
public IList<string>? IncludeDomains { get; set; }
```


Only return results from these bare domains. Maps to `includeDomains`.

**Value:** [IList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L551)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_PublishedAfter_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_PublishedAfter"></a>

### PublishedAfter


```csharp
public string? PublishedAfter { get; set; }
```


Keep results published on or after this ISO date. Maps to `publishedAfter`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L566)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_PublishedBefore_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_PublishedBefore"></a>

### PublishedBefore


```csharp
public string? PublishedBefore { get; set; }
```


Keep results published on or before this ISO date. Maps to `publishedBefore`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L569)

<a id="Vercel_AI_Gateway_TakoWebSourceConfig_SnippetMaxChars_"></a>
<a id="Vercel_AI_Gateway_TakoWebSourceConfig_SnippetMaxChars"></a>

### SnippetMaxChars


```csharp
public int? SnippetMaxChars { get; set; }
```


Maximum characters in each web-result snippet. Maps to `snippetMaxChars`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L557)
