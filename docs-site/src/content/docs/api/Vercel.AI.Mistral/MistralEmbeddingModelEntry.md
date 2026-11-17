---
title: "MistralEmbeddingModelEntry"
description: "Embedding model entry of the Mistral catalog."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralEmbeddingModelEntry"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralEmbeddingModelEntry"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralEmbeddingModelEntry
```


Embedding model entry of the Mistral catalog.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L199)

## Properties

<a id="Vercel_AI_Mistral_MistralEmbeddingModelEntry_Embedding_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModelEntry_Embedding"></a>

### Embedding


```csharp
public EmbeddingCapabilities Embedding { get; init; }
```


Upstream `capabilities.embedding`.

**Value:** [EmbeddingCapabilities](/api/Vercel.AI/EmbeddingCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L202)

<a id="Vercel_AI_Mistral_MistralEmbeddingModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Upstream `providerOptions.embedding`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L205)
