---
title: "BatchRequestOptions"
description: "Options for batch operations."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchRequestOptions"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchRequestOptions"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchRequestOptions : OperationRequest
```


Options for batch operations.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L665)

## Properties

<a id="Vercel_AI_Operations_BatchRequestOptions_Batch_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_Batch"></a>

### Batch


```csharp
public BatchReference? Batch { get; set; }
```


Batch reference.

**Value:** [BatchReference](/api/Vercel.AI.Operations/BatchReference/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L671)

<a id="Vercel_AI_Operations_BatchRequestOptions_Cursor_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_Cursor"></a>

### Cursor


```csharp
public string? Cursor { get; set; }
```


Page cursor.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L683)

<a id="Vercel_AI_Operations_BatchRequestOptions_Download_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_Download"></a>

### Download


```csharp
public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }
```


Downloads file URLs inside results.

**Value:** [Func&lt;string, CancellationToken, Task&lt;DownloadedMedia&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L692)

<a id="Vercel_AI_Operations_BatchRequestOptions_Limit_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_Limit"></a>

### Limit


```csharp
public int? Limit { get; set; }
```


Page size.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L680)

<a id="Vercel_AI_Operations_BatchRequestOptions_Provider_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_Provider"></a>

### Provider


```csharp
public ApiOrProvider<IBatchApi, IBatchProvider>? Provider { get; set; }
```


Batch API or a provider that exposes one. Null uses [Default](/api/Vercel.AI.Operations/BatchModels/#Vercel_AI_Operations_BatchModels_Default).

**Value:** `ApiOrProvider&lt;IBatchApi, IBatchProvider&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L668)

<a id="Vercel_AI_Operations_BatchRequestOptions_Requests_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_Requests"></a>

### Requests


```csharp
public IReadOnlyList<BatchRequest>? Requests { get; set; }
```


Requests to start.

**Value:** [IReadOnlyList&lt;BatchRequest&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L674)

<a id="Vercel_AI_Operations_BatchRequestOptions_TimeoutMs_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_TimeoutMs"></a>

### TimeoutMs


```csharp
public int? TimeoutMs { get; set; }
```


Total timeout in milliseconds.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L689)

<a id="Vercel_AI_Operations_BatchRequestOptions_Tools_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<BatchTool>? Tools { get; set; }
```


Tools used to parse client tool calls. They are not executed.

**Value:** [IReadOnlyList&lt;BatchTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L686)

<a id="Vercel_AI_Operations_BatchRequestOptions_WebhookUrl_"></a>
<a id="Vercel_AI_Operations_BatchRequestOptions_WebhookUrl"></a>

### WebhookUrl


```csharp
public string? WebhookUrl { get; set; }
```


Webhook URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L677)
