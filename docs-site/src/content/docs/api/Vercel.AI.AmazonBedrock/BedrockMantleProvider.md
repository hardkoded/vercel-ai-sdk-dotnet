---
title: "BedrockMantleProvider"
description: "Bedrock Mantle provider. Chat calls use /chat/completions and are signed for bedrock-mantle."
editUrl: false
prev: false
next: false
sidebar:
  label: "BedrockMantleProvider"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class BedrockMantleProvider : ProviderBase
```


Bedrock Mantle provider. Chat calls use `/chat/completions` and are signed for `bedrock-mantle`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L107)

## Constructors

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_AmazonBedrock_BedrockMantleOptions_"></a>

### BedrockMantleProvider(HttpClient, BedrockMantleOptions?)


```csharp
public BedrockMantleProvider(HttpClient httpClient, BedrockMantleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [BedrockMantleOptions](/api/Vercel.AI.AmazonBedrock/BedrockMantleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L113)

## Fields

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "bedrock-mantle.chat"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L110)

## Properties

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_HttpClient_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


HTTP client that receives the signed request.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L124)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Options_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Options"></a>

### Options


```csharp
public BedrockMantleOptions Options { get; }
```


Options.

**Value:** [BedrockMantleOptions](/api/Vercel.AI.AmazonBedrock/BedrockMantleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L121)

## Methods

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Chat_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Chat_System_String_"></a>

### Chat(string)


```csharp
public ILanguageModel Chat(string modelId)
```


Chat Completions model. Same route as [LanguageModel(string)](#Vercel_AI_AmazonBedrock_BedrockMantleProvider_LanguageModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L140)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Create_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Create_Vercel_AI_AmazonBedrock_BedrockMantleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(BedrockMantleOptions?, HttpMessageHandler?)


```csharp
public static BedrockMantleProvider Create(BedrockMantleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [BedrockMantleOptions](/api/Vercel.AI.AmazonBedrock/BedrockMantleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [BedrockMantleProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L127)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public override IEmbeddingModel EmbeddingModel(string modelId)
```


Mantle has no embedding models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L152)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_ImageModel_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public override IImageModel ImageModel(string modelId)
```


Mantle has no image models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L158)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_LanguageModel_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L134)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_New_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_New_System_String_"></a>

### New(string)


```csharp
public ILanguageModel New(string modelId)
```


Throws because the JavaScript provider function rejects `new`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L146)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Responses_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleProvider_Responses_System_String_"></a>

### Responses(string)


```csharp
public ILanguageModel Responses(string modelId)
```


Responses API model. The path is `/responses` on the same Mantle origin.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L164)
