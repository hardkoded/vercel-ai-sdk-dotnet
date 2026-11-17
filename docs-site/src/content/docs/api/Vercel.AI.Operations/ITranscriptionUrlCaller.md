---
title: "ITranscriptionUrlCaller"
description: "Optional native URL transcription. Its presence signals that HTTP(S) audio URLs can be sent to the provider without downloading them locally. Maps to the opt..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ITranscriptionUrlCaller"
  hidden: true
---

<a id="Vercel_AI_Operations_ITranscriptionUrlCaller"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public interface ITranscriptionUrlCaller : ITranscriptionCaller
```


Optional native URL transcription. Its presence signals that HTTP(S) audio URLs can be sent to the provider without
downloading them locally. Maps to the optional `doGenerateFromUrl` of `TranscriptionModelV4`. It is a separate
interface because `netstandard2.0` has no default interface members.

**Derived:** [GroqTranscriptionModel](/api/Vercel.AI.Groq/GroqTranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L305)
