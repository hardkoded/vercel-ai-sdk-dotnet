---
title: "TranslateOptions"
description: "Speech translation options."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranslateOptions"
  hidden: true
---

<a id="Vercel_AI_TranslateOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TranslateOptions
```


Speech translation options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L410)

## Properties

<a id="Vercel_AI_TranslateOptions_Audio_"></a>
<a id="Vercel_AI_TranslateOptions_Audio"></a>

### Audio


```csharp
public AudioInput Audio { get; set; }
```


Audio.

**Value:** [AudioInput](/api/Vercel.AI.Provider/AudioInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L416)

<a id="Vercel_AI_TranslateOptions_Model_"></a>
<a id="Vercel_AI_TranslateOptions_Model"></a>

### Model


```csharp
public ISpeechTranslationModel? Model { get; set; }
```


Speech translation model.

**Value:** [ISpeechTranslationModel](/api/Vercel.AI.Provider/ISpeechTranslationModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L413)
