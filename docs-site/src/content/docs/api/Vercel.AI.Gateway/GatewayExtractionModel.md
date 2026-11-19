---
title: "GatewayExtractionModel"
description: "Extracts typed field values through the Gateway /extraction-model route. Maps to GatewayExtractionModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayExtractionModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayExtractionModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayExtractionModel : IExtractionCaller
```


Extracts typed field values through the Gateway `/extraction-model` route. Maps to `GatewayExtractionModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IExtractionCaller](/api/Vercel.AI.Operations/IExtractionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayExtractionModel.cs#L15)

## Constructors

<a id="Vercel_AI_Gateway_GatewayExtractionModel__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayExtractionModel__ctor_Vercel_AI_Gateway_GatewayProvider_System_String_"></a>

### GatewayExtractionModel(GatewayProvider, string)


```csharp
public GatewayExtractionModel(GatewayProvider provider, string modelId)
```


Creates an extraction model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayExtractionModel.cs#L20)

## Properties

<a id="Vercel_AI_Gateway_GatewayExtractionModel_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayExtractionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayExtractionModel.cs#L30)

<a id="Vercel_AI_Gateway_GatewayExtractionModel_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayExtractionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayExtractionModel.cs#L27)

<a id="Vercel_AI_Gateway_GatewayExtractionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayExtractionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Only `v4` is accepted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayExtractionModel.cs#L33)

<a id="Vercel_AI_Gateway_GatewayExtractionModel_SupportedFieldTypes_"></a>
<a id="Vercel_AI_Gateway_GatewayExtractionModel_SupportedFieldTypes"></a>

### SupportedFieldTypes


```csharp
public IReadOnlyList<string> SupportedFieldTypes { get; }
```


Every field type. Support varies by model, so the Gateway rejects unsupported fields.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayExtractionModel.cs#L36)

## Methods

<a id="Vercel_AI_Gateway_GatewayExtractionModel_DoExtractAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayExtractionModel_DoExtractAsync_Vercel_AI_Operations_ExtractionModelCall_System_Threading_CancellationToken_"></a>

### DoExtractAsync(ExtractionModelCall, CancellationToken)


```csharp
public Task<ExtractionModelResult> DoExtractAsync(ExtractionModelCall call, CancellationToken cancellationToken = default)
```


Sends the state and every field in one request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [ExtractionModelCall](/api/Vercel.AI.Operations/ExtractionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ExtractionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayExtractionModel.cs#L39)
