---
title: "AssemblyAITranscriptionModelOptions"
description: "Provider options for AssemblyAI transcription, read from providerOptions.assemblyai. Maps to AssemblyAITranscriptionModelOptions of assemblyai-transcription-..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAITranscriptionModelOptions"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAITranscriptionModelOptions
```


Provider options for AssemblyAI transcription, read from `providerOptions.assemblyai`.
Maps to `AssemblyAITranscriptionModelOptions` of `assemblyai-transcription-model-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L61)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AudioEndAt_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AudioEndAt"></a>

### AudioEndAt


```csharp
public int? AudioEndAt { get; init; }
```


End time of the audio in milliseconds. Maps to `audioEndAt`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L64)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AudioStartFrom_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AudioStartFrom"></a>

### AudioStartFrom


```csharp
public int? AudioStartFrom { get; init; }
```


Start time of the audio in milliseconds. Maps to `audioStartFrom`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L67)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AutoChapters_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AutoChapters"></a>

### AutoChapters


```csharp
public bool? AutoChapters { get; init; }
```


Whether to automatically generate chapters. Maps to `autoChapters`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L70)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AutoHighlights_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_AutoHighlights"></a>

### AutoHighlights


```csharp
public bool? AutoHighlights { get; init; }
```


Whether to automatically generate highlights. Maps to `autoHighlights`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L73)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_BoostParam_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_BoostParam"></a>

### BoostParam


```csharp
public string? BoostParam { get; init; }
```


Boost parameter for [WordBoost](#Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WordBoost): `low`, `default`, or `high`. Deprecated, use [KeytermsPrompt](#Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_KeytermsPrompt). Maps to `boostParam`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L76)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_ContentSafety_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_ContentSafety"></a>

### ContentSafety


```csharp
public bool? ContentSafety { get; init; }
```


Whether to enable content safety filtering. Maps to `contentSafety`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L79)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_ContentSafetyConfidence_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_ContentSafetyConfidence"></a>

### ContentSafetyConfidence


```csharp
public int? ContentSafetyConfidence { get; init; }
```


Confidence threshold for content safety filtering (25-100). Maps to `contentSafetyConfidence`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L82)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_CustomSpelling_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_CustomSpelling"></a>

### CustomSpelling


```csharp
public IReadOnlyList<AssemblyAICustomSpelling>? CustomSpelling { get; init; }
```


Custom spelling rules. Maps to `customSpelling`.

**Value:** [IReadOnlyList&lt;AssemblyAICustomSpelling&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L85)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Disfluencies_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Disfluencies"></a>

### Disfluencies


```csharp
public bool? Disfluencies { get; init; }
```


Whether to include filler words (um, uh, etc.). Maps to `disfluencies`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L88)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Domain_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Domain"></a>

### Domain


```csharp
public string? Domain { get; init; }
```


Domain-specific model, currently `medical-v1`. Maps to `domain`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L91)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_EntityDetection_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_EntityDetection"></a>

### EntityDetection


```csharp
public bool? EntityDetection { get; init; }
```


Whether to enable entity detection. Maps to `entityDetection`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L94)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_FilterProfanity_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_FilterProfanity"></a>

### FilterProfanity


```csharp
public bool? FilterProfanity { get; init; }
```


Whether to filter profanity. Maps to `filterProfanity`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L97)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_FormatText_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_FormatText"></a>

### FormatText


```csharp
public bool? FormatText { get; init; }
```


Whether to format text with punctuation and capitalization. Maps to `formatText`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L100)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_IabCategories_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_IabCategories"></a>

### IabCategories


```csharp
public bool? IabCategories { get; init; }
```


Whether to enable IAB categories detection. Maps to `iabCategories`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L103)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_KeytermsPrompt_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_KeytermsPrompt"></a>

### KeytermsPrompt


```csharp
public IReadOnlyList<string>? KeytermsPrompt { get; init; }
```


Domain-specific keyterms to boost recognition for (max 6 words per phrase). Maps to `keytermsPrompt`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L106)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageCode_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageCode"></a>

### LanguageCode


```csharp
public string? LanguageCode { get; init; }
```


Language code for the transcription. Maps to `languageCode`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L109)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageConfidenceThreshold_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageConfidenceThreshold"></a>

### LanguageConfidenceThreshold


```csharp
public double? LanguageConfidenceThreshold { get; init; }
```


Confidence threshold for language detection. Maps to `languageConfidenceThreshold`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L112)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageDetection_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageDetection"></a>

### LanguageDetection


```csharp
public bool? LanguageDetection { get; init; }
```


Whether to enable language detection. Maps to `languageDetection`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L115)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageDetectionOptions_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_LanguageDetectionOptions"></a>

### LanguageDetectionOptions


```csharp
public AssemblyAILanguageDetectionOptions? LanguageDetectionOptions { get; init; }
```


Options for automatic language detection. Maps to `languageDetectionOptions`.

**Value:** [AssemblyAILanguageDetectionOptions](/api/Vercel.AI.AssemblyAI/AssemblyAILanguageDetectionOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L118)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Multichannel_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Multichannel"></a>

### Multichannel


```csharp
public bool? Multichannel { get; init; }
```


