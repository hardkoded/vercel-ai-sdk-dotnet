---
title: "BatchItemResult"
description: "Normalized batch item."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchItemResult"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchItemResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchItemResult
```


Normalized batch item.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L364)

## Properties

<a id="Vercel_AI_Operations_BatchItemResult_Content_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Content"></a>

### Content


```csharp
public IReadOnlyList<BatchContentPart>? Content { get; set; }
```


Text content.

**Value:** [IReadOnlyList&lt;BatchContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L376)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorCode_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorCode"></a>

### ErrorCode


```csharp
public string? ErrorCode { get; set; }
```


Error code.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L415)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorMessage_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorMessage"></a>

### ErrorMessage


```csharp
public string? ErrorMessage { get; set; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L412)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorStatusCode_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorStatusCode"></a>

### ErrorStatusCode


```csharp
public int? ErrorStatusCode { get; set; }
```


HTTP status code of a failed provider response.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L421)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorType_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorType"></a>

### ErrorType


```csharp
public string? ErrorType { get; set; }
```


Error type reported by the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L418)

<a id="Vercel_AI_Operations_BatchItemResult_FinishReason_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_FinishReason"></a>

### FinishReason


```csharp
public string? FinishReason { get; set; }
```


Finish reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L382)

<a id="Vercel_AI_Operations_BatchItemResult_Id_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Id"></a>

### Id


```csharp
public string Id { get; set; }
```


Request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L370)

<a id="Vercel_AI_Operations_BatchItemResult_ImageUsage_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ImageUsage"></a>

### ImageUsage


```csharp
public OperationUsage? ImageUsage { get; set; }
```


Image usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L391)

<a id="Vercel_AI_Operations_BatchItemResult_Images_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Images"></a>

### Images


```csharp
public IReadOnlyList<GeneratedImage>? Images { get; set; }
```


Images.

**Value:** [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L403)

<a id="Vercel_AI_Operations_BatchItemResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L409)

<a id="Vercel_AI_Operations_BatchItemResult_RawFinishReason_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_RawFinishReason"></a>

### RawFinishReason


```csharp
public string? RawFinishReason { get; set; }
```


Raw finish reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L385)

<a id="Vercel_AI_Operations_BatchItemResult_ResponseId_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; set; }
```


Response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L394)

<a id="Vercel_AI_Operations_BatchItemResult_ResponseModelId_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ResponseModelId"></a>

### ResponseModelId


```csharp
public string? ResponseModelId { get; set; }
```


Response model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L400)

<a id="Vercel_AI_Operations_BatchItemResult_Status_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Status"></a>

### Status


```csharp
public string Status { get; set; }
```


Status.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L373)

<a id="Vercel_AI_Operations_BatchItemResult_Text_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Joined text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L379)

<a id="Vercel_AI_Operations_BatchItemResult_Timestamp_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Timestamp"></a>

### Timestamp


```csharp
public DateTime? Timestamp { get; set; }
```


Response timestamp.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L397)

<a id="Vercel_AI_Operations_BatchItemResult_Type_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


Item type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L367)

<a id="Vercel_AI_Operations_BatchItemResult_Usage_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Usage"></a>

### Usage


```csharp
public BatchUsage? Usage { get; set; }
```


Text usage.

**Value:** [BatchUsage](/api/Vercel.AI.Operations/BatchUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L388)

<a id="Vercel_AI_Operations_BatchItemResult_Warnings_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning>? Warnings { get; set; }
```


Image warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L406)
