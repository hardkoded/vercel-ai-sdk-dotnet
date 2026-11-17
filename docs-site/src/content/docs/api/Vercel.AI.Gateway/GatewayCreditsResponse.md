---
title: "GatewayCreditsResponse"
description: "The Gateway credit balance. Maps to GatewayCreditsResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayCreditsResponse"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayCreditsResponse"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayCreditsResponse
```


The Gateway credit balance. Maps to `GatewayCreditsResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayFetchMetadata.cs#L17)

## Properties

<a id="Vercel_AI_Gateway_GatewayCreditsResponse_Balance_"></a>
<a id="Vercel_AI_Gateway_GatewayCreditsResponse_Balance"></a>

### Balance


```csharp
public string Balance { get; set; }
```


Gets or sets the remaining gateway credit balance available for API usage.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayFetchMetadata.cs#L20)

<a id="Vercel_AI_Gateway_GatewayCreditsResponse_TotalUsed_"></a>
<a id="Vercel_AI_Gateway_GatewayCreditsResponse_TotalUsed"></a>

### TotalUsed


```csharp
public string TotalUsed { get; set; }
```


Gets or sets the total amount of gateway credits that have been consumed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayFetchMetadata.cs#L23)
