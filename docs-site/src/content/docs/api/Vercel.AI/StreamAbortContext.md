---
title: "StreamAbortContext"
description: "Passed to OnAbort."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamAbortContext"
  hidden: true
---

<a id="Vercel_AI_StreamAbortContext"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamAbortContext
```


Passed to [OnAbort](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_OnAbort).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2246)

## Constructors

<a id="Vercel_AI_StreamAbortContext__ctor_"></a>
<a id="Vercel_AI_StreamAbortContext__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__System_Exception_System_String_"></a>

### `StreamAbortContext(IReadOnlyList<StepResult>, Exception, string)`


```csharp
public StreamAbortContext(IReadOnlyList<StepResult> steps, Exception exception, string callId = "")
```


Creates a context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `exception` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2249)

## Properties

<a id="Vercel_AI_StreamAbortContext_CallId_"></a>
<a id="Vercel_AI_StreamAbortContext_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this generation call, used to correlate events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2261)

<a id="Vercel_AI_StreamAbortContext_Exception_"></a>
<a id="Vercel_AI_StreamAbortContext_Exception"></a>

### Exception


```csharp
public Exception Exception { get; }
```


The cancellation exception, or the abort reason when it is an exception.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2267)

<a id="Vercel_AI_StreamAbortContext_Steps_"></a>
<a id="Vercel_AI_StreamAbortContext_Steps"></a>

### Steps


```csharp
public IReadOnlyList<StepResult> Steps { get; }
```


Steps that finished before the abort.

**Value:** [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2264)
