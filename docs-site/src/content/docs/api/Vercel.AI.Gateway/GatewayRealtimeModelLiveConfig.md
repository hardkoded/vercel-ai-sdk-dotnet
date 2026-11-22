---
title: "GatewayRealtimeModelLiveConfig"
description: "Configuration of a GatewayRealtimeModelLive. Maps to GatewayRealtimeModelLiveConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeModelLiveConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayRealtimeModelLiveConfig
```


Configuration of a [GatewayRealtimeModelLive](/api/Vercel.AI.Gateway/GatewayRealtimeModelLive/). Maps to `GatewayRealtimeModelLiveConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L14)

## Constructors

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig__ctor_System_String_System_String_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String____System_Func_System_String_System_Nullable_System_Int32__System_String_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Gateway_GatewayMintedClientSecret___System_String_"></a>

### `GatewayRealtimeModelLiveConfig(string, string, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>, Func<string, int?, string, CancellationToken, Task<GatewayMintedClientSecret>>, string?)`


```csharp
public GatewayRealtimeModelLiveConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> getHeaders, Func<string, int?, string, CancellationToken, Task<GatewayMintedClientSecret>> createClientSecret, string? teamIdOrSlug = null)
```


Creates a config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `getHeaders` | [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `createClientSecret` | [Func&lt;string, int?, string, CancellationToken, Task&lt;GatewayMintedClientSecret&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-5) |  |
| `teamIdOrSlug` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L17)

## Properties

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_BaseUrl_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


HTTP(S) base URL of the Gateway V4 routes.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L43)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_CreateClientSecret_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_CreateClientSecret"></a>

### CreateClientSecret


```csharp
public Func<string, int?, string, CancellationToken, Task<GatewayMintedClientSecret>> CreateClientSecret { get; }
```


Mints a Live-bound client secret (`vcst_`) server-side. Arguments are the model id, `expiresAfterSeconds`
and the route kind (always `live`).

**Value:** [Func&lt;string, int?, string, CancellationToken, Task&lt;GatewayMintedClientSecret&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-5)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L55)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_GetHeaders_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_GetHeaders"></a>

### GetHeaders


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> GetHeaders { get; }
```


Resolves the auth headers for a server-side connection.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L49)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L40)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_TeamIdOrSlug_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_TeamIdOrSlug"></a>

### TeamIdOrSlug


```csharp
public string? TeamIdOrSlug { get; }
```


Optional Vercel team scope.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L46)
