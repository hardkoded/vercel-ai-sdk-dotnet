---
title: "GoogleSpeechResult"
description: "Speech audio plus the response metadata recorded for the call."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechResult"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechResult"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleSpeechResult
```


Speech audio plus the response metadata recorded for the call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L500)

## Constructors

<a id="Vercel_AI_Google_GoogleSpeechResult__ctor_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult__ctor_System_Byte___System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleWarning__System_Text_Json_Nodes_JsonObject_System_String_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_DateTimeOffset_System_String_System_Int32_System_String_"></a>

### `GoogleSpeechResult(byte[], string, IReadOnlyList<GoogleWarning>, JsonObject?, string, JsonElement, IReadOnlyDictionary<string, string>, DateTimeOffset, string, int, string?)`


```csharp
public GoogleSpeechResult(byte[] audio, string mediaType, IReadOnlyList<GoogleWarning> warnings, JsonObject? usage, string requestBody, JsonElement responseBody, IReadOnlyDictionary<string, string> headers, DateTimeOffset timestamp, string modelId, int sampleRate, string? mimeType)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `requestBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseBody` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sampleRate` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `mimeType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L503)

## Properties

<a id="Vercel_AI_Google_GoogleSpeechResult_Audio_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes. Raw PCM is wrapped in a WAV container unless `pcm` was requested.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L530)

<a id="Vercel_AI_Google_GoogleSpeechResult_Headers_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L548)

<a id="Vercel_AI_Google_GoogleSpeechResult_MediaType_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type of the audio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L533)

<a id="Vercel_AI_Google_GoogleSpeechResult_MimeType_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_MimeType"></a>

### MimeType


```csharp
public string? MimeType { get; }
```


Response mime type, when the provider sent one. Maps to `providerMetadata.google.mimeType`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L560)

<a id="Vercel_AI_Google_GoogleSpeechResult_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id used for the call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L554)

<a id="Vercel_AI_Google_GoogleSpeechResult_RequestBody_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_RequestBody"></a>

### RequestBody


```csharp
public string RequestBody { get; }
```


Body sent to the provider. Maps to `request.body`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L542)

<a id="Vercel_AI_Google_GoogleSpeechResult_ResponseBody_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_ResponseBody"></a>

### ResponseBody


```csharp
public JsonElement ResponseBody { get; }
```


Raw response body. Maps to `response.body`.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L545)

<a id="Vercel_AI_Google_GoogleSpeechResult_SampleRate_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_SampleRate"></a>

### SampleRate


```csharp
public int SampleRate { get; }
```


Sample rate from the response mime type, or 24000. Maps to `providerMetadata.google.sampleRate`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L557)

<a id="Vercel_AI_Google_GoogleSpeechResult_Timestamp_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp recorded for the call.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L551)

<a id="Vercel_AI_Google_GoogleSpeechResult_Usage_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_Usage"></a>

### Usage


```csharp
public JsonObject? Usage { get; }
```


Raw `usageMetadata` from the response, when present.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L539)

<a id="Vercel_AI_Google_GoogleSpeechResult_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleSpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Warnings produced for the call.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L536)
