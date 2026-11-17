---
title: "GatewaySpendReportRow"
description: "A row of a spend report. Maps to GatewaySpendReportRow."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewaySpendReportRow"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewaySpendReportRow"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewaySpendReportRow
```


A row of a spend report. Maps to `GatewaySpendReportRow`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L85)

## Properties

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_CacheCreationInputTokens_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_CacheCreationInputTokens"></a>

### CacheCreationInputTokens


```csharp
public double? CacheCreationInputTokens { get; set; }
```


Gets or sets the number of cache creation input tokens.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L124)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_CachedInputTokens_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_CachedInputTokens"></a>

### CachedInputTokens


```csharp
public double? CachedInputTokens { get; set; }
```


Gets or sets the number of cached input tokens.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L121)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_CredentialType_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_CredentialType"></a>

### CredentialType


```csharp
public GatewayCredentialType? CredentialType { get; set; }
```


Gets or sets the credential type (present when grouped by credential type).

**Value:** `GatewayCredentialType?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L106)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Day_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Day"></a>

### Day


```csharp
public string? Day { get; set; }
```


Gets or sets the date string (present when grouped by day).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L88)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Hour_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Hour"></a>

### Hour


```csharp
public string? Hour { get; set; }
```


Gets or sets the hour timestamp (present when grouped by day with an hour date part).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L91)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_InputTokens_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_InputTokens"></a>

### InputTokens


```csharp
public double? InputTokens { get; set; }
```


Gets or sets the number of input tokens.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L115)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_MarketCost_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_MarketCost"></a>

### MarketCost


```csharp
public double? MarketCost { get; set; }
```


Gets or sets the market cost in USD.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L112)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Model_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Model"></a>

### Model


```csharp
public string? Model { get; set; }
```


Gets or sets the model identifier (present when grouped by model).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L97)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_OutputTokens_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_OutputTokens"></a>

### OutputTokens


```csharp
public double? OutputTokens { get; set; }
```


Gets or sets the number of output tokens.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L118)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Provider"></a>

### Provider


```csharp
public string? Provider { get; set; }
```


Gets or sets the provider name (present when grouped by provider).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L103)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_ReasoningTokens_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_ReasoningTokens"></a>

### ReasoningTokens


```csharp
public double? ReasoningTokens { get; set; }
```


Gets or sets the number of reasoning tokens.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L127)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_RequestCount_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_RequestCount"></a>

### RequestCount


```csharp
public double? RequestCount { get; set; }
```


Gets or sets the number of requests.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L130)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Tag_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_Tag"></a>

### Tag


```csharp
public string? Tag { get; set; }
```


Gets or sets the tag value (present when grouped by tag).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L100)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_TotalCost_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_TotalCost"></a>

### TotalCost


```csharp
public double TotalCost { get; set; }
```


Gets or sets the total cost in USD.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L109)

<a id="Vercel_AI_Gateway_GatewaySpendReportRow_User_"></a>
<a id="Vercel_AI_Gateway_GatewaySpendReportRow_User"></a>

### User


```csharp
public string? User { get; set; }
```


Gets or sets the user identifier (present when grouped by user).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpendReport.cs#L94)
