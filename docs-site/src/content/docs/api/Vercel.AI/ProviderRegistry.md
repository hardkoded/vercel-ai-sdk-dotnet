---
title: "ProviderRegistry"
description: "Looks up provider models by provider:model ids. Maps to createProviderRegistry. Everything after the first separator is the model id, so model ids may contai..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderRegistry"
  hidden: true
---

<a id="Vercel_AI_ProviderRegistry"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ProviderRegistry
```


Looks up provider models by `provider:model` ids. Maps to `createProviderRegistry`.
Everything after the first separator is the model id, so model ids may contain the separator.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L179)

## Constructors

<a id="Vercel_AI_ProviderRegistry__ctor_"></a>
<a id="Vercel_AI_ProviderRegistry__ctor_System_String_"></a>

### ProviderRegistry(string)


```csharp
public ProviderRegistry(string separator = ":")
```


Creates a registry. &lt;code class="paramref"&gt;separator&lt;/code&gt; splits the provider id from the model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `separator` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L185)

## Properties

<a id="Vercel_AI_ProviderRegistry_LanguageModelMiddleware_"></a>
<a id="Vercel_AI_ProviderRegistry_LanguageModelMiddleware"></a>

### LanguageModelMiddleware


```csharp
public IReadOnlyList<ILanguageModelMiddleware>? LanguageModelMiddleware { get; set; }
```


Middleware applied to every language model the registry returns. The first one sees the call first.

**Value:** [IReadOnlyList&lt;ILanguageModelMiddleware&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L196)

## Methods

<a id="Vercel_AI_ProviderRegistry_DecisionModel_"></a>
<a id="Vercel_AI_ProviderRegistry_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public IDecisionCaller DecisionModel(string providerAndModel)
```


Resolves `provider:model` to a decision model. The provider must implement [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/)
or the deprecated [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/), or [NoSuchModelError](/api/Vercel.AI.Util/NoSuchModelError/) is thrown.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L262)

<a id="Vercel_AI_ProviderRegistry_EmbeddingModel_"></a>
<a id="Vercel_AI_ProviderRegistry_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public IEmbeddingModel EmbeddingModel(string providerAndModel)
```


Resolves `provider:model` to an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L217)

<a id="Vercel_AI_ProviderRegistry_EvaluationModel_"></a>
<a id="Vercel_AI_ProviderRegistry_EvaluationModel_System_String_"></a>

### EvaluationModel(string)


```csharp
[Obsolete("Use DecisionModel instead.")]
public IEvaluationDecisionCaller EvaluationModel(string providerAndModel)
```


Deprecated alias of [DecisionModel(string)](#Vercel_AI_ProviderRegistry_DecisionModel_System_String_). Returns the same model, which also has the deprecated
`CancellationToken)`. Maps to `evaluationModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L271)

<a id="Vercel_AI_ProviderRegistry_FileStore_"></a>
<a id="Vercel_AI_ProviderRegistry_FileStore_System_String_"></a>

### FileStore(string)


```csharp
public IFileStore FileStore(string providerId)
```


Returns the file store of the provider registered under &lt;code class="paramref"&gt;providerId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L278)

<a id="Vercel_AI_ProviderRegistry_ImageModel_"></a>
<a id="Vercel_AI_ProviderRegistry_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public IImageModel ImageModel(string providerAndModel)
```


Resolves `provider:model` to an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L223)

<a id="Vercel_AI_ProviderRegistry_LanguageModel_"></a>
<a id="Vercel_AI_ProviderRegistry_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public ILanguageModel LanguageModel(string providerAndModel)
```


Resolves `provider:model` to a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L210)

<a id="Vercel_AI_ProviderRegistry_Register_"></a>
<a id="Vercel_AI_ProviderRegistry_Register_System_String_Vercel_AI_Provider_ProviderBase_"></a>

### Register(string, ProviderBase)


```csharp
public void Register(string id, ProviderBase provider)
```


Registers a provider under &lt;code class="paramref"&gt;id&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L199)

<a id="Vercel_AI_ProviderRegistry_RerankingModel_"></a>
<a id="Vercel_AI_ProviderRegistry_RerankingModel_System_String_"></a>

### RerankingModel(string)


```csharp
public IRerankingModel RerankingModel(string providerAndModel)
```


Resolves `provider:model` to a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L241)

<a id="Vercel_AI_ProviderRegistry_RubricModel_"></a>
<a id="Vercel_AI_ProviderRegistry_RubricModel_System_String_"></a>

### RubricModel(string)


```csharp
public IRubricModel RubricModel(string providerAndModel)
```


Resolves `provider:model` to a model that scores a candidate against a rubric.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L253)

<a id="Vercel_AI_ProviderRegistry_SkillStore_"></a>
<a id="Vercel_AI_ProviderRegistry_SkillStore_System_String_"></a>

### SkillStore(string)


```csharp
public ISkillStore SkillStore(string providerId)
```


Returns the skill store of the provider registered under &lt;code class="paramref"&gt;providerId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L284)

<a id="Vercel_AI_ProviderRegistry_SpeechModel_"></a>
<a id="Vercel_AI_ProviderRegistry_SpeechModel_System_String_"></a>

### SpeechModel(string)


```csharp
public ISpeechModel SpeechModel(string providerAndModel)
```


Resolves `provider:model` to a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L235)

<a id="Vercel_AI_ProviderRegistry_TranscriptionModel_"></a>
<a id="Vercel_AI_ProviderRegistry_TranscriptionModel_System_String_"></a>

### TranscriptionModel(string)


```csharp
public ITranscriptionModel TranscriptionModel(string providerAndModel)
```


Resolves `provider:model` to a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L229)

<a id="Vercel_AI_ProviderRegistry_VideoModel_"></a>
<a id="Vercel_AI_ProviderRegistry_VideoModel_System_String_"></a>

### VideoModel(string)


```csharp
public IVideoModel VideoModel(string providerAndModel)
```


Resolves `provider:model` to a video model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerAndModel` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L247)
