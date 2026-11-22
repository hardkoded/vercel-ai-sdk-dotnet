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
public Resolvable<string>? ApiKey { get; set; }
```


Explicit key. Falls back to `AI_GATEWAY_API_KEY`. A callback is resolved before each request.

**Value:** `Resolvable&lt;string&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L28)

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

<a id="Vercel_AI_Gateway_GatewayOptions_Headers_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Custom headers merged into the auth headers of realtime requests.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L42)

<a id="Vercel_AI_Gateway_GatewayOptions_MetadataCacheRefreshMillis_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_MetadataCacheRefreshMillis"></a>

### MetadataCacheRefreshMillis


```csharp
public int? MetadataCacheRefreshMillis { get; set; }
```


How frequently to refresh the metadata cache of [GetAvailableModelsAsync(CancellationToken)](/api/Vercel.AI.Gateway/GatewayProvider/#Vercel_AI_Gateway_GatewayProvider_GetAvailableModelsAsync_System_Threading_CancellationToken_), in milliseconds. Defaults to five minutes.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L45)

<a id="Vercel_AI_Gateway_GatewayOptions_O11yHeaders_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_O11yHeaders"></a>

### O11yHeaders


```csharp
public IReadOnlyDictionary<string, string>? O11yHeaders { get; set; }
```


Observability headers sent on every request. When null, they come from `VERCEL_DEPLOYMENT_ID`
(`ai-o11y-deployment-id`), `VERCEL_ENV` (`ai-o11y-environment`), `VERCEL_REGION`
(`ai-o11y-region`), the `x-vercel-id` header of the [VercelRequestContext](/api/Vercel.AI.Gateway/VercelRequestContext/) (`ai-o11y-request-id`),
and `VERCEL_PROJECT_ID` (`ai-o11y-project-id`).

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L36)

<a id="Vercel_AI_Gateway_GatewayOptions_TeamIdOrSlug_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_TeamIdOrSlug"></a>

### TeamIdOrSlug


```csharp
public string? TeamIdOrSlug { get; set; }
```


Vercel team ID or slug sent as `x-vercel-ai-gateway-team` and carried in realtime WebSocket subprotocols.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L39)

<a id="Vercel_AI_Gateway_GatewayOptions_WebSocket_"></a>
<a id="Vercel_AI_Gateway_GatewayOptions_WebSocket"></a>

### WebSocket


```csharp
public WebSocketFactory? WebSocket { get; set; }
```


Custom WebSocket implementation for streaming transcription. Defaults to `ClientWebSocket`. Maps to `webSocket`.

**Value:** [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L48)
