---
title: "AmazonBedrockProvider"
description: "Amazon Bedrock Converse provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockProvider"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockProvider : ProviderBase
```


Amazon Bedrock Converse provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L67)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_AmazonBedrock_AmazonBedrockOptions_"></a>

### AmazonBedrockProvider(HttpClient, AmazonBedrockOptions?)


```csharp
public AmazonBedrockProvider(HttpClient httpClient, AmazonBedrockOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [AmazonBedrockOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L73)

## Fields

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_ProviderName"></a>

### ProviderName


```csharp
public const string ProviderName = "amazon-bedrock"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L70)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_Http_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L88)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_HttpClient_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


HTTP client that receives the signed request.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L85)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_Options_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_Options"></a>

### Options


```csharp
public AmazonBedrockOptions Options { get; }
```


Options.

**Value:** [AmazonBedrockOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L82)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_AddHeadersAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_AddHeadersAsync_System_Net_Http_HttpRequestMessage_System_Collections_Generic_IEnumerable_System_Collections_Generic_KeyValuePair_System_String_System_String___System_Threading_CancellationToken_"></a>

### `AddHeadersAsync(HttpRequestMessage, IEnumerable<KeyValuePair<string, string?>>?, CancellationToken)`


```csharp
public Task AddHeadersAsync(HttpRequestMessage request, IEnumerable<KeyValuePair<string, string?>>? callHeaders, CancellationToken cancellationToken)
```


Adds the provider headers, then [HeadersProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockOptions/#Vercel_AI_AmazonBedrock_AmazonBedrockOptions_HeadersProvider), then the call headers
(which win; a null value drops the header), then the `ai-sdk-amazon-bedrock` user agent. Call before signing.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [HttpRequestMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httprequestmessage) |  |
| `callHeaders` | [IEnumerable&lt;KeyValuePair&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L119)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_Create_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_Create_Vercel_AI_AmazonBedrock_AmazonBedrockOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(AmazonBedrockOptions?, HttpMessageHandler?)


```csharp
public static AmazonBedrockProvider Create(AmazonBedrockOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [AmazonBedrockOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [AmazonBedrockProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L156)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L169)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_EmbeddingModel_System_String_Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelSettings_"></a>

### EmbeddingModel(string, AmazonBedrockEmbeddingModelSettings?)


```csharp
public AmazonBedrockEmbeddingModel EmbeddingModel(string modelId, AmazonBedrockEmbeddingModelSettings? settings)
```


Creates an embedding model with &lt;code class="paramref"&gt;settings&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AmazonBedrockEmbeddingModelSettings](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEmbeddingModelSettings/) |  |

**Returns:** [AmazonBedrockEmbeddingModel](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L172)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_LanguageModel_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L163)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_RerankingModel_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockProvider_RerankingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L166)
