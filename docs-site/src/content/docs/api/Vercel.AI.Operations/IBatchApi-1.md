---
title: "IBatchApi<TModelIds>"
description: "Batch API whose accepted model IDs have a declared type. Maps to BatchV4<TModelIds>. Derive a type from BatchModelIds to expose the closed model-id sets of a..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IBatchApi<TModelIds>"
  hidden: true
---

<a id="Vercel_AI_Operations_IBatchApi_1"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public interface IBatchApi<out TModelIds> : IBatchApi where TModelIds : BatchModelIds
```


Batch API whose accepted model IDs have a declared type. Maps to `BatchV4<TModelIds>`. Derive a type from
[BatchModelIds](/api/Vercel.AI.Operations/BatchModelIds/) to expose the closed model-id sets of a provider, and implement this interface to hand
callers that type. The runtime check of `CancellationToken)` still applies to every request.

**Type parameters**

| Name | Description |
| --- | --- |
| `TModelIds` | Model IDs accepted for each modality. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L946)
