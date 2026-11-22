---
title: "ProviderOptionDefinitions"
description: "The provider options a provider accepts, grouped by the operation they configure. Upstream ProviderOptionDefinitions. Section names match ModelCapabilities;..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderOptionDefinitions"
  hidden: true
---

<a id="Vercel_AI_ProviderOptionDefinitions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ProviderOptionDefinitions
```


The provider options a provider accepts, grouped by the operation they configure. Upstream
`ProviderOptionDefinitions`. Section names match [ModelCapabilities](/api/Vercel.AI/ModelCapabilities/); option names match the keys
accepted under `providerOptions.<provider>` for that operation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L363)

## Properties

<a id="Vercel_AI_ProviderOptionDefinitions_Decision_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Decision"></a>

### Decision


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Decision { get; init; }
```


Upstream `decision`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L381)

<a id="Vercel_AI_ProviderOptionDefinitions_Embedding_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Embedding"></a>

### Embedding


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Embedding { get; init; }
```


Upstream `embedding`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L369)

<a id="Vercel_AI_ProviderOptionDefinitions_Image_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Image"></a>

### Image


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Image { get; init; }
```


Upstream `image`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L372)

<a id="Vercel_AI_ProviderOptionDefinitions_Language_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Language"></a>

### Language


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Language { get; init; }
```


Upstream `language`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L366)

<a id="Vercel_AI_ProviderOptionDefinitions_Realtime_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Realtime"></a>

### Realtime


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Realtime { get; init; }
```


Upstream `realtime`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L393)

<a id="Vercel_AI_ProviderOptionDefinitions_Reranking_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Reranking"></a>

### Reranking


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Reranking { get; init; }
```


Upstream `reranking`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L378)

<a id="Vercel_AI_ProviderOptionDefinitions_Speech_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Speech"></a>

### Speech


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Speech { get; init; }
```


Upstream `speech`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L390)

<a id="Vercel_AI_ProviderOptionDefinitions_SpeechTranslation_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_SpeechTranslation"></a>

### SpeechTranslation


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? SpeechTranslation { get; init; }
```


Upstream `speechTranslation`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L387)

<a id="Vercel_AI_ProviderOptionDefinitions_Transcription_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Transcription"></a>

### Transcription


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Transcription { get; init; }
```


Upstream `transcription`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L384)

<a id="Vercel_AI_ProviderOptionDefinitions_Video_"></a>
<a id="Vercel_AI_ProviderOptionDefinitions_Video"></a>

### Video


```csharp
public IReadOnlyDictionary<string, ProviderOptionDefinition>? Video { get; init; }
```


Upstream `video`.

**Value:** [IReadOnlyDictionary&lt;string, ProviderOptionDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L375)
