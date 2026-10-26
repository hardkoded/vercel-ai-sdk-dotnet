---
title: "DeepInfraProvider"
description: "DeepInfra provider. OpenAI Chat Completions compatible at https://api.deepinfra.com/v1/openai."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepInfraProvider"
  hidden: true
---

<a id="Vercel_AI_DeepInfra_DeepInfraProvider"></a>

Namespace: [Vercel.AI.DeepInfra](/api/Vercel.AI.DeepInfra/)


```csharp
public sealed class DeepInfraProvider : OpenAICompatibleProvider
```


DeepInfra provider. OpenAI Chat Completions compatible at `https://api.deepinfra.com/v1/openai`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L14)

## Constructors

<a id="Vercel_AI_DeepInfra_DeepInfraProvider__ctor_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### DeepInfraProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public DeepInfraProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L26)

## Fields

<a id="Vercel_AI_DeepInfra_DeepInfraProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "DEEPINFRA_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L23)

<a id="Vercel_AI_DeepInfra_DeepInfraProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.deepinfra.com/v1/openai"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L20)

<a id="Vercel_AI_DeepInfra_DeepInfraProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "deepinfra"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L17)

## Methods

<a id="Vercel_AI_DeepInfra_DeepInfraProvider_Create_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static DeepInfraProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [DeepInfraProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L38)

<a id="Vercel_AI_DeepInfra_DeepInfraProvider_FixReasoningUsage_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraProvider_FixReasoningUsage_System_Text_Json_JsonElement_"></a>

### FixReasoningUsage(JsonElement)


```csharp
public static JsonElement FixReasoningUsage(JsonElement usage)
```


DeepInfra leaves reasoning tokens out of `completion_tokens` for some models (for example Gemini and Gemma).
When reasoning exceeds completion tokens, the reasoning tokens are added to the completion and total counts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L48)

<a id="Vercel_AI_DeepInfra_DeepInfraProvider_ImageModel_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L32)
