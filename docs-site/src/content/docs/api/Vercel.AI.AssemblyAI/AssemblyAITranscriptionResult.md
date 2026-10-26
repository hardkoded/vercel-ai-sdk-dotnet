---
title: "AssemblyAITranscriptionResult"
description: "AssemblyAI transcription, including word segments and audio-intelligence metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAITranscriptionResult"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAITranscriptionResult
```


AssemblyAI transcription, including word segments and audio-intelligence metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L137)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_AssemblyAI_AssemblyAITranscriptionSegment__System_String_System_Nullable_System_Double__System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_AssemblyAI_AssemblyAITranscriptionResponse_"></a>

### `AssemblyAITranscriptionResult(string, IReadOnlyList<AssemblyAITranscriptionSegment>, string?, double?, IReadOnlyList<ModelWarning>, JsonElement?, AssemblyAITranscriptionResponse)`


```csharp
public AssemblyAITranscriptionResult(string text, IReadOnlyList<AssemblyAITranscriptionSegment> segments, string? language, double? durationInSeconds, IReadOnlyList<ModelWarning> warnings, JsonElement? providerMetadata, AssemblyAITranscriptionResponse response)
```


Creates a transcription result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `segments` | [IReadOnlyList&lt;AssemblyAITranscriptionSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `durationInSeconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `warnings` | [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [AssemblyAITranscriptionResponse](/api/Vercel.AI.AssemblyAI/AssemblyAITranscriptionResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L140)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_DurationInSeconds_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Audio duration, or the last word end when the duration is missing.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L168)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Language_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Language code reported on the transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L165)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_ProviderMetadata_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Diarization and audio-intelligence results under `assemblyai`, when the transcript has any.
Timings inside these objects stay in milliseconds, as AssemblyAI returns them.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L177)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Response_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Response"></a>

### Response


```csharp
public AssemblyAITranscriptionResponse Response { get; }
```


Response metadata.

**Value:** [AssemblyAITranscriptionResponse](/api/Vercel.AI.AssemblyAI/AssemblyAITranscriptionResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L180)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Segments_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<AssemblyAITranscriptionSegment> Segments { get; }
```


Words with timings in seconds.

**Value:** [IReadOnlyList&lt;AssemblyAITranscriptionSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L162)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Text_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Full transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L159)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Warnings_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings produced while building the request.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L171)
