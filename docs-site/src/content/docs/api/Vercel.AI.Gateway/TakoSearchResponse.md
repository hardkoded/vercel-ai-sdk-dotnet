---
title: "TakoSearchResponse"
description: "Output of the Tako search tool. Deserialize the tool output into it. Maps to TakoSearchResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoSearchResponse"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoSearchResponse"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoSearchResponse
```


Output of the Tako search tool. Deserialize the tool output into it. Maps to `TakoSearchResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1149)

## Properties

<a id="Vercel_AI_Gateway_TakoSearchResponse_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_TakoSearchResponse_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1172)

<a id="Vercel_AI_Gateway_TakoSearchResponse_Cards_"></a>
<a id="Vercel_AI_Gateway_TakoSearchResponse_Cards"></a>

### Cards


```csharp
[JsonPropertyName("cards")]
public IList<TakoCard>? Cards { get; set; }
```


Cards.

**Value:** [IList&lt;TakoCard&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1156)

<a id="Vercel_AI_Gateway_TakoSearchResponse_Related_"></a>
<a id="Vercel_AI_Gateway_TakoSearchResponse_Related"></a>

### Related


```csharp
[JsonPropertyName("related")]
public IList<IDictionary<string, JsonElement>>? Related { get; set; }
```


Related search suggestions.

**Value:** [IList&lt;IDictionary&lt;string, JsonElement&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1168)

<a id="Vercel_AI_Gateway_TakoSearchResponse_RequestId_"></a>
<a id="Vercel_AI_Gateway_TakoSearchResponse_RequestId"></a>

### RequestId


```csharp
[JsonPropertyName("request_id")]
public string RequestId { get; set; }
```


Request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1152)

<a id="Vercel_AI_Gateway_TakoSearchResponse_Usage_"></a>
<a id="Vercel_AI_Gateway_TakoSearchResponse_Usage"></a>

### Usage


```csharp
[JsonPropertyName("usage")]
public TakoUsage? Usage { get; set; }
```


Usage.

**Value:** [TakoUsage](/api/Vercel.AI.Gateway/TakoUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1164)

<a id="Vercel_AI_Gateway_TakoSearchResponse_WebResults_"></a>
<a id="Vercel_AI_Gateway_TakoSearchResponse_WebResults"></a>

### WebResults


```csharp
[JsonPropertyName("web_results")]
public IList<TakoWebResult>? WebResults { get; set; }
```


Web results.

**Value:** [IList&lt;TakoWebResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L1160)
