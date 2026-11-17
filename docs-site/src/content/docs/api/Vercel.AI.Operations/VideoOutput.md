---
title: "VideoOutput"
description: "One generated video payload from the model."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoOutput"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoOutput"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoOutput
```


One generated video payload from the model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L152)

## Constructors

<a id="Vercel_AI_Operations_VideoOutput__ctor_"></a>
<a id="Vercel_AI_Operations_VideoOutput__ctor_System_String_System_String_System_Object_System_String_"></a>

### VideoOutput(string, string?, object?, string?)


```csharp
public VideoOutput(string type, string? url = null, object? data = null, string? mediaType = null)
```


Creates an output.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L155)

## Properties

<a id="Vercel_AI_Operations_VideoOutput_Data_"></a>
<a id="Vercel_AI_Operations_VideoOutput_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Base64 string or bytes.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L175)

<a id="Vercel_AI_Operations_VideoOutput_MediaType_"></a>
<a id="Vercel_AI_Operations_VideoOutput_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Provider media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L178)

<a id="Vercel_AI_Operations_VideoOutput_Type_"></a>
<a id="Vercel_AI_Operations_VideoOutput_Type"></a>

### Type


```csharp
public string Type { get; }
```


`url`, `base64`, `binary`, or `uploaded`. An `uploaded` output confirms the provider delivered the video to caller-managed storage. It has no URL or bytes, and its URL must never contain a signed upload URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L169)

<a id="Vercel_AI_Operations_VideoOutput_Url_"></a>
<a id="Vercel_AI_Operations_VideoOutput_Url"></a>

### Url


```csharp
public string? Url { get; }
```


URL for `url` outputs.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L172)
