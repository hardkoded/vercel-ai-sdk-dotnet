---
title: "GladiaTranscriptionSegment"
description: "One Gladia utterance mapped to a segment."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaTranscriptionSegment"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaTranscriptionSegment
```


One Gladia utterance mapped to a segment.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L89)

## Constructors

<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment__ctor_System_String_System_Double_System_Double_"></a>

### GladiaTranscriptionSegment(string, double, double)


```csharp
public GladiaTranscriptionSegment(string text, double startSecond, double endSecond)
```


Creates a segment.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `startSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `endSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L92)

## Properties

<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment_EndSecond_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment_EndSecond"></a>

### EndSecond


```csharp
public double EndSecond { get; }
```


End time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L106)

<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment_StartSecond_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment_StartSecond"></a>

### StartSecond


```csharp
public double StartSecond { get; }
```


Start time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L103)

<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment_Text_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionSegment_Text"></a>

### Text


```csharp
public string Text { get; }
```


Utterance text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L100)
