---
title: "SetAbortTimeout"
description: "Schedules an abort when a timeout elapses."
editUrl: false
prev: false
next: false
sidebar:
  label: "SetAbortTimeout"
  hidden: true
---

<a id="Vercel_AI_Util_SetAbortTimeout"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class SetAbortTimeout
```


Schedules an abort when a timeout elapses.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L243)

## Methods

<a id="Vercel_AI_Util_SetAbortTimeout_Schedule_"></a>
<a id="Vercel_AI_Util_SetAbortTimeout_Schedule_Vercel_AI_Util_AbortController_System_String_System_Nullable_System_Int32__System_Action_System_Action_System_Int32__"></a>

### `Schedule(AbortController?, string?, int?, Action<Action, int>?)`


```csharp
public static IDisposable? Schedule(AbortController? abortController, string? label, int? timeoutMs, Action<Action, int>? schedule = null)
```


Aborts &lt;code class="paramref"&gt;abortController&lt;/code&gt; after &lt;code class="paramref"&gt;timeoutMs&lt;/code&gt; with a
[TimeoutError](/api/Vercel.AI.Util/TimeoutError/) whose message includes &lt;code class="paramref"&gt;label&lt;/code&gt;.
Returns `null` when the controller or timeout is missing. Disposing the result cancels the abort.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `abortController` | [AbortController](/api/Vercel.AI.Util/AbortController/) |  |
| `label` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `timeoutMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `schedule` | [Action&lt;Action, int&gt;](https://learn.microsoft.com/dotnet/api/system.action-2) |  |

**Returns:** [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L250)
