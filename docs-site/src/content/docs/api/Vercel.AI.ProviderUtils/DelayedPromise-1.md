---
title: "DelayedPromise<T>"
description: "A task that is completed from outside. Maps to DelayedPromise."
editUrl: false
prev: false
next: false
sidebar:
  label: "DelayedPromise<T>"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_DelayedPromise_1"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class DelayedPromise<T>
```


A task that is completed from outside. Maps to `DelayedPromise`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DelayedPromise.cs#L8)

## Properties

<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_Promise_"></a>
<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_Promise"></a>

### Promise


```csharp
public Task<T> Promise { get; }
```


The task that completes on `Resolve(%600)` or faults on `Exception)`.

**Value:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DelayedPromise.cs#L13)

## Methods

<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_IsPending_"></a>
<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_IsPending"></a>

### IsPending()


```csharp
public bool IsPending()
```


True before resolve or reject.

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DelayedPromise.cs#L43)

<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_IsRejected_"></a>
<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_IsRejected"></a>

### IsRejected()


```csharp
public bool IsRejected()
```


True after `Exception)`.

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DelayedPromise.cs#L37)

<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_IsResolved_"></a>
<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_IsResolved"></a>

### IsResolved()


```csharp
public bool IsResolved()
```


True after `Resolve(%600)`.

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DelayedPromise.cs#L31)

<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_Reject_"></a>
<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_Reject_System_Exception_"></a>

### Reject(Exception)


```csharp
public void Reject(Exception error)
```


Faults `Promise` with &lt;code class="paramref"&gt;error&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DelayedPromise.cs#L25)

<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_Resolve_"></a>
<a id="Vercel_AI_ProviderUtils_DelayedPromise_1_Resolve__0_"></a>

### Resolve(T)


```csharp
public void Resolve(T value)
```


Completes `Promise` with &lt;code class="paramref"&gt;value&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/DelayedPromise.cs#L19)
