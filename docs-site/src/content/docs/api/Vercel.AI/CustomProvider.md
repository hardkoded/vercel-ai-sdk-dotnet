---
title: "CustomProvider"
description: "A provider backed by caller-supplied models. Maps to customProvider. A model id that is not registered goes to FallbackProvider, or throws NoSuchModelError w..."
editUrl: false
prev: false
next: false
sidebar:
  label: "CustomProvider"
  hidden: true
---

<a id="Vercel_AI_CustomProvider"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class CustomProvider : ProviderBase, IDecisionProvider
```


A provider backed by caller-supplied models. Maps to `customProvider`. A model id that is not registered goes to
[FallbackProvider](#Vercel_AI_CustomProvider_FallbackProvider), or throws [NoSuchModelError](/api/Vercel.AI.Util/NoSuchModelError/) when there is none.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

**Implements:** [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L320)

## Constructors

<a id="Vercel_AI_CustomProvider__ctor_"></a>
<a id="Vercel_AI_CustomProvider__ctor_System_String_"></a>

### CustomProvider(string)


```csharp
public CustomProvider(string name = "custom")
```


Creates a custom provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L334)

## Properties

<a id="Vercel_AI_CustomProvider_FallbackProvider_"></a>
<a id="Vercel_AI_CustomProvider_FallbackProvider"></a>

### FallbackProvider


```csharp
public ProviderBase? FallbackProvider { get; set; }
```


Provider asked for model ids that are not registered here.

**Value:** [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L340)

<a id="Vercel_AI_CustomProvider_Files_"></a>
<a id="Vercel_AI_CustomProvider_Files"></a>

### Files


```csharp
public IFileStore? Files { get; set; }
```


File store. When null, the fallback provider's file store is used.

**Value:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L343)

<a id="Vercel_AI_CustomProvider_Skills_"></a>
<a id="Vercel_AI_CustomProvider_Skills"></a>

### Skills


```csharp
public ISkillStore? Skills { get; set; }
```


Skill store. When null, the fallback provider's skill store is used.

**Value:** [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L346)

## Methods

<a id="Vercel_AI_CustomProvider_AddDecisionModel_"></a>
<a id="Vercel_AI_CustomProvider_AddDecisionModel_System_String_System_String_"></a>

### AddDecisionModel(string, string)


```csharp
public CustomProvider AddDecisionModel(string modelId, string providerModelId)
```


Registers a string alias for a decision model. The id is resolved through
[DefaultProvider](/api/Vercel.AI.Operations/DecisionModels/#Vercel_AI_Operations_DecisionModels_DefaultProvider), or Gateway when none is set, when [DecisionModel(string)](#Vercel_AI_CustomProvider_DecisionModel_System_String_) is called.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerModelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L440)

<a id="Vercel_AI_CustomProvider_AddDecisionModel_System_String_Vercel_AI_Operations_IDecisionCaller_"></a>

### AddDecisionModel(string, IDecisionCaller)


```csharp
public CustomProvider AddDecisionModel(string modelId, IDecisionCaller model)
```


Registers a decision model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;. Maps to `decisionModels`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L426)

<a id="Vercel_AI_CustomProvider_AddEmbeddingModel_"></a>
<a id="Vercel_AI_CustomProvider_AddEmbeddingModel_System_String_Vercel_AI_Provider_IEmbeddingModel_"></a>

### AddEmbeddingModel(string, IEmbeddingModel)


```csharp
public CustomProvider AddEmbeddingModel(string modelId, IEmbeddingModel model)
```


Registers an embedding model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L377)

<a id="Vercel_AI_CustomProvider_AddEmbeddingModel_Vercel_AI_Provider_IEmbeddingModel_"></a>

### AddEmbeddingModel(IEmbeddingModel)


```csharp
public CustomProvider AddEmbeddingModel(IEmbeddingModel model)
```


Registers an embedding model under its own model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L366)

<a id="Vercel_AI_CustomProvider_AddEvaluationModel_"></a>
<a id="Vercel_AI_CustomProvider_AddEvaluationModel_System_String_Vercel_AI_Operations_IEvaluationCaller_"></a>

### AddEvaluationModel(string, IEvaluationCaller)


```csharp
[Obsolete("Use AddDecisionModel instead.")]
public CustomProvider AddEvaluationModel(string modelId, IEvaluationCaller model)
```


Registers a deprecated decision model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;. Maps to `evaluationModels`.
These models are ignored once any model is added with `AddDecisionModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/) |  |

