---
title: "RealtimeModelV4ServerConnection"
description: "Descriptor for a server-initiated upstream WebSocket connection. Maps to RealtimeModelV4ServerConnection: the URL to dial plus the headers (and optional subp..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4ServerConnection"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4ServerConnection
```


Descriptor for a server-initiated upstream WebSocket connection. Maps to `RealtimeModelV4ServerConnection`:
the URL to dial plus the headers (and optional subprotocols) to send on the handshake. It carries the
long-lived provider credential in [Headers](#Vercel_AI_Provider_RealtimeModelV4ServerConnection_Headers) and is meant for a trusted server that owns the socket.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L26)

## Constructors

<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection__ctor_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `RealtimeModelV4ServerConnection(string, IReadOnlyDictionary<string, string>, IReadOnlyList<string>?)`


```csharp
public RealtimeModelV4ServerConnection(string url, IReadOnlyDictionary<string, string> headers, IReadOnlyList<string>? protocols = null)
```


Creates a server connection descriptor.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `protocols` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L29)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection_Headers_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers for the WebSocket handshake, including provider authentication.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L44)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection_Protocols_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection_Protocols"></a>

### Protocols


```csharp
public IReadOnlyList<string>? Protocols { get; }
```


Optional WebSocket subprotocols to negotiate.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L47)

<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection_Url_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ServerConnection_Url"></a>

### Url


```csharp
public string Url { get; }
```


The `wss://` (or `ws://`) URL, including provider-specific query parameters.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L41)
