---
title: "PerplexityProvider"
description: "Perplexity provider. Language generation uses the Agent API at {base}/v1/agent. Embeddings use {base}/v1/embeddings. Preset ids are fast, low, medium, high,..."
editUrl: false
prev: false
next: false
sidebar:
  label: "PerplexityProvider"
  hidden: true
---

<a id="Vercel_AI_Perplexity_PerplexityProvider"></a>

Namespace: [Vercel.AI.Perplexity](/api/Vercel.AI.Perplexity/)


```csharp
public sealed class PerplexityProvider : OpenAICompatibleProvider, IDecisionProvider
```


Perplexity provider. Language generation uses the Agent API at `{base}/v1/agent`.
Embeddings use `{base}/v1/embeddings`. Preset ids are
`fast`, `low`, `medium`, `high`, and `xhigh`; any other id is sent as a model id.
Legacy Sonar model ids are not aliased and Sonar provider options are not translated.
Sonar PDF input, video input, and image or video results have no Agent API equivalent.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

**Implements:** [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L19)

## Constructors

<a id="Vercel_AI_Perplexity_PerplexityProvider__ctor_"></a>
<a id="Vercel_AI_Perplexity_PerplexityProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### PerplexityProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public PerplexityProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L40)

## Fields

<a id="Vercel_AI_Perplexity_PerplexityProvider_AgentPath"></a>

### AgentPath


```csharp
public const string AgentPath = "v1/agent"
```


Relative Agent API path.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L28)

<a id="Vercel_AI_Perplexity_PerplexityProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "PERPLEXITY_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L31)

<a id="Vercel_AI_Perplexity_PerplexityProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.perplexity.ai"
```


Default API origin. Language calls append `/v1/agent`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L25)

<a id="Vercel_AI_Perplexity_PerplexityProvider_DefaultIntegration"></a>

### DefaultIntegration


```csharp
public const string DefaultIntegration = "vercel-ai-sdk"
```


Default [IntegrationHeader](#Vercel_AI_Perplexity_PerplexityProvider_IntegrationHeader) value.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L37)

<a id="Vercel_AI_Perplexity_PerplexityProvider_IntegrationHeader"></a>

### IntegrationHeader


```csharp
public const string IntegrationHeader = "X-Pplx-Integration"
```


Header Perplexity uses to attribute SDK requests.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L34)

<a id="Vercel_AI_Perplexity_PerplexityProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "perplexity"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L22)

## Methods

<a id="Vercel_AI_Perplexity_PerplexityProvider_AgentUri_"></a>
<a id="Vercel_AI_Perplexity_PerplexityProvider_AgentUri"></a>

### AgentUri()


```csharp
public Uri AgentUri()
```


Agent API URL. A custom base URL is prefixed to `v1/agent`.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L76)

<a id="Vercel_AI_Perplexity_PerplexityProvider_Create_"></a>
<a id="Vercel_AI_Perplexity_PerplexityProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static PerplexityProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [PerplexityProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L46)

<a id="Vercel_AI_Perplexity_PerplexityProvider_DecisionModel_"></a>
<a id="Vercel_AI_Perplexity_PerplexityProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public PerplexityDecisionModel DecisionModel(string modelId)
```


Decision model. Uses the Decisions API at `{base}/v1/decisions`. The provider id is `perplexity.decision`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [PerplexityDecisionModel](/api/Vercel.AI.Perplexity/PerplexityDecisionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L65)

<a id="Vercel_AI_Perplexity_PerplexityProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Perplexity_PerplexityProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L59)

<a id="Vercel_AI_Perplexity_PerplexityProvider_LanguageModel_"></a>
<a id="Vercel_AI_Perplexity_PerplexityProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L53)
