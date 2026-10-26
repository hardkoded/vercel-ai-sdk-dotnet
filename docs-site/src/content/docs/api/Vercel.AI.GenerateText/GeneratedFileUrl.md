---
title: "GeneratedFileUrl"
description: "A generated file that still points at a URL."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedFileUrl"
  hidden: true
---

<a id="Vercel_AI_GenerateText_GeneratedFileUrl"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class GeneratedFileUrl : GeneratedContent
```


A generated file that still points at a URL.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L11)

## Constructors

<a id="Vercel_AI_GenerateText_GeneratedFileUrl__ctor_"></a>
<a id="Vercel_AI_GenerateText_GeneratedFileUrl__ctor_System_Uri_System_String_"></a>

### GeneratedFileUrl(Uri, string)


```csharp
public GeneratedFileUrl(Uri url, string mediaType)
```


Creates a URL-backed file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L14)

## Properties

<a id="Vercel_AI_GenerateText_GeneratedFileUrl_MediaType_"></a>
<a id="Vercel_AI_GenerateText_GeneratedFileUrl_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L25)

<a id="Vercel_AI_GenerateText_GeneratedFileUrl_Url_"></a>
<a id="Vercel_AI_GenerateText_GeneratedFileUrl_Url"></a>

### Url


```csharp
public Uri Url { get; }
```


File URL.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L22)
