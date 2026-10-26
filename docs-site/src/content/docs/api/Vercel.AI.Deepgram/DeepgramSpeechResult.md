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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L156)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult__ctor_System_Byte___System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__System_String_Vercel_AI_Deepgram_DeepgramResponse_System_Text_Json_JsonElement_"></a>

### `DeepgramSpeechResult(byte[], string, IReadOnlyList<ModelWarning>, string, DeepgramResponse, JsonElement)`


```csharp
public DeepgramSpeechResult(byte[] audio, string mediaType, IReadOnlyList<ModelWarning> warnings, string requestBody, DeepgramResponse response, JsonElement providerMetadata)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L159)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Audio_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L170)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_MediaType_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Response media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L173)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


`{ "deepgram": { ... } }` with model and usage details from the `dg-*` response headers.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L185)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_RequestBody_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_RequestBody"></a>

### RequestBody


```csharp
public string RequestBody { get; }
```


JSON request body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L179)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Response_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Response"></a>

### Response


```csharp
public DeepgramResponse Response { get; }
```


Response metadata.

**Value:** [DeepgramResponse](/api/Vercel.AI.Deepgram/DeepgramResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L182)

<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Warnings_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings for ignored or adjusted settings.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L176)
