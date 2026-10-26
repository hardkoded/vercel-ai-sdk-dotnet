---
title: "OpenAIDecisionModel"
description: "Answers questions through the OpenAI Decisions API (POST /decisions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIDecisionModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIDecisionModel : IEvaluationDecisionCaller, IDecisionCaller, IEvaluationCaller
```


Answers questions through the OpenAI Decisions API (`POST /decisions`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/), [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/), [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L17)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIDecisionModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAIDecisionModel(OpenAIProvider, string)


```csharp
public OpenAIDecisionModel(OpenAIProvider provider, string modelId)
```


Creates a decision model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L27)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L37)

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L34)

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L40)

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L43)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_DoDecideAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

### DoDecideAsync(DecisionModelCall, CancellationToken)


```csharp
public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken = default)
```


Sends every question in one Decisions request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DecisionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L53)

<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_DoEvaluateAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIDecisionModel_DoEvaluateAsync_Vercel_AI_Operations_EvaluationModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Decision/OpenAIDecisionModel.cs#L46)
