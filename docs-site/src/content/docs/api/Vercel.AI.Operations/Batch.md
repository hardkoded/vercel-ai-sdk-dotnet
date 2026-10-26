---
title: "Batch"
description: "Batch operations. Maps to startBatch, getBatchStatus, getBatchResults, listBatches, and cancelBatch."
editUrl: false
prev: false
next: false
sidebar:
  label: "Batch"
  hidden: true
---

<a id="Vercel_AI_Operations_Batch"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class Batch
```


Batch operations. Maps to `startBatch`, `getBatchStatus`, `getBatchResults`, `listBatches`, and `cancelBatch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L696)

## Methods

<a id="Vercel_AI_Operations_Batch_CancelBatchAsync_"></a>
<a id="Vercel_AI_Operations_Batch_CancelBatchAsync_Vercel_AI_Operations_BatchRequestOptions_System_Threading_CancellationToken_"></a>

### CancelBatchAsync(BatchRequestOptions, CancellationToken)


```csharp
public static Task<BatchCancelResult> CancelBatchAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
```


Requests cancellation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchCancelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L699)

<a id="Vercel_AI_Operations_Batch_GetAllBatchResultsAsync_"></a>
<a id="Vercel_AI_Operations_Batch_GetAllBatchResultsAsync_Vercel_AI_Operations_BatchRequestOptions_System_Threading_CancellationToken_"></a>

### GetAllBatchResultsAsync(BatchRequestOptions, CancellationToken)


```csharp
public static Task<IReadOnlyList<BatchItemResult>> GetAllBatchResultsAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
```


Reads every result of `CancellationToken)` into a list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;BatchItemResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L828)

<a id="Vercel_AI_Operations_Batch_GetBatchResults_"></a>
<a id="Vercel_AI_Operations_Batch_GetBatchResults_Vercel_AI_Operations_BatchRequestOptions_System_Threading_CancellationToken_"></a>

### GetBatchResults(BatchRequestOptions, CancellationToken)


```csharp
public static IAsyncEnumerable<BatchItemResult> GetBatchResults(BatchRequestOptions request, CancellationToken cancellationToken = default)
```


Opens results immediately and normalizes them as they are read. Each item is read from the provider only when
the caller asks for it. Use `CancellationToken)` to read every result into a list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;BatchItemResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L817)

<a id="Vercel_AI_Operations_Batch_GetBatchStatusAsync_"></a>
<a id="Vercel_AI_Operations_Batch_GetBatchStatusAsync_Vercel_AI_Operations_BatchRequestOptions_System_Threading_CancellationToken_"></a>

### GetBatchStatusAsync(BatchRequestOptions, CancellationToken)


```csharp
public static Task<BatchStatus> GetBatchStatusAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
```


Reads the latest status.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchStatus&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L804)

<a id="Vercel_AI_Operations_Batch_ListBatchesAsync_"></a>
<a id="Vercel_AI_Operations_Batch_ListBatchesAsync_Vercel_AI_Operations_BatchRequestOptions_System_Threading_CancellationToken_"></a>

### ListBatchesAsync(BatchRequestOptions, CancellationToken)


```csharp
public static Task<BatchListResult> ListBatchesAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
```


Lists one page. `doListBatches` is invoked on the batch API instance.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchListResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L714)

<a id="Vercel_AI_Operations_Batch_StartBatchAsync_"></a>
<a id="Vercel_AI_Operations_Batch_StartBatchAsync_Vercel_AI_Operations_BatchRequestOptions_System_Threading_CancellationToken_"></a>

### StartBatchAsync(BatchRequestOptions, CancellationToken)


```csharp
public static Task<BatchStartResult> StartBatchAsync(BatchRequestOptions request, CancellationToken cancellationToken = default)
```


Validates and starts a batch.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchStartResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L740)
