---
title: "AssetDownloadCall"
description: "Arguments passed to a single asset download."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssetDownloadCall"
  hidden: true
---

<a id="Vercel_AI_Util_AssetDownloadCall"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class AssetDownloadCall
```


Arguments passed to a single asset download.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L777)

## Constructors

<a id="Vercel_AI_Util_AssetDownloadCall__ctor_"></a>
<a id="Vercel_AI_Util_AssetDownloadCall__ctor_System_Uri_System_Boolean_System_Threading_CancellationToken_"></a>

### AssetDownloadCall(Uri, bool, CancellationToken)


```csharp
public AssetDownloadCall(Uri url, bool isUrlSupportedByModel, CancellationToken abortSignal)
```


Creates the call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `isUrlSupportedByModel` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L780)

## Properties

<a id="Vercel_AI_Util_AssetDownloadCall_AbortSignal_"></a>
<a id="Vercel_AI_Util_AssetDownloadCall_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; }
```


Cancellation signal forwarded from the caller.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L794)

<a id="Vercel_AI_Util_AssetDownloadCall_IsUrlSupportedByModel_"></a>
<a id="Vercel_AI_Util_AssetDownloadCall_IsUrlSupportedByModel"></a>

### IsUrlSupportedByModel


```csharp
public bool IsUrlSupportedByModel { get; }
```


Whether the model can read the URL itself.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L791)

<a id="Vercel_AI_Util_AssetDownloadCall_Url_"></a>
<a id="Vercel_AI_Util_AssetDownloadCall_Url"></a>

### Url


```csharp
public Uri Url { get; }
```


URL to download.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L788)
