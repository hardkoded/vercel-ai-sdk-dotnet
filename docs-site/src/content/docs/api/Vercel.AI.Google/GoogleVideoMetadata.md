---
title: "GoogleVideoMetadata"
description: "Metadata for preprocessing video file parts."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVideoMetadata"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVideoMetadata"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVideoMetadata
```


Metadata for preprocessing video file parts.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L17)

## Properties

<a id="Vercel_AI_Google_GoogleVideoMetadata_EndOffset_"></a>
<a id="Vercel_AI_Google_GoogleVideoMetadata_EndOffset"></a>

### EndOffset


```csharp
[JsonPropertyName("endOffset")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? EndOffset { get; set; }
```


End offset as a duration in seconds, for example `10s`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L25)

<a id="Vercel_AI_Google_GoogleVideoMetadata_Fps_"></a>
<a id="Vercel_AI_Google_GoogleVideoMetadata_Fps"></a>

### Fps


```csharp
[JsonPropertyName("fps")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public double? Fps { get; set; }
```


Frames per second in the range (0, 24]. Defaults to 1.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L30)

<a id="Vercel_AI_Google_GoogleVideoMetadata_StartOffset_"></a>
<a id="Vercel_AI_Google_GoogleVideoMetadata_StartOffset"></a>

### StartOffset


```csharp
[JsonPropertyName("startOffset")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? StartOffset { get; set; }
```


Start offset as a duration in seconds, for example `3.5s`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L20)
