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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2314)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2317)

## Properties

<a id="Vercel_AI_StreamAbortContext_CallId_"></a>
<a id="Vercel_AI_StreamAbortContext_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier for this generation call, used to correlate events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2329)

<a id="Vercel_AI_StreamAbortContext_Exception_"></a>
<a id="Vercel_AI_StreamAbortContext_Exception"></a>

### Exception


```csharp
public Exception Exception { get; }
```


The cancellation exception, or the abort reason when it is an exception.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2335)

<a id="Vercel_AI_StreamAbortContext_Steps_"></a>
<a id="Vercel_AI_StreamAbortContext_Steps"></a>

### Steps


```csharp
public IReadOnlyList<StepResult> Steps { get; }
```


Steps that finished before the abort.

**Value:** [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2332)

<a id="Vercel_AI_StreamAbortContext_Timestamp_"></a>
<a id="Vercel_AI_StreamAbortContext_Timestamp"></a>

### Timestamp


```csharp
public long? Timestamp { get; init; }
```


Time at which the event occurred, in milliseconds since the Unix epoch. Set when events are reported after they happened, for example when a durable workflow reports a run that spanned several processes. Integrations use it instead of the current time when present. Maps to `timestamp`.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2338)
