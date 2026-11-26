---
title: "GuardedFetch"
description: "An opt-in download transport that validates every resolved address and pins the connection to the validated address. Maps to createGuardedFetch of the guarde..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GuardedFetch"
  hidden: true
---

<a id="Vercel_AI_Util_GuardedFetch"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class GuardedFetch : IDisposable
```


An opt-in download transport that validates every resolved address and pins the connection to the validated address.
Maps to `createGuardedFetch` of the `guarded-fetch` package. On net10.0 the check runs inside the connect
callback, so a DNS answer cannot change between validation and connection. On netstandard2.0 the addresses are resolved
and validated before the request, and the runtime resolves again when it connects, so rebinding is not prevented there.
Pass it as the `fetch` of `DownloadFetch)`. Dispose it to release pooled connections.
It does not use a proxy, because a proxy would resolve the target itself. Restrict egress at the platform level to use one.
Both targets throw [DownloadError](/api/Vercel.AI.Util/DownloadError/) for an unsafe address.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/GuardedFetch.cs#L21)

## Constructors

<a id="Vercel_AI_Util_GuardedFetch__ctor_"></a>
<a id="Vercel_AI_Util_GuardedFetch__ctor_System_Collections_Generic_IEnumerable_System_String__"></a>

### `GuardedFetch(IEnumerable<string>?)`


```csharp
public GuardedFetch(IEnumerable<string>? trustedOrigins = null)
```


Creates a guarded transport.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `trustedOrigins` | [IEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) | Developer-configured origins, such as `http://localhost:11434`, whose connections skip address validation. Never build them from response data. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/GuardedFetch.cs#L32)

## Properties

<a id="Vercel_AI_Util_GuardedFetch_Client_"></a>
<a id="Vercel_AI_Util_GuardedFetch_Client"></a>

### Client


```csharp
public HttpClient Client { get; }
```


The pinned client behind this transport. Pass it to a provider or MCP transport that takes an [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient), so its
API calls use the same validated connections. It does not follow redirects. Disposing the transport disposes it.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/GuardedFetch.cs#L60)

## Methods

<a id="Vercel_AI_Util_GuardedFetch_Dispose_"></a>
<a id="Vercel_AI_Util_GuardedFetch_Dispose"></a>

### Dispose()


```csharp
public void Dispose()
```


Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/GuardedFetch.cs#L102)

<a id="Vercel_AI_Util_GuardedFetch_FetchAsync_"></a>
<a id="Vercel_AI_Util_GuardedFetch_FetchAsync_System_String_Vercel_AI_Util_DownloadRequest_"></a>

### FetchAsync(string, DownloadRequest)


```csharp
public Task<DownloadResponse> FetchAsync(string url, DownloadRequest request)
```


Requests &lt;code class="paramref"&gt;url&lt;/code&gt; with validated, pinned addresses.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `request` | [DownloadRequest](/api/Vercel.AI.Util/DownloadRequest/) |  |

**Returns:** [Task&lt;DownloadResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | The host resolves to a local, private, or reserved address. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/GuardedFetch.cs#L64)

<a id="Vercel_AI_Util_GuardedFetch_IsSafeIpAddress_"></a>
<a id="Vercel_AI_Util_GuardedFetch_IsSafeIpAddress_System_String_"></a>

### IsSafeIpAddress(string)


```csharp
public static bool IsSafeIpAddress(string address)
```


Returns whether &lt;code class="paramref"&gt;address&lt;/code&gt; is a public address. Maps to `isSafeIpAddress` of `guarded-fetch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `address` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/GuardedFetch.cs#L110)

## Operators

<a id="Vercel_AI_Util_GuardedFetch_op_Implicit_"></a>
<a id="Vercel_AI_Util_GuardedFetch_op_Implicit_Vercel_AI_Util_GuardedFetch__Vercel_AI_Util_DownloadFetch"></a>

### implicit operator DownloadFetch(GuardedFetch)


```csharp
public static implicit operator DownloadFetch(GuardedFetch fetch)
```


Converts the transport to the [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/) delegate.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fetch` | [GuardedFetch](./) |  |

**Returns:** [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/GuardedFetch.cs#L91)
