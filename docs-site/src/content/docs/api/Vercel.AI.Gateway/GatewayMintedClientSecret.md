---
title: "GatewayMintedClientSecret"
description: "A client secret minted through the Gateway. ExpiresAt is epoch seconds."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayMintedClientSecret"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayMintedClientSecret"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayMintedClientSecret
```


A client secret minted through the Gateway. [ExpiresAt](#Vercel_AI_Gateway_GatewayMintedClientSecret_ExpiresAt) is epoch seconds.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L12)

## Constructors

<a id="Vercel_AI_Gateway_GatewayMintedClientSecret__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayMintedClientSecret__ctor_System_String_System_Nullable_System_Int64__"></a>

### GatewayMintedClientSecret(string, long?)


```csharp
public GatewayMintedClientSecret(string token, long? expiresAt = null)
```


Creates a minted secret.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAt` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L15)

## Properties

<a id="Vercel_AI_Gateway_GatewayMintedClientSecret_ExpiresAt_"></a>
<a id="Vercel_AI_Gateway_GatewayMintedClientSecret_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


Expiry in epoch seconds, when the endpoint returned one.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L25)

<a id="Vercel_AI_Gateway_GatewayMintedClientSecret_Token_"></a>
<a id="Vercel_AI_Gateway_GatewayMintedClientSecret_Token"></a>

### Token


```csharp
public string Token { get; }
```


The `vcst_` token.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L22)
