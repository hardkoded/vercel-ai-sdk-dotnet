---
title: "SpeechModelStreamResponse"
description: "Response data returned by doStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechModelStreamResponse"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechModelStreamResponse"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechModelStreamResponse
```


Response data returned by `doStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L257)

## Properties

<a id="Vercel_AI_Operations_SpeechModelStreamResponse_Body_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResponse_Body"></a>

### Body


```csharp
public JsonElement? Body { get; set; }
```


Response body.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L269)

<a id="Vercel_AI_Operations_SpeechModelStreamResponse_Headers_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L266)

<a id="Vercel_AI_Operations_SpeechModelStreamResponse_ModelId_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResponse_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Id of the response model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L263)

<a id="Vercel_AI_Operations_SpeechModelStreamResponse_Timestamp_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTime? Timestamp { get; set; }
```


Timestamp for the start of the streamed response.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L260)
