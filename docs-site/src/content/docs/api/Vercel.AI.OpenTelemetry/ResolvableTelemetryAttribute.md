---
title: "ResolvableTelemetryAttribute"
description: "An attribute whose value is computed only when inputs or outputs are recorded."
editUrl: false
prev: false
next: false
sidebar:
  label: "ResolvableTelemetryAttribute"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class ResolvableTelemetryAttribute
```


An attribute whose value is computed only when inputs or outputs are recorded.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L23)

## Properties

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_IsInput_"></a>
<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_IsInput"></a>

### IsInput


```csharp
public bool IsInput { get; }
```


True when this value is an input.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L37)

## Methods

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Input_"></a>
<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Input_System_Func_System_Object__"></a>

### `Input(Func<object?>)`


```csharp
public static ResolvableTelemetryAttribute Input(Func<object?> resolve)
```


Creates an input resolver.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `resolve` | [Func&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [ResolvableTelemetryAttribute](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L40)

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Input_System_Func_System_Threading_Tasks_Task_System_Object___"></a>

### `Input(Func<Task<object?>>)`


```csharp
public static ResolvableTelemetryAttribute Input(Func<Task<object?>> resolve)
```


Creates an asynchronous input resolver.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `resolve` | [Func&lt;Task&lt;object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [ResolvableTelemetryAttribute](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L62)

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Output_"></a>
<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Output_System_Func_System_Object__"></a>

### `Output(Func<object?>)`


```csharp
public static ResolvableTelemetryAttribute Output(Func<object?> resolve)
```


Creates an output resolver.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `resolve` | [Func&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [ResolvableTelemetryAttribute](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L51)

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Output_System_Func_System_Threading_Tasks_Task_System_Object___"></a>

### `Output(Func<Task<object?>>)`


```csharp
public static ResolvableTelemetryAttribute Output(Func<Task<object?>> resolve)
```


Creates an asynchronous output resolver.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `resolve` | [Func&lt;Task&lt;object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [ResolvableTelemetryAttribute](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L73)

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Resolve_"></a>
<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_Resolve"></a>

### Resolve()


```csharp
public object? Resolve()
```


Resolves the value.

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L84)

<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_ResolveAsync_"></a>
<a id="Vercel_AI_OpenTelemetry_ResolvableTelemetryAttribute_ResolveAsync"></a>

### ResolveAsync()


```csharp
public Task<object?> ResolveAsync()
```


Resolves the value asynchronously.

**Returns:** [Task&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/TelemetryAttributes.cs#L100)
