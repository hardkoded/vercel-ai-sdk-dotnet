---
title: "GatewayDecisionModel"
description: "Answers questions through the Gateway /decision-model route."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayDecisionModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayDecisionModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayDecisionModel : IEvaluationDecisionCaller, IDecisionCaller, IEvaluationCaller
```


Answers questions through the Gateway `/decision-model` route.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/), [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/), [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L16)

## Constructors

<a id="Vercel_AI_Gateway_GatewayDecisionModel__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayDecisionModel__ctor_Vercel_AI_Gateway_GatewayProvider_System_String_"></a>

### GatewayDecisionModel(GatewayProvider, string)


```csharp
public GatewayDecisionModel(GatewayProvider provider, string modelId)
```


Creates a decision model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L22)

## Properties

<a id="Vercel_AI_Gateway_GatewayDecisionModel_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayDecisionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L32)

<a id="Vercel_AI_Gateway_GatewayDecisionModel_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayDecisionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L29)

<a id="Vercel_AI_Gateway_GatewayDecisionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayDecisionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L35)

<a id="Vercel_AI_Gateway_GatewayDecisionModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_Gateway_GatewayDecisionModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L38)

## Methods

<a id="Vercel_AI_Gateway_GatewayDecisionModel_DoDecideAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayDecisionModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

### DoDecideAsync(DecisionModelCall, CancellationToken)


```csharp
public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken = default)
```


Sends the state and every question in one request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DecisionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L48)

<a id="Vercel_AI_Gateway_GatewayDecisionModel_DoEvaluateAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayDecisionModel_DoEvaluateAsync_Vercel_AI_Operations_EvaluationModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayDecisionModel.cs#L41)
