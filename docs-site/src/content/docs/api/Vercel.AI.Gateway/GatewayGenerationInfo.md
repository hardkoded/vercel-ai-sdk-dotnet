---
title: "GatewayGenerationInfo"
description: "Information about a Gateway generation."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayGenerationInfo"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayGenerationInfo"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayGenerationInfo
```


Information about a Gateway generation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L43)

## Properties

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_BillableWebSearchCalls_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_BillableWebSearchCalls"></a>

### BillableWebSearchCalls


```csharp
[JsonPropertyName("billableWebSearchCalls")]
public double BillableWebSearchCalls { get; set; }
```


Gets or sets the billable web search calls.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L129)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CacheCreationTokens_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CacheCreationTokens"></a>

### CacheCreationTokens


```csharp
[JsonPropertyName("cacheCreationTokens")]
public double CacheCreationTokens { get; set; }
```


Gets or sets the cache creation input tokens.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L125)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CachedTokens_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CachedTokens"></a>

### CachedTokens


```csharp
[JsonPropertyName("cachedTokens")]
public double CachedTokens { get; set; }
```


Gets or sets the cached tokens used.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L121)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CompletionTokens_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CompletionTokens"></a>

### CompletionTokens


```csharp
[JsonPropertyName("completionTokens")]
public double CompletionTokens { get; set; }
```


Gets or sets the number of completion tokens.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L113)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CreatedAt_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_CreatedAt"></a>

### CreatedAt


```csharp
[JsonPropertyName("createdAt")]
public string CreatedAt { get; set; }
```


Gets or sets the ISO 8601 timestamp when the generation was created.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L77)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_FinishReason_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_FinishReason"></a>

### FinishReason


```csharp
[JsonPropertyName("finishReason")]
public string FinishReason { get; set; }
```


Gets or sets the finish reason (for example `stop`).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L97)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_GatewayCost_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_GatewayCost"></a>

### GatewayCost


```csharp
[JsonPropertyName("gatewayCost")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public double? GatewayCost { get; set; }
```


Gets or sets the total amount debited in USD. Null when unavailable.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L64)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_GenerationTime_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_GenerationTime"></a>

### GenerationTime


```csharp
[JsonPropertyName("generationTime")]
public double GenerationTime { get; set; }
```


Gets or sets the total generation time in milliseconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L105)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Id_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Id"></a>

### Id


```csharp
[JsonPropertyName("id")]
public string Id { get; set; }
```


Gets or sets the generation ID.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L46)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_IsByok_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_IsByok"></a>

### IsByok


```csharp
[JsonPropertyName("isByok")]
public bool IsByok { get; set; }
```


Gets or sets a value indicating whether BYOK credentials were used.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L85)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Latency_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Latency"></a>

### Latency


```csharp
[JsonPropertyName("latency")]
public double Latency { get; set; }
```


Gets or sets the time to first token in milliseconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L101)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_MarketCost_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_MarketCost"></a>

### MarketCost


```csharp
[JsonPropertyName("marketCost")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public double? MarketCost { get; set; }
```


Gets or sets the market cost in USD, excluding surcharges. Null when unavailable.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L54)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Model_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Model"></a>

### Model


```csharp
[JsonPropertyName("model")]
public string Model { get; set; }
```


Gets or sets the model identifier.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L81)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_PromptTokens_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_PromptTokens"></a>

### PromptTokens


```csharp
[JsonPropertyName("promptTokens")]
public double PromptTokens { get; set; }
```


Gets or sets the number of prompt tokens.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L109)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_ProviderName_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_ProviderName"></a>

### ProviderName


```csharp
[JsonPropertyName("providerName")]
public string ProviderName { get; set; }
```


Gets or sets the provider that served this generation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L89)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_ReasoningTokens_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_ReasoningTokens"></a>

### ReasoningTokens


```csharp
[JsonPropertyName("reasoningTokens")]
public double ReasoningTokens { get; set; }
```


Gets or sets the reasoning tokens used.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L117)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Streamed_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Streamed"></a>

### Streamed


```csharp
[JsonPropertyName("streamed")]
public bool Streamed { get; set; }
```


Gets or sets a value indicating whether streaming was used.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L93)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_SurchargeCost_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_SurchargeCost"></a>

### SurchargeCost


```csharp
[JsonPropertyName("surchargeCost")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public double? SurchargeCost { get; set; }
```


Gets or sets the total surcharges applied in USD. Null when unavailable.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L59)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_TotalCost_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_TotalCost"></a>

### TotalCost


```csharp
[JsonPropertyName("totalCost")]
public double TotalCost { get; set; }
```


Gets or sets the total cost in USD.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L50)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_UpstreamInferenceCost_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_UpstreamInferenceCost"></a>

### UpstreamInferenceCost


```csharp
[JsonPropertyName("upstreamInferenceCost")]
public double UpstreamInferenceCost { get; set; }
```


Gets or sets the upstream inference cost in USD (BYOK only).

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L69)

<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Usage_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfo_Usage"></a>

### Usage


```csharp
[JsonPropertyName("usage")]
public double Usage { get; set; }
```


Gets or sets the usage cost in USD (same as [TotalCost](#Vercel_AI_Gateway_GatewayGenerationInfo_TotalCost)).

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L73)
