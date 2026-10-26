---
title: "MoonshotProvider"
description: "Moonshot provider. OpenAI Chat Completions compatible at https://api.moonshot.ai/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "MoonshotProvider"
  hidden: true
---

<a id="Vercel_AI_Moonshot_MoonshotProvider"></a>

Namespace: [Vercel.AI.Moonshot](/api/Vercel.AI.Moonshot/)


```csharp
public sealed class MoonshotProvider : OpenAICompatibleProvider
```


Moonshot provider. OpenAI Chat Completions compatible at `https://api.moonshot.ai/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L11)

## Constructors

<a id="Vercel_AI_Moonshot_MoonshotProvider__ctor_"></a>
<a id="Vercel_AI_Moonshot_MoonshotProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### MoonshotProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public MoonshotProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L23)

## Fields

<a id="Vercel_AI_Moonshot_MoonshotProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "MOONSHOT_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L20)

<a id="Vercel_AI_Moonshot_MoonshotProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.moonshot.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L17)

<a id="Vercel_AI_Moonshot_MoonshotProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "moonshotai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L14)

## Methods

<a id="Vercel_AI_Moonshot_MoonshotProvider_Create_"></a>
<a id="Vercel_AI_Moonshot_MoonshotProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static MoonshotProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [MoonshotProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L48)

<a id="Vercel_AI_Moonshot_MoonshotProvider_CreateChatModel_"></a>
<a id="Vercel_AI_Moonshot_MoonshotProvider_CreateChatModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L40)

<a id="Vercel_AI_Moonshot_MoonshotProvider_SupportsStructuredOutputs_"></a>
<a id="Vercel_AI_Moonshot_MoonshotProvider_SupportsStructuredOutputs_System_String_"></a>

### SupportsStructuredOutputs(string)


```csharp
public static bool SupportsStructuredOutputs(string modelId)
```


Whether &lt;code class="paramref"&gt;modelId&lt;/code&gt; accepts JSON schema responses. Every non-empty id does.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L29)
