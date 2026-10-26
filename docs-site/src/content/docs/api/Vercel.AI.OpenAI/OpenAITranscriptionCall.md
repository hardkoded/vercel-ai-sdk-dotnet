---
title: "OpenAITranscriptionCall"
description: "Transcription settings beyond the audio payload."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAITranscriptionCall"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAITranscriptionCall
```


Transcription settings beyond the audio payload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L17)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall__ctor_Vercel_AI_Provider_AudioInput_"></a>

### OpenAITranscriptionCall(AudioInput)


```csharp
public OpenAITranscriptionCall(AudioInput audio)
```


Creates a transcription call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L20)

## Properties

<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall_Audio_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall_Audio"></a>

### Audio


```csharp
public AudioInput Audio { get; }
```


Audio to transcribe.

**Value:** [AudioInput](/api/Vercel.AI.Provider/AudioInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L26)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra HTTP headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L32)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall_ProviderOptions_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. The `openai` object is read.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L29)
