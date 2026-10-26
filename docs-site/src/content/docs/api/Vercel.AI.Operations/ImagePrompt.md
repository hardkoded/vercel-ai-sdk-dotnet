---
title: "ImagePrompt"
description: "A string prompt or an image edit prompt."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImagePrompt"
  hidden: true
---

<a id="Vercel_AI_Operations_ImagePrompt"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ImagePrompt
```


A string prompt or an image edit prompt.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L281)

## Constructors

<a id="Vercel_AI_Operations_ImagePrompt__ctor_"></a>
<a id="Vercel_AI_Operations_ImagePrompt__ctor_System_String_"></a>

### ImagePrompt(string)


```csharp
public ImagePrompt(string text)
```


Creates a text prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L284)

<a id="Vercel_AI_Operations_ImagePrompt__ctor_System_String_System_Collections_Generic_IReadOnlyList_System_Object__System_Object_"></a>

### `ImagePrompt(string?, IReadOnlyList<object?>, object?)`


```csharp
public ImagePrompt(string? text, IReadOnlyList<object?> images, object? mask = null)
```


Creates an edit prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `images` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `mask` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L290)

## Properties

<a id="Vercel_AI_Operations_ImagePrompt_HasImages_"></a>
<a id="Vercel_AI_Operations_ImagePrompt_HasImages"></a>

### HasImages


```csharp
public bool HasImages { get; }
```


True when the caller passed the object form.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L308)

<a id="Vercel_AI_Operations_ImagePrompt_Images_"></a>
<a id="Vercel_AI_Operations_ImagePrompt_Images"></a>

### Images


```csharp
public IReadOnlyList<object?> Images { get; }
```


Reference images when this is an edit prompt.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L302)

<a id="Vercel_AI_Operations_ImagePrompt_Mask_"></a>
<a id="Vercel_AI_Operations_ImagePrompt_Mask"></a>

### Mask


```csharp
public object? Mask { get; }
```


Optional mask.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L305)

<a id="Vercel_AI_Operations_ImagePrompt_Text_"></a>
<a id="Vercel_AI_Operations_ImagePrompt_Text"></a>

### Text


```csharp
public string? Text { get; }
```


Prompt text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L299)
