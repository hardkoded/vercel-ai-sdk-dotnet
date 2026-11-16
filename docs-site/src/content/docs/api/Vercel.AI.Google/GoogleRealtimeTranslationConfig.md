---
title: "GoogleRealtimeTranslationConfig"
description: "Gemini Live Translation configuration. Maps to translationConfig of GoogleRealtimeModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeTranslationConfig"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeTranslationConfig"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeTranslationConfig
```


Gemini Live Translation configuration. Maps to `translationConfig` of `GoogleRealtimeModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L10)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeTranslationConfig_EchoTargetLanguage_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeTranslationConfig_EchoTargetLanguage"></a>

### EchoTargetLanguage


```csharp
public bool? EchoTargetLanguage { get; set; }
```


Whether input audio already in the target language should be echoed instead of producing silence.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L16)

<a id="Vercel_AI_Google_GoogleRealtimeTranslationConfig_TargetLanguageCode_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeTranslationConfig_TargetLanguageCode"></a>

### TargetLanguageCode


```csharp
public string? TargetLanguageCode { get; set; }
```


BCP-47 language code of the language to translate into. Defaults to `en` in the Gemini API.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtimeModelOptions.cs#L13)
