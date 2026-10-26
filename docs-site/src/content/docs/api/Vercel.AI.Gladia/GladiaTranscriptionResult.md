---
title: "GladiaTranscriptionResult"
description: "Gladia transcription, including utterance metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaTranscriptionResult"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaTranscriptionResult
```


Gladia transcription, including utterance metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L131)

## Constructors

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Gladia_GladiaTranscriptionSegment__System_String_System_Nullable_System_Double__System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyList_System_String__Vercel_AI_Gladia_GladiaTranscriptionResponse_"></a>

### `GladiaTranscriptionResult(string, IReadOnlyList<GladiaTranscriptionSegment>, string?, double?, JsonElement, IReadOnlyList<string>, GladiaTranscriptionResponse)`


```csharp
public GladiaTranscriptionResult(string text, IReadOnlyList<GladiaTranscriptionSegment> segments, string? language, double? durationInSeconds, JsonElement providerMetadata, IReadOnlyList<string> warnings, GladiaTranscriptionResponse response)
```


Creates a transcription result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `segments` | [IReadOnlyList&lt;GladiaTranscriptionSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `durationInSeconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `warnings` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `response` | [GladiaTranscriptionResponse](/api/Vercel.AI.Gladia/GladiaTranscriptionResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L134)

## Properties

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_DurationInSeconds_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Audio duration in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L162)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Language_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


First detected language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L159)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Raw Gladia result object.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L165)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Response_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Response"></a>

### Response


```csharp
public GladiaTranscriptionResponse Response { get; }
```


Response metadata.

**Value:** [GladiaTranscriptionResponse](/api/Vercel.AI.Gladia/GladiaTranscriptionResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L171)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Segments_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<GladiaTranscriptionSegment> Segments { get; }
```


Utterances.

**Value:** [IReadOnlyList&lt;GladiaTranscriptionSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L156)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Text_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Full transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L153)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Warnings_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<string> Warnings { get; }
```


Warnings produced while building the request.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L168)
