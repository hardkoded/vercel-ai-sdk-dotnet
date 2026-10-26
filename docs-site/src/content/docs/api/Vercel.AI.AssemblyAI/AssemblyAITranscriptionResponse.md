---
title: "AssemblyAITranscriptionResponse"
description: "Metadata returned with a transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAITranscriptionResponse"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAITranscriptionResponse
```


Metadata returned with a transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L112)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `AssemblyAITranscriptionResponse(DateTimeOffset, string, IReadOnlyDictionary<string, string>, string)`


```csharp
public AssemblyAITranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, string body)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L115)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_Body_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_Body"></a>

### Body


```csharp
public string Body { get; }
```


Raw transcript JSON from the final poll.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L133)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_Headers_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers from the final transcript poll.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L130)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_ModelId_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L127)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_Timestamp_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L124)
