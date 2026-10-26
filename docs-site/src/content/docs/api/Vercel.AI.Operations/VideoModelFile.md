---
title: "VideoModelFile"
description: "A file or URL passed to a video model."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoModelFile"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoModelFile"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoModelFile
```


A file or URL passed to a video model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L13)

## Properties

<a id="Vercel_AI_Operations_VideoModelFile_Data_"></a>
<a id="Vercel_AI_Operations_VideoModelFile_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L42)

<a id="Vercel_AI_Operations_VideoModelFile_MediaType_"></a>
<a id="Vercel_AI_Operations_VideoModelFile_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L45)

<a id="Vercel_AI_Operations_VideoModelFile_Type_"></a>
<a id="Vercel_AI_Operations_VideoModelFile_Type"></a>

### Type


```csharp
public string Type { get; }
```


`url` or `file`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L36)

<a id="Vercel_AI_Operations_VideoModelFile_Url_"></a>
<a id="Vercel_AI_Operations_VideoModelFile_Url"></a>

### Url


```csharp
public string? Url { get; }
```


URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L39)

## Methods

<a id="Vercel_AI_Operations_VideoModelFile_FromFile_"></a>
<a id="Vercel_AI_Operations_VideoModelFile_FromFile_System_Byte___System_String_"></a>

### FromFile(byte[], string)


```csharp
public static VideoModelFile FromFile(byte[] data, string mediaType)
```


Creates a binary file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [VideoModelFile](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L22)

<a id="Vercel_AI_Operations_VideoModelFile_FromUrl_"></a>
<a id="Vercel_AI_Operations_VideoModelFile_FromUrl_System_String_System_String_"></a>

### FromUrl(string, string?)


```csharp
public static VideoModelFile FromUrl(string url, string? mediaType = null)
```


Creates a URL file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [VideoModelFile](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L16)
