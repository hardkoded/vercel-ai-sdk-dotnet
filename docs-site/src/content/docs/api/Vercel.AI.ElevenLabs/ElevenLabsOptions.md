---
title: "ElevenLabsOptions"
description: "ElevenLabs settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsOptions"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsOptions"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public class ElevenLabsOptions : OpenAICompatibleOptions
```


ElevenLabs settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L19)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsOptions_Clock_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsOptions_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for response timestamps. Defaults to UTC now.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L22)
