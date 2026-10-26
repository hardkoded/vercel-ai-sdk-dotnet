---
title: "DownloadResult"
description: "Bytes and media type returned by Download.GetAsync."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadResult"
  hidden: true
---

<a id="Vercel_AI_Util_DownloadResult"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DownloadResult
```


Bytes and media type returned by `Download.GetAsync`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L57)

## Constructors

<a id="Vercel_AI_Util_DownloadResult__ctor_"></a>
<a id="Vercel_AI_Util_DownloadResult__ctor_System_Byte___System_String_"></a>

### DownloadResult(byte[]?, string?)


```csharp
public DownloadResult(byte[]? data, string? mediaType)
```


Creates a download result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L60)

## Properties

<a id="Vercel_AI_Util_DownloadResult_Data_"></a>
<a id="Vercel_AI_Util_DownloadResult_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


Response body.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L67)

<a id="Vercel_AI_Util_DownloadResult_MediaType_"></a>
<a id="Vercel_AI_Util_DownloadResult_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Content-Type header, when present.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L70)
