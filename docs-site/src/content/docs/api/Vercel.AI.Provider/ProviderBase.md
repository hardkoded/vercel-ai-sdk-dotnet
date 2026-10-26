---
title: "ProviderBase"
description: "Provider factory. Maps to ProviderV4. Modalities a provider does not implement throw AiSdkException."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderBase"
  hidden: true
---

<a id="Vercel_AI_Provider_ProviderBase"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class ProviderBase
```


Provider factory. Maps to `ProviderV4`. Modalities a provider does not implement throw
[AiSdkException](/api/Vercel.AI.Provider/AiSdkException/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AmazonBedrockAnthropicProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicProvider/), [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/), [BedrockMantleProvider](/api/Vercel.AI.AmazonBedrock/BedrockMantleProvider/), [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/), [CohereProvider](/api/Vercel.AI.Cohere/CohereProvider/), [CustomProvider](/api/Vercel.AI/CustomProvider/), [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/), [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/), [MiniMaxProvider](/api/Vercel.AI.MiniMax/MiniMaxProvider/), [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/), [OpenResponsesProvider](/api/Vercel.AI.OpenResponses/OpenResponsesProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L695)

## Constructors

<a id="Vercel_AI_Provider_ProviderBase__ctor_"></a>
<a id="Vercel_AI_Provider_ProviderBase__ctor_System_String_"></a>

### ProviderBase(string)


```csharp
protected ProviderBase(string name)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L698)

## Properties

<a id="Vercel_AI_Provider_ProviderBase_Name_"></a>
<a id="Vercel_AI_Provider_ProviderBase_Name"></a>

### Name


```csharp
public string Name { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L704)

<a id="Vercel_AI_Provider_ProviderBase_SpecificationVersion_"></a>
<a id="Vercel_AI_Provider_ProviderBase_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public virtual string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L707)

## Methods

<a id="Vercel_AI_Provider_ProviderBase_BatchModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_BatchModel"></a>

### BatchModel()


```csharp
public virtual IBatchModel BatchModel()
```


Returns a batch model.

**Returns:** [IBatchModel](/api/Vercel.AI.Provider/IBatchModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L761)

<a id="Vercel_AI_Provider_ProviderBase_EmbeddingModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public virtual IEmbeddingModel EmbeddingModel(string modelId)
```


Returns an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L713)

<a id="Vercel_AI_Provider_ProviderBase_FileStore_"></a>
<a id="Vercel_AI_Provider_ProviderBase_FileStore"></a>

### FileStore()


```csharp
public virtual IFileStore FileStore()
```


Returns a file store.

**Returns:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L767)

<a id="Vercel_AI_Provider_ProviderBase_ImageModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public virtual IImageModel ImageModel(string modelId)
```


Returns an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L719)

<a id="Vercel_AI_Provider_ProviderBase_LanguageModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public abstract ILanguageModel LanguageModel(string modelId)
```


Returns a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L710)

<a id="Vercel_AI_Provider_ProviderBase_RealtimeModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_RealtimeModel_System_String_"></a>

### RealtimeModel(string)


```csharp
public virtual IRealtimeModel RealtimeModel(string modelId)
```


Returns a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L779)

<a id="Vercel_AI_Provider_ProviderBase_RerankingModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_RerankingModel_System_String_"></a>

### RerankingModel(string)


```csharp
public virtual IRerankingModel RerankingModel(string modelId)
```


Returns a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L749)

<a id="Vercel_AI_Provider_ProviderBase_RubricModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_RubricModel_System_String_"></a>

### RubricModel(string)


```csharp
public virtual IRubricModel RubricModel(string modelId)
```


Returns a model that scores a candidate against a rubric.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L755)

<a id="Vercel_AI_Provider_ProviderBase_SkillStore_"></a>
<a id="Vercel_AI_Provider_ProviderBase_SkillStore"></a>

### SkillStore()


```csharp
public virtual ISkillStore SkillStore()
```


Returns a skill store.

**Returns:** [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L773)

<a id="Vercel_AI_Provider_ProviderBase_SpeechModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_SpeechModel_System_String_"></a>

### SpeechModel(string)


```csharp
public virtual ISpeechModel SpeechModel(string modelId)
```


Returns a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L725)

<a id="Vercel_AI_Provider_ProviderBase_SpeechTranslationModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_SpeechTranslationModel_System_String_"></a>

### SpeechTranslationModel(string)


```csharp
public virtual ISpeechTranslationModel SpeechTranslationModel(string modelId)
```


Returns a speech translation model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechTranslationModel](/api/Vercel.AI.Provider/ISpeechTranslationModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L737)

<a id="Vercel_AI_Provider_ProviderBase_TranscriptionModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_TranscriptionModel_System_String_"></a>

### TranscriptionModel(string)


```csharp
public virtual ITranscriptionModel TranscriptionModel(string modelId)
```


Returns a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L731)

<a id="Vercel_AI_Provider_ProviderBase_VideoModel_"></a>
<a id="Vercel_AI_Provider_ProviderBase_VideoModel_System_String_"></a>

### VideoModel(string)


```csharp
public virtual IVideoModel VideoModel(string modelId)
```


Returns a video model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L743)
