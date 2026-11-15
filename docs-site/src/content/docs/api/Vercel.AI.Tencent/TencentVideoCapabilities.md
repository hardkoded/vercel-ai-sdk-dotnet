---
title: "TencentVideoCapabilities"
description: "Video capabilities of a model. Upstream ModelCapabilities.video."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentVideoCapabilities"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentVideoCapabilities"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentVideoCapabilities
```


Video capabilities of a model. Upstream `ModelCapabilities.video`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L324)

## Properties

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_AspectRatio_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_AspectRatio"></a>

### AspectRatio


```csharp
public ModelCapability AspectRatio { get; init; }
```


Aspect ratio.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L348)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Duration_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Duration"></a>

### Duration


```csharp
public ModelCapability Duration { get; init; }
```


Duration.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L354)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_FirstFrame_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_FirstFrame"></a>

### FirstFrame


```csharp
public ModelCapability FirstFrame { get; init; }
```


Role-tagged first frame passed through `frameImages`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L333)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Fps_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Fps"></a>

### Fps


```csharp
public ModelCapability Fps { get; init; }
```


Frames per second.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L357)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_GenerateAudio_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_GenerateAudio"></a>

### GenerateAudio


```csharp
public ModelCapability GenerateAudio { get; init; }
```


Audio generation.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L345)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Image_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Image"></a>

### Image


```csharp
public ModelCapability Image { get; init; }
```


The single starting image passed through `image`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L330)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_LastFrame_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_LastFrame"></a>

### LastFrame


```csharp
public ModelCapability LastFrame { get; init; }
```


Role-tagged last frame passed through `frameImages`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L336)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_MaxVideosPerCall_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_MaxVideosPerCall"></a>

### MaxVideosPerCall


```csharp
public int? MaxVideosPerCall { get; init; }
```


Fixed limit of videos per call. Null when the catalog declares none.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L363)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_ReferenceImage_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_ReferenceImage"></a>

### ReferenceImage


```csharp
public ModelCapability ReferenceImage { get; init; }
```


Reference images passed through `inputReferences`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L339)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_ReferenceVideo_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_ReferenceVideo"></a>

### ReferenceVideo


```csharp
public ModelCapability ReferenceVideo { get; init; }
```


Reference videos passed through `inputReferences`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L342)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Resolution_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Resolution"></a>

### Resolution


```csharp
public ModelCapability Resolution { get; init; }
```


Resolution.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L351)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Seed_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Seed"></a>

### Seed


```csharp
public ModelCapability Seed { get; init; }
```


Seed.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L360)

<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Text_"></a>
<a id="Vercel_AI_Tencent_TencentVideoCapabilities_Text"></a>

### Text


```csharp
public ModelCapability Text { get; init; }
```


Text input.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L327)
