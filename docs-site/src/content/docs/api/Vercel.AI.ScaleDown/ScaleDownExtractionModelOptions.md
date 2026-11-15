---
title: "ScaleDownExtractionModelOptions"
description: "Provider options of a ScaleDown extraction model, under providerOptions.scaledown. Maps to ScaleDownExtractionModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownExtractionModelOptions"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelOptions"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public sealed class ScaleDownExtractionModelOptions
```


Provider options of a ScaleDown extraction model, under `providerOptions.scaledown`. Maps to `ScaleDownExtractionModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModelOptions.cs#L8)

## Properties

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelOptions_Threshold_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModelOptions_Threshold"></a>

### Threshold


```csharp
public double? Threshold { get; init; }
```


Minimum confidence for returned matches, from 0 to 1. Defaults to 0.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModelOptions.cs#L11)
