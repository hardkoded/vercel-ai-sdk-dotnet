---
title: "PendingPromise<T>"
description: "A promise that can be resolved once."
editUrl: false
prev: false
next: false
sidebar:
  label: "PendingPromise<T>"
  hidden: true
---

<a id="Vercel_AI_Operations_PendingPromise_1"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class PendingPromise<T>
```


A promise that can be resolved once.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L210)

## Properties

<a id="Vercel_AI_Operations_PendingPromise_1_IsPending_"></a>
<a id="Vercel_AI_Operations_PendingPromise_1_IsPending"></a>

### IsPending


```csharp
public bool IsPending { get; }
```


True until `Resolve(%600)` or `Exception)`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L215)

<a id="Vercel_AI_Operations_PendingPromise_1_IsResolved_"></a>
<a id="Vercel_AI_Operations_PendingPromise_1_IsResolved"></a>

### IsResolved


```csharp
public bool IsResolved { get; }
```


True after `Resolve(%600)`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L218)

<a id="Vercel_AI_Operations_PendingPromise_1_Task_"></a>
<a id="Vercel_AI_Operations_PendingPromise_1_Task"></a>

### Task


```csharp
public Task<T> Task { get; }
```


The underlying task.

**Value:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L221)

## Methods

<a id="Vercel_AI_Operations_PendingPromise_1_Reject_"></a>
<a id="Vercel_AI_Operations_PendingPromise_1_Reject_System_Exception_"></a>

### Reject(Exception)


```csharp
public void Reject(Exception error)
```


Rejects the promise when it is still pending.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L240)

<a id="Vercel_AI_Operations_PendingPromise_1_Resolve_"></a>
<a id="Vercel_AI_Operations_PendingPromise_1_Resolve__0_"></a>

### Resolve(T)


```csharp
public void Resolve(T value)
```


Resolves the promise when it is still pending.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L227)
