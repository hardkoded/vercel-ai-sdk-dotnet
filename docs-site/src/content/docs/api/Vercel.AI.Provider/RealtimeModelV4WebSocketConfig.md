---
title: "RealtimeModelV4WebSocketConfig"
description: "URL and optional subprotocols a client opens a realtime WebSocket with. Maps to the result of RealtimeModelV4.getWebSocketConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4WebSocketConfig"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4WebSocketConfig"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4WebSocketConfig
```


URL and optional subprotocols a client opens a realtime WebSocket with. Maps to the result of
`RealtimeModelV4.getWebSocketConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4WebSocketConfig.cs#L11)

## Constructors

<a id="Vercel_AI_Provider_RealtimeModelV4WebSocketConfig__ctor_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4WebSocketConfig__ctor_System_String_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `RealtimeModelV4WebSocketConfig(string, IReadOnlyList<string>?)`


```csharp
public RealtimeModelV4WebSocketConfig(string url, IReadOnlyList<string>? protocols = null)
```


Creates a WebSocket config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `protocols` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4WebSocketConfig.cs#L14)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4WebSocketConfig_Protocols_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4WebSocketConfig_Protocols"></a>

### Protocols


```csharp
public IReadOnlyList<string>? Protocols { get; }
```


Subprotocols to offer, when the provider authenticates through them.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4WebSocketConfig.cs#L24)

<a id="Vercel_AI_Provider_RealtimeModelV4WebSocketConfig_Url_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4WebSocketConfig_Url"></a>

### Url


```csharp
public string Url { get; }
```


The WebSocket URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4WebSocketConfig.cs#L21)
