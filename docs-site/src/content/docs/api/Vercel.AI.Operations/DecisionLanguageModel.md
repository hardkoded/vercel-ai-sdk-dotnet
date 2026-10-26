---
title: "DecisionLanguageModel"
description: "Adapts structured language-model output to Choice, Score, and Boolean decisions. Maps to DecisionLanguageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionLanguageModel"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionLanguageModel : IEvaluationDecisionCaller, IDecisionCaller, IEvaluationCaller
```


Adapts structured language-model output to Choice, Score, and Boolean decisions. Maps to `DecisionLanguageModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/), [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/), [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L35)

## Constructors

<a id="Vercel_AI_Operations_DecisionLanguageModel__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel__ctor_Vercel_AI_Provider_ILanguageModel_System_String_"></a>

### DecisionLanguageModel(ILanguageModel, string?)


```csharp
public DecisionLanguageModel(ILanguageModel model, string? provider = null)
```


Wraps a language model. The provider id defaults to `{model.Provider}.decision`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L45)

## Properties

<a id="Vercel_AI_Operations_DecisionLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L60)

<a id="Vercel_AI_Operations_DecisionLanguageModel_Provider_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L57)

<a id="Vercel_AI_Operations_DecisionLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L63)

<a id="Vercel_AI_Operations_DecisionLanguageModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L66)

## Methods

<a id="Vercel_AI_Operations_DecisionLanguageModel_Deserialize_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_Deserialize_Vercel_AI_Operations_DecisionLanguageModelOptions_"></a>

### Deserialize(DecisionLanguageModelOptions)


```csharp
public static DecisionLanguageModel Deserialize(DecisionLanguageModelOptions options)
```


Restores a model from [Serialize(DecisionLanguageModel)](#Vercel_AI_Operations_DecisionLanguageModel_Serialize_Vercel_AI_Operations_DecisionLanguageModel_). Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [DecisionLanguageModelOptions](/api/Vercel.AI.Operations/DecisionLanguageModelOptions/) |  |

**Returns:** [DecisionLanguageModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L81)

<a id="Vercel_AI_Operations_DecisionLanguageModel_DoDecideAsync_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

### DoDecideAsync(DecisionModelCall, CancellationToken)


```csharp
public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken = default)
```


Answers every question with one structured-output model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DecisionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L99)

<a id="Vercel_AI_Operations_DecisionLanguageModel_DoEvaluateAsync_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_DoEvaluateAsync_Vercel_AI_Operations_EvaluationModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L92)

<a id="Vercel_AI_Operations_DecisionLanguageModel_Serialize_"></a>
<a id="Vercel_AI_Operations_DecisionLanguageModel_Serialize_Vercel_AI_Operations_DecisionLanguageModel_"></a>

### Serialize(DecisionLanguageModel)


```csharp
public static DecisionLanguageModelOptions Serialize(DecisionLanguageModel model)
```


Serializes the wrapped model and the provider id for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [DecisionLanguageModel](./) |  |

**Returns:** [DecisionLanguageModelOptions](/api/Vercel.AI.Operations/DecisionLanguageModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionLanguageModel.cs#L69)
