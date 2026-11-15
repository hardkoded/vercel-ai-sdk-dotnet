---
title: "ExtractionLanguageModel"
description: "Adapts structured language-model output to typed field extraction. Maps to ExtractionLanguageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionLanguageModel"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionLanguageModel : IExtractionCaller
```


Adapts structured language-model output to typed field extraction. Maps to `ExtractionLanguageModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IExtractionCaller](/api/Vercel.AI.Operations/IExtractionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L39)

## Constructors

<a id="Vercel_AI_Operations_ExtractionLanguageModel__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel__ctor_Vercel_AI_Provider_ILanguageModel_System_String_System_Nullable_System_Int32__"></a>

### ExtractionLanguageModel(ILanguageModel, string?, int?)


```csharp
public ExtractionLanguageModel(ILanguageModel model, string? provider = null, int? maxFieldsPerCall = null)
```


Wraps a language model. The provider id defaults to `{model.Provider}.extraction`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) | The language model. |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The extraction provider id. |
| `maxFieldsPerCall` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Most fields per language-model call, for providers that limit schema size. Larger field maps are split into parallel calls whose answers are merged. Usage is summed, and response and provider metadata come from the first call. Null means no limit. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L66)

## Properties

<a id="Vercel_AI_Operations_ExtractionLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L87)

<a id="Vercel_AI_Operations_ExtractionLanguageModel_Provider_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L84)

<a id="Vercel_AI_Operations_ExtractionLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L90)

<a id="Vercel_AI_Operations_ExtractionLanguageModel_SupportedFieldTypes_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel_SupportedFieldTypes"></a>

### SupportedFieldTypes


```csharp
public IReadOnlyList<string> SupportedFieldTypes { get; }
```


Field types this model can extract. Unsupported types are rejected before any provider I/O.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L93)

## Methods

<a id="Vercel_AI_Operations_ExtractionLanguageModel_Deserialize_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel_Deserialize_Vercel_AI_Operations_ExtractionLanguageModelOptions_"></a>

### Deserialize(ExtractionLanguageModelOptions)


```csharp
public static ExtractionLanguageModel Deserialize(ExtractionLanguageModelOptions options)
```


Restores a model from [Serialize(ExtractionLanguageModel)](#Vercel_AI_Operations_ExtractionLanguageModel_Serialize_Vercel_AI_Operations_ExtractionLanguageModel_). Maps to `[WORKFLOW_DESERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ExtractionLanguageModelOptions](/api/Vercel.AI.Operations/ExtractionLanguageModelOptions/) |  |

**Returns:** [ExtractionLanguageModel](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L108)

<a id="Vercel_AI_Operations_ExtractionLanguageModel_DoExtractAsync_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel_DoExtractAsync_Vercel_AI_Operations_ExtractionModelCall_System_Threading_CancellationToken_"></a>

### DoExtractAsync(ExtractionModelCall, CancellationToken)


```csharp
public Task<ExtractionModelResult> DoExtractAsync(ExtractionModelCall call, CancellationToken cancellationToken = default)
```


Extracts every field with one structured-output model call, or one per group of `maxFieldsPerCall` fields.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [ExtractionModelCall](/api/Vercel.AI.Operations/ExtractionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ExtractionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L119)

<a id="Vercel_AI_Operations_ExtractionLanguageModel_Serialize_"></a>
<a id="Vercel_AI_Operations_ExtractionLanguageModel_Serialize_Vercel_AI_Operations_ExtractionLanguageModel_"></a>

### Serialize(ExtractionLanguageModel)


```csharp
public static ExtractionLanguageModelOptions Serialize(ExtractionLanguageModel model)
```


Serializes the wrapped model, the provider id, and the field limit for a workflow boundary. Maps to `[WORKFLOW_SERIALIZE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ExtractionLanguageModel](./) |  |

**Returns:** [ExtractionLanguageModelOptions](/api/Vercel.AI.Operations/ExtractionLanguageModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionLanguageModel.cs#L96)
