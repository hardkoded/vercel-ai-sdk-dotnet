---
title: "OpenAISpeechModelOptions"
description: "OpenAI speech model options, read from the openai provider options. Routers that also call the OpenAI speech API (Azure) accept the same options under their..."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISpeechModelOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISpeechModelOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISpeechModelOptions
```


OpenAI speech model options, read from the `openai` provider options. Routers that also call the OpenAI
speech API (Azure) accept the same options under their own key. Maps to `OpenAISpeechModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISpeechModelOptions_Instructions_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModelOptions_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Control the voice of the generated audio, for example `Speak in a slow and steady tone`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModelOptions.cs#L16)

<a id="Vercel_AI_OpenAI_OpenAISpeechModelOptions_Speed_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModelOptions_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speed of the generated audio, from 0.25 to 4.0.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModelOptions.cs#L19)

## Methods

<a id="Vercel_AI_OpenAI_OpenAISpeechModelOptions_Shape_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechModelOptions_Shape"></a>

### Shape()


```csharp
public static IReadOnlyList<string> Shape()
```


The option keys. Maps to `openaiSpeechModelOptionsShape`.

**Returns:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModelOptions.cs#L22)
