---
title: "LiquidProvider"
description: "Liquid provider. It supports experimental decision models."
editUrl: false
prev: false
next: false
sidebar:
  label: "LiquidProvider"
  hidden: true
---

<a id="Vercel_AI_Liquid_LiquidProvider"></a>

Namespace: [Vercel.AI.Liquid](/api/Vercel.AI.Liquid/)


```csharp
public sealed class LiquidProvider : OpenAICompatibleProvider, IDecisionProvider
```


Liquid provider. It supports experimental decision models.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

**Implements:** [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L12)

## Constructors

<a id="Vercel_AI_Liquid_LiquidProvider__ctor_"></a>
<a id="Vercel_AI_Liquid_LiquidProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### LiquidProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public LiquidProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider. The API key is read when a model is used, not here.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L23)

## Fields

<a id="Vercel_AI_Liquid_LiquidProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.liquid.ai"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L18)

<a id="Vercel_AI_Liquid_LiquidProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "liquid"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L15)

## Methods

<a id="Vercel_AI_Liquid_LiquidProvider_Create_"></a>
<a id="Vercel_AI_Liquid_LiquidProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static LiquidProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [LiquidProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L30)

<a id="Vercel_AI_Liquid_LiquidProvider_DecisionModel_"></a>
<a id="Vercel_AI_Liquid_LiquidProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public DecisionLiquidModel DecisionModel(string modelId)
```


Creates an experimental decision model using the Liquid Decisions API. The provider id is `liquid.decision`. Maps to `decisionModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DecisionLiquidModel](/api/Vercel.AI.Liquid/DecisionLiquidModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L50)

<a id="Vercel_AI_Liquid_LiquidProvider_LanguageModel_"></a>
<a id="Vercel_AI_Liquid_LiquidProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L42)
