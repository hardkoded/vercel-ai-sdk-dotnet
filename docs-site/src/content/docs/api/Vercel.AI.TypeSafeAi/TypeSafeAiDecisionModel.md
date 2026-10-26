---
title: "TypeSafeAiDecisionModel"
description: "Answers questions through the TypeSafe /systemone route."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeSafeAiDecisionModel"
  hidden: true
---

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel"></a>

Namespace: [Vercel.AI.TypeSafeAi](/api/Vercel.AI.TypeSafeAi/)


```csharp
public class TypeSafeAiDecisionModel : IEvaluationDecisionCaller, IDecisionCaller, IEvaluationCaller
```


Answers questions through the TypeSafe `/systemone` route.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/), [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/), [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/)

**Derived:** [TypeSafeAiEvaluationModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiEvaluationModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L36)

## Constructors

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel__ctor_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel__ctor_System_String_Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModelConfig_"></a>

### TypeSafeAiDecisionModel(string, TypeSafeAiDecisionModelConfig?)


```csharp
public TypeSafeAiDecisionModel(string modelId, TypeSafeAiDecisionModelConfig? config = null)
```


Creates a decision model. &lt;code class="paramref"&gt;modelId&lt;/code&gt; may be a model id this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [TypeSafeAiDecisionModelConfig](/api/Vercel.AI.TypeSafeAi/TypeSafeAiDecisionModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L44)

## Properties

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_ModelId_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L54)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_Provider_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L51)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L57)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L60)

## Methods

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_Deserialize_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_Net_Http_HttpClient_"></a>

### Deserialize(SerializedModelOptions, HttpClient?)


```csharp
public static TypeSafeAiDecisionModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
```


Restores a model from [Serialize()](#Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_Serialize). When no headers were serialized, authentication is read from
`TYPESAFE_AI_API_KEY` at call time. The HTTP client is not serialized, so pass one to use a custom handler.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [TypeSafeAiDecisionModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L91)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_DoDecideAsync_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

### DoDecideAsync(DecisionModelCall, CancellationToken)


```csharp
public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
```


Decides every question in one call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DecisionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L132)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_DoEvaluateAsync_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_DoEvaluateAsync_Vercel_AI_Operations_EvaluationModelCall_System_Threading_CancellationToken_"></a>

### DoEvaluateAsync(EvaluationModelCall, CancellationToken)


```csharp
[Obsolete("Use DoDecideAsync instead.")]
public Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
```


Deprecated. Use `CancellationToken)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [EvaluationModelCall](/api/Vercel.AI.Operations/EvaluationModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EvaluationModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L125)

<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_Serialize_"></a>
<a id="Vercel_AI_TypeSafeAi_TypeSafeAiDecisionModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and the JSON-serializable part of the config for a workflow boundary.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TypeSafeAi/TypeSafeAiDecisionModel.cs#L63)
