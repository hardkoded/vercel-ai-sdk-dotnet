---
title: "GatewayOptions"
description: "AI Gateway settings. The default base URL is https://ai-gateway.vercel.sh/v4/ai."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayOptions"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayOptions"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayOptions
```


AI Gateway settings. The default base URL is `https://ai-gateway.vercel.sh/v4/ai`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L17)

## Properties

<a id="Vercel_AI_Gateway_GatewayOptions_ApiKey_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Explicit key. Falls back to `AI_GATEWAY_API_KEY`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L26)

<a id="Vercel_AI_Gateway_GatewayOptions_BaseUrl_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


Gateway origin for the V4 routes.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L23)

<a id="Vercel_AI_Gateway_GatewayOptions_BatchResultDownloads_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_BatchResultDownloads"></a>

### BatchResultDownloads


```csharp
public BatchResultDownloads? BatchResultDownloads { get; set; }
```


Settings for downloading JSON Lines batch results.

**Value:** [BatchResultDownloads](/api/Vercel.AI.ProviderUtils/BatchResultDownloads/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L20)

<a id="Vercel_AI_Gateway_GatewayOptions_O11yHeaders_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_O11yHeaders"></a>

### O11yHeaders


```csharp
public IReadOnlyDictionary<string, string>? O11yHeaders { get; set; }
```


Observability headers sent on every request. When null, they come from `VERCEL_DEPLOYMENT_ID`
(`ai-o11y-deployment-id`), `VERCEL_ENV` (`ai-o11y-environment`), `VERCEL_REGION`
(`ai-o11y-region`), and `VERCEL_PROJECT_ID` (`ai-o11y-project-id`).

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L33)
