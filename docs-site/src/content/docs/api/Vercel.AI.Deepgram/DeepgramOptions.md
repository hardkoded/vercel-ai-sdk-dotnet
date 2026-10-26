---
title: "DeepgramOptions"
description: "Deepgram settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramOptions"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramOptions"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public class DeepgramOptions : OpenAICompatibleOptions
```


Deepgram settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L19)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramOptions_Clock_"></a>
<a id="Vercel_AI_Deepgram_DeepgramOptions_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for response timestamps. Defaults to UTC now.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L22)
