---
title: "BatchOperationCall"
description: "Arguments for a batch operation."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchOperationCall"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchOperationCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchOperationCall
```


Arguments for a batch operation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L576)

## Constructors

<a id="Vercel_AI_Operations_BatchOperationCall__ctor_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_System_Nullable_System_Int32__System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_NormalizedBatchRequest__"></a>

### `BatchOperationCall(string?, JsonElement?, IReadOnlyDictionary<string, string>, CancellationToken, int?, string?, string?, IReadOnlyList<NormalizedBatchRequest>?)`


```csharp
public BatchOperationCall(string? batchId, JsonElement? providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, int? limit = null, string? cursor = null, string? webhookUrl = null, IReadOnlyList<NormalizedBatchRequest>? requests = null)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `batchId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `limit` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cursor` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `webhookUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requests` | [IReadOnlyList&lt;NormalizedBatchRequest&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L579)

## Properties

<a id="Vercel_AI_Operations_BatchOperationCall_BatchId_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_BatchId"></a>

### BatchId


```csharp
public string? BatchId { get; }
```


Batch id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L592)

<a id="Vercel_AI_Operations_BatchOperationCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L601)

<a id="Vercel_AI_Operations_BatchOperationCall_Cursor_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_Cursor"></a>

### Cursor


```csharp
public string? Cursor { get; }
```


Page cursor.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L607)

<a id="Vercel_AI_Operations_BatchOperationCall_Headers_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L598)

<a id="Vercel_AI_Operations_BatchOperationCall_Limit_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_Limit"></a>

### Limit


```csharp
public int? Limit { get; }
```


Page size.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L604)

<a id="Vercel_AI_Operations_BatchOperationCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L595)

<a id="Vercel_AI_Operations_BatchOperationCall_Receiver_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_Receiver"></a>

### Receiver


```csharp
public object? Receiver { get; set; }
```


True when this call was made on the batch API instance.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L616)

<a id="Vercel_AI_Operations_BatchOperationCall_Requests_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_Requests"></a>

### Requests


```csharp
public IReadOnlyList<NormalizedBatchRequest>? Requests { get; }
```


Normalized requests.

**Value:** [IReadOnlyList&lt;NormalizedBatchRequest&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L613)

<a id="Vercel_AI_Operations_BatchOperationCall_WebhookUrl_"></a>
<a id="Vercel_AI_Operations_BatchOperationCall_WebhookUrl"></a>

### WebhookUrl


```csharp
public string? WebhookUrl { get; }
```


Webhook URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L610)
