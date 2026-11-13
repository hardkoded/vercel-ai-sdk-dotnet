---
title: "GoogleDecisionModel"
description: "Answers questions through the Gemini Decisions API (POST /decisions). Maps to DecisionGoogleModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleDecisionModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleDecisionModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleDecisionModel : IEvaluationDecisionCaller, IDecisionCaller, IEvaluationCaller
```


Answers questions through the Gemini Decisions API (`POST /decisions`). Maps to `DecisionGoogleModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEvaluationDecisionCaller](/api/Vercel.AI.Operations/IEvaluationDecisionCaller/), [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/), [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L17)

## Constructors

<a id="Vercel_AI_Google_GoogleDecisionModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleDecisionModel__ctor_Vercel_AI_Google_GoogleProvider_System_String_System_String_"></a>

### GoogleDecisionModel(GoogleProvider, string, string)


```csharp
public GoogleDecisionModel(GoogleProvider provider, string modelId, string providerId)
```


Creates a decision model. &lt;code class="paramref"&gt;providerId&lt;/code&gt; is the model provider id, such as `google.decision`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L27)

## Properties

<a id="Vercel_AI_Google_GoogleDecisionModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleDecisionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L38)

<a id="Vercel_AI_Google_GoogleDecisionModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleDecisionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L35)

<a id="Vercel_AI_Google_GoogleDecisionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Google_GoogleDecisionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L41)

<a id="Vercel_AI_Google_GoogleDecisionModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_Google_GoogleDecisionModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L44)

## Methods

<a id="Vercel_AI_Google_GoogleDecisionModel_DoDecideAsync_"></a>
<a id="Vercel_AI_Google_GoogleDecisionModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L54)

<a id="Vercel_AI_Google_GoogleDecisionModel_DoEvaluateAsync_"></a>
<a id="Vercel_AI_Google_GoogleDecisionModel_DoEvaluateAsync_Vercel_AI_Operations_EvaluationModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleDecisionModel.cs#L47)
