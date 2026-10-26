---
title: "StreamSpeech"
description: "Streams speech audio. Maps to experimentalstreamSpeech."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamSpeech"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamSpeech"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class StreamSpeech
```


Streams speech audio. Maps to `experimental_streamSpeech`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L749)

## Methods

<a id="Vercel_AI_Operations_StreamSpeech_Start_"></a>
<a id="Vercel_AI_Operations_StreamSpeech_Start_Vercel_AI_Operations_StreamSpeechRequest_System_Threading_CancellationToken_"></a>

### Start(StreamSpeechRequest, CancellationToken)


```csharp
public static StreamSpeechResult Start(StreamSpeechRequest request, CancellationToken cancellationToken = default)
```


Starts a speech stream. `doStream` is invoked immediately.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [StreamSpeechRequest](/api/Vercel.AI.Operations/StreamSpeechRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [StreamSpeechResult](/api/Vercel.AI.Operations/StreamSpeechResult/)

**Exceptions**

| Type | Condition |
| --- | --- |
| [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/) | The model does not support streaming speech. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L753)
