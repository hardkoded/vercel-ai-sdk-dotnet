---
title: "DownloadRequestItem"
description: "One URL passed to DefaultDownloadFunction."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadRequestItem"
  hidden: true
---

<a id="Vercel_AI_Util_DownloadRequestItem"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DownloadRequestItem
```


One URL passed to [DefaultDownloadFunction](/api/Vercel.AI.Util/DefaultDownloadFunction/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L856)

## Constructors

<a id="Vercel_AI_Util_DownloadRequestItem__ctor_"></a>
<a id="Vercel_AI_Util_DownloadRequestItem__ctor_System_Uri_System_Boolean_"></a>

### DownloadRequestItem(Uri, bool)


```csharp
public DownloadRequestItem(Uri url, bool isUrlSupportedByModel)
```


Creates a request item.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `isUrlSupportedByModel` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L859)

## Properties

<a id="Vercel_AI_Util_DownloadRequestItem_AbortSignal_"></a>
<a id="Vercel_AI_Util_DownloadRequestItem_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; init; }
```


Signal for cancellation and timeouts of the enclosing call.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L869)

<a id="Vercel_AI_Util_DownloadRequestItem_IsUrlSupportedByModel_"></a>
<a id="Vercel_AI_Util_DownloadRequestItem_IsUrlSupportedByModel"></a>

### IsUrlSupportedByModel


```csharp
public bool IsUrlSupportedByModel { get; }
```


When true, the model accepts the URL and the download function returns null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L872)

<a id="Vercel_AI_Util_DownloadRequestItem_Url_"></a>
<a id="Vercel_AI_Util_DownloadRequestItem_Url"></a>

### Url


```csharp
public Uri Url { get; }
```


URL to download or pass through.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L866)
