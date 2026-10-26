---
title: "TranscriptSegment"
description: "One timed transcript segment."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptSegment"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptSegment"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptSegment
```


One timed transcript segment.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L10)

## Constructors

<a id="Vercel_AI_Operations_TranscriptSegment__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptSegment__ctor_System_String_System_Double_System_Double_"></a>

### TranscriptSegment(string, double, double)


```csharp
public TranscriptSegment(string text, double startSecond, double endSecond)
```


Creates a segment.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `startSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `endSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L13)

## Properties

<a id="Vercel_AI_Operations_TranscriptSegment_EndSecond_"></a>
<a id="Vercel_AI_Operations_TranscriptSegment_EndSecond"></a>

### EndSecond


```csharp
public double EndSecond { get; }
```


End time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L27)

<a id="Vercel_AI_Operations_TranscriptSegment_StartSecond_"></a>
<a id="Vercel_AI_Operations_TranscriptSegment_StartSecond"></a>

### StartSecond


```csharp
public double StartSecond { get; }
```


Start time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L24)

<a id="Vercel_AI_Operations_TranscriptSegment_Text_"></a>
<a id="Vercel_AI_Operations_TranscriptSegment_Text"></a>

### Text


```csharp
public string Text { get; }
```


Segment text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L21)
