---
title: "ElevenLabsMusicTimestampsResponse"
description: "Audio and optional lyric word timings from /v1/music/detailed. Maps to ElevenLabsMusicTimestampsResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsMusicTimestampsResponse"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicTimestampsResponse"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsMusicTimestampsResponse
```


Audio and optional lyric word timings from `/v1/music/detailed`. Maps to `ElevenLabsMusicTimestampsResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L36)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicTimestampsResponse__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicTimestampsResponse__ctor_System_Byte___System_Collections_Generic_IReadOnlyList_Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp__"></a>

### `ElevenLabsMusicTimestampsResponse(byte[], IReadOnlyList<ElevenLabsMusicWordTimestamp>?)`


```csharp
public ElevenLabsMusicTimestampsResponse(byte[] audio, IReadOnlyList<ElevenLabsMusicWordTimestamp>? wordTimestamps)
```


Creates a response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `wordTimestamps` | [IReadOnlyList&lt;ElevenLabsMusicWordTimestamp&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L39)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicTimestampsResponse_Audio_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicTimestampsResponse_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L49)

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicTimestampsResponse_WordTimestamps_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicTimestampsResponse_WordTimestamps"></a>

### WordTimestamps


```csharp
public IReadOnlyList<ElevenLabsMusicWordTimestamp>? WordTimestamps { get; }
```


Lyric word timings. Null when the response carries none.

**Value:** [IReadOnlyList&lt;ElevenLabsMusicWordTimestamp&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L52)
