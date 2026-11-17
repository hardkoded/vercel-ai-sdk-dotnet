---
title: "ModelCapabilities"
description: "Model metadata shared by provider implementations and generated documentation. Upstream ModelCapabilities. These declarations describe the capabilities expos..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelCapabilities"
  hidden: true
---

<a id="Vercel_AI_ModelCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ModelCapabilities
```


Model metadata shared by provider implementations and generated documentation. Upstream `ModelCapabilities`.
These declarations describe the capabilities exposed by the provider. They do not introduce validation or change
request behavior. A null section is undeclared, not explicitly unsupported.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L64)

## Properties

<a id="Vercel_AI_ModelCapabilities_Decision_"></a>
<a id="Vercel_AI_ModelCapabilities_Decision"></a>

### Decision


```csharp
public DecisionCapabilities? Decision { get; init; }
```


Upstream `decision`.

**Value:** [DecisionCapabilities](/api/Vercel.AI/DecisionCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L82)

<a id="Vercel_AI_ModelCapabilities_Embedding_"></a>
<a id="Vercel_AI_ModelCapabilities_Embedding"></a>

### Embedding


```csharp
public EmbeddingCapabilities? Embedding { get; init; }
```


Upstream `embedding`.

**Value:** [EmbeddingCapabilities](/api/Vercel.AI/EmbeddingCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L70)

<a id="Vercel_AI_ModelCapabilities_Image_"></a>
<a id="Vercel_AI_ModelCapabilities_Image"></a>

### Image


```csharp
public ImageCapabilities? Image { get; init; }
```


Upstream `image`.

**Value:** [ImageCapabilities](/api/Vercel.AI/ImageCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L73)

<a id="Vercel_AI_ModelCapabilities_Language_"></a>
<a id="Vercel_AI_ModelCapabilities_Language"></a>

### Language


```csharp
public LanguageCapabilities? Language { get; init; }
```


Upstream `language`.

**Value:** [LanguageCapabilities](/api/Vercel.AI/LanguageCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L67)

<a id="Vercel_AI_ModelCapabilities_Realtime_"></a>
<a id="Vercel_AI_ModelCapabilities_Realtime"></a>

### Realtime


```csharp
public RealtimeCapabilities? Realtime { get; init; }
```


Upstream `realtime`.

**Value:** [RealtimeCapabilities](/api/Vercel.AI/RealtimeCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L94)

<a id="Vercel_AI_ModelCapabilities_Reranking_"></a>
<a id="Vercel_AI_ModelCapabilities_Reranking"></a>

### Reranking


```csharp
public RerankingCapabilities? Reranking { get; init; }
```


Upstream `reranking`.

**Value:** [RerankingCapabilities](/api/Vercel.AI/RerankingCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L79)

<a id="Vercel_AI_ModelCapabilities_Speech_"></a>
<a id="Vercel_AI_ModelCapabilities_Speech"></a>

### Speech


```csharp
public SpeechCapabilities? Speech { get; init; }
```


Upstream `speech`.

**Value:** [SpeechCapabilities](/api/Vercel.AI/SpeechCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L91)

<a id="Vercel_AI_ModelCapabilities_SpeechTranslation_"></a>
<a id="Vercel_AI_ModelCapabilities_SpeechTranslation"></a>

### SpeechTranslation


```csharp
public SpeechTranslationCapabilities? SpeechTranslation { get; init; }
```


Upstream `speechTranslation`.

**Value:** [SpeechTranslationCapabilities](/api/Vercel.AI/SpeechTranslationCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L88)

<a id="Vercel_AI_ModelCapabilities_Transcription_"></a>
<a id="Vercel_AI_ModelCapabilities_Transcription"></a>

### Transcription


```csharp
public TranscriptionCapabilities? Transcription { get; init; }
```


Upstream `transcription`.

**Value:** [TranscriptionCapabilities](/api/Vercel.AI/TranscriptionCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L85)

<a id="Vercel_AI_ModelCapabilities_Video_"></a>
<a id="Vercel_AI_ModelCapabilities_Video"></a>

### Video


```csharp
public VideoCapabilities? Video { get; init; }
```


Upstream `video`.

**Value:** [VideoCapabilities](/api/Vercel.AI/VideoCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L76)
