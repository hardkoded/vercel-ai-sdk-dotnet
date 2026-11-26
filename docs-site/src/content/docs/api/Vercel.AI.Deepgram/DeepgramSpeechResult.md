---
title: "DeepgramSpeechResult"
description: "Synthesized Deepgram audio."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramSpeechResult"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramSpeechResult
```


Synthesized Deepgram audio.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L174)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult__ctor_System_Byte___System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__System_String_Vercel_AI_Deepgram_DeepgramResponse_System_Text_Json_JsonElement_Vercel_AI_Provider_AudioUsage_"></a>

### `DeepgramSpeechResult(byte[], string, IReadOnlyList<ModelWarning>, string, DeepgramResponse, JsonElement, AudioUsage?)`


```csharp
public DeepgramSpeechResult(byte[] audio, string mediaType, IReadOnlyList<ModelWarning> warnings, string requestBody, DeepgramResponse response, JsonElement providerMetadata, AudioUsage? usage = null)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `requestBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `response` | [DeepgramResponse](/api/Vercel.AI.Deepgram/DeepgramResponse/) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `usage` | [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L177)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Audio_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L197)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_MediaType_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Response media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L200)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


`{ "deepgram": { ... } }` with model and usage details from the `dg-*` response headers.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L212)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_RequestBody_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_RequestBody"></a>

### RequestBody


```csharp
public string RequestBody { get; }
```


JSON request body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L206)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Response_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Response"></a>

### Response


```csharp
public DeepgramResponse Response { get; }
```


Response metadata.

**Value:** [DeepgramResponse](/api/Vercel.AI.Deepgram/DeepgramResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L209)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Usage_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Usage"></a>

### Usage


```csharp
public AudioUsage? Usage { get; }
```


Characters billed, from the `dg-char-count` header. Null when the header is absent.

**Value:** [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L215)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Warnings_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings for ignored or adjusted settings.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L203)
