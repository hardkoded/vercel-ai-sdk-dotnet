---
title: "OpenAITranscriptionResponse"
description: "Response metadata recorded for a transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAITranscriptionResponse"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAITranscriptionResponse
```


Response metadata recorded for a transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L36)

## Properties

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_Body_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_Body"></a>

### Body


```csharp
public string Body { get; }
```


Raw response JSON.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L56)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L53)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id used for the call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L50)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_Timestamp_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L47)
