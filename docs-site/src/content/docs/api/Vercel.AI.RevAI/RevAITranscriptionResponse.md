---
title: "RevAITranscriptionResponse"
description: "Metadata returned with a transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAITranscriptionResponse"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAITranscriptionResponse"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public sealed class RevAITranscriptionResponse
```


Metadata returned with a transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L111)

## Constructors

<a id="Vercel_AI_RevAI_RevAITranscriptionResponse__ctor_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResponse__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `RevAITranscriptionResponse(DateTimeOffset, string, IReadOnlyDictionary<string, string>, string)`


```csharp
public RevAITranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, string body)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L114)

## Properties

<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_Body_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_Body"></a>

### Body


```csharp
public string Body { get; }
```


Raw transcript JSON.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L132)

<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_Headers_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers from the transcript response.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L129)

<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_ModelId_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id sent as `transcriber`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L126)

<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_Timestamp_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L123)
