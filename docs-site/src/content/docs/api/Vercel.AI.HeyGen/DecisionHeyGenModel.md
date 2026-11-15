---
title: "DecisionHeyGenModel"
description: "Answers questions through the HeyGen Decision API (POST {base}/v3/models/decisions). Boolean questions are sent as native noul questions. Image and video fil..."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionHeyGenModel"
  hidden: true
---

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel"></a>

Namespace: [Vercel.AI.HeyGen](/api/Vercel.AI.HeyGen/)


```csharp
public sealed class DecisionHeyGenModel : IDecisionCaller
```


Answers questions through the HeyGen Decision API (`POST {base}/v3/models/decisions`). Boolean questions are sent as
native `noul` questions. Image and video file parts are sent as HeyGen assets. Maps to `DecisionHeyGenModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L19)

## Constructors

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel__ctor_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel__ctor_Vercel_AI_HeyGen_HeyGenProvider_System_String_"></a>

### DecisionHeyGenModel(HeyGenProvider, string)


```csharp
public DecisionHeyGenModel(HeyGenProvider provider, string modelId)
```


Creates a decision model. &lt;code class="paramref"&gt;modelId&lt;/code&gt; may be a model id this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [HeyGenProvider](/api/Vercel.AI.HeyGen/HeyGenProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L24)

## Properties

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_ModelId_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L34)

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_Provider_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L31)

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_SpecificationVersion_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L37)

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types this model can answer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L40)

## Methods

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_Deserialize_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_Net_Http_HttpClient_"></a>

### Deserialize(SerializedModelOptions, HttpClient?)


```csharp
public static DecisionHeyGenModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
```


Restores a model from [Serialize()](#Vercel_AI_HeyGen_DecisionHeyGenModel_Serialize). Maps to `[WORKFLOW_DESERIALIZE]`. Pass &lt;code class="paramref"&gt;httpClient&lt;/code&gt; to
send the requests through a custom handler.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [DecisionHeyGenModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L70)

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_DoDecideAsync_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_DoDecideAsync_Vercel_AI_Operations_DecisionModelCall_System_Threading_CancellationToken_"></a>

### DoDecideAsync(DecisionModelCall, CancellationToken)


```csharp
public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken = default)
```


Sends every question in one Decision request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [DecisionModelCall](/api/Vercel.AI.Operations/DecisionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DecisionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L114)

<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_Serialize_"></a>
<a id="Vercel_AI_HeyGen_DecisionHeyGenModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id, the endpoint, and the resolved request headers (including authentication) for a workflow boundary.
Maps to `[WORKFLOW_SERIALIZE]`. The HTTP client is not serialized.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenDecisionModel.cs#L46)
