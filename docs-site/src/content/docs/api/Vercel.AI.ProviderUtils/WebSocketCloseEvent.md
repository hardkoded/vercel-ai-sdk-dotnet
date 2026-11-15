---
title: "WebSocketCloseEvent"
description: "Close diagnostics carried by OnClose."
editUrl: false
prev: false
next: false
sidebar:
  label: "WebSocketCloseEvent"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_WebSocketCloseEvent"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class WebSocketCloseEvent
```


Close diagnostics carried by [OnClose](/api/Vercel.AI.ProviderUtils/IWebSocketLike/#Vercel_AI_ProviderUtils_IWebSocketLike_OnClose).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L11)

## Constructors

<a id="Vercel_AI_ProviderUtils_WebSocketCloseEvent__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketCloseEvent__ctor_System_Nullable_System_Int32__System_String_"></a>

### WebSocketCloseEvent(int?, string?)


```csharp
public WebSocketCloseEvent(int? code, string? reason)
```


Creates a close event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `code` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L14)

## Properties

<a id="Vercel_AI_ProviderUtils_WebSocketCloseEvent_Code_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketCloseEvent_Code"></a>

### Code


```csharp
public int? Code { get; }
```


Close code, when the transport provides one.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L21)

<a id="Vercel_AI_ProviderUtils_WebSocketCloseEvent_Reason_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketCloseEvent_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Close reason, when the transport provides one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L24)
