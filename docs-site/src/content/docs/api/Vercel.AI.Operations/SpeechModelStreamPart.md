---
title: "SpeechModelStreamPart"
description: "A part emitted by a streaming speech model. Maps to SpeechModelV4StreamPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechModelStreamPart"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechModelStreamPart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechModelStreamPart
```


A part emitted by a streaming speech model. Maps to `SpeechModelV4StreamPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L131)

## Properties

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Audio_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Audio"></a>

### Audio


```csharp
public byte[]? Audio { get; set; }
```


Raw audio bytes of an `audio` chunk.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L143)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_AudioBase64_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_AudioBase64"></a>

### AudioBase64


```csharp
public string? AudioBase64 { get; set; }
```


Base64-encoded raw audio bytes of an `audio` chunk. Used when [Audio](#Vercel_AI_Operations_SpeechModelStreamPart_Audio) is null.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L146)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_AudioFormat_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_AudioFormat"></a>

### AudioFormat


```csharp
public SpeechAudioFormat? AudioFormat { get; set; }
```


Format of an `audio` chunk.

**Value:** [SpeechAudioFormat](/api/Vercel.AI.Operations/SpeechAudioFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L149)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Body_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Body"></a>

### Body


```csharp
public JsonElement? Body { get; set; }
```


Response body of a `response-metadata` part.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L164)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Error_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Error"></a>

### Error


```csharp
public Exception? Error { get; set; }
```


Non-fatal error of an `error` part. The stream continues.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L173)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Headers_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Response headers of a `response-metadata` part.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L161)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Id_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


Optional provider identifier for an `audio` chunk.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L140)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_ModelId_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Response model id of a `response-metadata` part.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L158)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata of an `audio` or `finish` part.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L152)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_RawValue_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_RawValue"></a>

### RawValue


```csharp
public JsonElement? RawValue { get; set; }
```


Raw provider chunk of a `raw` part.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L170)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Timestamp_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Timestamp"></a>

### Timestamp


```csharp
public DateTime? Timestamp { get; set; }
```


Response timestamp of a `response-metadata` part.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L155)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Type_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


Part type: `stream-start`, `audio`, `response-metadata`, `finish`, `raw`, or `error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L134)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Usage_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Usage"></a>

### Usage


```csharp
public SpeechStreamUsage? Usage { get; set; }
```


Usage of a `finish` part.

**Value:** [SpeechStreamUsage](/api/Vercel.AI.Operations/SpeechStreamUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L167)

<a id="Vercel_AI_Operations_SpeechModelStreamPart_Warnings_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamPart_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning>? Warnings { get; set; }
```


Warnings carried by `stream-start`.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L137)
