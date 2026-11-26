---
title: "Resolvable<T>"
description: "A value, a task, or a synchronous or asynchronous callback that produces the value. Maps to the upstream Resolvable<T> of @ai-sdk/provider-utils. A callback..."
editUrl: false
prev: false
next: false
sidebar:
  label: "Resolvable<T>"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_Resolvable_1"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class Resolvable<T>
```


A value, a task, or a synchronous or asynchronous callback that produces the value. Maps to the upstream
`Resolvable<T>` of `@ai-sdk/provider-utils`. A callback is called every time the value is
resolved, so a provider setting such as an API key is resolved per request.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` | The resolved value type. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L13)

## Properties

<a id="Vercel_AI_ProviderUtils_Resolvable_1_IsCallback_"></a>
<a id="Vercel_AI_ProviderUtils_Resolvable_1_IsCallback"></a>

### IsCallback


```csharp
public bool IsCallback { get; }
```


True when the value comes from a callback rather than a static value or a task.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L27)

## Methods

<a id="Vercel_AI_ProviderUtils_Resolvable_1_From_"></a>
<a id="Vercel_AI_ProviderUtils_Resolvable_1_From_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task__0___"></a>

### `From(Func<CancellationToken, Task<T>>)`


```csharp
public static Resolvable<T> From(Func<CancellationToken, Task<T>> callback)
```


Wraps an asynchronous callback that receives the request's cancellation token.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callback` | [Func&lt;CancellationToken, Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [Resolvable&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L71)

<a id="Vercel_AI_ProviderUtils_Resolvable_1_From_System_Func_System_Threading_Tasks_Task__0___"></a>

### `From(Func<Task<T>>)`


```csharp
public static Resolvable<T> From(Func<Task<T>> callback)
```


Wraps an asynchronous callback, called every time the value is resolved.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callback` | [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [Resolvable&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L60)

<a id="Vercel_AI_ProviderUtils_Resolvable_1_From_System_Func__0__"></a>

### `From(Func<T>)`


```csharp
public static Resolvable<T> From(Func<T> callback)
```


Wraps a synchronous callback, called every time the value is resolved.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callback` | [Func&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [Resolvable&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L49)

<a id="Vercel_AI_ProviderUtils_Resolvable_1_From_System_Threading_Tasks_Task__0__"></a>

### `From(Task<T>)`


```csharp
public static Resolvable<T> From(Task<T> task)
```


Wraps a task, awaited every time the value is resolved.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `task` | [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1) |  |

**Returns:** [Resolvable&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L45)

<a id="Vercel_AI_ProviderUtils_Resolvable_1_From__0_"></a>

### From(T?)


```csharp
public static Resolvable<T> From(T? value)
```


Wraps a value, which may be null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

**Returns:** [Resolvable&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L42)

<a id="Vercel_AI_ProviderUtils_Resolvable_1_ResolveAsync_"></a>
<a id="Vercel_AI_ProviderUtils_Resolvable_1_ResolveAsync_System_Threading_CancellationToken_"></a>

### ResolveAsync(CancellationToken)


```csharp
public Task<T> ResolveAsync(CancellationToken cancellationToken = default)
```


Resolves the value. Maps to `resolve`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L75)

<a id="Vercel_AI_ProviderUtils_Resolvable_1_ResolveWithAsync_"></a>
<a id="Vercel_AI_ProviderUtils_Resolvable_1_ResolveWithAsync__1_System_Func__0___0__System_Threading_CancellationToken_"></a>

### `ResolveWithAsync<TResult>(Func<T, TResult>, CancellationToken)`


```csharp
public Task<TResult> ResolveWithAsync<TResult>(Func<T, TResult> transform, CancellationToken cancellationToken = default)
```


Resolves the value and transforms it. Maps to `resolveWith`.

**Type parameters**

| Name | Description |
| --- | --- |
| `TResult` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transform` | [Func&lt;T, TResult&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L91)

## Operators

<a id="Vercel_AI_ProviderUtils_Resolvable_1_op_Implicit_"></a>
<a id="Vercel_AI_ProviderUtils_Resolvable_1_op_Implicit_System_Threading_Tasks_Task__0___Vercel_AI_ProviderUtils_Resolvable__0_"></a>

### `implicit operator Resolvable<T>(Task<T>)`


```csharp
public static implicit operator Resolvable<T>(Task<T> task)
```


Wraps a task.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `task` | [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1) |  |

**Returns:** [Resolvable&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L39)

<a id="Vercel_AI_ProviderUtils_Resolvable_1_op_Implicit__0__Vercel_AI_ProviderUtils_Resolvable__0_"></a>

### `implicit operator Resolvable<T>(T?)`


```csharp
public static implicit operator Resolvable<T>(T? value)
```


Wraps a value. A null value is accepted so `ApiKey = maybeNull` compiles without a nullable warning;
it resolves to null, as an unset upstream setting resolves to `undefined`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

**Returns:** [Resolvable&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Resolvable.cs#L36)
