---
title: "TogetherAIProvider"
description: "TogetherAI provider. OpenAI Chat Completions compatible at https://api.together.xyz/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "TogetherAIProvider"
  hidden: true
---

<a id="Vercel_AI_TogetherAI_TogetherAIProvider"></a>

Namespace: [Vercel.AI.TogetherAI](/api/Vercel.AI.TogetherAI/)


```csharp
public sealed class TogetherAIProvider : OpenAICompatibleProvider
```


TogetherAI provider. OpenAI Chat Completions compatible at `https://api.together.xyz/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L12)

## Constructors

<a id="Vercel_AI_TogetherAI_TogetherAIProvider__ctor_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### TogetherAIProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public TogetherAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L33)

## Fields

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "TOGETHER_AI_API_KEY"
```


Deprecated environment variable. [PreferredApiKeyVariable](#Vercel_AI_TogetherAI_TogetherAIProvider_PreferredApiKeyVariable) wins when both are set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L21)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.together.xyz/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L18)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_PreferredApiKeyVariable"></a>

### PreferredApiKeyVariable


```csharp
public const string PreferredApiKeyVariable = "TOGETHER_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L24)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "togetherai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L15)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_StructuredOutputModelId"></a>

### StructuredOutputModelId


```csharp
public const string StructuredOutputModelId = "deepseek-ai/DeepSeek-V4-Flash-0731"
```


The only chat model that receives structured outputs.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L27)

## Properties

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ApiKeyDeprecationWarning_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ApiKeyDeprecationWarning"></a>

### ApiKeyDeprecationWarning


```csharp
public string? ApiKeyDeprecationWarning { get; }
```


Set when the key was read from [ApiKeyVariable](#Vercel_AI_TogetherAI_TogetherAIProvider_ApiKeyVariable).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L30)

## Methods

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_Create_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static TogetherAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [TogetherAIProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L99)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_CreateChatModel_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider_CreateChatModel_System_String_"></a>

### CreateChatModel(string)


```csharp
public override OpenAICompatibleLanguageModel CreateChatModel(string modelId)
```


Creates a chat model. Providers override this when support differs by model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAICompatibleLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L45)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ImageModel_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L53)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_RerankingModel_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider_RerankingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L59)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ResolveApiKey_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider_ResolveApiKey"></a>

### ResolveApiKey()


```csharp
protected override string? ResolveApiKey()
```


Resolves the API key. A missing key is omitted unless [RequireApiKey](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_RequireApiKey) is set.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L65)

<a id="Vercel_AI_TogetherAI_TogetherAIProvider_SupportsStructuredOutputs_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIProvider_SupportsStructuredOutputs_System_String_"></a>

### SupportsStructuredOutputs(string)


```csharp
public static bool SupportsStructuredOutputs(string modelId)
```


Whether &lt;code class="paramref"&gt;modelId&lt;/code&gt; should send JSON schema responses.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L39)
