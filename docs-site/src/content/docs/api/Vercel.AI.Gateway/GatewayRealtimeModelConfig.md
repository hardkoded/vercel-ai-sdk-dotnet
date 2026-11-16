---
title: "GatewayRealtimeModelConfig"
description: "Configuration of a GatewayRealtimeModel. Maps to GatewayRealtimeModelConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeModelConfig"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayRealtimeModelConfig
```


Configuration of a [GatewayRealtimeModel](/api/Vercel.AI.Gateway/GatewayRealtimeModel/). Maps to `GatewayRealtimeModelConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L67)

## Constructors

<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig__ctor_System_String_System_String_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String____System_Func_System_String_System_Nullable_System_Int32__System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Gateway_GatewayMintedClientSecret___System_String_"></a>

### `GatewayRealtimeModelConfig(string, string, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>>, Func<string, int?, CancellationToken, Task<GatewayMintedClientSecret>>, string?)`


```csharp
public GatewayRealtimeModelConfig(string provider, string baseUrl, Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> getHeaders, Func<string, int?, CancellationToken, Task<GatewayMintedClientSecret>> createClientSecret, string? teamIdOrSlug = null)
```


Creates a config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `getHeaders` | [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `createClientSecret` | [Func&lt;string, int?, CancellationToken, Task&lt;GatewayMintedClientSecret&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-4) |  |
| `teamIdOrSlug` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L70)

## Properties

<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_BaseUrl_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; }
```


HTTP(S) base URL of the Gateway V4 routes.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L88)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_CreateClientSecret_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_CreateClientSecret"></a>

### CreateClientSecret


```csharp
public Func<string, int?, CancellationToken, Task<GatewayMintedClientSecret>> CreateClientSecret { get; }
```


Mints a short-lived client secret (`vcst_`) through the Gateway's `/v1/realtime/client-secrets` endpoint.
Arguments are the model id and `expiresAfterSeconds`. Implemented by the provider because minting needs the
long-lived Gateway credential.

**Value:** [Func&lt;string, int?, CancellationToken, Task&lt;GatewayMintedClientSecret&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-4)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L101)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_GetHeaders_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_GetHeaders"></a>

### GetHeaders


```csharp
public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> GetHeaders { get; }
```


Resolves the auth headers for a server-side connection.

**Value:** [Func&lt;CancellationToken, Task&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L94)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L85)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_TeamIdOrSlug_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelConfig_TeamIdOrSlug"></a>

### TeamIdOrSlug


```csharp
public string? TeamIdOrSlug { get; }
```


Optional Vercel team scope.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L91)
