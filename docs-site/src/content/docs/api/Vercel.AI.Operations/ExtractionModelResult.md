---
title: "ExtractionModelResult"
description: "Model response for doExtract. Maps to ExtractionModelV4Result."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionModelResult
```


Model response for `doExtract`. Maps to `ExtractionModelV4Result`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L165)

## Constructors

<a id="Vercel_AI_Operations_ExtractionModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionModelResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionAnswer__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__Vercel_AI_Operations_OperationUsage_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `ExtractionModelResult(IReadOnlyDictionary<string, ExtractionAnswer>, IReadOnlyList<OperationWarning>?, OperationUsage?, JsonElement?, ProviderResponse?)`


```csharp
public ExtractionModelResult(IReadOnlyDictionary<string, ExtractionAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a model result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `answers` | [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L168)

## Properties

<a id="Vercel_AI_Operations_ExtractionModelResult_Answers_"></a>
<a id="Vercel_AI_Operations_ExtractionModelResult_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }
```


Exactly one answer per field, under the original field ids.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L178)

<a id="Vercel_AI_Operations_ExtractionModelResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ExtractionModelResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L187)

<a id="Vercel_AI_Operations_ExtractionModelResult_Response_"></a>
<a id="Vercel_AI_Operations_ExtractionModelResult_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Response metadata. Timestamp and model id are filled when omitted.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L190)

<a id="Vercel_AI_Operations_ExtractionModelResult_Usage_"></a>
<a id="Vercel_AI_Operations_ExtractionModelResult_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Token usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L184)

<a id="Vercel_AI_Operations_ExtractionModelResult_Warnings_"></a>
<a id="Vercel_AI_Operations_ExtractionModelResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionModel.cs#L181)
