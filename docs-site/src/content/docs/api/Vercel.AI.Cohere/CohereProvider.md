---
title: "CohereProvider"
description: "Cohere provider for chat, embeddings, and rerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereProvider"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereProvider"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereProvider : ProviderBase
```


Cohere provider for chat, embeddings, and rerank.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L27)

## Constructors

<a id="Vercel_AI_Cohere_CohereProvider__ctor_"></a>
<a id="Vercel_AI_Cohere_CohereProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Cohere_CohereOptions_"></a>

### CohereProvider(HttpClient, CohereOptions?)


```csharp
public CohereProvider(HttpClient httpClient, CohereOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [CohereOptions](/api/Vercel.AI.Cohere/CohereOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L35)

## Fields

<a id="Vercel_AI_Cohere_CohereProvider_ProviderName"></a>

### ProviderName


```csharp
public const string ProviderName = "cohere"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L30)

## Properties

<a id="Vercel_AI_Cohere_CohereProvider_Http_"></a>
<a id="Vercel_AI_Cohere_CohereProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L47)

<a id="Vercel_AI_Cohere_CohereProvider_Options_"></a>
<a id="Vercel_AI_Cohere_CohereProvider_Options"></a>

### Options


```csharp
public CohereOptions Options { get; }
```


Options.

**Value:** [CohereOptions](/api/Vercel.AI.Cohere/CohereOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L44)

## Methods

<a id="Vercel_AI_Cohere_CohereProvider_Create_"></a>
<a id="Vercel_AI_Cohere_CohereProvider_Create_Vercel_AI_Cohere_CohereOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(CohereOptions?, HttpMessageHandler?)


```csharp
public static CohereProvider Create(CohereOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [CohereOptions](/api/Vercel.AI.Cohere/CohereOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [CohereProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L50)

<a id="Vercel_AI_Cohere_CohereProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Cohere_CohereProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L60)

<a id="Vercel_AI_Cohere_CohereProvider_LanguageModel_"></a>
<a id="Vercel_AI_Cohere_CohereProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L57)

<a id="Vercel_AI_Cohere_CohereProvider_RerankingModel_"></a>
<a id="Vercel_AI_Cohere_CohereProvider_RerankingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L63)
