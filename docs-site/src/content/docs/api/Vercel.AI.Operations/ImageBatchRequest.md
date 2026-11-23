---
title: "ImageBatchRequest"
description: "One image generation request within a batch. Maps to ImageBatchRequest."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageBatchRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_ImageBatchRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ImageBatchRequest : BatchRequest
```


One image generation request within a batch. Maps to `ImageBatchRequest`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchRequest](/api/Vercel.AI.Operations/BatchRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L137)

## Constructors

<a id="Vercel_AI_Operations_ImageBatchRequest__ctor_"></a>
<a id="Vercel_AI_Operations_ImageBatchRequest__ctor_System_String_System_String_"></a>

### ImageBatchRequest(string, string)


```csharp
public ImageBatchRequest(string id, string model)
```


Creates an image request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L140)
