---
title: "OpenAIProvider"
description: "OpenAI provider: Chat Completions, Responses, embeddings, images, speech, transcription, files, and batches."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIProvider"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIProvider"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIProvider : OpenAICompatibleProvider, IBatchProvider
```


OpenAI provider: Chat Completions, Responses, embeddings, images, speech, transcription, files, and batches.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

**Implements:** [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L91)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIProvider__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAI_OpenAIOptions_"></a>

### OpenAIProvider(HttpClient, OpenAIOptions?)


```csharp
public OpenAIProvider(HttpClient httpClient, OpenAIOptions? options = null)
```


Creates an OpenAI provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAIOptions](/api/Vercel.AI.OpenAI/OpenAIOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L102)

## Fields

<a id="Vercel_AI_OpenAI_OpenAIProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "openai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L94)

<a id="Vercel_AI_OpenAI_OpenAIProvider_UserAgentSuffix"></a>

### UserAgentSuffix


```csharp
public const string UserAgentSuffix = "ai-sdk/openai/4.0.73"
```


User-Agent suffix appended to every request. Tracks `@ai-sdk/openai` 4.0.73.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L97)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIProvider_HttpClient_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


HTTP client used for calls that need response headers.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L111)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIProvider_BatchModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_BatchModel"></a>

### BatchModel()


```csharp
public override IBatchModel BatchModel()
```


Returns a batch model.

**Returns:** [IBatchModel](/api/Vercel.AI.Provider/IBatchModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L202)

<a id="Vercel_AI_OpenAI_OpenAIProvider_ChatModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_ChatModel_System_String_"></a>

### ChatModel(string)


```csharp
public OpenAIChatLanguageModel ChatModel(string modelId)
```


Chat Completions model. The provider id is `openai.chat`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIChatLanguageModel](/api/Vercel.AI.OpenAI/OpenAIChatLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L122)

<a id="Vercel_AI_OpenAI_OpenAIProvider_CompletionModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_CompletionModel_System_String_"></a>

### CompletionModel(string)


```csharp
public OpenAICompletionLanguageModel CompletionModel(string modelId)
```


Legacy completions model. The provider id is `openai.completion`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAICompletionLanguageModel](/api/Vercel.AI.OpenAI/OpenAICompletionLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L134)

<a id="Vercel_AI_OpenAI_OpenAIProvider_Create_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_Create_Vercel_AI_OpenAI_OpenAIOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAIOptions?, HttpMessageHandler?)


```csharp
public static OpenAIProvider Create(OpenAIOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIOptions](/api/Vercel.AI.OpenAI/OpenAIOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [OpenAIProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L114)

<a id="Vercel_AI_OpenAI_OpenAIProvider_CreateOpenAIHeaders_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_CreateOpenAIHeaders_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `CreateOpenAIHeaders(IReadOnlyDictionary<string, string?>?)`


```csharp
public Dictionary<string, string?> CreateOpenAIHeaders(IReadOnlyDictionary<string, string?>? callHeaders = null)
```


Builds request headers. Custom provider headers override the API key, organization, and project.
The user-agent suffix is appended after that, and per-call headers override the result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L229)

<a id="Vercel_AI_OpenAI_OpenAIProvider_DecisionModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public OpenAIDecisionModel DecisionModel(string modelId)
```


Decisions API model. The provider id is `openai.decision`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIDecisionModel](/api/Vercel.AI.OpenAI/OpenAIDecisionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L140)

<a id="Vercel_AI_OpenAI_OpenAIProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L164)

<a id="Vercel_AI_OpenAI_OpenAIProvider_EvaluationModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_EvaluationModel_System_String_"></a>

### EvaluationModel(string)


```csharp
[Obsolete("Use DecisionModel instead.")]
public OpenAIDecisionModel EvaluationModel(string modelId)
```


Deprecated alias of [DecisionModel(string)](#Vercel_AI_OpenAI_OpenAIProvider_DecisionModel_System_String_). Maps to `evaluationModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIDecisionModel](/api/Vercel.AI.OpenAI/OpenAIDecisionModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L146)

<a id="Vercel_AI_OpenAI_OpenAIProvider_ExperimentalBatch_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_ExperimentalBatch"></a>

### ExperimentalBatch()


```csharp
public IBatchApi? ExperimentalBatch()
```


Batch API. Downloads batch results.

**Returns:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L194)

<a id="Vercel_AI_OpenAI_OpenAIProvider_FileStore_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_FileStore"></a>

### FileStore()


```csharp
public override IFileStore FileStore()
```


Returns a file store.

**Returns:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L208)

<a id="Vercel_AI_OpenAI_OpenAIProvider_ImageModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L170)

<a id="Vercel_AI_OpenAI_OpenAIProvider_LanguageModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L153)

<a id="Vercel_AI_OpenAI_OpenAIProvider_RealtimeModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_RealtimeModel_System_String_"></a>

### RealtimeModel(string)


```csharp
public override IRealtimeModel RealtimeModel(string modelId)
```


Returns a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L220)

<a id="Vercel_AI_OpenAI_OpenAIProvider_ResponsesModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_ResponsesModel_System_String_"></a>

### ResponsesModel(string)


```csharp
public OpenAIResponsesLanguageModel ResponsesModel(string modelId)
```


Responses API model. The provider id is `openai.responses`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIResponsesLanguageModel](/api/Vercel.AI.OpenAI/OpenAIResponsesLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L128)

<a id="Vercel_AI_OpenAI_OpenAIProvider_SkillStore_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_SkillStore"></a>

### SkillStore()


```csharp
public override ISkillStore SkillStore()
```


Returns a skill store.

**Returns:** [ISkillStore](/api/Vercel.AI.Provider/ISkillStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L214)

<a id="Vercel_AI_OpenAI_OpenAIProvider_SpeechModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_SpeechModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L176)

<a id="Vercel_AI_OpenAI_OpenAIProvider_SpeechTranslationModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_SpeechTranslationModel_System_String_"></a>

### SpeechTranslationModel(string)


```csharp
public override ISpeechTranslationModel SpeechTranslationModel(string modelId)
```


Returns a speech translation model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechTranslationModel](/api/Vercel.AI.Provider/ISpeechTranslationModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L188)

<a id="Vercel_AI_OpenAI_OpenAIProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIProvider_TranscriptionModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L182)
