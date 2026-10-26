---
title: "XaiProvider"
description: "xAI provider at https://api.x.ai/v1. LanguageModel(string) returns the Responses API model, as upstream does."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiProvider"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiProvider"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiProvider : OpenAICompatibleProvider
```


xAI provider at `https://api.x.ai/v1`. [LanguageModel(string)](#Vercel_AI_Xai_XaiProvider_LanguageModel_System_String_) returns the Responses API model, as upstream does.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L14)

## Constructors

<a id="Vercel_AI_Xai_XaiProvider__ctor_"></a>
<a id="Vercel_AI_Xai_XaiProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### XaiProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public XaiProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L28)

## Fields

<a id="Vercel_AI_Xai_XaiProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "XAI_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L23)

<a id="Vercel_AI_Xai_XaiProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.x.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L20)

<a id="Vercel_AI_Xai_XaiProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "xai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L17)

## Methods

<a id="Vercel_AI_Xai_XaiProvider_Create_"></a>
<a id="Vercel_AI_Xai_XaiProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static XaiProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [XaiProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L74)

<a id="Vercel_AI_Xai_XaiProvider_CreateChatModel_"></a>
<a id="Vercel_AI_Xai_XaiProvider_CreateChatModel_System_String_"></a>

### CreateChatModel(string)


```csharp
[Obsolete("xAI language models use the Responses API, as upstream does. Use LanguageModel or ResponsesModel. The Chat Completions model will be removed.")]
public override OpenAICompatibleLanguageModel CreateChatModel(string modelId)
```


Creates a Chat Completions model. Upstream xAI has no chat model; use [LanguageModel(string)](#Vercel_AI_Xai_XaiProvider_LanguageModel_System_String_) or [ResponsesModel(string)](#Vercel_AI_Xai_XaiProvider_ResponsesModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAICompatibleLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleLanguageModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L48)

<a id="Vercel_AI_Xai_XaiProvider_FileStore_"></a>
<a id="Vercel_AI_Xai_XaiProvider_FileStore"></a>

### FileStore()


```csharp
public override IFileStore FileStore()
```


Returns a file store.

**Returns:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L62)

<a id="Vercel_AI_Xai_XaiProvider_Files_"></a>
<a id="Vercel_AI_Xai_XaiProvider_Files"></a>

### Files()


```csharp
public XaiFiles Files()
```


xAI file store.

**Returns:** [XaiFiles](/api/Vercel.AI.Xai/XaiFiles/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L56)

<a id="Vercel_AI_Xai_XaiProvider_LanguageModel_"></a>
<a id="Vercel_AI_Xai_XaiProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Creates a Responses API model, like [ResponsesModel(string)](#Vercel_AI_Xai_XaiProvider_ResponsesModel_System_String_). The provider id is `xai.responses`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L35)

<a id="Vercel_AI_Xai_XaiProvider_RealtimeModel_"></a>
<a id="Vercel_AI_Xai_XaiProvider_RealtimeModel_System_String_"></a>

### RealtimeModel(string)


```csharp
public override IRealtimeModel RealtimeModel(string modelId)
```


Returns a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L68)

<a id="Vercel_AI_Xai_XaiProvider_ResponsesModel_"></a>
<a id="Vercel_AI_Xai_XaiProvider_ResponsesModel_System_String_"></a>

### ResponsesModel(string)


```csharp
public XaiResponsesLanguageModel ResponsesModel(string modelId)
```


Creates a Responses API model. The provider id is `xai.responses`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [XaiResponsesLanguageModel](/api/Vercel.AI.Xai/XaiResponsesLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L41)
