---
title: "RerankingModelCallEndEvent"
description: "Event fired after the doRerank call. Maps to RerankingModelCallEndEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankingModelCallEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankingModelCallEndEvent
```


Event fired after the `doRerank` call. Maps to `RerankingModelCallEndEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L455)

## Constructors

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent__ctor_System_String_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_RerankModelRank__"></a>

### `RerankingModelCallEndEvent(string, string, string, string, string, IReadOnlyList<RerankModelRank>)`


```csharp
public RerankingModelCallEndEvent(string callId, string operationId, string provider, string modelId, string documentsType, IReadOnlyList<RerankModelRank> ranking)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `documentsType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `ranking` | [IReadOnlyList&lt;RerankModelRank&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L458)

## Properties

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L494)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_CallId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L476)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_DocumentsType_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_DocumentsType"></a>

### DocumentsType


```csharp
public string DocumentsType { get; }
```


`text` or `object`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L488)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L503)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L485)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.rerank.doRerank`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L479)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_Provider_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L482)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_Ranking_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RerankModelRank> Ranking { get; }
```


Ranking returned by the model.

**Value:** [IReadOnlyList&lt;RerankModelRank&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L491)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L497)

<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_RerankingModelCallEndEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L500)
