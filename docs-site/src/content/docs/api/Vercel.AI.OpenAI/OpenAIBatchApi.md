---
title: "OpenAIBatchApi"
description: "OpenAI Batch API. Starts, reads, cancels, and lists Responses batches, and downloads their results."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIBatchApi"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIBatchApi"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIBatchApi : BatchResultsApiBase, IBatchApi
```


OpenAI Batch API. Starts, reads, cancels, and lists Responses batches, and downloads their results.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchResultsApiBase](/api/Vercel.AI.ProviderUtils/BatchResultsApiBase/)

**Implements:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L18)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_CanCancel_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_CanCancel"></a>

### CanCancel


```csharp
public override bool CanCancel { get; }
```


True when cancellation is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L44)

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_CanList_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_CanList"></a>

### CanList


```csharp
public override bool CanList { get; }
```


True when listing is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L47)

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_Provider"></a>

### Provider


```csharp
public override string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L41)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoCancelAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoCancelAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L149)

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoGetResultsAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoGetResultsAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L198)

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoGetStatusAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoGetStatusAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L143)

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoListAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoListAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L156)

<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoStartAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIBatchApi_DoStartAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoStartAsync(BatchOperationCall, CancellationToken)


```csharp
public override Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Uploads the requests as a JSONL input file, then creates a `/v1/responses` batch with a 24-hour completion window.
Every request must be a text request for the same model. `providerOptions.openai.inputFileExpiresAfter` sets the
input file TTL in seconds (3600 to 2592000, default 48 hours).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchStartResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIBatchApi.cs#L54)
