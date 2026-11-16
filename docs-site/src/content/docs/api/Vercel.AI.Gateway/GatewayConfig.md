---
title: "GatewayConfig"
description: "Configuration shared by Gateway fetchers (GatewayConfig)."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayConfig
```


Configuration shared by Gateway fetchers (`GatewayConfig`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L13)

## Properties

<a id="Vercel_AI_Gateway_GatewayConfig_BaseUrl_"></a>
<a id="Vercel_AI_Gateway_GatewayConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


Gets or sets the base URL of the Gateway.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L16)

<a id="Vercel_AI_Gateway_GatewayConfig_Headers_"></a>
<a id="Vercel_AI_Gateway_GatewayConfig_Headers"></a>

### Headers


```csharp
public Func<IReadOnlyDictionary<string, string?>>? Headers { get; set; }
```


Gets or sets the function that resolves the request headers.

**Value:** [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L19)

<a id="Vercel_AI_Gateway_GatewayConfig_Http_"></a>
<a id="Vercel_AI_Gateway_GatewayConfig_Http"></a>

### Http


```csharp
public ProviderHttp? Http { get; set; }
```


Gets or sets the HTTP client (stands in for upstream's `fetch`).

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L22)
