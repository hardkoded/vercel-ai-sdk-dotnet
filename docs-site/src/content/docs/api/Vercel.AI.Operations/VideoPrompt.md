---
title: "VideoPrompt"
description: "Text prompt or a prompt with a start image."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoPrompt"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoPrompt"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoPrompt
```


Text prompt or a prompt with a start image.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L110)

## Constructors

<a id="Vercel_AI_Operations_VideoPrompt__ctor_"></a>
<a id="Vercel_AI_Operations_VideoPrompt__ctor_System_String_"></a>

### VideoPrompt(string?)


```csharp
public VideoPrompt(string? text)
```


Creates a text prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L113)

<a id="Vercel_AI_Operations_VideoPrompt__ctor_System_String_System_Object_"></a>

### VideoPrompt(string?, object?)


```csharp
public VideoPrompt(string? text, object? image)
```


Creates a prompt with an image.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `image` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L119)

## Properties

<a id="Vercel_AI_Operations_VideoPrompt_HasImage_"></a>
<a id="Vercel_AI_Operations_VideoPrompt_HasImage"></a>

### HasImage


```csharp
public bool HasImage { get; }
```


True when the object form was used.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L133)

<a id="Vercel_AI_Operations_VideoPrompt_Image_"></a>
<a id="Vercel_AI_Operations_VideoPrompt_Image"></a>

### Image


```csharp
public object? Image { get; }
```


Start image.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L130)

<a id="Vercel_AI_Operations_VideoPrompt_Text_"></a>
<a id="Vercel_AI_Operations_VideoPrompt_Text"></a>

### Text


```csharp
public string? Text { get; }
```


Prompt text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L127)
