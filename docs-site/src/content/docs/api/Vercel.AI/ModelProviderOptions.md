---
title: "ModelProviderOptions"
description: "Per-model support for the options in the provider's ProviderOptionDefinitions, keyed by the same sections and option names. Upstream ModelProviderOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelProviderOptions"
  hidden: true
---

<a id="Vercel_AI_ModelProviderOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ModelProviderOptions
```


Per-model support for the options in the provider's [ProviderOptionDefinitions](/api/Vercel.AI/ProviderOptionDefinitions/), keyed by the same
sections and option names. Upstream `ModelProviderOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L394)

## Properties

<a id="Vercel_AI_ModelProviderOptions_Decision_"></a>
<a id="Vercel_AI_ModelProviderOptions_Decision"></a>

### Decision


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Decision { get; init; }
```


Upstream `decision`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L412)

<a id="Vercel_AI_ModelProviderOptions_Embedding_"></a>
<a id="Vercel_AI_ModelProviderOptions_Embedding"></a>

### Embedding


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Embedding { get; init; }
```


Upstream `embedding`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L400)

<a id="Vercel_AI_ModelProviderOptions_Image_"></a>
<a id="Vercel_AI_ModelProviderOptions_Image"></a>

### Image


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Image { get; init; }
```


Upstream `image`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L403)

<a id="Vercel_AI_ModelProviderOptions_Language_"></a>
<a id="Vercel_AI_ModelProviderOptions_Language"></a>

### Language


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Language { get; init; }
```


Upstream `language`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L397)

<a id="Vercel_AI_ModelProviderOptions_Realtime_"></a>
<a id="Vercel_AI_ModelProviderOptions_Realtime"></a>

### Realtime


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Realtime { get; init; }
```


Upstream `realtime`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L424)

<a id="Vercel_AI_ModelProviderOptions_Reranking_"></a>
<a id="Vercel_AI_ModelProviderOptions_Reranking"></a>

### Reranking


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Reranking { get; init; }
```


Upstream `reranking`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L409)

<a id="Vercel_AI_ModelProviderOptions_Speech_"></a>
<a id="Vercel_AI_ModelProviderOptions_Speech"></a>

### Speech


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Speech { get; init; }
```


Upstream `speech`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L421)

<a id="Vercel_AI_ModelProviderOptions_SpeechTranslation_"></a>
<a id="Vercel_AI_ModelProviderOptions_SpeechTranslation"></a>

### SpeechTranslation


```csharp
public IReadOnlyDictionary<string, ModelCapability>? SpeechTranslation { get; init; }
```


Upstream `speechTranslation`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L418)

<a id="Vercel_AI_ModelProviderOptions_Transcription_"></a>
<a id="Vercel_AI_ModelProviderOptions_Transcription"></a>

### Transcription


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Transcription { get; init; }
```


Upstream `transcription`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L415)

<a id="Vercel_AI_ModelProviderOptions_Video_"></a>
<a id="Vercel_AI_ModelProviderOptions_Video"></a>

### Video


```csharp
public IReadOnlyDictionary<string, ModelCapability>? Video { get; init; }
```


Upstream `video`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L406)
