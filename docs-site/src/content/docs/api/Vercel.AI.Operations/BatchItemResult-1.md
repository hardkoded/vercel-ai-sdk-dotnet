---
title: "BatchItemResult<TOutput>"
description: "Normalized batch item whose parsed output has the type of the OutputSpec%601 passed to GetBatchResults. Maps to BatchItemResult<OUTPUT>."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchItemResult<TOutput>"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchItemResult_1"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchItemResult<TOutput> : BatchItemResult
```


Normalized batch item whose parsed output has the type of the `OutputSpec%601` passed to
`GetBatchResults`. Maps to `BatchItemResult<OUTPUT>`.

**Type parameters**

| Name | Description |
| --- | --- |
| `TOutput` | The type of the parsed output. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchItemResult](/api/Vercel.AI.Operations/BatchItemResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L514)

## Properties

<a id="Vercel_AI_Operations_BatchItemResult_1_Output_"></a>
<a id="Vercel_AI_Operations_BatchItemResult_1_Output"></a>

### Output


```csharp
public TOutput? Output { get; set; }
```


The parsed output of a successful text item. Not set when [OutputError](/api/Vercel.AI.Operations/BatchItemResult/#Vercel_AI_Operations_BatchItemResult_OutputError) is set.
Maps to `output`.

**Value:** `TOutput`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L520)
