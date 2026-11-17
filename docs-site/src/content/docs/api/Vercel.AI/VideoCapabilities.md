---
title: "VideoCapabilities"
description: "Upstream ModelCapabilities.video."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoCapabilities"
  hidden: true
---

<a id="Vercel_AI_VideoCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class VideoCapabilities
```


Upstream `ModelCapabilities.video`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L189)

## Properties

<a id="Vercel_AI_VideoCapabilities_AspectRatio_"></a>
<a id="Vercel_AI_VideoCapabilities_AspectRatio"></a>

### AspectRatio


```csharp
public ModelCapability AspectRatio { get; init; }
```


Aspect ratio.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L198)

<a id="Vercel_AI_VideoCapabilities_Duration_"></a>
<a id="Vercel_AI_VideoCapabilities_Duration"></a>

### Duration


```csharp
public ModelCapability Duration { get; init; }
```


Duration.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L204)

<a id="Vercel_AI_VideoCapabilities_Fps_"></a>
<a id="Vercel_AI_VideoCapabilities_Fps"></a>

### Fps


```csharp
public ModelCapability Fps { get; init; }
```


Frames per second.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L207)

<a id="Vercel_AI_VideoCapabilities_GenerateAudio_"></a>
<a id="Vercel_AI_VideoCapabilities_GenerateAudio"></a>

### GenerateAudio


```csharp
public ModelCapability GenerateAudio { get; init; }
```


Audio generation.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L195)

<a id="Vercel_AI_VideoCapabilities_Inputs_"></a>
<a id="Vercel_AI_VideoCapabilities_Inputs"></a>

### Inputs


```csharp
public VideoInputCapabilities Inputs { get; init; }
```


Upstream `inputs`.

**Value:** [VideoInputCapabilities](/api/Vercel.AI/VideoInputCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L192)

<a id="Vercel_AI_VideoCapabilities_MaxVideosPerCall_"></a>
<a id="Vercel_AI_VideoCapabilities_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; init; }
```


Null means the catalog does not declare a fixed limit.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L213)

<a id="Vercel_AI_VideoCapabilities_Resolution_"></a>
<a id="Vercel_AI_VideoCapabilities_Resolution"></a>

### Resolution


```csharp
public ModelCapability Resolution { get; init; }
```


Resolution.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L201)

<a id="Vercel_AI_VideoCapabilities_Seed_"></a>
<a id="Vercel_AI_VideoCapabilities_Seed"></a>

### Seed


```csharp
public ModelCapability Seed { get; init; }
```


Seed.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L210)
