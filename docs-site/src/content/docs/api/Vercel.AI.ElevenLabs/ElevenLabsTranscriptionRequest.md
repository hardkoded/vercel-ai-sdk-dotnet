---
title: "ElevenLabsTranscriptionRequest"
description: "Provider options and headers for one ElevenLabs transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsTranscriptionRequest"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionRequest"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsTranscriptionRequest
```


Provider options and headers for one ElevenLabs transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L170)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionRequest_Headers_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L176)

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionRequest_ProviderOptions_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. The `elevenlabs` object is read.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L173)