**Returns:** [CustomProvider](./)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L454)

<a id="Vercel_AI_CustomProvider_AddEvaluationModel_System_String_Vercel_AI_Provider_IEvaluationModel_"></a>

### AddEvaluationModel(string, IEvaluationModel)


```csharp
[Obsolete("Use AddRubricModel instead.")]
public CustomProvider AddEvaluationModel(string modelId, IEvaluationModel model)
```


Deprecated name of `IRubricModel)`. Registers a rubric model, not a decision model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IEvaluationModel](/api/Vercel.AI.Provider/IEvaluationModel/) |  |

**Returns:** [CustomProvider](./)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L419)

<a id="Vercel_AI_CustomProvider_AddImageModel_"></a>
<a id="Vercel_AI_CustomProvider_AddImageModel_System_String_Vercel_AI_Provider_IImageModel_"></a>

### AddImageModel(string, IImageModel)


```csharp
public CustomProvider AddImageModel(string modelId, IImageModel model)
```


Registers an image model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IImageModel](/api/Vercel.AI.Provider/IImageModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L383)

<a id="Vercel_AI_CustomProvider_AddLanguageModel_"></a>
<a id="Vercel_AI_CustomProvider_AddLanguageModel_System_String_Vercel_AI_Provider_ILanguageModel_"></a>

### AddLanguageModel(string, ILanguageModel)


```csharp
public CustomProvider AddLanguageModel(string modelId, ILanguageModel model)
```


Registers a language model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L360)

<a id="Vercel_AI_CustomProvider_AddLanguageModel_Vercel_AI_Provider_ILanguageModel_"></a>

### AddLanguageModel(ILanguageModel)


```csharp
public CustomProvider AddLanguageModel(ILanguageModel model)
```


Registers a language model under its own model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L349)

<a id="Vercel_AI_CustomProvider_AddRerankingModel_"></a>
<a id="Vercel_AI_CustomProvider_AddRerankingModel_System_String_Vercel_AI_Provider_IRerankingModel_"></a>

### AddRerankingModel(string, IRerankingModel)


```csharp
public CustomProvider AddRerankingModel(string modelId, IRerankingModel model)
```


Registers a reranking model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L401)

<a id="Vercel_AI_CustomProvider_AddRubricModel_"></a>
<a id="Vercel_AI_CustomProvider_AddRubricModel_System_String_Vercel_AI_Provider_IRubricModel_"></a>

### AddRubricModel(string, IRubricModel)


```csharp
public CustomProvider AddRubricModel(string modelId, IRubricModel model)
```


Registers a model that scores a candidate against a rubric under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L413)

<a id="Vercel_AI_CustomProvider_AddSpeechModel_"></a>
<a id="Vercel_AI_CustomProvider_AddSpeechModel_System_String_Vercel_AI_Provider_ISpeechModel_"></a>

### AddSpeechModel(string, ISpeechModel)


```csharp
public CustomProvider AddSpeechModel(string modelId, ISpeechModel model)
```


Registers a speech model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L395)

<a id="Vercel_AI_CustomProvider_AddTranscriptionModel_"></a>
<a id="Vercel_AI_CustomProvider_AddTranscriptionModel_System_String_Vercel_AI_Provider_ITranscriptionModel_"></a>

### AddTranscriptionModel(string, ITranscriptionModel)


```csharp
public CustomProvider AddTranscriptionModel(string modelId, ITranscriptionModel model)
```


