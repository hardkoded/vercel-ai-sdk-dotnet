---
title: "ImagePromptParts"
description: "Normalized image prompt fields."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImagePromptParts"
  hidden: true
---

<a id="Vercel_AI_Operations_ImagePromptParts"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ImagePromptParts
```


Normalized image prompt fields.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L677)

## Constructors

<a id="Vercel_AI_Operations_ImagePromptParts__ctor_"></a>
<a id="Vercel_AI_Operations_ImagePromptParts__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ImageModelFile__Vercel_AI_Operations_ImageModelFile_"></a>

### `ImagePromptParts(string?, IReadOnlyList<ImageModelFile>?, ImageModelFile?)`


```csharp
public ImagePromptParts(string? prompt, IReadOnlyList<ImageModelFile>? files, ImageModelFile? mask)
```


Creates normalized prompt fields.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `files` | [IReadOnlyList&lt;ImageModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `mask` | [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L680)

## Properties

<a id="Vercel_AI_Operations_ImagePromptParts_Files_"></a>
<a id="Vercel_AI_Operations_ImagePromptParts_Files"></a>

### Files


```csharp
public IReadOnlyList<ImageModelFile>? Files { get; }
```


Reference files.

**Value:** [IReadOnlyList&lt;ImageModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L691)

<a id="Vercel_AI_Operations_ImagePromptParts_Mask_"></a>
<a id="Vercel_AI_Operations_ImagePromptParts_Mask"></a>

### Mask


```csharp
public ImageModelFile? Mask { get; }
```


Mask file.

**Value:** [ImageModelFile](/api/Vercel.AI.Operations/ImageModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L694)

<a id="Vercel_AI_Operations_ImagePromptParts_Prompt_"></a>
<a id="Vercel_AI_Operations_ImagePromptParts_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; }
```


Prompt text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L688)
