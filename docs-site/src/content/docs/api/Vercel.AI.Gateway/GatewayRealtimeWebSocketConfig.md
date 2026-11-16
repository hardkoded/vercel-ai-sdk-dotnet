---
title: "GatewayRealtimeWebSocketConfig"
description: "URL and subprotocols a client opens a WebSocket with. Maps to the result of getWebSocketConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeWebSocketConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeWebSocketConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayRealtimeWebSocketConfig
```


URL and subprotocols a client opens a WebSocket with. Maps to the result of `getWebSocketConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L50)

## Constructors

<a id="Vercel_AI_Gateway_GatewayRealtimeWebSocketConfig__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeWebSocketConfig__ctor_System_String_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `GatewayRealtimeWebSocketConfig(string, IReadOnlyList<string>?)`


```csharp
public GatewayRealtimeWebSocketConfig(string url, IReadOnlyList<string>? protocols)
```


Creates a WebSocket config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `protocols` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L53)

## Properties

<a id="Vercel_AI_Gateway_GatewayRealtimeWebSocketConfig_Protocols_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeWebSocketConfig_Protocols"></a>

### Protocols


```csharp
public IReadOnlyList<string>? Protocols { get; }
```


Subprotocols to offer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L63)

<a id="Vercel_AI_Gateway_GatewayRealtimeWebSocketConfig_Url_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeWebSocketConfig_Url"></a>

### Url


```csharp
public string Url { get; }
```


The WebSocket URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L60)
