---
title: "GatewayModelPricing"
description: "Pricing of a Gateway model, in USD per token. Maps to the pricing of GatewayLanguageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayModelPricing"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayModelPricing"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayModelPricing
```


Pricing of a Gateway model, in USD per token. Maps to the `pricing` of `GatewayLanguageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L8)

## Properties

<a id="Vercel_AI_Gateway_GatewayModelPricing_CacheCreationInputTokens_"></a>
<a id="Vercel_AI_Gateway_GatewayModelPricing_CacheCreationInputTokens"></a>

### CacheCreationInputTokens


```csharp
public string? CacheCreationInputTokens { get; set; }
```


Gets or sets the cost per input token to create cache entries in USD. Only present for models that support prompt caching.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L20)

<a id="Vercel_AI_Gateway_GatewayModelPricing_CachedInputTokens_"></a>
<a id="Vercel_AI_Gateway_GatewayModelPricing_CachedInputTokens"></a>

### CachedInputTokens


```csharp
public string? CachedInputTokens { get; set; }
```


Gets or sets the cost per cached input token in USD. Only present for models that support prompt caching.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L17)

<a id="Vercel_AI_Gateway_GatewayModelPricing_Input_"></a>
<a id="Vercel_AI_Gateway_GatewayModelPricing_Input"></a>

### Input


```csharp
public string Input { get; set; }
```


Gets or sets the cost per input token in USD.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L11)

<a id="Vercel_AI_Gateway_GatewayModelPricing_Output_"></a>
<a id="Vercel_AI_Gateway_GatewayModelPricing_Output"></a>

### Output


```csharp
public string Output { get; set; }
```


Gets or sets the cost per output token in USD.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayModelEntry.cs#L14)
