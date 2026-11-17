---
title: "AnthropicBatchApi"
description: "Anthropic Message Batches API. Starts, inspects, cancels, and lists batches, and downloads the JSONL results of a finished batch."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicBatchApi"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicBatchApi"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicBatchApi : BatchResultsApiBase, IBatchApi
```


Anthropic Message Batches API. Starts, inspects, cancels, and lists batches, and downloads the JSONL results of a finished batch.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchResultsApiBase](/api/Vercel.AI.ProviderUtils/BatchResultsApiBase/)

**Implements:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L17)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_CanCancel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_CanCancel"></a>

### CanCancel


```csharp
public override bool CanCancel { get; }
```


True when cancellation is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L31)

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_CanList_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_CanList"></a>

### CanList


```csharp
public override bool CanList { get; }
```


True when listing is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L34)

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_Provider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_Provider"></a>

### Provider


```csharp
public override string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L27)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoCancelAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoCancelAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L208)

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoGetResultsAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoGetResultsAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L464)

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoGetStatusAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoGetStatusAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L196)

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoListAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoListAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L222)

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoStartAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_DoStartAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L37)
