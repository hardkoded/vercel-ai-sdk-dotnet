---
title: "AssemblyAILanguageDetectionOptions"
description: "Options for automatic language detection. Maps to languageDetectionOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAILanguageDetectionOptions"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAILanguageDetectionOptions
```


Options for automatic language detection. Maps to `languageDetectionOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L22)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_CodeSwitching_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_CodeSwitching"></a>

### CodeSwitching


```csharp
public bool? CodeSwitching { get; init; }
```


Whether code switching should be detected. Maps to `codeSwitching`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L31)

<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_CodeSwitchingConfidenceThreshold_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_CodeSwitchingConfidenceThreshold"></a>

### CodeSwitchingConfidenceThreshold


```csharp
public double? CodeSwitchingConfidenceThreshold { get; init; }
```


Confidence threshold for code switching detection (0-1). Maps to `codeSwitchingConfidenceThreshold`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L34)

<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_ExpectedLanguages_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_ExpectedLanguages"></a>

### ExpectedLanguages


```csharp
public IReadOnlyList<string>? ExpectedLanguages { get; init; }
```


List of languages expected in the audio file. Maps to `expectedLanguages`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L25)

<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_FallbackLanguage_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAILanguageDetectionOptions_FallbackLanguage"></a>

### FallbackLanguage


```csharp
public string? FallbackLanguage { get; init; }
```


Fallback language if the detected language is not expected. Maps to `fallbackLanguage`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L28)
