---
title: "ImageCapabilities"
description: "Upstream ModelCapabilities.image."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageCapabilities"
  hidden: true
---

<a id="Vercel_AI_ImageCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ImageCapabilities
```


Upstream `ModelCapabilities.image`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L170)

## Properties

<a id="Vercel_AI_ImageCapabilities_Description_"></a>
<a id="Vercel_AI_ImageCapabilities_Description"></a>

### Description


```csharp
public string? Description { get; init; }
```


A human-readable model description. Not in the upstream type; upstream Fal entries spread it into `capabilities.image`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L173)

<a id="Vercel_AI_ImageCapabilities_FileInput_"></a>
<a id="Vercel_AI_ImageCapabilities_FileInput"></a>

### FileInput


```csharp
public ModelCapability FileInput { get; init; }
```


Upstream `fileInput`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L182)

<a id="Vercel_AI_ImageCapabilities_MaskInput_"></a>
<a id="Vercel_AI_ImageCapabilities_MaskInput"></a>

### MaskInput


```csharp
public ModelCapability MaskInput { get; init; }
```


Upstream `maskInput`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L185)

<a id="Vercel_AI_ImageCapabilities_MaxImagesPerCall_"></a>
<a id="Vercel_AI_ImageCapabilities_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; init; }
```


Upstream `maxImagesPerCall`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L188)

<a id="Vercel_AI_ImageCapabilities_Sizes_"></a>
<a id="Vercel_AI_ImageCapabilities_Sizes"></a>

### Sizes


```csharp
public IReadOnlyList<string>? Sizes { get; init; }
```


Upstream `sizes`, when it is a list. Null when [SizesDescription](#Vercel_AI_ImageCapabilities_SizesDescription) is used.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L176)

<a id="Vercel_AI_ImageCapabilities_SizesDescription_"></a>
<a id="Vercel_AI_ImageCapabilities_SizesDescription"></a>

### SizesDescription


```csharp
public string? SizesDescription { get; init; }
```


Upstream `sizes`, when it is `{ description }`. Null when [Sizes](#Vercel_AI_ImageCapabilities_Sizes) is used.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L179)
