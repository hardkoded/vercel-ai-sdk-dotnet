---
title: "TranscriptionModelResult"
description: "Transcription model response."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionModelResult
```


Transcription model response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L56)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_TranscriptSegment__System_String_System_Nullable_System_Double__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `TranscriptionModelResult(string?, IReadOnlyList<TranscriptSegment>?, string?, double?, IReadOnlyList<OperationWarning>?, JsonElement?, ProviderResponse?)`


```csharp
public TranscriptionModelResult(string? text, IReadOnlyList<TranscriptSegment>? segments = null, string? language = null, double? durationInSeconds = null, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
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
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L59)

## Properties

<a id="Vercel_AI_Operations_TranscriptionModelResult_DurationInSeconds_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Duration in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L80)

<a id="Vercel_AI_Operations_TranscriptionModelResult_Language_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L77)

<a id="Vercel_AI_Operations_TranscriptionModelResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L86)

<a id="Vercel_AI_Operations_TranscriptionModelResult_Response_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L89)

<a id="Vercel_AI_Operations_TranscriptionModelResult_Segments_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment> Segments { get; }
```


Segments.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L74)

<a id="Vercel_AI_Operations_TranscriptionModelResult_Text_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult_Text"></a>

### Text


```csharp
public string? Text { get; }
```


Transcript text, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; when the model returned none. An empty string is a valid transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L71)

<a id="Vercel_AI_Operations_TranscriptionModelResult_Warnings_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L83)
