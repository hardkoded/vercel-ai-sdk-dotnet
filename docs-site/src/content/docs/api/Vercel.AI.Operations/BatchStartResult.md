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

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [BatchStatus](/api/Vercel.AI.Operations/BatchStatus/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L516)

## Properties

<a id="Vercel_AI_Operations_BatchStartResult_Id_"></a>
<a id="Vercel_AI_Operations_BatchStartResult_Id"></a>

### Id


```csharp
public string Id { get; set; }
```


Batch id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L522)

<a id="Vercel_AI_Operations_BatchStartResult_Provider_"></a>
<a id="Vercel_AI_Operations_BatchStartResult_Provider"></a>

### Provider


```csharp
public string Provider { get; set; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L525)

<a id="Vercel_AI_Operations_BatchStartResult_Version_"></a>
<a id="Vercel_AI_Operations_BatchStartResult_Version"></a>

### Version


```csharp
public int Version { get; set; }
```


Reference version.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L519)

<a id="Vercel_AI_Operations_BatchStartResult_Warnings_"></a>
<a id="Vercel_AI_Operations_BatchStartResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<BatchRequestWarning> Warnings { get; set; }
```


Warnings.

**Value:** [IReadOnlyList&lt;BatchRequestWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L528)
