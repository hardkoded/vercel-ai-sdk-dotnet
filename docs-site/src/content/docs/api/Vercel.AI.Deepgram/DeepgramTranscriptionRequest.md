---
title: "DeepgramTranscriptionRequest"
description: "Provider options and headers for one Deepgram transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramTranscriptionRequest"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionRequest"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramTranscriptionRequest
```


Provider options and headers for one Deepgram transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L189)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionRequest_Headers_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L195)

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. The `deepgram` object is read.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L192)
