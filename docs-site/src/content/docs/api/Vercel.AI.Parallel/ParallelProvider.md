---
title: "ParallelProvider"
description: "Parallel's experimental web-grounded decision capability. It has no language, embedding, or image models. Maps to ParallelProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParallelProvider"
  hidden: true
---

<a id="Vercel_AI_Parallel_ParallelProvider"></a>

Namespace: [Vercel.AI.Parallel](/api/Vercel.AI.Parallel/)


```csharp
public sealed class ParallelProvider
```


Parallel's experimental web-grounded decision capability. It has no language, embedding, or image models. Maps to `ParallelProvider`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L26)

## Constructors

<a id="Vercel_AI_Parallel_ParallelProvider__ctor_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider__ctor_Vercel_AI_Parallel_ParallelProviderSettings_System_Net_Http_HttpClient_"></a>

### ParallelProvider(ParallelProviderSettings?, HttpClient?)


```csharp
public ParallelProvider(ParallelProviderSettings? options = null, HttpClient? httpClient = null)
```


Creates a provider. Maps to `createParallel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ParallelProviderSettings](/api/Vercel.AI.Parallel/ParallelProviderSettings/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L46)

## Fields

<a id="Vercel_AI_Parallel_ParallelProvider_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public const string ApiKeyEnvironmentVariable = "PARALLEL_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L35)

<a id="Vercel_AI_Parallel_ParallelProvider_DecisionProviderId"></a>

### DecisionProviderId


```csharp
public const string DecisionProviderId = "parallel.decision"
```


Provider id of the decision model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L29)

<a id="Vercel_AI_Parallel_ParallelProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.parallel.ai/v1"
```


Default API base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L32)

## Properties

<a id="Vercel_AI_Parallel_ParallelProvider_Default_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider_Default"></a>

### Default


```csharp
public static ParallelProvider Default { get; }
```


Provider that reads `PARALLEL_API_KEY` when a request is made. Maps to `parallel`.

**Value:** [ParallelProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L56)

<a id="Vercel_AI_Parallel_ParallelProvider_SpecificationVersion_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L59)

## Methods

<a id="Vercel_AI_Parallel_ParallelProvider_Create_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider_Create_Vercel_AI_Parallel_ParallelProviderSettings_System_Net_Http_HttpMessageHandler_"></a>

### Create(ParallelProviderSettings?, HttpMessageHandler?)


```csharp
public static ParallelProvider Create(ParallelProviderSettings? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests. Maps to `createParallel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ParallelProviderSettings](/api/Vercel.AI.Parallel/ParallelProviderSettings/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [ParallelProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L62)

<a id="Vercel_AI_Parallel_ParallelProvider_DecisionModel_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public DecisionParallelModel DecisionModel(string modelId)
```


Creates a web-grounded decision model. The id may be a mode this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionParallelModel](/api/Vercel.AI.Parallel/DecisionParallelModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L74)

<a id="Vercel_AI_Parallel_ParallelProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public IEmbeddingModel EmbeddingModel(string modelId)
```


Parallel has no embedding models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L100)

<a id="Vercel_AI_Parallel_ParallelProvider_ImageModel_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public IImageModel ImageModel(string modelId)
```


Parallel has no image models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L106)

<a id="Vercel_AI_Parallel_ParallelProvider_LanguageModel_"></a>
<a id="Vercel_AI_Parallel_ParallelProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public ILanguageModel LanguageModel(string modelId)
```


Parallel has no language models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L94)
