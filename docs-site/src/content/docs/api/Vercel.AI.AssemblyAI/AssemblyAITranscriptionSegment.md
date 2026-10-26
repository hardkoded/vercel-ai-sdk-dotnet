---
title: "AssemblyAITranscriptionSegment"
description: "One transcribed word."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAITranscriptionSegment"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAITranscriptionSegment
```


One transcribed word.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L91)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment__ctor_System_String_System_Double_System_Double_"></a>

### AssemblyAITranscriptionSegment(string, double, double)


```csharp
public AssemblyAITranscriptionSegment(string text, double startSecond, double endSecond)
```


Creates a segment.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `startSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `endSecond` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L94)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment_EndSecond_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment_EndSecond"></a>

### EndSecond


```csharp
public double EndSecond { get; }
```


End time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L108)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment_StartSecond_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment_StartSecond"></a>

### StartSecond


```csharp
public double StartSecond { get; }
```


Start time in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L105)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment_Text_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment_Text"></a>

### Text


```csharp
public string Text { get; }
```


Word text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L102)
