---
title: "GoogleVertexSpeechTranscriptionModel"
description: "Cloud Speech-to-Text recognize."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexSpeechTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexSpeechTranscriptionModel : ITranscriptionModel
```


Cloud Speech-to-Text recognize.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L99)

## Constructors

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel__ctor_Vercel_AI_Google_GoogleVertexProvider_System_String_"></a>

### GoogleVertexSpeechTranscriptionModel(GoogleVertexProvider, string)


```csharp
public GoogleVertexSpeechTranscriptionModel(GoogleVertexProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L104)

## Properties

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L117)

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L111)

## Methods

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

### DoTranscribeAsync(AudioInput, CancellationToken)


```csharp
public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
```


Transcribes audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L120)

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_ParseDuration_"></a>
<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_ParseDuration_System_String_"></a>

### ParseDuration(string?)


```csharp
public static double? ParseDuration(string? value)
```


Parses a duration such as `1.200s`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L177)

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_Request_"></a>
<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_Request_System_String_System_Byte___System_String___"></a>

### Request(string, byte[], string[]?)


```csharp
public static JsonObject Request(string modelId, byte[] audio, string[]? languageCodes)
```


Builds the recognize body. Audio bytes are base64-encoded.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `languageCodes` | [string\[\]](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L130)

<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_ToIso6391_"></a>
<a id="Vercel_AI_Google_GoogleVertexSpeechTranscriptionModel_ToIso6391_System_String_"></a>

### ToIso6391(string?)


```csharp
public static string? ToIso6391(string? tag)
```


Maps a BCP-47 tag onto a two-letter ISO-639-1 code.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tag` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L159)
