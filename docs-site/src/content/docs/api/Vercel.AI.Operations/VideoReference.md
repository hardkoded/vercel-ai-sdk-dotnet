---
title: "VideoReference"
description: "Reference media, either raw content or an object with a media type."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoReference"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoReference"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoReference
```


Reference media, either raw content or an object with a media type.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L83)

## Constructors

<a id="Vercel_AI_Operations_VideoReference__ctor_"></a>
<a id="Vercel_AI_Operations_VideoReference__ctor_System_Object_"></a>

### VideoReference(object?)


```csharp
public VideoReference(object? data)
```


Creates a raw reference.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L86)

<a id="Vercel_AI_Operations_VideoReference__ctor_System_Object_System_String_"></a>

### VideoReference(object?, string?)


```csharp
public VideoReference(object? data, string? mediaType)
```


Creates an object-form reference.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L92)

## Properties

<a id="Vercel_AI_Operations_VideoReference_Data_"></a>
<a id="Vercel_AI_Operations_VideoReference_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Reference content.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L100)

<a id="Vercel_AI_Operations_VideoReference_HasMediaType_"></a>
<a id="Vercel_AI_Operations_VideoReference_HasMediaType"></a>

### HasMediaType


```csharp
public bool HasMediaType { get; }
```


True when the object form was used.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L106)

<a id="Vercel_AI_Operations_VideoReference_MediaType_"></a>
<a id="Vercel_AI_Operations_VideoReference_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Explicit media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L103)
