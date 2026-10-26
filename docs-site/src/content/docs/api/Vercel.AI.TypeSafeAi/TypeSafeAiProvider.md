---
title: "TypeSafeAiProvider"
description: "TypeSafe's experimental decision capability. It has no language, embedding, or image models."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeSafeAiProvider"
  hidden: true
---

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider"></a>

Namespace: [Vercel.AI.TypeSafeAi](/api/Vercel.AI.TypeSafeAi/)


```csharp
public sealed class TypeSafeAiProvider
```


TypeSafe's experimental decision capability. It has no language, embedding, or image models.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L26)

## Constructors

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider__ctor_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider__ctor_Vercel_AI_TypeSafeAi_TypeSafeAiOptions_System_Net_Http_HttpClient_"></a>

### TypeSafeAiProvider(TypeSafeAiOptions?, HttpClient?)


```csharp
public TypeSafeAiProvider(TypeSafeAiOptions? options = null, HttpClient? httpClient = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [TypeSafeAiOptions](/api/Vercel.AI.TypeSafeAi/TypeSafeAiOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L51)

## Fields

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public const string ApiKeyEnvironmentVariable = "TYPESAFE_AI_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L39)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_BaseUrlEnvironmentVariable"></a>

### BaseUrlEnvironmentVariable


```csharp
public const string BaseUrlEnvironmentVariable = "TYPESAFE_AI_BASE_URL"
```


Environment variable for the API base URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L42)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_DecisionProviderId"></a>

### DecisionProviderId


```csharp
public const string DecisionProviderId = "typesafe.decision"
```


Provider id of the decision model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L29)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.typesafe.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L36)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_EvaluationProviderId"></a>

### EvaluationProviderId


```csharp
[Obsolete("Use DecisionProviderId instead.")]
public const string EvaluationProviderId = "typesafe.decision"
```


Deprecated alias of [DecisionProviderId](#Vercel_AI_TypeSafeAi_TypeSafeAiProvider_DecisionProviderId).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L33)

## Properties

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_Default_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_Default"></a>

### Default


```csharp
public static TypeSafeAiProvider Default { get; }
```


Provider that reads `TYPESAFE_AI_API_KEY` when a request is made.
It reads `TYPESAFE_AI_BASE_URL` when it is first created.

**Value:** [TypeSafeAiProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L65)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_SpecificationVersion_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L75)

## Methods

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_Create_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_Create_Vercel_AI_TypeSafeAi_TypeSafeAiOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(TypeSafeAiOptions?, HttpMessageHandler?)


```csharp
public static TypeSafeAiProvider Create(TypeSafeAiOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [TypeSafeAiOptions](/api/Vercel.AI.TypeSafeAi/TypeSafeAiOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [TypeSafeAiProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L78)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_DecisionModel_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public TypeSafeAiDecisionModel DecisionModel(string modelId)
```


Returns a decision model. The id may be one this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [TypeSafeAiDecisionModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiDecisionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L84)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public IEmbeddingModel EmbeddingModel(string modelId)
```


TypeSafe has no embedding models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L114)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_EvaluationModel_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_EvaluationModel_System_String_"></a>

### EvaluationModel(string)


```csharp
[Obsolete("Use DecisionModel instead.")]
public TypeSafeAiEvaluationModel EvaluationModel(string modelId)
```


Deprecated alias of [DecisionModel(string)](#Vercel_AI_TypeSafeAi_TypeSafeAiProvider_DecisionModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [TypeSafeAiEvaluationModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiEvaluationModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L90)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_ImageModel_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public IImageModel ImageModel(string modelId)
```


TypeSafe has no image models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L120)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_LanguageModel_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public ILanguageModel LanguageModel(string modelId)
```


TypeSafe has no language models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiProvider.cs#L108)
