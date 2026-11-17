---
title: "GatewayRealtimeClientSecret"
description: "A client secret together with the WebSocket URL to open. Maps to RealtimeModelV4ClientSecretResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeClientSecret"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayRealtimeClientSecret
```


A client secret together with the WebSocket URL to open. Maps to `RealtimeModelV4ClientSecretResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L29)

## Constructors

<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret__ctor_System_String_System_String_System_Nullable_System_Int64__"></a>

### GatewayRealtimeClientSecret(string, string, long?)


```csharp
public GatewayRealtimeClientSecret(string token, string url, long? expiresAt = null)
```


Creates a client secret result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAt` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L32)

## Properties

<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret_ExpiresAt_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


Expiry in epoch seconds, when known.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L46)

<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret_Token_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret_Token"></a>

### Token


```csharp
public string Token { get; }
```


The `vcst_` token.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L40)

<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret_Url_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeClientSecret_Url"></a>

### Url


```csharp
public string Url { get; }
```


The realtime WebSocket URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L43)
