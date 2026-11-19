---
title: "GoogleBatchApi"
description: "Gemini Batch API. Starts, reads, cancels, and lists batches and downloads their results."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleBatchApi"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleBatchApi"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleBatchApi : BatchResultsApiBase, IBatchApi
```


Gemini Batch API. Starts, reads, cancels, and lists batches and downloads their results.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchResultsApiBase](/api/Vercel.AI.ProviderUtils/BatchResultsApiBase/)

**Implements:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L18)

## Properties

<a id="Vercel_AI_Google_GoogleBatchApi_CanCancel_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_CanCancel"></a>

### CanCancel


```csharp
public override bool CanCancel { get; }
```


True when cancellation is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L34)

<a id="Vercel_AI_Google_GoogleBatchApi_CanList_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_CanList"></a>

### CanList


```csharp
public override bool CanList { get; }
```


True when listing is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L37)

<a id="Vercel_AI_Google_GoogleBatchApi_Provider_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_Provider"></a>

### Provider


```csharp
public override string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L31)

## Methods

<a id="Vercel_AI_Google_GoogleBatchApi_DoCancelAsync_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_DoCancelAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoCancelAsync(BatchOperationCall, CancellationToken)


```csharp
public override Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Cancels a batch.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchCancelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L274)

<a id="Vercel_AI_Google_GoogleBatchApi_DoGetResultsAsync_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_DoGetResultsAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoGetResultsAsync(BatchOperationCall, CancellationToken)


```csharp
public override Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Opens the result stream. Errors found while opening, such as a batch that is not complete, throw from the task.
Errors found while reading, such as a malformed row, throw from the stream.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IAsyncEnumerable&lt;BatchItem&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L343)

<a id="Vercel_AI_Google_GoogleBatchApi_DoGetStatusAsync_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_DoGetStatusAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoGetStatusAsync(BatchOperationCall, CancellationToken)


```csharp
public override Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Reads status.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchStatus&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L264)

<a id="Vercel_AI_Google_GoogleBatchApi_DoListAsync_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_DoListAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoListAsync(BatchOperationCall, CancellationToken)


```csharp
public override Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Lists batches. The implementation is invoked on this instance.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchListResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L293)

<a id="Vercel_AI_Google_GoogleBatchApi_DoStartAsync_"></a>
<a id="Vercel_AI_Google_GoogleBatchApi_DoStartAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoStartAsync(BatchOperationCall, CancellationToken)


```csharp
public override Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Starts a batch.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchStartResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleBatchApi.cs#L40)
