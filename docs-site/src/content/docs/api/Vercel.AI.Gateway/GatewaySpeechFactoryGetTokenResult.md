---
title: "GatewaySpeechFactoryGetTokenResult"
description: "The result of minting a streaming speech client secret. Maps to GatewaySpeechFactoryGetTokenResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewaySpeechFactoryGetTokenResult"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewaySpeechFactoryGetTokenResult
```


The result of minting a streaming speech client secret. Maps to `GatewaySpeechFactoryGetTokenResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L853)

## Constructors

<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult__ctor_System_String_System_String_System_Nullable_System_Int64__"></a>

### GatewaySpeechFactoryGetTokenResult(string, string, long?)


```csharp
public GatewaySpeechFactoryGetTokenResult(string token, string url, long? expiresAt = null)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAt` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L856)

## Properties

<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult_ExpiresAt_"></a>
<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


Token expiry, epoch seconds.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L870)

<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult_Token_"></a>
<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult_Token"></a>

### Token


```csharp
public string Token { get; }
```


The minted `vcst_` client secret.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L864)

<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult_Url_"></a>
<a id="Vercel_AI_Gateway_GatewaySpeechFactoryGetTokenResult_Url"></a>

### Url


```csharp
public string Url { get; }
```


WebSocket URL of the streaming speech surface for this model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L867)
