---
title: "BatchModels"
description: "Default provider used when a batch call omits one."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchModels"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchModels"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class BatchModels
```


Default provider used when a batch call omits one.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L658)

## Properties

<a id="Vercel_AI_Operations_BatchModels_Default_"></a>
<a id="Vercel_AI_Operations_BatchModels_Default"></a>

### Default


```csharp
public static Func<IBatchProvider?>? Default { get; set; }
```


Resolves the global default provider.

**Value:** [Func&lt;IBatchProvider&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L661)
