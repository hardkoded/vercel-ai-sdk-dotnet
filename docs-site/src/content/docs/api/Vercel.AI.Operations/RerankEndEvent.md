---
title: "RerankEndEvent"
description: "End event for rerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankEndEvent : IOperationEndEvent
```


End event for rerank.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IOperationEndEvent](/api/Vercel.AI/IOperationEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L316)

## Constructors

<a id="Vercel_AI_Operations_RerankEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_System_Collections_Generic_IReadOnlyList_System_Object__System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_RankedDocument__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_RerankResponseInfo_"></a>

### `RerankEndEvent(string, string, IReadOnlyDictionary<string, object?>, string, string, IReadOnlyList<object?>, string, IReadOnlyList<RankedDocument>, IReadOnlyList<OperationWarning>, JsonElement?, RerankResponseInfo)`


```csharp
public RerankEndEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, IReadOnlyList<object?> documents, string query, IReadOnlyList<RankedDocument> ranking, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, RerankResponseInfo response)
```


Creates an end event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `documents` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `ranking` | [IReadOnlyList&lt;RankedDocument&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [RerankResponseInfo](/api/Vercel.AI.Operations/RerankResponseInfo/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L319)

## Properties

<a id="Vercel_AI_Operations_RerankEndEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L380)

<a id="Vercel_AI_Operations_RerankEndEvent_CallId_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L347)

<a id="Vercel_AI_Operations_RerankEndEvent_Documents_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Documents"></a>

### Documents


```csharp
public IReadOnlyList<object?> Documents { get; }
```


Submitted documents.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L362)

<a id="Vercel_AI_Operations_RerankEndEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L389)

<a id="Vercel_AI_Operations_RerankEndEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L359)

<a id="Vercel_AI_Operations_RerankEndEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.rerank`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L350)

<a id="Vercel_AI_Operations_RerankEndEvent_Provider_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L356)

<a id="Vercel_AI_Operations_RerankEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L374)

<a id="Vercel_AI_Operations_RerankEndEvent_Query_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Query"></a>

### Query


```csharp
public string Query { get; }
```


Query.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L365)

<a id="Vercel_AI_Operations_RerankEndEvent_Ranking_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RankedDocument> Ranking { get; }
```


Ranking visible to the callback. The result keeps its own copy.

**Value:** [IReadOnlyList&lt;RankedDocument&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L368)

<a id="Vercel_AI_Operations_RerankEndEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L383)

<a id="Vercel_AI_Operations_RerankEndEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L386)

<a id="Vercel_AI_Operations_RerankEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Response"></a>

### Response


```csharp
public RerankResponseInfo Response { get; }
```


Response metadata.

**Value:** [RerankResponseInfo](/api/Vercel.AI.Operations/RerankResponseInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L377)

<a id="Vercel_AI_Operations_RerankEndEvent_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; }
```


Runtime context.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L353)

<a id="Vercel_AI_Operations_RerankEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L371)
