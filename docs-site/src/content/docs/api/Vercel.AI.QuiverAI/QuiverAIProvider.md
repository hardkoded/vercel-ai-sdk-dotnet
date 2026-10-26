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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L13)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L22)

## Fields

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.quiver.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L19)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "quiverai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L16)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L34)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L41)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_LanguageModel_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L28)

<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ResolveApiKey_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIProvider_ResolveApiKey"></a>

### ResolveApiKey()


```csharp
protected override string? ResolveApiKey()
```


Throws when neither the option nor `QUIVERAI_API_KEY` holds a key.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIProvider.cs#L44)
