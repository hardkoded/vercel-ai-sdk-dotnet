---
title: "ReplicateProvider"
description: "Replicate provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReplicateProvider"
  hidden: true
---

<a id="Vercel_AI_Replicate_ReplicateProvider"></a>

Namespace: [Vercel.AI.Replicate](/api/Vercel.AI.Replicate/)


```csharp
public sealed class ReplicateProvider : OpenAICompatibleProvider
```


Replicate provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L14)

## Constructors

<a id="Vercel_AI_Replicate_ReplicateProvider__ctor_"></a>
<a id="Vercel_AI_Replicate_ReplicateProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### ReplicateProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public ReplicateProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L25)

## Fields

<a id="Vercel_AI_Replicate_ReplicateProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.replicate.com/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L20)

<a id="Vercel_AI_Replicate_ReplicateProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "replicate"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L17)

## Methods

<a id="Vercel_AI_Replicate_ReplicateProvider_Create_"></a>
<a id="Vercel_AI_Replicate_ReplicateProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static ReplicateProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [ReplicateProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L38)

<a id="Vercel_AI_Replicate_ReplicateProvider_ImageModel_"></a>
<a id="Vercel_AI_Replicate_ReplicateProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L64)

<a id="Vercel_AI_Replicate_ReplicateProvider_LanguageModel_"></a>
<a id="Vercel_AI_Replicate_ReplicateProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L32)
