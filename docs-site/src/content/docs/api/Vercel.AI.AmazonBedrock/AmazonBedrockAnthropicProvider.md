---
title: "AmazonBedrockAnthropicProvider"
description: "Anthropic Messages provider hosted on Amazon Bedrock."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockAnthropicProvider"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockAnthropicProvider : ProviderBase
```


Anthropic Messages provider hosted on Amazon Bedrock.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L260)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_"></a>

### AmazonBedrockAnthropicProvider(HttpClient, AmazonBedrockAnthropicOptions?)


```csharp
public AmazonBedrockAnthropicProvider(HttpClient httpClient, AmazonBedrockAnthropicOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [AmazonBedrockAnthropicOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L266)

## Fields

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "bedrock.anthropic.messages"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L263)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_HttpClient_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


HTTP client that receives the signed request.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L277)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_Options_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_Options"></a>

### Options


```csharp
public AmazonBedrockAnthropicOptions Options { get; }
```


Options.

**Value:** [AmazonBedrockAnthropicOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L274)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_Tools_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_Tools"></a>

### Tools


```csharp
public static IReadOnlyList<string> Tools { get; }
```


The Anthropic provider tool ids. The same list as [Tools](/api/Vercel.AI.Anthropic/AnthropicProvider/#Vercel_AI_Anthropic_AnthropicProvider_Tools).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L287)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_Create_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_Create_Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(AmazonBedrockAnthropicOptions?, HttpMessageHandler?)


```csharp
public static AmazonBedrockAnthropicProvider Create(AmazonBedrockAnthropicOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [AmazonBedrockAnthropicOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [AmazonBedrockAnthropicProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L280)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public override IEmbeddingModel EmbeddingModel(string modelId)
```


Bedrock Anthropic has no embedding models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L308)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_ImageModel_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public override IImageModel ImageModel(string modelId)
```


Bedrock Anthropic has no image models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L314)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_LanguageModel_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L296)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_New_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_New_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L302)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_ResolveBaseUrl_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicProvider_ResolveBaseUrl"></a>

### ResolveBaseUrl()


```csharp
public string ResolveBaseUrl()
```


Bedrock Runtime origin for these options.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockAnthropicProvider.cs#L290)
