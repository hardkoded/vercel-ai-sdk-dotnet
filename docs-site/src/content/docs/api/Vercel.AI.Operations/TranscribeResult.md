---
title: "TranscribeResult"
description: "Transcription result."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscribeResult"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscribeResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscribeResult
```


Transcription result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L172)

## Constructors

<a id="Vercel_AI_Operations_TranscribeResult__ctor_"></a>
<a id="Vercel_AI_Operations_TranscribeResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_TranscriptSegment__System_String_System_Nullable_System_Double__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__System_Text_Json_JsonElement_"></a>

### `TranscribeResult(string, IReadOnlyList<TranscriptSegment>, string?, double?, IReadOnlyList<OperationWarning>, IReadOnlyList<ProviderResponse>, JsonElement)`


```csharp
public TranscribeResult(string text, IReadOnlyList<TranscriptSegment> segments, string? language, double? durationInSeconds, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `segments` | [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `durationInSeconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L175)

## Properties

<a id="Vercel_AI_Operations_TranscribeResult_DurationInSeconds_"></a>
<a id="Vercel_AI_Operations_TranscribeResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Duration in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L196)

<a id="Vercel_AI_Operations_TranscribeResult_Language_"></a>
<a id="Vercel_AI_Operations_TranscribeResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L193)

<a id="Vercel_AI_Operations_TranscribeResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_TranscribeResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L205)

<a id="Vercel_AI_Operations_TranscribeResult_Responses_"></a>
<a id="Vercel_AI_Operations_TranscribeResult_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse> Responses { get; }
```


Responses.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L202)

<a id="Vercel_AI_Operations_TranscribeResult_Segments_"></a>
<a id="Vercel_AI_Operations_TranscribeResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment> Segments { get; }
```


Segments.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L190)

<a id="Vercel_AI_Operations_TranscribeResult_Text_"></a>
<a id="Vercel_AI_Operations_TranscribeResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L187)

<a id="Vercel_AI_Operations_TranscribeResult_Warnings_"></a>
<a id="Vercel_AI_Operations_TranscribeResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L199)
