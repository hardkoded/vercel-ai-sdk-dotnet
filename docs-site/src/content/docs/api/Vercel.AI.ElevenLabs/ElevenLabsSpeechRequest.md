---
title: "ElevenLabsSpeechRequest"
description: "One ElevenLabs text-to-speech call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsSpeechRequest"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsSpeechRequest
```


One ElevenLabs text-to-speech call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L79)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest__ctor_System_String_"></a>

### ElevenLabsSpeechRequest(string)


```csharp
public ElevenLabsSpeechRequest(string text)
```


Creates a speech request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L85)

## Fields

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_DefaultVoiceId"></a>

### DefaultVoiceId


```csharp
public const string DefaultVoiceId = "21m00Tcm4TlvDq8ikWAM"
```


Default voice used when the caller does not set one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L82)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Headers_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L112)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Instructions_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Ignored. ElevenLabs does not accept instructions; a warning is returned.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L100)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Language_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Language code, sent as `language_code`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L103)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_OutputFormat_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_OutputFormat"></a>

### OutputFormat


```csharp
public string OutputFormat { get; set; }
```


Output format, such as `mp3` or `pcm_44100`. Sent as the `output_format` query parameter.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L97)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_ProviderOptions_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. The `elevenlabs` object is read.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L109)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Speed_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speaking rate, sent in `voice_settings`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L106)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Text_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L91)

<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Voice_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id. Null selects [DefaultVoiceId](#Vercel_AI_ElevenLabs_ElevenLabsSpeechRequest_DefaultVoiceId).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L94)
