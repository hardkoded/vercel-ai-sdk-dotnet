---
title: "XaiVideoKeyframe"
description: "A mid-video image anchor. Upstream XaiVideoKeyframe."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiVideoKeyframe"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiVideoKeyframe"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiVideoKeyframe
```


A mid-video image anchor. Upstream `XaiVideoKeyframe`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_Xai_XaiVideoKeyframe_ImageUrl_"></a>
<a id="Vercel_AI_Xai_XaiVideoKeyframe_ImageUrl"></a>

### ImageUrl


```csharp
public string ImageUrl { get; set; }
```


Image URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L14)

<a id="Vercel_AI_Xai_XaiVideoKeyframe_TimestampSeconds_"></a>
<a id="Vercel_AI_Xai_XaiVideoKeyframe_TimestampSeconds"></a>

### TimestampSeconds


```csharp
public double TimestampSeconds { get; set; }
```


Position of the anchor in seconds. Must be positive.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiVideoModelOptions.cs#L17)