Registers a transcription model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L389)

<a id="Vercel_AI_CustomProvider_AddVideoModel_"></a>
<a id="Vercel_AI_CustomProvider_AddVideoModel_System_String_Vercel_AI_Provider_IVideoModel_"></a>

### AddVideoModel(string, IVideoModel)


```csharp
public CustomProvider AddVideoModel(string modelId, IVideoModel model)
```


Registers a video model under &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/) |  |

**Returns:** [CustomProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L407)

<a id="Vercel_AI_CustomProvider_DecisionModel_"></a>
<a id="Vercel_AI_CustomProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public IDecisionCaller DecisionModel(string modelId)
```


Returns the decision model registered under &lt;code class="paramref"&gt;modelId&lt;/code&gt;, or asks [FallbackProvider](#Vercel_AI_CustomProvider_FallbackProvider)
when it implements [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/) or the deprecated [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L513)

<a id="Vercel_AI_CustomProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_CustomProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public override IEmbeddingModel EmbeddingModel(string modelId)
```


Returns an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L468)

<a id="Vercel_AI_CustomProvider_EvaluationModel_"></a>
<a id="Vercel_AI_CustomProvider_EvaluationModel_System_String_"></a>

### EvaluationModel(string)


```csharp
[Obsolete("Use DecisionModel instead.")]
public IEvaluationDecisionCaller EvaluationModel(string modelId)
```


Deprecated alias of [DecisionModel(string)](#Vercel_AI_CustomProvider_DecisionModel_System_String_). Returns the same model, which also has the deprecated
`CancellationToken)`. Maps to `evaluationModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L524)

<a id="Vercel_AI_CustomProvider_FileStore_"></a>
<a id="Vercel_AI_CustomProvider_FileStore"></a>

### FileStore()


```csharp
public override IFileStore FileStore()
```


Returns a file store.

**Returns:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L531)

<a id="Vercel_AI_CustomProvider_ImageModel_"></a>
<a id="Vercel_AI_CustomProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public override IImageModel ImageModel(string modelId)
```


Returns an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L474)

<a id="Vercel_AI_CustomProvider_LanguageModel_"></a>
<a id="Vercel_AI_CustomProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Returns a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L462)

<a id="Vercel_AI_CustomProvider_RerankingModel_"></a>
<a id="Vercel_AI_CustomProvider_RerankingModel_System_String_"></a>

### RerankingModel(string)


```csharp
public override IRerankingModel RerankingModel(string modelId)
```


Returns a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L492)

<a id="Vercel_AI_CustomProvider_RubricModel_"></a>
<a id="Vercel_AI_CustomProvider_RubricModel_System_String_"></a>

### RubricModel(string)


```csharp
public override IRubricModel RubricModel(string modelId)
```


Returns a model that scores a candidate against a rubric.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L504)

<a id="Vercel_AI_CustomProvider_SkillStore_"></a>
<a id="Vercel_AI_CustomProvider_SkillStore"></a>

### SkillStore()


```csharp
public override ISkillStore SkillStore()
```


Returns a skill store.

**Returns:** [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L537)

<a id="Vercel_AI_CustomProvider_SpeechModel_"></a>
<a id="Vercel_AI_CustomProvider_SpeechModel_System_String_"></a>

### SpeechModel(string)


```csharp
public override ISpeechModel SpeechModel(string modelId)
```


Returns a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L486)

<a id="Vercel_AI_CustomProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_CustomProvider_TranscriptionModel_System_String_"></a>

### TranscriptionModel(string)


```csharp
public override ITranscriptionModel TranscriptionModel(string modelId)
```


Returns a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L480)

<a id="Vercel_AI_CustomProvider_VideoModel_"></a>
<a id="Vercel_AI_CustomProvider_VideoModel_System_String_"></a>

### VideoModel(string)


```csharp
public override IVideoModel VideoModel(string modelId)
```


Returns a video model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L498)
