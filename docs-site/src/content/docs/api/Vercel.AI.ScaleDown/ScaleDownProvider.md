---
title: "ScaleDownProvider"
description: "ScaleDown's experimental decision capability. It has no language, embedding, or image models."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownProvider"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownProvider"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public sealed class ScaleDownProvider
```


ScaleDown's experimental decision capability. It has no language, embedding, or image models.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L26)

## Constructors

<a id="Vercel_AI_ScaleDown_ScaleDownProvider__ctor_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider__ctor_Vercel_AI_ScaleDown_ScaleDownProviderSettings_System_Net_Http_HttpClient_"></a>

### ScaleDownProvider(ScaleDownProviderSettings?, HttpClient?)


```csharp
public ScaleDownProvider(ScaleDownProviderSettings? options = null, HttpClient? httpClient = null)
```


Creates a provider. Maps to `createScaleDown`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ScaleDownProviderSettings](/api/Vercel.AI.ScaleDown/ScaleDownProviderSettings/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L44)

## Fields

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public const string ApiKeyEnvironmentVariable = "SCALEDOWN_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L35)

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_DecisionProviderId"></a>

### DecisionProviderId


```csharp
public const string DecisionProviderId = "scaledown.decision"
```


Provider id of the decision model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L29)

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.scaledown.xyz"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L32)

## Properties

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_Default_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider_Default"></a>

### Default


```csharp
public static ScaleDownProvider Default { get; }
```


Provider that reads `SCALEDOWN_API_KEY` when a request is made. Maps to `scaledown` and `scaleDown`.

**Value:** [ScaleDownProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L52)

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_SpecificationVersion_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L55)

## Methods

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_Create_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider_Create_Vercel_AI_ScaleDown_ScaleDownProviderSettings_System_Net_Http_HttpMessageHandler_"></a>

### Create(ScaleDownProviderSettings?, HttpMessageHandler?)


```csharp
public static ScaleDownProvider Create(ScaleDownProviderSettings? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests. Maps to `createScaleDown`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ScaleDownProviderSettings](/api/Vercel.AI.ScaleDown/ScaleDownProviderSettings/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [ScaleDownProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L58)

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_DecisionModel_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public DecisionScaleDownModel DecisionModel(string modelId)
```


Creates a model for ScaleDown's experimental Decisions API. The id may be one this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionScaleDownModel](/api/Vercel.AI.ScaleDown/DecisionScaleDownModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L64)

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public IEmbeddingModel EmbeddingModel(string modelId)
```


ScaleDown has no embedding models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L82)

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_ImageModel_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public IImageModel ImageModel(string modelId)
```


ScaleDown has no image models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L88)

<a id="Vercel_AI_ScaleDown_ScaleDownProvider_LanguageModel_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public ILanguageModel LanguageModel(string modelId)
```


ScaleDown has no language models.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L76)
