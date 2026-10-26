---
title: "ModelPartStream"
description: "Model stream that stays open until Cancel() when Completes is false."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelPartStream"
  hidden: true
---

<a id="Vercel_AI_Operations_ModelPartStream"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ModelPartStream
```


Model stream that stays open until [Cancel()](#Vercel_AI_Operations_ModelPartStream_Cancel) when [Completes](#Vercel_AI_Operations_ModelPartStream_Completes) is false.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L116)

## Constructors

<a id="Vercel_AI_Operations_ModelPartStream__ctor_"></a>
<a id="Vercel_AI_Operations_ModelPartStream__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ModelStreamPart__System_Boolean_System_Exception_"></a>

### `ModelPartStream(IReadOnlyList<ModelStreamPart>, bool, Exception?)`


```csharp
public ModelPartStream(IReadOnlyList<ModelStreamPart> parts, bool completes = true, Exception? failure = null)
```


Creates a scripted stream.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parts` | [IReadOnlyList&lt;ModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `completes` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `failure` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L122)

## Properties

<a id="Vercel_AI_Operations_ModelPartStream_Cancelled_"></a>
<a id="Vercel_AI_Operations_ModelPartStream_Cancelled"></a>

### Cancelled


```csharp
public bool Cancelled { get; }
```


True after [Cancel()](#Vercel_AI_Operations_ModelPartStream_Cancel).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L142)

<a id="Vercel_AI_Operations_ModelPartStream_Completes_"></a>
<a id="Vercel_AI_Operations_ModelPartStream_Completes"></a>

### Completes


```csharp
public bool Completes { get; }
```


True when the stream ends after [Parts](#Vercel_AI_Operations_ModelPartStream_Parts).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L130)

<a id="Vercel_AI_Operations_ModelPartStream_Failure_"></a>
<a id="Vercel_AI_Operations_ModelPartStream_Failure"></a>

### Failure


```csharp
public Exception? Failure { get; }
```


Error raised after the scripted parts.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L133)

<a id="Vercel_AI_Operations_ModelPartStream_Parts_"></a>
<a id="Vercel_AI_Operations_ModelPartStream_Parts"></a>

### Parts


```csharp
public IReadOnlyList<ModelStreamPart> Parts { get; }
```


Scripted parts.

**Value:** [IReadOnlyList&lt;ModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L136)

## Methods

<a id="Vercel_AI_Operations_ModelPartStream_Cancel_"></a>
<a id="Vercel_AI_Operations_ModelPartStream_Cancel"></a>

### Cancel()


```csharp
public void Cancel()
```


Stops a stream that has not completed.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L145)

<a id="Vercel_AI_Operations_ModelPartStream_ReadAsync_"></a>
<a id="Vercel_AI_Operations_ModelPartStream_ReadAsync_System_Threading_CancellationToken_"></a>

### ReadAsync(CancellationToken)


```csharp
public IAsyncEnumerable<ModelStreamPart> ReadAsync(CancellationToken cancellationToken = default)
```


Reads the scripted parts, then waits when the stream stays open.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;ModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L152)
