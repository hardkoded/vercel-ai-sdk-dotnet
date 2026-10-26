---
title: "TranscriptionModels"
description: "Resolves string transcription model ids."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionModels"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionModels"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class TranscriptionModels
```


Resolves string transcription model ids.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L165)

## Properties

<a id="Vercel_AI_Operations_TranscriptionModels_Default_"></a>
<a id="Vercel_AI_Operations_TranscriptionModels_Default"></a>

### Default


```csharp
public static Func<string, ITranscriptionCaller?>? Default { get; set; }
```


Default provider factory. Null means the provider has no transcription models.

**Value:** [Func&lt;string, ITranscriptionCaller&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L168)
