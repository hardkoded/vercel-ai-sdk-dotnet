---
title: "MockEmbedResult"
description: "Embedding result stored in a scripted list."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockEmbedResult"
  hidden: true
---

<a id="Vercel_AI_Testing_MockEmbedResult"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockEmbedResult
```


Embedding result stored in a scripted list.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L103)

## Constructors

<a id="Vercel_AI_Testing_MockEmbedResult__ctor_"></a>
<a id="Vercel_AI_Testing_MockEmbedResult__ctor_System_Single___"></a>

### MockEmbedResult(float[])


```csharp
public MockEmbedResult(float[] embedding)
```


Creates a result with one embedding.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `embedding` | [float\[\]](https://learn.microsoft.com/dotnet/api/system.single) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L106)

## Properties

<a id="Vercel_AI_Testing_MockEmbedResult_Embeddings_"></a>
<a id="Vercel_AI_Testing_MockEmbedResult_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<float[]> Embeddings { get; }
```


Embeddings, one per input value.

**Value:** [IReadOnlyList&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L112)
