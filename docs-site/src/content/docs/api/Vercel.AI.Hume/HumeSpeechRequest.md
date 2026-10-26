---
title: "HumeSpeechRequest"
description: "Hume speech request."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeSpeechRequest"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeSpeechRequest"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeSpeechRequest
```


Hume speech request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L95)

## Constructors

<a id="Vercel_AI_Hume_HumeSpeechRequest__ctor_"></a>
<a id="Vercel_AI_Hume_HumeSpeechRequest__ctor_System_String_"></a>

### HumeSpeechRequest(string)


```csharp
public HumeSpeechRequest(string text)
```


Creates a speech request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L101)

## Fields

<a id="Vercel_AI_Hume_HumeSpeechRequest_DefaultVoiceId"></a>

### DefaultVoiceId


```csharp
public const string DefaultVoiceId = "d8ab67c6-953d-4bd8-9370-8fa53a0f1453"
```


Default Hume voice used when the caller does not set one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L98)

## Properties

<a id="Vercel_AI_Hume_HumeSpeechRequest_Headers_"></a>
<a id="Vercel_AI_Hume_HumeSpeechRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L122)

<a id="Vercel_AI_Hume_HumeSpeechRequest_Language_"></a>
<a id="Vercel_AI_Hume_HumeSpeechRequest_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Ignored. Hume does not accept a language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L119)

<a id="Vercel_AI_Hume_HumeSpeechRequest_OutputFormat_"></a>
<a id="Vercel_AI_Hume_HumeSpeechRequest_OutputFormat"></a>

### OutputFormat


```csharp
public string OutputFormat { get; set; }
```


Output container. `mp3`, `pcm`, and `wav` are sent as-is.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L113)

<a id="Vercel_AI_Hume_HumeSpeechRequest_Speed_"></a>
<a id="Vercel_AI_Hume_HumeSpeechRequest_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speaking rate, when set.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L116)

<a id="Vercel_AI_Hume_HumeSpeechRequest_Text_"></a>
<a id="Vercel_AI_Hume_HumeSpeechRequest_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L107)

<a id="Vercel_AI_Hume_HumeSpeechRequest_Voice_"></a>
<a id="Vercel_AI_Hume_HumeSpeechRequest_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id. Null selects [DefaultVoiceId](#Vercel_AI_Hume_HumeSpeechRequest_DefaultVoiceId).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L110)
