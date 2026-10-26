---
title: "DownloadedMedia"
description: "Downloaded media."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadedMedia"
  hidden: true
---

<a id="Vercel_AI_Operations_DownloadedMedia"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DownloadedMedia
```


Downloaded media.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L10)

## Constructors

<a id="Vercel_AI_Operations_DownloadedMedia__ctor_"></a>
<a id="Vercel_AI_Operations_DownloadedMedia__ctor_System_Byte___System_String_"></a>

### DownloadedMedia(byte[], string?)


```csharp
public DownloadedMedia(byte[] data, string? mediaType)
```


Creates a download.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L13)

## Properties

<a id="Vercel_AI_Operations_DownloadedMedia_Data_"></a>
<a id="Vercel_AI_Operations_DownloadedMedia_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


Bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L20)

<a id="Vercel_AI_Operations_DownloadedMedia_MediaType_"></a>
<a id="Vercel_AI_Operations_DownloadedMedia_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Content type reported by the download.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L23)
