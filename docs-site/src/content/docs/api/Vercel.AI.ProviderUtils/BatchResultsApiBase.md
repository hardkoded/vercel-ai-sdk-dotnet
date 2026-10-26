---
title: "BatchResultsApiBase"
description: "Batch API that downloads results. Start, status, cancel, and list throw unless a provider overrides them."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchResultsApiBase"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public abstract class BatchResultsApiBase : IBatchApi
```


Batch API that downloads results. Start, status, cancel, and list throw unless a provider overrides them.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

**Derived:** [AnthropicBatchApi](/api/Vercel.AI.Anthropic/AnthropicBatchApi/), [GatewayBatchApi](/api/Vercel.AI.Gateway/GatewayBatchApi/), [GoogleBatchApi](/api/Vercel.AI.Google/GoogleBatchApi/), [OpenAIBatchApi](/api/Vercel.AI.OpenAI/OpenAIBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L19)

## Properties

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_CanCancel_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_CanCancel"></a>

### CanCancel


```csharp
public virtual bool CanCancel { get; }
```


True when cancellation is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L25)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_CanList_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_CanList"></a>

### CanList


```csharp
public virtual bool CanList { get; }
```


True when listing is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L28)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_Provider_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_Provider"></a>

### Provider


```csharp
public abstract string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L22)

## Methods

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_ConvertRows_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_ConvertRows_System_Collections_Generic_IAsyncEnumerable_System_Text_Json_JsonElement__System_Func_System_Text_Json_JsonElement_Vercel_AI_Operations_BatchItem__System_Threading_CancellationToken_"></a>

### `ConvertRows(IAsyncEnumerable<JsonElement>, Func<JsonElement, BatchItem>, CancellationToken)`


```csharp
protected static IAsyncEnumerable<BatchItem> ConvertRows(IAsyncEnumerable<JsonElement> rows, Func<JsonElement, BatchItem> convert, CancellationToken cancellationToken = default)
```


Converts each row of a JSON Lines download as it is read.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `rows` | [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) |  |
| `convert` | [Func&lt;JsonElement, BatchItem&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;BatchItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L58)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoCancelAsync_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoCancelAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoCancelAsync(BatchOperationCall, CancellationToken)


```csharp
public virtual Task<BatchCancelResult> DoCancelAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Cancels a batch.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchCancelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L46)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoGetResultsAsync_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoGetResultsAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoGetResultsAsync(BatchOperationCall, CancellationToken)


```csharp
public abstract Task<IAsyncEnumerable<BatchItem>> DoGetResultsAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Opens the result stream. Errors found while opening, such as a batch that is not complete, throw from the task.
Errors found while reading, such as a malformed row, throw from the stream.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IAsyncEnumerable&lt;BatchItem&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L31)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoGetStatusAsync_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoGetStatusAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoGetStatusAsync(BatchOperationCall, CancellationToken)


```csharp
public virtual Task<BatchStatus> DoGetStatusAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Reads status.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchStatus&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L40)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoListAsync_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoListAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoListAsync(BatchOperationCall, CancellationToken)


```csharp
public virtual Task<BatchListResult> DoListAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Lists batches. The implementation is invoked on this instance.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchListResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L52)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoStartAsync_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_DoStartAsync_Vercel_AI_Operations_BatchOperationCall_System_Threading_CancellationToken_"></a>

### DoStartAsync(BatchOperationCall, CancellationToken)


```csharp
public virtual Task<BatchStartResult> DoStartAsync(BatchOperationCall call, CancellationToken cancellationToken)
```


Starts a batch.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [BatchOperationCall](/api/Vercel.AI.Operations/BatchOperationCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;BatchStartResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L34)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_Str_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_Str_System_Text_Json_JsonElement_System_String_"></a>

### Str(JsonElement, string)


```csharp
protected static string? Str(JsonElement element, string name)
```


Text of a string property, or null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `element` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L67)

<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_TextItem_"></a>
<a id="Vercel_AI_ProviderUtils_BatchResultsApiBase_TextItem_System_String_System_Collections_Generic_IEnumerable_System_String__System_String_System_String_System_String_"></a>

### `TextItem(string, IEnumerable<string>, string?, string?, string?)`


```csharp
protected static BatchItem TextItem(string id, IEnumerable<string> texts, string? finishReason, string? responseId, string? modelId)
```


Builds a succeeded text item from joined text parts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `texts` | [IEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |
| `finishReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [BatchItem](/api/Vercel.AI.Operations/BatchItem/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/BatchResultDownloads.cs#L73)
