---
title: "AnthropicBatchApi"
description: "Anthropic Message Batches API. Downloads the JSONL results of a finished batch."
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


Anthropic Message Batches API. Downloads the JSONL results of a finished batch.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchResultsApiBase](/api/Vercel.AI.ProviderUtils/BatchResultsApiBase/)

**Implements:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L14)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicBatchApi_Provider_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchApi_Provider"></a>

### Provider


```csharp
public override string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L24)

## Methods

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatchApi.cs#L27)