Whether to process audio as multichannel. Maps to `multichannel`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L121)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Prompt_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; init; }
```


Natural-language context (up to 1,500 words) to steer the model. Maps to `prompt`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L124)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Punctuate_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Punctuate"></a>

### Punctuate


```csharp
public bool? Punctuate { get; init; }
```


Whether to add punctuation. Maps to `punctuate`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L127)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPii_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPii"></a>

### RedactPii


```csharp
public bool? RedactPii { get; init; }
```


Whether to redact personally identifiable information (PII). Maps to `redactPii`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L130)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiAudio_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiAudio"></a>

### RedactPiiAudio


```csharp
public bool? RedactPiiAudio { get; init; }
```


Whether to redact PII in the audio file. Maps to `redactPiiAudio`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L133)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiAudioOptions_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiAudioOptions"></a>

### RedactPiiAudioOptions


```csharp
public AssemblyAIRedactPiiAudioOptions? RedactPiiAudioOptions { get; init; }
```


Options for PII-redacted audio files. Requires [RedactPiiAudio](#Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiAudio). Maps to `redactPiiAudioOptions`.

**Value:** [AssemblyAIRedactPiiAudioOptions](/api/Vercel.AI.AssemblyAI/AssemblyAIRedactPiiAudioOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L136)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiAudioQuality_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiAudioQuality"></a>

### RedactPiiAudioQuality


```csharp
public string? RedactPiiAudioQuality { get; init; }
```


Audio format for PII redaction. Maps to `redactPiiAudioQuality`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L139)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiPolicies_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiPolicies"></a>

### RedactPiiPolicies


```csharp
public IReadOnlyList<string>? RedactPiiPolicies { get; init; }
```


List of PII types to redact. Maps to `redactPiiPolicies`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L142)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiReturnUnredacted_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiReturnUnredacted"></a>

### RedactPiiReturnUnredacted


```csharp
public bool? RedactPiiReturnUnredacted { get; init; }
```


Return the original unredacted transcript alongside the redacted one. Requires [RedactPii](#Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPii). Maps to `redactPiiReturnUnredacted`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L145)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiSub_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPiiSub"></a>

### RedactPiiSub


```csharp
public string? RedactPiiSub { get; init; }
```


Substitution method for redacted PII. Maps to `redactPiiSub`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L148)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactStaticEntities_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactStaticEntities"></a>

### RedactStaticEntities


```csharp
public IReadOnlyDictionary<string, IReadOnlyList<string>>? RedactStaticEntities { get; init; }
```


Map of user-defined labels to exact terms to redact. Requires [RedactPii](#Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RedactPii). Maps to `redactStaticEntities`.

**Value:** [IReadOnlyDictionary&lt;string, IReadOnlyList&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L151)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RemoveAudioTags_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_RemoveAudioTags"></a>

### RemoveAudioTags


```csharp
public string? RemoveAudioTags { get; init; }
```


Remove inline annotations from rich transcripts: `all` or `speaker`. Maps to `removeAudioTags`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L154)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SentimentAnalysis_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SentimentAnalysis"></a>

### SentimentAnalysis


```csharp
public bool? SentimentAnalysis { get; init; }
```


Whether to enable sentiment analysis. Maps to `sentimentAnalysis`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L157)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeakerLabels_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeakerLabels"></a>

### SpeakerLabels


```csharp
public bool? SpeakerLabels { get; init; }
```


Whether to identify different speakers. Maps to `speakerLabels`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L160)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeakerOptions_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeakerOptions"></a>

### SpeakerOptions


```csharp
public AssemblyAISpeakerOptions? SpeakerOptions { get; init; }
```


Options for speaker diarization. Maps to `speakerOptions`.

**Value:** [AssemblyAISpeakerOptions](/api/Vercel.AI.AssemblyAI/AssemblyAISpeakerOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L163)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeakersExpected_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeakersExpected"></a>

### SpeakersExpected


```csharp
public int? SpeakersExpected { get; init; }
```


Number of speakers expected in the audio. Maps to `speakersExpected`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L166)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeechThreshold_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SpeechThreshold"></a>

### SpeechThreshold


```csharp
public double? SpeechThreshold { get; init; }
```


Threshold for speech detection (0-1). Maps to `speechThreshold`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L169)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Summarization_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Summarization"></a>

### Summarization


```csharp
public bool? Summarization { get; init; }
```


Whether to generate a summary. Maps to `summarization`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L172)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SummaryModel_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SummaryModel"></a>

### SummaryModel


```csharp
public string? SummaryModel { get; init; }
```


Model to use for summarization. Maps to `summaryModel`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L175)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SummaryType_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_SummaryType"></a>

### SummaryType


```csharp
public string? SummaryType { get; init; }
```


Type of summary to generate. Maps to `summaryType`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L178)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Temperature_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; init; }
```


Sampling temperature (0-1). Maps to `temperature`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L181)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WebhookAuthHeaderName_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WebhookAuthHeaderName"></a>

### WebhookAuthHeaderName


```csharp
public string? WebhookAuthHeaderName { get; init; }
```


Name of the authentication header for webhook requests. Maps to `webhookAuthHeaderName`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L184)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WebhookAuthHeaderValue_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WebhookAuthHeaderValue"></a>

### WebhookAuthHeaderValue


```csharp
public string? WebhookAuthHeaderValue { get; init; }
```


Value of the authentication header for webhook requests. Maps to `webhookAuthHeaderValue`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L187)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WebhookUrl_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WebhookUrl"></a>

### WebhookUrl


```csharp
public string? WebhookUrl { get; init; }
```


URL to send webhook notifications to. Maps to `webhookUrl`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L190)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WordBoost_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_WordBoost"></a>

### WordBoost


```csharp
public IReadOnlyList<string>? WordBoost { get; init; }
```


List of words to boost recognition for. Deprecated, use [KeytermsPrompt](#Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_KeytermsPrompt). Maps to `wordBoost`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L193)

## Methods

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Parse_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionModelOptions_Parse_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `Parse(IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public static AssemblyAITranscriptionModelOptions? Parse(IReadOnlyDictionary<string, JsonElement>? providerOptions)
```


Reads `providerOptions.assemblyai`. Maps to `parseProviderOptions` with `assemblyaiTranscriptionModelOptionsSchema`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [AssemblyAITranscriptionModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape or is out of range. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L197)
