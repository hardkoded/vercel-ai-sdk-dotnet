---
title: "Rerank"
description: "Reranks documents. Maps to rerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "Rerank"
  hidden: true
---

<a id="Vercel_AI_Operations_Rerank"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class Rerank
```


Reranks documents. Maps to `rerank`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L408)

## Methods

<a id="Vercel_AI_Operations_Rerank_RerankAsync_"></a>
<a id="Vercel_AI_Operations_Rerank_RerankAsync_Vercel_AI_Operations_RerankRequest_System_Threading_CancellationToken_"></a>

### RerankAsync(RerankRequest, CancellationToken)


```csharp
public static Task<RerankResult> RerankAsync(RerankRequest request, CancellationToken cancellationToken = default)
```


Reranks [Documents](/api/Vercel.AI.Operations/RerankRequest/#Vercel_AI_Operations_RerankRequest_Documents).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [RerankRequest](/api/Vercel.AI.Operations/RerankRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RerankResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L411)
