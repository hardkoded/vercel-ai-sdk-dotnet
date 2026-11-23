---
title: "VideoInputCapabilities"
description: "Upstream ModelCapabilities.video.inputs."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoInputCapabilities"
  hidden: true
---

<a id="Vercel_AI_VideoInputCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class VideoInputCapabilities
```


Upstream `ModelCapabilities.video.inputs`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L220)

## Properties

<a id="Vercel_AI_VideoInputCapabilities_FirstFrame_"></a>
<a id="Vercel_AI_VideoInputCapabilities_FirstFrame"></a>

### FirstFrame


```csharp
public ModelCapability FirstFrame { get; init; }
```


Role-tagged first frame passed through the `frameImages` option.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L229)

<a id="Vercel_AI_VideoInputCapabilities_Image_"></a>
<a id="Vercel_AI_VideoInputCapabilities_Image"></a>

### Image


```csharp
public ModelCapability Image { get; init; }
```


The single starting image passed through the `image` option.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L226)

<a id="Vercel_AI_VideoInputCapabilities_LastFrame_"></a>
<a id="Vercel_AI_VideoInputCapabilities_LastFrame"></a>

### LastFrame


```csharp
public ModelCapability LastFrame { get; init; }
```


Role-tagged last frame passed through the `frameImages` option.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L232)

<a id="Vercel_AI_VideoInputCapabilities_ReferenceImage_"></a>
<a id="Vercel_AI_VideoInputCapabilities_ReferenceImage"></a>

### ReferenceImage


```csharp
public ModelCapability ReferenceImage { get; init; }
```


Reference images passed through the `inputReferences` option.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L235)

<a id="Vercel_AI_VideoInputCapabilities_ReferenceVideo_"></a>
<a id="Vercel_AI_VideoInputCapabilities_ReferenceVideo"></a>

### ReferenceVideo


```csharp
public ModelCapability ReferenceVideo { get; init; }
```


Reference videos passed through the `inputReferences` option.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L238)

<a id="Vercel_AI_VideoInputCapabilities_Text_"></a>
<a id="Vercel_AI_VideoInputCapabilities_Text"></a>

### Text


```csharp
public ModelCapability Text { get; init; }
```


Text input.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L223)
