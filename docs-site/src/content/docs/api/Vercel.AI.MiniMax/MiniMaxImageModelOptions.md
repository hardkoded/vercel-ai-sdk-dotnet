---
title: "MiniMaxImageModelOptions"
description: "Provider options for MiniMax image generation and editing. Pass them under minimax (upstream MiniMaxImageModelOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxImageModelOptions"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxImageModelOptions
```


Provider options for MiniMax image generation and editing. Pass them under `minimax` (upstream `MiniMaxImageModelOptions`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModelOptions.cs#L10)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_Area_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_Area"></a>

### Area


```csharp
[JsonPropertyName("area")]
public string? Area { get; set; }
```


Output area when a pixel size is not supplied: `1K`, `2K`, or `4K`. Defaults to 4K.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModelOptions.cs#L19)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_DisablePromptEnhanced_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_DisablePromptEnhanced"></a>

### DisablePromptEnhanced


```csharp
[JsonPropertyName("disablePromptEnhanced")]
public bool? DisablePromptEnhanced { get; set; }
```


Disable prompt enhancement for text-to-image requests.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModelOptions.cs#L23)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_MiniMaxImageAreas_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_MiniMaxImageAreas"></a>

### MiniMaxImageAreas


```csharp
public static IReadOnlyList<string> MiniMaxImageAreas { get; }
```


Output areas accepted by [Area](#Vercel_AI_MiniMax_MiniMaxImageModelOptions_Area).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModelOptions.cs#L13)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_MiniMaxImageOutputFormats_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_MiniMaxImageOutputFormats"></a>

### MiniMaxImageOutputFormats


```csharp
public static IReadOnlyList<string> MiniMaxImageOutputFormats { get; }
```


Output formats accepted by [OutputFormat](#Vercel_AI_MiniMax_MiniMaxImageModelOptions_OutputFormat).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModelOptions.cs#L16)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_OutputCompression_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_OutputCompression"></a>

### OutputCompression


```csharp
[JsonPropertyName("outputCompression")]
public int? OutputCompression { get; set; }
```


Compression quality for JPEG and WebP output, from 0 to 100.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModelOptions.cs#L27)

<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_OutputFormat_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxImageModelOptions_OutputFormat"></a>

### OutputFormat


```csharp
[JsonPropertyName("outputFormat")]
public string? OutputFormat { get; set; }
```


Generated image format: `jpeg`, `jpg`, `png`, or `webp`. Defaults to JPEG.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxImageModelOptions.cs#L31)
