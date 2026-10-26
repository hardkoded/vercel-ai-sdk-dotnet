---
title: "XaiClientSecret"
description: "Client secret returned by /realtime/clientsecrets."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiClientSecret"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiClientSecret"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiClientSecret
```


Client secret returned by `/realtime/client_secrets`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L13)

## Constructors

<a id="Vercel_AI_Xai_XaiClientSecret__ctor_"></a>
<a id="Vercel_AI_Xai_XaiClientSecret__ctor_System_String_System_String_System_Nullable_System_Int64__"></a>

### XaiClientSecret(string, string, long?)


```csharp
public XaiClientSecret(string token, string url, long? expiresAt)
```


Creates a client secret.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAt` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L16)

## Properties

<a id="Vercel_AI_Xai_XaiClientSecret_ExpiresAt_"></a>
<a id="Vercel_AI_Xai_XaiClientSecret_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


Unix expiration, when the response includes one.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L30)

<a id="Vercel_AI_Xai_XaiClientSecret_Token_"></a>
<a id="Vercel_AI_Xai_XaiClientSecret_Token"></a>

### Token


```csharp
public string Token { get; }
```


Secret token.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L24)

<a id="Vercel_AI_Xai_XaiClientSecret_Url_"></a>
<a id="Vercel_AI_Xai_XaiClientSecret_Url"></a>

### Url


```csharp
public string Url { get; }
```


WebSocket URL. The model is a query parameter.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L27)
