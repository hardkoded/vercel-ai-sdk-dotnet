---
title: "Download"
description: "Downloads a URL with SSRF checks on every redirect hop."
editUrl: false
prev: false
next: false
sidebar:
  label: "Download"
  hidden: true
---

<a id="Vercel_AI_Util_Download"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class Download
```


Downloads a URL with SSRF checks on every redirect hop.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L189)

## Fields

<a id="Vercel_AI_Util_Download_DefaultMaxBytes"></a>

### DefaultMaxBytes


```csharp
public const long DefaultMaxBytes = 2147483648
```


Default maximum download size: 2 GiB.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L192)

## Properties

<a id="Vercel_AI_Util_Download_Fetch_"></a>
<a id="Vercel_AI_Util_Download_Fetch"></a>

### Fetch


```csharp
public static DownloadFetch? Fetch { get; set; }
```


Fetch used when a call does not pass its own. Read on each download so a replacement is picked up immediately.

**Value:** [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L199)

<a id="Vercel_AI_Util_Download_IsBrowser_"></a>
<a id="Vercel_AI_Util_Download_IsBrowser"></a>

### IsBrowser


```csharp
public static bool IsBrowser { get; set; }
```


When true, an opaque redirect is followed natively. Server runtimes leave this false and fail closed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L204)

<a id="Vercel_AI_Util_Download_UserAgent_"></a>
<a id="Vercel_AI_Util_Download_UserAgent"></a>

### UserAgent


```csharp
public static string? UserAgent { get; set; }
```


User-Agent suffix sent on download requests.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L207)

## Methods

<a id="Vercel_AI_Util_Download_FetchWithValidatedEndpointAsync_"></a>
<a id="Vercel_AI_Util_Download_FetchWithValidatedEndpointAsync_System_String_Vercel_AI_Util_DownloadRequest_Vercel_AI_Util_DownloadFetch_System_String_System_String_"></a>

### FetchWithValidatedEndpointAsync(string, DownloadRequest?, DownloadFetch?, string?, string)


```csharp
public static Task<DownloadResponse> FetchWithValidatedEndpointAsync(string url, DownloadRequest? request = null, DownloadFetch? fetch = null, string? trustedOrigin = null, string redirect = "error")
```


Fetches one validated URL without following redirects. A redirect is rejected by default (`error`). A caller that passes
`manual` must validate the `Location` target before it requests another URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) | URL to request. |
| `request` | [DownloadRequest](/api/Vercel.AI.Util/DownloadRequest/) | Method, headers, body, and cancellation signal. Its [Redirect](/api/Vercel.AI.Util/DownloadRequest/#Vercel_AI_Util_DownloadRequest_Redirect) is replaced by &lt;code class="paramref"&gt;redirect&lt;/code&gt;. |
| `fetch` | [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/) | Fetch used for the request. Defaults to [Fetch](#Vercel_AI_Util_Download_Fetch), then the built-in fetch. |
| `trustedOrigin` | [string](https://learn.microsoft.com/dotnet/api/system.string) | A developer-configured origin whose requests skip validation. Never derive it from response data. |
| `redirect` | [string](https://learn.microsoft.com/dotnet/api/system.string) | `error` or `manual`. |

**Returns:** [Task&lt;DownloadResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | The URL is unsafe. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L346)

<a id="Vercel_AI_Util_Download_FetchWithValidatedRedirectsAsync_"></a>
<a id="Vercel_AI_Util_Download_FetchWithValidatedRedirectsAsync_System_String_Vercel_AI_Util_HeaderCollection_System_Threading_CancellationToken_System_Int32_Vercel_AI_Util_DownloadFetch_System_String_"></a>

### FetchWithValidatedRedirectsAsync(string, HeaderCollection?, CancellationToken, int, DownloadFetch?, string?)


```csharp
public static Task<DownloadResponse> FetchWithValidatedRedirectsAsync(string url, HeaderCollection? headers = null, CancellationToken abortSignal = default, int maxRedirects = 10, DownloadFetch? fetch = null, string? trustedOrigin = null)
```


Fetches a URL and validates every hop with [ValidateDownloadUrl(string)](#Vercel_AI_Util_Download_ValidateDownloadUrl_System_String_) before it is requested.
Redirects are followed one hop at a time. All caller headers except `user-agent` are dropped on a redirect that
crosses origin. Each hop gets its own copy of the headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) | First URL to request. |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) | Request headers, or `null` to send none. |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancellation signal passed to every hop. |
| `maxRedirects` | [int](https://learn.microsoft.com/dotnet/api/system.int32) | Largest number of redirects to follow. |
| `fetch` | [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/) | Fetch used for every hop. Defaults to [Fetch](#Vercel_AI_Util_Download_Fetch), then the built-in fetch. |
| `trustedOrigin` | [string](https://learn.microsoft.com/dotnet/api/system.string) | A developer-configured origin, such as a provider base URL, whose hops skip validation. Never derive it from response data. |

**Returns:** [Task&lt;DownloadResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1) — The final response. The caller checks [Ok](/api/Vercel.AI.Util/DownloadResponse/#Vercel_AI_Util_DownloadResponse_Ok) and reads the body.

**Exceptions**

| Type | Condition |
| --- | --- |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | A hop is unsafe, the redirect limit is exceeded, or a redirect cannot be validated. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L325)

<a id="Vercel_AI_Util_Download_GetAsync_"></a>
<a id="Vercel_AI_Util_Download_GetAsync_System_Uri_System_Nullable_System_Int64__System_Threading_CancellationToken_Vercel_AI_Util_DownloadFetch_"></a>

### GetAsync(Uri, long?, CancellationToken, DownloadFetch?)


```csharp
public static Task<DownloadResult> GetAsync(Uri url, long? maxBytes = null, CancellationToken abortSignal = default, DownloadFetch? fetch = null)
```


Downloads &lt;code class="paramref"&gt;url&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `maxBytes` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `fetch` | [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/) |  |

**Returns:** [Task&lt;DownloadResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L210)

<a id="Vercel_AI_Util_Download_ValidateDownloadUrl_"></a>
<a id="Vercel_AI_Util_Download_ValidateDownloadUrl_System_String_"></a>

### ValidateDownloadUrl(string)


```csharp
public static void ValidateDownloadUrl(string url)
```


Throws [DownloadError](/api/Vercel.AI.Util/DownloadError/) when &lt;code class="paramref"&gt;url&lt;/code&gt; is not safe to request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L222)
