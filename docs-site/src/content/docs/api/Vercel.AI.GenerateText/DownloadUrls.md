---
title: "DownloadUrls"
description: "Rejects unsafe file URLs before a download starts. Maps to validateDownloadUrl."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadUrls"
  hidden: true
---

<a id="Vercel_AI_GenerateText_DownloadUrls"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class DownloadUrls
```


Rejects unsafe file URLs before a download starts. Maps to `validateDownloadUrl`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L50)

## Methods

<a id="Vercel_AI_GenerateText_DownloadUrls_ValidateDownloadUrl_"></a>
<a id="Vercel_AI_GenerateText_DownloadUrls_ValidateDownloadUrl_System_String_"></a>

### ValidateDownloadUrl(string)


```csharp
public static void ValidateDownloadUrl(string url)
```


Throws [DownloadException](/api/Vercel.AI.GenerateText/DownloadException/) when &lt;code class="paramref"&gt;url&lt;/code&gt; is not safe to download.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L53)

<a id="Vercel_AI_GenerateText_DownloadUrls_ValidateDownloadUrl_System_Uri_"></a>

### ValidateDownloadUrl(Uri)


```csharp
public static void ValidateDownloadUrl(Uri url)
```


Throws [DownloadException](/api/Vercel.AI.GenerateText/DownloadException/) when &lt;code class="paramref"&gt;url&lt;/code&gt; is not safe to download.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L69)
