---
title: "BatchStatus"
description: "The latest status of a batch. It is also upstream's Batch (BatchReference & BatchStatus), so the start, list, and status results are all BatchReference values."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchStatus"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchStatus"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class BatchStatus : BatchReference
```


The latest status of a batch. It is also upstream's `Batch` (`BatchReference & BatchStatus`), so the
start, list, and status results are all [BatchReference](/api/Vercel.AI.Operations/BatchReference/) values.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchReference](/api/Vercel.AI.Operations/BatchReference/)

**Derived:** [BatchStartResult](/api/Vercel.AI.Operations/BatchStartResult/), [ListedBatch](/api/Vercel.AI.Operations/ListedBatch/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L667)

## Properties

<a id="Vercel_AI_Operations_BatchStatus_CreatedAt_"></a>
<a id="Vercel_AI_Operations_BatchStatus_CreatedAt"></a>

### CreatedAt


```csharp
public string? CreatedAt { get; set; }
```


Creation time.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L679)

<a id="Vercel_AI_Operations_BatchStatus_ErrorCode_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ErrorCode"></a>

### ErrorCode


```csharp
public string? ErrorCode { get; set; }
```


Code of the batch-level error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L688)

<a id="Vercel_AI_Operations_BatchStatus_ErrorMessage_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ErrorMessage"></a>

### ErrorMessage


```csharp
public string? ErrorMessage { get; set; }
```


Message of the batch-level error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L685)

<a id="Vercel_AI_Operations_BatchStatus_ExpiresAt_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ExpiresAt"></a>

### ExpiresAt


```csharp
public string? ExpiresAt { get; set; }
```


Expiry time.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L682)

<a id="Vercel_AI_Operations_BatchStatus_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L691)

<a id="Vercel_AI_Operations_BatchStatus_RawStatus_"></a>
<a id="Vercel_AI_Operations_BatchStatus_RawStatus"></a>

### RawStatus


```csharp
public string? RawStatus { get; set; }
```


Provider status string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L673)

<a id="Vercel_AI_Operations_BatchStatus_RequestCounts_"></a>
<a id="Vercel_AI_Operations_BatchStatus_RequestCounts"></a>

### RequestCounts


```csharp
public BatchRequestCounts? RequestCounts { get; set; }
```


Request counts.

**Value:** [BatchRequestCounts](/api/Vercel.AI.Operations/BatchRequestCounts/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L676)

<a id="Vercel_AI_Operations_BatchStatus_Status_"></a>
<a id="Vercel_AI_Operations_BatchStatus_Status"></a>

### Status


```csharp
public string? Status { get; set; }
```


Status name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L670)
