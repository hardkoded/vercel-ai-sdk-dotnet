---
title: "BatchStartResult"
description: "Start acknowledgement."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchStartResult"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchStartResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchStartResult : BatchStatus
```


Start acknowledgement.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchReference](/api/Vercel.AI.Operations/BatchReference/) → [BatchStatus](/api/Vercel.AI.Operations/BatchStatus/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L790)

## Properties

<a id="Vercel_AI_Operations_BatchStartResult_Warnings_"></a>
<a id="Vercel_AI_Operations_BatchStartResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<BatchRequestWarning> Warnings { get; set; }
```


Warnings.

**Value:** [IReadOnlyList&lt;BatchRequestWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L793)
