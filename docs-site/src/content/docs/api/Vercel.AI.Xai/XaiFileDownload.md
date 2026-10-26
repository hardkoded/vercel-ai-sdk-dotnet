---
title: "XaiFileDownload"
description: "Downloaded xAI file bytes."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiFileDownload"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiFileDownload"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiFileDownload
```


Downloaded xAI file bytes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L127)

## Constructors

<a id="Vercel_AI_Xai_XaiFileDownload__ctor_"></a>
<a id="Vercel_AI_Xai_XaiFileDownload__ctor_System_IO_Stream_System_String_"></a>

### XaiFileDownload(Stream, string?)


```csharp
public XaiFileDownload(Stream content, string? mediaType)
```


Creates a download.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L130)

## Properties

<a id="Vercel_AI_Xai_XaiFileDownload_Content_"></a>
<a id="Vercel_AI_Xai_XaiFileDownload_Content"></a>

### Content


```csharp
public Stream Content { get; }
```


File contents.

**Value:** [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L141)

<a id="Vercel_AI_Xai_XaiFileDownload_MediaType_"></a>
<a id="Vercel_AI_Xai_XaiFileDownload_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Content type with parameters removed. Null when the response has none.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L144)

<a id="Vercel_AI_Xai_XaiFileDownload_Warnings_"></a>
<a id="Vercel_AI_Xai_XaiFileDownload_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Always empty.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L138)
