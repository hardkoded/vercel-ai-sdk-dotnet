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
public class BatchItemResult
```


Normalized batch item.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [BatchItemResult&lt;TOutput&gt;](/api/Vercel.AI.Operations/BatchItemResult-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L437)

## Properties

<a id="Vercel_AI_Operations_BatchItemResult_Content_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Content"></a>

### Content


```csharp
public IReadOnlyList<BatchContentPart>? Content { get; set; }
```


Text content.

**Value:** [IReadOnlyList&lt;BatchContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L449)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorCode_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorCode"></a>

### ErrorCode


```csharp
public string? ErrorCode { get; set; }
```


Error code.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L488)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorMessage_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorMessage"></a>

### ErrorMessage


```csharp
public string? ErrorMessage { get; set; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L485)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorStatusCode_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorStatusCode"></a>

### ErrorStatusCode


```csharp
public int? ErrorStatusCode { get; set; }
```


HTTP status code of a failed provider response.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L494)

<a id="Vercel_AI_Operations_BatchItemResult_ErrorType_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ErrorType"></a>

### ErrorType


```csharp
public string? ErrorType { get; set; }
```


Error type reported by the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L491)

<a id="Vercel_AI_Operations_BatchItemResult_FinishReason_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_FinishReason"></a>

### FinishReason


```csharp
public string? FinishReason { get; set; }
```


Finish reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L455)

<a id="Vercel_AI_Operations_BatchItemResult_Id_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Id"></a>

### Id


```csharp
public string Id { get; set; }
```


Request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L443)

<a id="Vercel_AI_Operations_BatchItemResult_ImageUsage_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ImageUsage"></a>

### ImageUsage


```csharp
public OperationUsage? ImageUsage { get; set; }
```


Image usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L464)

<a id="Vercel_AI_Operations_BatchItemResult_Images_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Images"></a>

### Images


```csharp
public IReadOnlyList<GeneratedImage>? Images { get; set; }
```


Images.

**Value:** [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L476)

<a id="Vercel_AI_Operations_BatchItemResult_Output_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Output"></a>

### Output


```csharp
public JsonElement? Output { get; set; }
```


The parsed output of a successful text item. Only set when `BatchRequestOptions.Output` is passed to
`GetBatchResults` and the text parsed. Maps to `output`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L500)

<a id="Vercel_AI_Operations_BatchItemResult_OutputError_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_OutputError"></a>

### OutputError


```csharp
public BatchError? OutputError { get; set; }
```


Why no [Output](#Vercel_AI_Operations_BatchItemResult_Output) is available. A parsing failure does not fail the item: [Text](#Vercel_AI_Operations_BatchItemResult_Text) and
[Usage](#Vercel_AI_Operations_BatchItemResult_Usage) remain available. Maps to `outputError`.

**Value:** [BatchError](/api/Vercel.AI.Operations/BatchError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L506)

<a id="Vercel_AI_Operations_BatchItemResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L482)

<a id="Vercel_AI_Operations_BatchItemResult_RawFinishReason_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_RawFinishReason"></a>

### RawFinishReason


```csharp
public string? RawFinishReason { get; set; }
```


Raw finish reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L458)

<a id="Vercel_AI_Operations_BatchItemResult_ResponseId_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; set; }
```


Response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L467)

<a id="Vercel_AI_Operations_BatchItemResult_ResponseModelId_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_ResponseModelId"></a>

### ResponseModelId


```csharp
public string? ResponseModelId { get; set; }
```


Response model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L473)

<a id="Vercel_AI_Operations_BatchItemResult_Status_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Status"></a>

### Status


```csharp
public string Status { get; set; }
```


Status.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L446)

<a id="Vercel_AI_Operations_BatchItemResult_Text_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Joined text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L452)

<a id="Vercel_AI_Operations_BatchItemResult_Timestamp_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Timestamp"></a>

### Timestamp


```csharp
public DateTime? Timestamp { get; set; }
```


Response timestamp.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L470)

<a id="Vercel_AI_Operations_BatchItemResult_Type_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


Item type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L440)

<a id="Vercel_AI_Operations_BatchItemResult_Usage_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Usage"></a>

### Usage


```csharp
public BatchUsage? Usage { get; set; }
```


Text usage.

**Value:** [BatchUsage](/api/Vercel.AI.Operations/BatchUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L461)

<a id="Vercel_AI_Operations_BatchItemResult_Warnings_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning>? Warnings { get; set; }
```


Image warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L479)
