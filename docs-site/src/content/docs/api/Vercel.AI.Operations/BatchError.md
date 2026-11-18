---
title: "BatchError"
description: "An error of a batch item. Maps to BatchError."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchError"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchError"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchError
```


An error of a batch item. Maps to `BatchError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L363)

## Properties

<a id="Vercel_AI_Operations_BatchError_Message_"></a>
<a id="Vercel_AI_Operations_BatchError_Message"></a>

### Message


```csharp
public string Message { get; set; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L366)

<a id="Vercel_AI_Operations_BatchError_Type_"></a>
<a id="Vercel_AI_Operations_BatchError_Type"></a>

### Type


```csharp
public string? Type { get; set; }
```


Error type, for example the name of the error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L369)
