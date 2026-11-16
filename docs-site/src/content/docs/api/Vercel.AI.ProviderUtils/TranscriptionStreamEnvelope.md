---
title: "TranscriptionStreamEnvelope"
description: "Experimental transcription-stream WebSocket envelope (v1): the standard serialization of the transcription model doStream over a WebSocket. Maps to transcrip..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionStreamEnvelope"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class TranscriptionStreamEnvelope
```


Experimental transcription-stream WebSocket envelope (v1): the standard serialization of the transcription model
`doStream` over a WebSocket. Maps to `transcription-stream-envelope.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

Envelope rules:
1. The client sends exactly one `transcription-stream.start` text frame first.
2. Audio rides binary frames containing raw bytes in the declared `inputAudioFormat` (base64 string chunks are decoded before sending).
3. The client signals end of audio with the `transcription-stream.audio-done` text frame; a plain close without it is an abort.
4. Every server-to-client text frame is one JSON-serialized stream part (flattened, no wrapper). Timestamps ride as ISO 8601 strings.
5. The server closes with code 1000 after the `finish` part; on failure it sends an `error` part and closes non-1000. A close without a prior `finish` is an error.
6. Unknown frame and part types are ignored in both directions (forward compatibility).
7. Servers may enforce a maximum frame size (the AI Gateway rejects frames over 256 KiB); clients should split audio into frames of at most 64 KiB.
8. Connection establishment (URL, auth) is transport-specific and out of scope.
The envelope validates frame shape only. Both parsers reject frames carrying `__proto__` keys.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L102)

## Fields

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_TranscriptionStreamAudioDoneFrameType"></a>

### TranscriptionStreamAudioDoneFrameType


```csharp
public const string TranscriptionStreamAudioDoneFrameType = "transcription-stream.audio-done"
```


Type of the client text frame that signals the end of the audio input. Maps to `TRANSCRIPTION_STREAM_AUDIO_DONE_FRAME_TYPE`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L108)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_TranscriptionStreamStartFrameType"></a>

### TranscriptionStreamStartFrameType


```csharp
public const string TranscriptionStreamStartFrameType = "transcription-stream.start"
```


Type of the first client text frame. Maps to `TRANSCRIPTION_STREAM_START_FRAME_TYPE`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L105)

## Methods

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_ParseTranscriptionStreamClientFrame_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_ParseTranscriptionStreamClientFrame_System_String_"></a>

### ParseTranscriptionStreamClientFrame(string)


```csharp
public static TranscriptionStreamClientFrame ParseTranscriptionStreamClientFrame(string text)
```


Server-side: parses a client text frame. Validates envelope shape only and rejects prototype-pollution
payloads. Never throws. Maps to `parseTranscriptionStreamClientFrame`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [TranscriptionStreamClientFrame](/api/Vercel.AI.ProviderUtils/TranscriptionStreamClientFrame/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L114)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_ParseTranscriptionStreamPart_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_ParseTranscriptionStreamPart_System_String_"></a>

### ParseTranscriptionStreamPart(string)


```csharp
public static ModelStreamPart? ParseTranscriptionStreamPart(string text)
```


Client-side: parses a server text frame into a transcription stream part. Returns null for malformed or unsafe
(prototype-polluting) JSON, unknown part types, and known part types whose required or optional fields are
missing or mistyped (including warning and segment elements): downstream SDK code dereferences those fields,
so a drifted server must not crash or pollute the stream. Revives `response-metadata.timestamp` to a date.
Maps to `parseTranscriptionStreamPart`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ModelStreamPart](/api/Vercel.AI.Operations/ModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L297)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_SerializeTranscriptionStreamPart_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_SerializeTranscriptionStreamPart_Vercel_AI_Operations_ModelStreamPart_"></a>

### SerializeTranscriptionStreamPart(ModelStreamPart)


```csharp
public static string? SerializeTranscriptionStreamPart(ModelStreamPart part)
```


Server-side: serializes a transcription stream part as one text frame. Returns null for a part type the
envelope does not carry; callers drop the frame. Maps to `serializeTranscriptionStreamPart`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `part` | [ModelStreamPart](/api/Vercel.AI.Operations/ModelStreamPart/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L194)
