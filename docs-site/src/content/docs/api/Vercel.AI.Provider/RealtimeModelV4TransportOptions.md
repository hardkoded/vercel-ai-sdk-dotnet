---
title: "RealtimeModelV4TransportOptions"
description: "Callbacks for a provider-owned transport. Maps to RealtimeModelV4TransportOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4TransportOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4TransportOptions
```


Callbacks for a provider-owned transport. Maps to `RealtimeModelV4TransportOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L15)

## Constructors

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions__ctor_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions__ctor_System_Func_Vercel_AI_Provider_RealtimeModelV4ServerEvent_System_Threading_Tasks_Task__System_Action_System_Exception__System_Action_System_Exception_System_Threading_Tasks_Task__System_Action_System_Exception__System_Action_System_Boolean__System_Action_System_Boolean__"></a>

### `RealtimeModelV4TransportOptions(Func<RealtimeModelV4ServerEvent, Task>, Action<Exception>, Action<Exception, Task?>, Action<Exception?>, Action<bool>, Action<bool>)`


```csharp
public RealtimeModelV4TransportOptions(Func<RealtimeModelV4ServerEvent, Task> onEvent, Action<Exception> onError, Action<Exception, Task?> onFatalError, Action<Exception?> onClose, Action<bool> onCapturing, Action<bool> onPlaying)
```


Creates transport options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `onEvent` | [Func&lt;RealtimeModelV4ServerEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `onError` | [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onFatalError` | [Action&lt;Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.action-2) |  |
| `onClose` | [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onCapturing` | [Action&lt;bool&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onPlaying` | [Action&lt;bool&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L18)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnCapturing_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnCapturing"></a>

### OnCapturing


```csharp
public Action<bool> OnCapturing { get; }
```


Called when local capture starts or stops. Maps to `onCapturing`.

**Value:** [Action&lt;bool&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L48)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnClose_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnClose"></a>

### OnClose


```csharp
public Action<Exception?> OnClose { get; }
```


Called when the transport closes, with the error that closed it. Maps to `onClose`.

**Value:** [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L45)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnError_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnError"></a>

### OnError


```csharp
public Action<Exception> OnError { get; }
```


Called with a recoverable error. Maps to `onError`.

**Value:** [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L39)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnEvent_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnEvent"></a>

### OnEvent


```csharp
public Func<RealtimeModelV4ServerEvent, Task> OnEvent { get; }
```


Called with each normalized server event. Maps to `onEvent`.

**Value:** [Func&lt;RealtimeModelV4ServerEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L36)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnFatalError_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnFatalError"></a>

### OnFatalError


```csharp
public Action<Exception, Task?> OnFatalError { get; }
```


Called with an unrecoverable error and an optional drain task. Maps to `onFatalError`.

**Value:** [Action&lt;Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.action-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L42)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnPlaying_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportOptions_OnPlaying"></a>

### OnPlaying


```csharp
public Action<bool> OnPlaying { get; }
```


Called when remote playback starts or stops. Maps to `onPlaying`.

**Value:** [Action&lt;bool&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L51)
