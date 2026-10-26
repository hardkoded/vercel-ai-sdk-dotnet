---
title: "BatchStatus"
description: "Status fields shared by start, list, and status calls."
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
public class BatchStatus
```


Status fields shared by start, list, and status calls.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [BatchStartResult](/api/Vercel.AI.Operations/BatchStartResult/), [ListedBatch](/api/Vercel.AI.Operations/ListedBatch/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L488)

## Properties

<a id="Vercel_AI_Operations_BatchStatus_CreatedAt_"></a>
<a id="Vercel_AI_Operations_BatchStatus_CreatedAt"></a>

### CreatedAt


```csharp
public string? CreatedAt { get; set; }
```


Creation time.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L500)

<a id="Vercel_AI_Operations_BatchStatus_ErrorCode_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ErrorCode"></a>

### ErrorCode


```csharp
public string? ErrorCode { get; set; }
```


Code of the batch-level error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L509)

<a id="Vercel_AI_Operations_BatchStatus_ErrorMessage_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ErrorMessage"></a>

### ErrorMessage


```csharp
public string? ErrorMessage { get; set; }
```


Message of the batch-level error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L506)

<a id="Vercel_AI_Operations_BatchStatus_ExpiresAt_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ExpiresAt"></a>

### ExpiresAt


```csharp
public string? ExpiresAt { get; set; }
```


Expiry time.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L503)

<a id="Vercel_AI_Operations_BatchStatus_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchStatus_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L512)

<a id="Vercel_AI_Operations_BatchStatus_RawStatus_"></a>
<a id="Vercel_AI_Operations_BatchStatus_RawStatus"></a>

### RawStatus


```csharp
public string? RawStatus { get; set; }
```


Provider status string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L494)

<a id="Vercel_AI_Operations_BatchStatus_RequestCounts_"></a>
<a id="Vercel_AI_Operations_BatchStatus_RequestCounts"></a>

### RequestCounts


```csharp
public BatchRequestCounts? RequestCounts { get; set; }
```


Request counts.

**Value:** [BatchRequestCounts](/api/Vercel.AI.Operations/BatchRequestCounts/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L497)

<a id="Vercel_AI_Operations_BatchStatus_Status_"></a>
<a id="Vercel_AI_Operations_BatchStatus_Status"></a>

### Status


```csharp
public string? Status { get; set; }
```


Status name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L491)
