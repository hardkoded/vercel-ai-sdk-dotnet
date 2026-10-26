---
title: "RerankModelCallEvent"
description: "Inner rerank telemetry event."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankModelCallEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankModelCallEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankModelCallEvent
```


Inner rerank telemetry event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L341)

## Constructors

<a id="Vercel_AI_Operations_RerankModelCallEvent__ctor_"></a>
<a id="Vercel_AI_Operations_RerankModelCallEvent__ctor_System_String_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_RerankModelRank__"></a>

### `RerankModelCallEvent(string, string, string, string, string?, IReadOnlyList<RerankModelRank>?)`


```csharp
public RerankModelCallEvent(string callId, string operationId, string provider, string modelId, string? documentsType, IReadOnlyList<RerankModelRank>? ranking)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L344)

## Properties

<a id="Vercel_AI_Operations_RerankModelCallEvent_CallId_"></a>
<a id="Vercel_AI_Operations_RerankModelCallEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L355)

<a id="Vercel_AI_Operations_RerankModelCallEvent_DocumentsType_"></a>
<a id="Vercel_AI_Operations_RerankModelCallEvent_DocumentsType"></a>

### DocumentsType


```csharp
public string? DocumentsType { get; }
```


`text` or `object`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L367)

<a id="Vercel_AI_Operations_RerankModelCallEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_RerankModelCallEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L364)

<a id="Vercel_AI_Operations_RerankModelCallEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_RerankModelCallEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.rerank.doRerank`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L358)

<a id="Vercel_AI_Operations_RerankModelCallEvent_Provider_"></a>
<a id="Vercel_AI_Operations_RerankModelCallEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L361)

<a id="Vercel_AI_Operations_RerankModelCallEvent_Ranking_"></a>
<a id="Vercel_AI_Operations_RerankModelCallEvent_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RerankModelRank>? Ranking { get; }
```


Ranking, on the end event.

**Value:** [IReadOnlyList&lt;RerankModelRank&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L370)
