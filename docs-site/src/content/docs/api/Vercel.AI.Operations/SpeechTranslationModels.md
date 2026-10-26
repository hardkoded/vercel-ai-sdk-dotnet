---
title: "SpeechTranslationModels"
description: "Resolves string speech-translation model ids."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechTranslationModels"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechTranslationModels"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class SpeechTranslationModels
```


Resolves string speech-translation model ids.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L88)

## Properties

<a id="Vercel_AI_Operations_SpeechTranslationModels_Default_"></a>
<a id="Vercel_AI_Operations_SpeechTranslationModels_Default"></a>

### Default


```csharp
public static Func<string, ISpeechTranslationCaller?>? Default { get; set; }
```


Default provider factory. Null means the provider has no speech translation models.

**Value:** [Func&lt;string, ISpeechTranslationCaller&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L91)
