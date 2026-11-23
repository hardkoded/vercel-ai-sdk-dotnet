---
title: "GatewayTranscriptionFactoryGetTokenResult"
description: "The result of minting a streaming transcription client secret. Maps to GatewayTranscriptionFactoryGetTokenResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayTranscriptionFactoryGetTokenResult"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayTranscriptionFactoryGetTokenResult
```


The result of minting a streaming transcription client secret. Maps to `GatewayTranscriptionFactoryGetTokenResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L832)

## Constructors

<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult__ctor_System_String_System_String_System_Nullable_System_Int64__"></a>

### GatewayTranscriptionFactoryGetTokenResult(string, string, long?)


```csharp
public GatewayTranscriptionFactoryGetTokenResult(string token, string url, long? expiresAt = null)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAt` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L835)

## Properties

<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult_ExpiresAt_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


Token expiry, epoch seconds.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L853)

<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult_Token_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult_Token"></a>

### Token


```csharp
public string Token { get; }
```


The minted `vcst_` client secret.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L847)

<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult_Url_"></a>
<a id="Vercel_AI_Gateway_GatewayTranscriptionFactoryGetTokenResult_Url"></a>

### Url


```csharp
public string Url { get; }
```


WebSocket URL of the streaming transcription surface for this model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L850)
