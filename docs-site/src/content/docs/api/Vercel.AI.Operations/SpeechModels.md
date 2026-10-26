---
title: "SpeechModels"
description: "Resolves string speech model ids."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechModels"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechModels"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class SpeechModels
```


Resolves string speech model ids.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L304)

## Properties

<a id="Vercel_AI_Operations_SpeechModels_Default_"></a>
<a id="Vercel_AI_Operations_SpeechModels_Default"></a>

### Default


```csharp
public static Func<string, ISpeechCaller?>? Default { get; set; }
```


Default provider factory. Null means the provider has no speech models.

**Value:** [Func&lt;string, ISpeechCaller&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L307)
