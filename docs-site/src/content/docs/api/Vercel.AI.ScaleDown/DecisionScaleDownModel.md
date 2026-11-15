---
title: "DecisionScaleDownModel"
description: "Answers questions through the ScaleDown /v1/scaledown route."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionScaleDownModel"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public sealed class DecisionScaleDownModel : IDecisionCaller
```


Answers questions through the ScaleDown `/v1/scaledown` route.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L47)

## Constructors

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel__ctor_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel__ctor_System_String_Vercel_AI_ScaleDown_ScaleDownDecisionModelConfig_"></a>

### DecisionScaleDownModel(string, ScaleDownDecisionModelConfig?)


```csharp
public DecisionScaleDownModel(string modelId, ScaleDownDecisionModelConfig? config = null)
```


Creates a decision model. &lt;code class="paramref"&gt;modelId&lt;/code&gt; may be `classify`, `classify-1`, or an id this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [ScaleDownDecisionModelConfig](/api/Vercel.AI.ScaleDown/ScaleDownDecisionModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L56)

## Properties

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_ModelId_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L66)

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_Provider_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L63)

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_SpecificationVersion_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L69)

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L72)

## Methods

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_Deserialize_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_Net_Http_HttpClient_"></a>

### Deserialize(SerializedModelOptions, HttpClient?)


```csharp
public static DecisionScaleDownModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
```


Restores a model from [Serialize()](#Vercel_AI_ScaleDown_DecisionScaleDownModel_Serialize). When no headers were serialized, authentication is read from
`SCALEDOWN_API_KEY` at call time. Maps to `WORKFLOW_DESERIALIZE`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [DecisionScaleDownModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L94)

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_DoDecideAsync_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L122)

<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_Serialize_"></a>
<a id="Vercel_AI_ScaleDown_DecisionScaleDownModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and the JSON-serializable part of the config for a workflow boundary. Maps to `WORKFLOW_SERIALIZE`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/DecisionScaleDownModel.cs#L75)
