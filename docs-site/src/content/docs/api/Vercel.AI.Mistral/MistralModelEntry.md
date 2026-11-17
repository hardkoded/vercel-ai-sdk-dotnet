---
title: "MistralModelEntry"
description: "An entry of Entries. Maps to MistralModelEntry. Exactly the sections the model declares are set."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralModelEntry"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralModelEntry"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralModelEntry
```


An entry of [Entries](/api/Vercel.AI.Mistral/MistralModelCatalog/#Vercel_AI_Mistral_MistralModelCatalog_Entries). Maps to `MistralModelEntry`. Exactly the sections the
model declares are set.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L213)

## Properties

<a id="Vercel_AI_Mistral_MistralModelEntry_Embedding_"></a>
<a id="Vercel_AI_Mistral_MistralModelEntry_Embedding"></a>

### Embedding


```csharp
public MistralEmbeddingModelEntry? Embedding { get; init; }
```


Embedding section, or null.

**Value:** [MistralEmbeddingModelEntry](/api/Vercel.AI.Mistral/MistralEmbeddingModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L225)

<a id="Vercel_AI_Mistral_MistralModelEntry_Language_"></a>
<a id="Vercel_AI_Mistral_MistralModelEntry_Language"></a>

### Language


```csharp
public MistralLanguageModelEntry? Language { get; init; }
```


Language section, or null.

**Value:** [MistralLanguageModelEntry](/api/Vercel.AI.Mistral/MistralLanguageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L216)

<a id="Vercel_AI_Mistral_MistralModelEntry_Speech_"></a>
<a id="Vercel_AI_Mistral_MistralModelEntry_Speech"></a>

### Speech


```csharp
public MistralSpeechModelEntry? Speech { get; init; }
```


Speech section, or null.

**Value:** [MistralSpeechModelEntry](/api/Vercel.AI.Mistral/MistralSpeechModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L222)

<a id="Vercel_AI_Mistral_MistralModelEntry_Transcription_"></a>
<a id="Vercel_AI_Mistral_MistralModelEntry_Transcription"></a>

### Transcription


```csharp
public MistralTranscriptionModelEntry? Transcription { get; init; }
```


Transcription section, or null.

**Value:** [MistralTranscriptionModelEntry](/api/Vercel.AI.Mistral/MistralTranscriptionModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L219)
