---
title: "DeepgramSpeechRequest"
description: "One Deepgram text-to-speech call."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramSpeechRequest"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramSpeechRequest
```


One Deepgram text-to-speech call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L122)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest__ctor_System_String_"></a>

### DeepgramSpeechRequest(string)


```csharp
public DeepgramSpeechRequest(string text)
```


Creates a speech request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L125)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Headers_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L152)

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Instructions_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Ignored. Deepgram does not accept instructions; a warning is returned.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L146)

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Language_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Language. Used to compose family model ids. `auto` falls back to `en`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L143)

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_OutputFormat_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_OutputFormat"></a>

### OutputFormat


```csharp
public string OutputFormat { get; set; }
```


Output format, such as `mp3`, `wav`, or `linear16_16000`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L137)

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. The `deepgram` object is read.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L149)

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Speed_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speaking rate, sent as the `speed` query parameter.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L140)

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Text_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L131)

<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Voice_"></a>
<a id="Vercel_AI_Deepgram_DeepgramSpeechRequest_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice. Required for the `aura` and `aura-2` family ids, where it becomes part of the model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L134)
