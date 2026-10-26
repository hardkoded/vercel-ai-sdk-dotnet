---
title: "VideoResult"
description: "Video generation result."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoResult"
  hidden: true
---

<a id="Vercel_AI_Provider_VideoResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class VideoResult
```


Video generation result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L389)

## Constructors

<a id="Vercel_AI_Provider_VideoResult__ctor_"></a>
<a id="Vercel_AI_Provider_VideoResult__ctor_System_String_System_Byte___System_String_"></a>

### VideoResult(string?, byte[]?, string)


```csharp
public VideoResult(string? url, byte[]? data, string mediaType)
```


Creates a video result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L392)

## Properties

<a id="Vercel_AI_Provider_VideoResult_Data_"></a>
<a id="Vercel_AI_Provider_VideoResult_Data"></a>

### Data


```csharp
public byte[]? Data { get; }
```


Inline video bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L403)

<a id="Vercel_AI_Provider_VideoResult_MediaType_"></a>
<a id="Vercel_AI_Provider_VideoResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L406)

<a id="Vercel_AI_Provider_VideoResult_Url_"></a>
<a id="Vercel_AI_Provider_VideoResult_Url"></a>

### Url


```csharp
public string? Url { get; }
```


Remote video URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L400)
