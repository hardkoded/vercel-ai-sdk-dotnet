---
title: "StreamTranscribe"
description: "Streams a transcript. Maps to streamTranscribe."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranscribe"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranscribe"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class StreamTranscribe
```


Streams a transcript. Maps to `streamTranscribe`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L613)

## Methods

<a id="Vercel_AI_Operations_StreamTranscribe_Start_"></a>
<a id="Vercel_AI_Operations_StreamTranscribe_Start_Vercel_AI_Operations_StreamTranscribeRequest_System_Threading_CancellationToken_"></a>

### Start(StreamTranscribeRequest, CancellationToken)


```csharp
public static StreamTranscribeResult Start(StreamTranscribeRequest request, CancellationToken cancellationToken = default)
```


Starts a transcription stream. `doStream` is invoked immediately.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [StreamTranscribeRequest](/api/Vercel.AI.Operations/StreamTranscribeRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [StreamTranscribeResult](/api/Vercel.AI.Operations/StreamTranscribeResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L616)
