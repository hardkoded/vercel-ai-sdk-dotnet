---
title: "AzureOpenAIProvider"
description: "Azure OpenAI provider. Calls use the /openai/v1 routes and the api-key header, or the deployments routes when UseDeploymentBasedUrls is set."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureOpenAIProvider"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureOpenAIProvider"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureOpenAIProvider : OpenAICompatibleProvider
```


Azure OpenAI provider. Calls use the `/openai/v1` routes and the `api-key` header, or the
deployments routes when [UseDeploymentBasedUrls](/api/Vercel.AI.Azure/AzureOpenAIOptions/#Vercel_AI_Azure_AzureOpenAIOptions_UseDeploymentBasedUrls) is set.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L72)

## Constructors

<a id="Vercel_AI_Azure_AzureOpenAIProvider__ctor_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Azure_AzureOpenAIOptions_"></a>

### AzureOpenAIProvider(HttpClient, AzureOpenAIOptions?)


```csharp
public AzureOpenAIProvider(HttpClient httpClient, AzureOpenAIOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [AzureOpenAIOptions](/api/Vercel.AI.Azure/AzureOpenAIOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L85)

## Fields

<a id="Vercel_AI_Azure_AzureOpenAIProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "azure"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L75)

## Methods

<a id="Vercel_AI_Azure_AzureOpenAIProvider_ChatModel_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider_ChatModel_System_String_Vercel_AI_Azure_AzureOpenAILanguageModelSettings_"></a>

### ChatModel(string, AzureOpenAILanguageModelSettings?)


```csharp
public OpenAIChatLanguageModel ChatModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
```


Chat model for a deployment. The optional model ID is used for reported identity and capability
detection; requests always use the deployment ID.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AzureOpenAILanguageModelSettings](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/) |  |

**Returns:** [OpenAIChatLanguageModel](/api/Vercel.AI.OpenAI/OpenAIChatLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L222)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_CompletionModel_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider_CompletionModel_System_String_Vercel_AI_Azure_AzureOpenAILanguageModelSettings_"></a>

### CompletionModel(string, AzureOpenAILanguageModelSettings?)


```csharp
public OpenAICompatibleCompletionLanguageModel CompletionModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
```


Completion model for a deployment. The optional model ID is used for reported identity;
requests always use the deployment ID.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AzureOpenAILanguageModelSettings](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/) |  |

**Returns:** [OpenAICompatibleCompletionLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleCompletionLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L231)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_Create_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider_Create_Vercel_AI_Azure_AzureOpenAIOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(AzureOpenAIOptions?, HttpMessageHandler?)


```csharp
public static AzureOpenAIProvider Create(AzureOpenAIOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [AzureOpenAIOptions](/api/Vercel.AI.Azure/AzureOpenAIOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [AzureOpenAIProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L168)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_Image_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider_Image_System_String_"></a>

### Image(string)


```csharp
public AzureImageModel Image(string deploymentId)
```


Image model. MAI-Image models use the MAI image API by default; other IDs use OpenAI. Override with
`providerOptions.azure.api`. Maps to `image`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [AzureImageModel](/api/Vercel.AI.Azure/AzureImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L244)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_ImageModel_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public override IImageModel ImageModel(string deploymentId)
```


Image model. Alias of [Image(string)](#Vercel_AI_Azure_AzureOpenAIProvider_Image_System_String_). Maps to `imageModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L250)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_LanguageModel_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L195)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_LanguageModel_System_String_Vercel_AI_Azure_AzureOpenAILanguageModelSettings_"></a>

### LanguageModel(string, AzureOpenAILanguageModelSettings?)


```csharp
public OpenAIResponsesLanguageModel LanguageModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
```


Responses API model, like [ResponsesModel(string)](#Vercel_AI_Azure_AzureOpenAIProvider_ResponsesModel_System_String_). The optional model ID in
&lt;code class="paramref"&gt;settings&lt;/code&gt; is used for reported identity and capability detection.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AzureOpenAILanguageModelSettings](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/) |  |

**Returns:** [OpenAIResponsesLanguageModel](/api/Vercel.AI.OpenAI/OpenAIResponsesLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L189)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_ResponsesModel_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIProvider_ResponsesModel_System_String_"></a>

### ResponsesModel(string)


```csharp
public OpenAIResponsesLanguageModel ResponsesModel(string modelId)
```


Responses API model. The provider id is `azure.responses`.
Options are read from the `azure` provider options and fall back to `openai`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIResponsesLanguageModel](/api/Vercel.AI.OpenAI/OpenAIResponsesLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L204)

<a id="Vercel_AI_Azure_AzureOpenAIProvider_ResponsesModel_System_String_Vercel_AI_Azure_AzureOpenAILanguageModelSettings_"></a>

### ResponsesModel(string, AzureOpenAILanguageModelSettings?)


```csharp
public OpenAIResponsesLanguageModel ResponsesModel(string deploymentId, AzureOpenAILanguageModelSettings? settings)
```


Responses API model for a deployment. The optional model ID is used for reported identity and
capability detection; requests always use the deployment ID.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `deploymentId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AzureOpenAILanguageModelSettings](/api/Vercel.AI.Azure/AzureOpenAILanguageModelSettings/) |  |

**Returns:** [OpenAIResponsesLanguageModel](/api/Vercel.AI.OpenAI/OpenAIResponsesLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L213)
