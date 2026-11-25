---
title: "BatchModelIds"
description: "Model IDs accepted by each batch modality. Maps to BatchV4ModelIds. C# has no string-literal union types, so a batch API declares the closed set at runtime a..."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchModelIds"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchModelIds"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchModelIds
```


Model IDs accepted by each batch modality. Maps to `BatchV4ModelIds`. C# has no string-literal union types, so a
batch API declares the closed set at runtime and `CancellationToken)` checks every request against it.
A null list leaves that modality open.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L850)

## Properties

<a id="Vercel_AI_Operations_BatchModelIds_Image_"></a>
<a id="Vercel_AI_Operations_BatchModelIds_Image"></a>

### Image


```csharp
public IReadOnlyList<string>? Image { get; set; }
```


Model IDs accepted by image requests. Null accepts any id.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L856)

<a id="Vercel_AI_Operations_BatchModelIds_Text_"></a>
<a id="Vercel_AI_Operations_BatchModelIds_Text"></a>

### Text


```csharp
public IReadOnlyList<string>? Text { get; set; }
```


Model IDs accepted by text requests. Null accepts any id.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L853)
