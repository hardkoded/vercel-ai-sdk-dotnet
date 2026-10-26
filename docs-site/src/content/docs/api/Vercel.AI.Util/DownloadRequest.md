---
title: "DownloadRequest"
description: "Options passed to a download fetch implementation."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadRequest"
  hidden: true
---

<a id="Vercel_AI_Util_DownloadRequest"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DownloadRequest
```


Options passed to a download fetch implementation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L74)

## Constructors

<a id="Vercel_AI_Util_DownloadRequest__ctor_"></a>
<a id="Vercel_AI_Util_DownloadRequest__ctor_Vercel_AI_Util_HeaderCollection_System_Threading_CancellationToken_System_String_"></a>

### DownloadRequest(HeaderCollection?, CancellationToken, string)


```csharp
public DownloadRequest(HeaderCollection? headers, CancellationToken abortSignal, string redirect)
```


Creates the request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `redirect` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L77)

## Properties

<a id="Vercel_AI_Util_DownloadRequest_AbortSignal_"></a>
<a id="Vercel_AI_Util_DownloadRequest_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; }
```


Cancellation signal for the attempt.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L88)

<a id="Vercel_AI_Util_DownloadRequest_Body_"></a>
<a id="Vercel_AI_Util_DownloadRequest_Body"></a>

### Body


```csharp
public byte[]? Body { get; set; }
```


Request body, or `null` for none.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L97)

<a id="Vercel_AI_Util_DownloadRequest_Headers_"></a>
<a id="Vercel_AI_Util_DownloadRequest_Headers"></a>

### Headers


```csharp
public HeaderCollection Headers { get; }
```


Request headers.

**Value:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L85)

<a id="Vercel_AI_Util_DownloadRequest_MaxBytes_"></a>
<a id="Vercel_AI_Util_DownloadRequest_MaxBytes"></a>

### MaxBytes


```csharp
public long MaxBytes { get; set; }
```


Largest body the fetch may read. The default fetch stops reading past it.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L100)

<a id="Vercel_AI_Util_DownloadRequest_Method_"></a>
<a id="Vercel_AI_Util_DownloadRequest_Method"></a>

### Method


```csharp
public string Method { get; set; }
```


HTTP method. Defaults to `GET`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L94)

<a id="Vercel_AI_Util_DownloadRequest_Redirect_"></a>
<a id="Vercel_AI_Util_DownloadRequest_Redirect"></a>

### Redirect


```csharp
public string Redirect { get; }
```


`manual`, `follow`, or `error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L91)
