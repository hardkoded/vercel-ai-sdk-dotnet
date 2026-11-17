---
title: "MistralModelCatalog"
description: "Mistral model catalog. Maps to mistral-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralModelCatalog"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralModelCatalog"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public static class MistralModelCatalog
```


Mistral model catalog. Maps to `mistral-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L229)

## Properties

<a id="Vercel_AI_Mistral_MistralModelCatalog_EmbeddingModelIdList_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_EmbeddingModelIdList"></a>

### EmbeddingModelIdList


```csharp
public static IReadOnlyList<string> EmbeddingModelIdList { get; }
```


Known embedding model ids. Maps to `MistralModelIds['embedding']`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L315)

<a id="Vercel_AI_Mistral_MistralModelCatalog_Entries_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_Entries"></a>

### Entries


```csharp
public static IReadOnlyDictionary<string, MistralModelEntry> Entries { get; }
```


Entries of every model id that declares capabilities. Maps to `mistralModelCatalog`; renamed because C# does not allow a member named like its class.

**Value:** [IReadOnlyDictionary&lt;string, MistralModelEntry&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L391)

<a id="Vercel_AI_Mistral_MistralModelCatalog_LanguageModelIds_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_LanguageModelIds"></a>

### LanguageModelIds


```csharp
public static IReadOnlyList<string> LanguageModelIds { get; }
```


Known language model ids. Maps to `MistralModelIds['language']`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L311)

<a id="Vercel_AI_Mistral_MistralModelCatalog_SpeechModelIdList_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_SpeechModelIdList"></a>

### SpeechModelIdList


```csharp
public static IReadOnlyList<string> SpeechModelIdList { get; }
```


Known speech model ids. Maps to `MistralModelIds['speech']`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L318)

<a id="Vercel_AI_Mistral_MistralModelCatalog_TranscriptionModelIdList_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_TranscriptionModelIdList"></a>

### TranscriptionModelIdList


```csharp
public static IReadOnlyList<string> TranscriptionModelIdList { get; }
```


Known transcription model ids. Maps to `MistralModelIds['transcription']`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L321)

## Methods

<a id="Vercel_AI_Mistral_MistralModelCatalog_GetMistralLanguageModelEntry_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_GetMistralLanguageModelEntry_System_String_"></a>

### GetMistralLanguageModelEntry(string)


```csharp
public static MistralLanguageModelEntry GetMistralLanguageModelEntry(string modelId)
```


Returns the entry of a language model. Any other model id resolves to an entry with unknown support. Maps to `getMistralLanguageModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [MistralLanguageModelEntry](/api/Vercel.AI.Mistral/MistralLanguageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L394)

<a id="Vercel_AI_Mistral_MistralModelCatalog_GetMistralSpeechModelEntry_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_GetMistralSpeechModelEntry_System_String_"></a>

### GetMistralSpeechModelEntry(string)


```csharp
public static MistralSpeechModelEntry GetMistralSpeechModelEntry(string modelId)
```


Returns the entry of a speech model. Maps to `getMistralSpeechModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [MistralSpeechModelEntry](/api/Vercel.AI.Mistral/MistralSpeechModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L408)

<a id="Vercel_AI_Mistral_MistralModelCatalog_GetMistralTranscriptionModelEntry_"></a>
<a id="Vercel_AI_Mistral_MistralModelCatalog_GetMistralTranscriptionModelEntry_System_String_"></a>

### GetMistralTranscriptionModelEntry(string)


```csharp
public static MistralTranscriptionModelEntry GetMistralTranscriptionModelEntry(string modelId)
```


Returns the entry of a transcription model. Maps to `getMistralTranscriptionModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [MistralTranscriptionModelEntry](/api/Vercel.AI.Mistral/MistralTranscriptionModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L400)
