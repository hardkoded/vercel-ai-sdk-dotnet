---
title: "TakoWebResult"
description: "A web result of a Tako search response. Maps to TakoWebResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoWebResult"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoWebResult"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoWebResult
```


A web result of a Tako search response. Maps to `TakoWebResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1035)

## Properties

<a id="Vercel_AI_Gateway_TakoWebResult_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoWebResult_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1062)

<a id="Vercel_AI_Gateway_TakoWebResult_Content_"></a>
<a id="Vercel_AI_Gateway_TakoWebResult_Content"></a>

### Content


```csharp
[JsonPropertyName("content")]
public TakoResultContent? Content { get; set; }
```


Content.

**Value:** [TakoResultContent](/api/Vercel.AI.Gateway/TakoResultContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1058)

<a id="Vercel_AI_Gateway_TakoWebResult_PublishDate_"></a>
<a id="Vercel_AI_Gateway_TakoWebResult_PublishDate"></a>

### PublishDate


```csharp
[JsonPropertyName("publish_date")]
public string? PublishDate { get; set; }
```


Publish date.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1054)

<a id="Vercel_AI_Gateway_TakoWebResult_Snippet_"></a>
<a id="Vercel_AI_Gateway_TakoWebResult_Snippet"></a>

### Snippet


```csharp
[JsonPropertyName("snippet")]
public string? Snippet { get; set; }
```


Snippet.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1046)

<a id="Vercel_AI_Gateway_TakoWebResult_SourceName_"></a>
<a id="Vercel_AI_Gateway_TakoWebResult_SourceName"></a>

### SourceName


```csharp
[JsonPropertyName("source_name")]
public string? SourceName { get; set; }
```


Source name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1050)

<a id="Vercel_AI_Gateway_TakoWebResult_Title_"></a>
<a id="Vercel_AI_Gateway_TakoWebResult_Title"></a>

### Title


```csharp
[JsonPropertyName("title")]
public string Title { get; set; }
```


Title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1038)

<a id="Vercel_AI_Gateway_TakoWebResult_Url_"></a>
<a id="Vercel_AI_Gateway_TakoWebResult_Url"></a>

### Url


```csharp
[JsonPropertyName("url")]
public string Url { get; set; }
```


Url.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1042)
