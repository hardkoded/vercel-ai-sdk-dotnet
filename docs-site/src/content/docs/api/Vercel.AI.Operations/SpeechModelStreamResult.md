---
title: "SpeechModelStreamResult"
description: "The result of a speech model doStream call. Maps to SpeechModelV4StreamResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechModelStreamResult"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechModelStreamResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechModelStreamResult
```


The result of a speech model `doStream` call. Maps to `SpeechModelV4StreamResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L273)

## Constructors

<a id="Vercel_AI_Operations_SpeechModelStreamResult__ctor_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResult__ctor_System_Collections_Generic_IAsyncEnumerable_Vercel_AI_Operations_SpeechModelStreamPart__Vercel_AI_Operations_SpeechModelStreamResponse_System_Nullable_System_Text_Json_JsonElement__"></a>

### `SpeechModelStreamResult(IAsyncEnumerable<SpeechModelStreamPart>, SpeechModelStreamResponse?, JsonElement?)`


```csharp
public SpeechModelStreamResult(IAsyncEnumerable<SpeechModelStreamPart> stream, SpeechModelStreamResponse? response = null, JsonElement? requestBody = null)
```


Creates a stream result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [IAsyncEnumerable&lt;SpeechModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) |  |
| `response` | [SpeechModelStreamResponse](/api/Vercel.AI.Operations/SpeechModelStreamResponse/) |  |
| `requestBody` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L276)

## Properties

<a id="Vercel_AI_Operations_SpeechModelStreamResult_RequestBody_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResult_RequestBody"></a>

### RequestBody


```csharp
public JsonElement? RequestBody { get; }
```


Request body or setup payload sent to the provider API.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L290)

<a id="Vercel_AI_Operations_SpeechModelStreamResult_Response_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResult_Response"></a>

### Response


```csharp
public SpeechModelStreamResponse? Response { get; }
```


Optional response data.

**Value:** [SpeechModelStreamResponse](/api/Vercel.AI.Operations/SpeechModelStreamResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L287)

<a id="Vercel_AI_Operations_SpeechModelStreamResult_Stream_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamResult_Stream"></a>

### Stream


```csharp
public IAsyncEnumerable<SpeechModelStreamPart> Stream { get; }
```


The stream.

**Value:** [IAsyncEnumerable&lt;SpeechModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L284)
