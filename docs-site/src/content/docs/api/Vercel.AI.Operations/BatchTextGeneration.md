---
title: "BatchTextGeneration"
description: "A provider text generation inside a batch item."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchTextGeneration"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchTextGeneration"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchTextGeneration
```


A provider text generation inside a batch item.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L331)

## Properties

<a id="Vercel_AI_Operations_BatchTextGeneration_Content_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_Content"></a>

### Content


```csharp
public IReadOnlyList<BatchContentPart> Content { get; set; }
```


Content parts.

**Value:** [IReadOnlyList&lt;BatchContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L334)

<a id="Vercel_AI_Operations_BatchTextGeneration_FinishReason_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_FinishReason"></a>

### FinishReason


```csharp
public string? FinishReason { get; set; }
```


Unified finish reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L337)

<a id="Vercel_AI_Operations_BatchTextGeneration_ModelId_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Response model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L352)

<a id="Vercel_AI_Operations_BatchTextGeneration_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L355)

<a id="Vercel_AI_Operations_BatchTextGeneration_RawFinishReason_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_RawFinishReason"></a>

### RawFinishReason


```csharp
public string? RawFinishReason { get; set; }
```


Raw finish reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L340)

<a id="Vercel_AI_Operations_BatchTextGeneration_ResponseId_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; set; }
```


Response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L346)

<a id="Vercel_AI_Operations_BatchTextGeneration_Timestamp_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_Timestamp"></a>

### Timestamp


```csharp
public DateTime? Timestamp { get; set; }
```


Response timestamp.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L349)

<a id="Vercel_AI_Operations_BatchTextGeneration_Usage_"></a>
<a id="Vercel_AI_Operations_BatchTextGeneration_Usage"></a>

### Usage


```csharp
public BatchModelUsage? Usage { get; set; }
```


Provider usage.

**Value:** [BatchModelUsage](/api/Vercel.AI.Operations/BatchModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L343)
