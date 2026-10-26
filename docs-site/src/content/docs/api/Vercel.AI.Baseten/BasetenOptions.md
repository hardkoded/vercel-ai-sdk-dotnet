---
title: "BasetenOptions"
description: "Baseten settings, including an optional dedicated deployment URL."
editUrl: false
prev: false
next: false
sidebar:
  label: "BasetenOptions"
  hidden: true
---

<a id="Vercel_AI_Baseten_BasetenOptions"></a>

Namespace: [Vercel.AI.Baseten](/api/Vercel.AI.Baseten/)


```csharp
public sealed class BasetenOptions : OpenAICompatibleOptions
```


Baseten settings, including an optional dedicated deployment URL.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L117)

## Properties

<a id="Vercel_AI_Baseten_BasetenOptions_ModelUrl_"></a>
<a id="Vercel_AI_Baseten_BasetenOptions_ModelUrl"></a>

### ModelUrl


```csharp
public string? ModelUrl { get; set; }
```


Dedicated deployment URL. Chat requires `/sync/v1`. Embeddings require `/sync`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L120)
