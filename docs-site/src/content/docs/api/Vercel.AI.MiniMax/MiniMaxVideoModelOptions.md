---
title: "MiniMaxVideoModelOptions"
description: "Provider options for MiniMax video generation. Pass them under minimax."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxVideoModelOptions"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxVideoModelOptions
```


Provider options for MiniMax video generation. Pass them under `minimax`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L10)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_AigcWatermark_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_AigcWatermark"></a>

### AigcWatermark


```csharp
[JsonPropertyName("aigcWatermark")]
public bool? AigcWatermark { get; set; }
```


Whether to embed an AIGC watermark in the output. Defaults to `false`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L31)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_MiniMaxVideoRatios_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_MiniMaxVideoRatios"></a>

### MiniMaxVideoRatios


```csharp
public static IReadOnlyList<string> MiniMaxVideoRatios { get; }
```


Aspect ratios supported by the MiniMax video models (`minimaxVideoRatios`).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L13)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_MiniMaxVideoResolutions_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_MiniMaxVideoResolutions"></a>

### MiniMaxVideoResolutions


```csharp
public static IReadOnlyList<string> MiniMaxVideoResolutions { get; }
```


Output resolutions supported by MiniMax video models (`minimaxVideoResolutions`). Availability depends on the selected model.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L16)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_PollIntervalMs_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_PollIntervalMs"></a>

### PollIntervalMs


```csharp
[JsonPropertyName("pollIntervalMs")]
public int? PollIntervalMs { get; set; }
```


Interval in milliseconds between task status polls. Default: 10000.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L35)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_PollTimeoutMs_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_PollTimeoutMs"></a>

### PollTimeoutMs


```csharp
[JsonPropertyName("pollTimeoutMs")]
public int? PollTimeoutMs { get; set; }
```


Maximum time in milliseconds to poll before timing out. Default: 600000.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L39)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_Ratio_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_Ratio"></a>

### Ratio


```csharp
[JsonPropertyName("ratio")]
public string? Ratio { get; set; }
```


Aspect ratio of the generated video. One of [MiniMaxVideoRatios](#Vercel_AI_MiniMax_MiniMaxVideoModelOptions_MiniMaxVideoRatios). Overrides the top-level aspect ratio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L23)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_ReferenceAudioUrls_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_ReferenceAudioUrls"></a>

### ReferenceAudioUrls


```csharp
[JsonPropertyName("referenceAudioUrls")]
public IList<string>? ReferenceAudioUrls { get; set; }
```


Reference audio URLs for reference-to-video generation.

**Value:** [IList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L27)

<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_Resolution_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxVideoModelOptions_Resolution"></a>

### Resolution


```csharp
[JsonPropertyName("resolution")]
public string? Resolution { get; set; }
```


Output resolution. One of [MiniMaxVideoResolutions](#Vercel_AI_MiniMax_MiniMaxVideoModelOptions_MiniMaxVideoResolutions).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxVideoModelOptions.cs#L19)
