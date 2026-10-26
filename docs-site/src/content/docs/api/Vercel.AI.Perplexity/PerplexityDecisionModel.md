---
title: "PerplexityDecisionModel"
description: "Answers questions through the Perplexity Decisions API (POST {base}/v1/decisions). Boolean questions are sent as native noul questions. Probabilities and sco..."
editUrl: false
prev: false
next: false
sidebar:
  label: "PerplexityDecisionModel"
  hidden: true
---

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel"></a>

Namespace: [Vercel.AI.Perplexity](/api/Vercel.AI.Perplexity/)


```csharp
public sealed class PerplexityDecisionModel : IDecisionCaller
```


Answers questions through the Perplexity Decisions API (`POST {base}/v1/decisions`). Boolean questions are sent as
native `noul` questions. Probabilities and scores keep full precision. Provider options are not supported.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L19)

## Constructors

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel__ctor_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel__ctor_Vercel_AI_Perplexity_PerplexityProvider_System_String_"></a>

### PerplexityDecisionModel(PerplexityProvider, string)


```csharp
public PerplexityDecisionModel(PerplexityProvider provider, string modelId)
```


Creates a decision model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L28)

## Properties

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_ModelId_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L38)

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_Provider_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L35)

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L41)

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L44)

## Methods

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_Deserialize_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_Net_Http_HttpClient_"></a>

### Deserialize(SerializedModelOptions, HttpClient?)


```csharp
public static PerplexityDecisionModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
```


Restores a model from [Serialize()](#Vercel_AI_Perplexity_PerplexityDecisionModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`. Pass &lt;code class="paramref"&gt;httpClient&lt;/code&gt; to
send the requests through a custom handler.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [PerplexityDecisionModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L74)

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_DoDecideAsync_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L118)

<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_Serialize_"></a>
<a id="Vercel_AI_Perplexity_PerplexityDecisionModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id, the endpoint, and the resolved request headers (including authentication) for a workflow boundary.
Maps to `[WORKFLOW_SERIALIZE]`. The HTTP client is not serialized.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityDecisionModel.cs#L50)
