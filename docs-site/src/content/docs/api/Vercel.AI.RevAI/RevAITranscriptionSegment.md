---
title: "RevAITranscriptionSegment"
description: "One timed transcript fragment."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAITranscriptionSegment"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAITranscriptionSegment"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public sealed class RevAITranscriptionSegment
```


One timed transcript fragment.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L90)

## Constructors

<a id="Vercel_AI_RevAI_RevAITranscriptionSegment__ctor_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionSegment__ctor_System_String_System_Double_System_Double_"></a>

### RevAITranscriptionSegment(string, double, double)


```csharp
public RevAITranscriptionSegment(string text, double startSecond, double endSecond)
```


Creates a segment.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `startSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `endSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L93)

## Properties

<a id="Vercel_AI_RevAI_RevAITranscriptionSegment_EndSecond_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionSegment_EndSecond"></a>

### EndSecond


```csharp
public double EndSecond { get; }
```


End time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L107)

<a id="Vercel_AI_RevAI_RevAITranscriptionSegment_StartSecond_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionSegment_StartSecond"></a>

### StartSecond


```csharp
public double StartSecond { get; }
```


Start time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L104)

<a id="Vercel_AI_RevAI_RevAITranscriptionSegment_Text_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionSegment_Text"></a>

### Text


```csharp
public string Text { get; }
```


Segment text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L101)
