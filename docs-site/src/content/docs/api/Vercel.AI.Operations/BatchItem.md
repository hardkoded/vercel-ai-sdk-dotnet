---
title: "BatchItem"
description: "One raw batch item."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchItem"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchItem"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchItem
```


One raw batch item.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L322)

## Constructors

<a id="Vercel_AI_Operations_BatchItem__ctor_"></a>
<a id="Vercel_AI_Operations_BatchItem__ctor_System_String_System_String_System_String_"></a>

### BatchItem(string, string, string)


```csharp
public BatchItem(string type, string id, string status)
```


Creates an item.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `status` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L325)

## Properties

<a id="Vercel_AI_Operations_BatchItem_ErrorCode_"></a>
<a id="Vercel_AI_Operations_BatchItem_ErrorCode"></a>

### ErrorCode


```csharp
public string? ErrorCode { get; set; }
```


Error code.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L351)

<a id="Vercel_AI_Operations_BatchItem_ErrorMessage_"></a>
<a id="Vercel_AI_Operations_BatchItem_ErrorMessage"></a>

### ErrorMessage


```csharp
public string? ErrorMessage { get; set; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L348)

<a id="Vercel_AI_Operations_BatchItem_ErrorStatusCode_"></a>
<a id="Vercel_AI_Operations_BatchItem_ErrorStatusCode"></a>

### ErrorStatusCode


```csharp
public int? ErrorStatusCode { get; set; }
```


HTTP status code of a failed provider response.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L357)

<a id="Vercel_AI_Operations_BatchItem_ErrorType_"></a>
<a id="Vercel_AI_Operations_BatchItem_ErrorType"></a>

### ErrorType


```csharp
public string? ErrorType { get; set; }
```


Error type reported by the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L354)

<a id="Vercel_AI_Operations_BatchItem_Id_"></a>
<a id="Vercel_AI_Operations_BatchItem_Id"></a>

### Id


```csharp
public string Id { get; }
```


Request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L336)

<a id="Vercel_AI_Operations_BatchItem_Image_"></a>
<a id="Vercel_AI_Operations_BatchItem_Image"></a>

### Image


```csharp
public BatchImageGeneration? Image { get; set; }
```


Image generation.

**Value:** [BatchImageGeneration](/api/Vercel.AI.Operations/BatchImageGeneration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L345)

<a id="Vercel_AI_Operations_BatchItem_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchItem_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata for a failed item.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L360)

<a id="Vercel_AI_Operations_BatchItem_Status_"></a>
<a id="Vercel_AI_Operations_BatchItem_Status"></a>

### Status


```csharp
public string Status { get; }
```


`succeeded`, `failed`, `cancelled`, or `expired`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L339)

<a id="Vercel_AI_Operations_BatchItem_Text_"></a>
<a id="Vercel_AI_Operations_BatchItem_Text"></a>

### Text


```csharp
public BatchTextGeneration? Text { get; set; }
```


Text generation.

**Value:** [BatchTextGeneration](/api/Vercel.AI.Operations/BatchTextGeneration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L342)

<a id="Vercel_AI_Operations_BatchItem_Type_"></a>
<a id="Vercel_AI_Operations_BatchItem_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text` or `image`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L333)
