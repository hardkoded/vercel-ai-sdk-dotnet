---
title: "ScaleDownExtractionModel"
description: "Extracts text fields and lists through the ScaleDown /v1/chat/completions route. Maps to ScaleDownExtractionModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownExtractionModel"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public sealed class ScaleDownExtractionModel : IExtractionCaller
```


Extracts text fields and lists through the ScaleDown `/v1/chat/completions` route. Maps to `ScaleDownExtractionModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IExtractionCaller](/api/Vercel.AI.Operations/IExtractionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L33)

## Constructors

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel__ctor_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel__ctor_System_String_Vercel_AI_ScaleDown_ScaleDownExtractionModelConfig_"></a>

### ScaleDownExtractionModel(string, ScaleDownExtractionModelConfig?)


```csharp
public ScaleDownExtractionModel(string modelId, ScaleDownExtractionModelConfig? config = null)
```


Creates an extraction model. &lt;code class="paramref"&gt;modelId&lt;/code&gt; may be `extract`, `extract-1`, or an id this library does not know yet.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [ScaleDownExtractionModelConfig](/api/Vercel.AI.ScaleDown/ScaleDownExtractionModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L40)

## Properties

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_ModelId_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L50)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_Provider_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L47)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L53)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_SupportedFieldTypes_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_SupportedFieldTypes"></a>

### SupportedFieldTypes


```csharp
public IReadOnlyList<string> SupportedFieldTypes { get; }
```


Field types this model can extract. Unsupported types are rejected before any provider I/O.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L56)

## Methods

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_Deserialize_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_Deserialize_Vercel_AI_ProviderUtils_SerializedModelOptions_System_Net_Http_HttpClient_"></a>

### Deserialize(SerializedModelOptions, HttpClient?)


```csharp
public static ScaleDownExtractionModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
```


Restores a model from [Serialize()](#Vercel_AI_ScaleDown_ScaleDownExtractionModel_Serialize). When no headers were serialized, authentication is read from
`SCALEDOWN_API_KEY` at call time. Maps to `WORKFLOW_DESERIALIZE`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [ScaleDownExtractionModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L78)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_DoExtractAsync_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_DoExtractAsync_Vercel_AI_Operations_ExtractionModelCall_System_Threading_CancellationToken_"></a>

### DoExtractAsync(ExtractionModelCall, CancellationToken)


```csharp
public Task<ExtractionModelResult> DoExtractAsync(ExtractionModelCall call, CancellationToken cancellationToken)
```


Extracts every field from the same state. No partial results.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [ExtractionModelCall](/api/Vercel.AI.Operations/ExtractionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ExtractionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L106)

<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_Serialize_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownExtractionModel_Serialize"></a>

### Serialize()


```csharp
public SerializedModelOptions Serialize()
```


Serializes the model id and the JSON-serializable part of the config for a workflow boundary. Maps to `WORKFLOW_SERIALIZE`.

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownExtractionModel.cs#L59)
