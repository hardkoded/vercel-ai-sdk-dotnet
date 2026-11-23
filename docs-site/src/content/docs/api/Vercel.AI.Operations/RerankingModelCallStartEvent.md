---
title: "RerankingModelCallStartEvent"
description: "Event fired before the doRerank call. Maps to RerankingModelCallStartEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankingModelCallStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class RerankingModelCallStartEvent
```


Event fired before the `doRerank` call. Maps to `RerankingModelCallStartEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L393)

## Constructors

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent__ctor_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_Object__System_String_System_String_System_Nullable_System_Int32__"></a>

### `RerankingModelCallStartEvent(string, string, string, string, IReadOnlyList<object?>, string, string, int?)`


```csharp
public RerankingModelCallStartEvent(string callId, string operationId, string provider, string modelId, IReadOnlyList<object?> documents, string documentsType, string query, int? topN)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `documents` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `documentsType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `topN` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L396)

## Properties

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L442)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L418)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_Documents_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_Documents"></a>

### Documents


```csharp
public IReadOnlyList<object?> Documents { get; }
```


Documents sent to the model.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L430)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_DocumentsType_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_DocumentsType"></a>

### DocumentsType


```csharp
public string DocumentsType { get; }
```


`text` or `object`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L433)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L451)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L427)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.rerank.doRerank`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L421)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L424)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_Query_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_Query"></a>

### Query


```csharp
public string Query { get; }
```


Query.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L436)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L445)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L448)

<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_TopN_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallStartEvent_TopN"></a>

### TopN


```csharp
public int? TopN { get; }
```


Maximum results.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L439)
