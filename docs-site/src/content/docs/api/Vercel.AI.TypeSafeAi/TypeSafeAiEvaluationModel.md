---
title: "TypeSafeAiEvaluationModel"
description: "Deprecated alias of TypeSafeAiDecisionModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeSafeAiEvaluationModel"
  hidden: true
---

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiEvaluationModel"></a>

Namespace: [Vercel.AI.TypeSafeAi](/api/Vercel.AI.TypeSafeAi/)


```csharp
[Obsolete("Use TypeSafeAiDecisionModel instead.")]
public sealed class TypeSafeAiEvaluationModel : TypeSafeAiDecisionModel, IEvaluationDecisionCaller, IDecisionCaller, IEvaluationCaller
```


Deprecated alias of [TypeSafeAiDecisionModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiDecisionModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TypeSafeAiDecisionModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiDecisionModel/)

**Implements:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/), [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/), [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L728)

## Constructors

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiEvaluationModel__ctor_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiEvaluationModel__ctor_System_String_Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_"></a>

### TypeSafeAiEvaluationModel(string, TypeSafeAiDecisionModelConfig?)


```csharp
public TypeSafeAiEvaluationModel(string modelId, TypeSafeAiDecisionModelConfig? config = null)
```


Creates a decision model. &lt;code class="paramref"&gt;modelId&lt;/code&gt; may be a model id this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [TypeSafeAiDecisionModelConfig](/api/Vercel.AI.TypeSafeAi/TypeSafeAiDecisionModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L732)

## Methods

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiEvaluationModel_Deserialize_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiEvaluationModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_Net_Http_HttpClient_"></a>

### Deserialize(SerializedModelOptions, HttpClient?)


```csharp
public static TypeSafeAiEvaluationModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
```


Deprecated alias of `HttpClient)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [TypeSafeAiEvaluationModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L738)
