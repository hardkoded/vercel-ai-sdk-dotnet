---
title: "StreamTranslateRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranslateRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranslateRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamTranslateRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L95)

## Properties

<a id="Vercel_AI_Operations_StreamTranslateRequest_Audio_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_Audio"></a>

### Audio


```csharp
public AudioChunkStream? Audio { get; set; }
```


Audio stream.

**Value:** [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L104)

<a id="Vercel_AI_Operations_StreamTranslateRequest_IncludeRawChunks_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool? IncludeRawChunks { get; set; }
```


Requests raw provider chunks.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L119)

<a id="Vercel_AI_Operations_StreamTranslateRequest_InputAudioFormat_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_InputAudioFormat"></a>

### InputAudioFormat


```csharp
public string InputAudioFormat { get; set; }
```


Input audio format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L107)

<a id="Vercel_AI_Operations_StreamTranslateRequest_Model_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_Model"></a>

### Model


```csharp
public ISpeechTranslationCaller? Model { get; set; }
```


Model instance. Takes precedence over [ModelId](#Vercel_AI_Operations_StreamTranslateRequest_ModelId).

**Value:** [ISpeechTranslationCaller](/api/Vercel.AI.Operations/ISpeechTranslationCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L98)

<a id="Vercel_AI_Operations_StreamTranslateRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Model id resolved through [Default](/api/Vercel.AI.Operations/SpeechTranslationModels/#Vercel_AI_Operations_SpeechTranslationModels_Default).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L101)

<a id="Vercel_AI_Operations_StreamTranslateRequest_OutputAudioFormat_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_OutputAudioFormat"></a>

### OutputAudioFormat


```csharp
public string? OutputAudioFormat { get; set; }
```


Output audio format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L116)

<a id="Vercel_AI_Operations_StreamTranslateRequest_SourceLanguage_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_SourceLanguage"></a>

### SourceLanguage


```csharp
public string? SourceLanguage { get; set; }
```


Source language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L113)

<a id="Vercel_AI_Operations_StreamTranslateRequest_TargetLanguage_"></a>
<a id="Vercel_AI_Operations_StreamTranslateRequest_TargetLanguage"></a>

### TargetLanguage


```csharp
public string TargetLanguage { get; set; }
```


Target language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L110)
