---
title: "SpeechCapabilities"
description: "Upstream ModelCapabilities.speech."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechCapabilities"
  hidden: true
---

<a id="Vercel_AI_SpeechCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class SpeechCapabilities
```


Upstream `ModelCapabilities.speech`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L336)

## Properties

<a id="Vercel_AI_SpeechCapabilities_Description_"></a>
<a id="Vercel_AI_SpeechCapabilities_Description"></a>

### Description


```csharp
public string? Description { get; init; }
```


A human-readable model description. Not in the upstream type; upstream Fal entries spread it into `capabilities.speech`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L339)

<a id="Vercel_AI_SpeechCapabilities_Instructions_"></a>
<a id="Vercel_AI_SpeechCapabilities_Instructions"></a>

### Instructions


```csharp
public ModelCapability Instructions { get; init; }
```


Upstream `instructions`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L342)
