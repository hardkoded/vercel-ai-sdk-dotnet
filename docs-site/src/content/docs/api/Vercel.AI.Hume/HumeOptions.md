---
title: "HumeOptions"
description: "Hume settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeOptions"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeOptions"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public class HumeOptions : OpenAICompatibleOptions
```


Hume settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L14)

## Properties

<a id="Vercel_AI_Hume_HumeOptions_Clock_"></a>
<a id="Vercel_AI_Hume_HumeOptions_Clock"></a>

### Clock


```csharp
public Func<DateTimeOffset>? Clock { get; set; }
```


Clock used for response timestamps.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L17)
