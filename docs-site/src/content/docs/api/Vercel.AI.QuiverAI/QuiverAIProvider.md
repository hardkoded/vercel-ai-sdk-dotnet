---
title: "QuiverAIProvider"
description: "QuiverAI provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIProvider"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIProvider"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAIProvider : OpenAICompatibleProvider
```


QuiverAI provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L16)

## Constructors

<a id="Vercel_AI_QuiverAI_QuiverAIProvider__ctor_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### QuiverAIProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public QuiverAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L28)

## Fields

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.quiver.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L22)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "quiverai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L19)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_Tools_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_Tools"></a>

### Tools


```csharp
public OpenResponsesTools Tools { get; }
```


QuiverAI caller-executed tools, such as custom tools. Maps to `provider.tools`.

**Value:** [OpenResponsesTools](/api/Vercel.AI.OpenResponses/OpenResponsesTools/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L41)

## Methods

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_Create_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static QuiverAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [QuiverAIProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L53)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L65)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ImageModel_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L71)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_LanguageModel_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Creates a language model for the QuiverAI Responses API. The provider id is `quiverai.responses`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L44)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ResolveApiKeyAsync_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ResolveApiKeyAsync_System_Threading_CancellationToken_"></a>

### ResolveApiKeyAsync(CancellationToken)


```csharp
protected override Task<string?> ResolveApiKeyAsync(CancellationToken cancellationToken)
```


Throws when neither the option nor `QUIVERAI_API_KEY` holds a key.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L74)
