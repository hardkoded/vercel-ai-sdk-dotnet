---
title: "DecisionLiquidModel"
description: "Answers questions through the Liquid Decisions API (POST {base}/decisions/v1/systemone). Boolean questions are sent as native noul questions. Image and audio..."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionLiquidModel"
  hidden: true
---

<a id="Vercel_AI_Liquid_DecisionLiquidModel"></a>

Namespace: [Vercel.AI.Liquid](/api/Vercel.AI.Liquid/)


```csharp
public sealed class DecisionLiquidModel : IDecisionCaller
```


Answers questions through the Liquid Decisions API (`POST {base}/decisions/v1/systemone`). Boolean questions are sent as
native `noul` questions. Image and audio file parts are sent as base64 data URLs. Maps to `DecisionLiquidModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L19)

## Constructors

<a id="Vercel_AI_Liquid_DecisionLiquidModel__ctor_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel__ctor_Vercel_AI_Liquid_LiquidProvider_System_String_"></a>

### DecisionLiquidModel(LiquidProvider, string)


```csharp
public DecisionLiquidModel(LiquidProvider provider, string modelId)
```


Creates a decision model. &lt;code class="paramref"&gt;modelId&lt;/code&gt; may be a model id this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [LiquidProvider](/api/Vercel.AI.Liquid/LiquidProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L31)

## Properties

<a id="Vercel_AI_Liquid_DecisionLiquidModel_ModelId_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L41)

<a id="Vercel_AI_Liquid_DecisionLiquidModel_Provider_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L38)

<a id="Vercel_AI_Liquid_DecisionLiquidModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L44)

<a id="Vercel_AI_Liquid_DecisionLiquidModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L47)

## Methods

<a id="Vercel_AI_Liquid_DecisionLiquidModel_Deserialize_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_Net_Http_HttpClient_"></a>

### Deserialize(SerializedModelOptions, HttpClient?)


```csharp
public static DecisionLiquidModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
```


Restores a model from [Serialize()](#Vercel_AI_Liquid_DecisionLiquidModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`. Pass &lt;code class="paramref"&gt;httpClient&lt;/code&gt; to
send the requests through a custom handler.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [DecisionLiquidModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L78)

<a id="Vercel_AI_Liquid_DecisionLiquidModel_DoDecideAsync_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L127)

<a id="Vercel_AI_Liquid_DecisionLiquidModel_Serialize_"></a>
<a id="Vercel_AI_Liquid_DecisionLiquidModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id, the endpoint, and the resolved request headers (including authentication) for a workflow boundary.
Maps to `[WORKFLOW_SERIALIZE]`. The HTTP client is not serialized.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidDecisionModel.cs#L54)
