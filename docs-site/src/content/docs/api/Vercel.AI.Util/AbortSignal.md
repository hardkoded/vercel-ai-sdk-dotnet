---
title: "AbortSignal"
description: "Abort signal that records the first reason."
editUrl: false
prev: false
next: false
sidebar:
  label: "AbortSignal"
  hidden: true
---

<a id="Vercel_AI_Util_AbortSignal"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class AbortSignal
```


Abort signal that records the first reason.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L24)

## Properties

<a id="Vercel_AI_Util_AbortSignal_Aborted_"></a>
<a id="Vercel_AI_Util_AbortSignal_Aborted"></a>

### Aborted


```csharp
public bool Aborted { get; }
```


Whether [Abort(object)](#Vercel_AI_Util_AbortSignal_Abort_System_Object_) has been called.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L40)

<a id="Vercel_AI_Util_AbortSignal_Reason_"></a>
<a id="Vercel_AI_Util_AbortSignal_Reason"></a>

### Reason


```csharp
public object? Reason { get; }
```


Reason passed to the abort that won.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L43)

## Methods

<a id="Vercel_AI_Util_AbortSignal_Abort_"></a>
<a id="Vercel_AI_Util_AbortSignal_Abort_System_Object_"></a>

### Abort(object?)


```csharp
public void Abort(object? reason)
```


Aborts unless this signal is already aborted. Every handler runs, even when an earlier one throws.
The first handler exception is rethrown after the last handler ran.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reason` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L49)

<a id="Vercel_AI_Util_AbortSignal_AddAbortHandler_"></a>
<a id="Vercel_AI_Util_AbortSignal_AddAbortHandler_System_Action_"></a>

### AddAbortHandler(Action)


```csharp
public void AddAbortHandler(Action handler)
```


Runs &lt;code class="paramref"&gt;handler&lt;/code&gt; when aborted. Runs immediately if already aborted.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `handler` | [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L85)

<a id="Vercel_AI_Util_AbortSignal_RemoveAbortHandler_"></a>
<a id="Vercel_AI_Util_AbortSignal_RemoveAbortHandler_System_Action_"></a>

### RemoveAbortHandler(Action)


```csharp
public void RemoveAbortHandler(Action handler)
```


Removes a handler added by [AddAbortHandler(Action)](#Vercel_AI_Util_AbortSignal_AddAbortHandler_System_Action_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `handler` | [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L112)

<a id="Vercel_AI_Util_AbortSignal_Timeout_"></a>
<a id="Vercel_AI_Util_AbortSignal_Timeout_System_Int32_System_Action_System_Action_System_Int32__"></a>

### `Timeout(int, Action<Action, int>?)`


```csharp
public static AbortSignal Timeout(int timeoutMs, Action<Action, int>? schedule = null)
```


Signal that aborts with [TimeoutError](/api/Vercel.AI.Util/TimeoutError/) after &lt;code class="paramref"&gt;timeoutMs&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timeoutMs` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `schedule` | [Action&lt;Action, int&gt;](https://learn.microsoft.com/dotnet/api/system.action-2) |  |

**Returns:** [AbortSignal](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L121)
