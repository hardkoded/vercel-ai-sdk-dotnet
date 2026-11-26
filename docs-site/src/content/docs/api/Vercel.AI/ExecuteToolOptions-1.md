---
title: "ExecuteToolOptions<T>"
description: "The options of ExecuteTool: the tool-execution start event as context, the ids and the function that runs the tool. Maps to Partial<ToolExecutionStartEvent>..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExecuteToolOptions<T>"
  hidden: true
---

<a id="Vercel_AI_ExecuteToolOptions_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ExecuteToolOptions<T>
```


The options of [ExecuteTool](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_ExecuteTool): the tool-execution start event as context, the ids and the function that
runs the tool. Maps to `Partial<ToolExecutionStartEvent> & { callId, toolCallId, execute }`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L204)

## Constructors

<a id="Vercel_AI_ExecuteToolOptions_1__ctor_"></a>
<a id="Vercel_AI_ExecuteToolOptions_1__ctor_Vercel_AI_ToolExecutionStartEvent_System_Func_System_Threading_Tasks_Task__0___"></a>

### `ExecuteToolOptions(ToolExecutionStartEvent, Func<Task<T>>)`


```csharp
public ExecuteToolOptions(ToolExecutionStartEvent @event, Func<Task<T>> execute)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `event` | [ToolExecutionStartEvent](/api/Vercel.AI/ToolExecutionStartEvent/) |  |
| `execute` | [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L207)

## Properties

<a id="Vercel_AI_ExecuteToolOptions_1_CallId_"></a>
<a id="Vercel_AI_ExecuteToolOptions_1_CallId"></a>

### CallId


```csharp
public string? CallId { get; }
```


Id of the generation call. Maps to `callId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L217)

<a id="Vercel_AI_ExecuteToolOptions_1_Event_"></a>
<a id="Vercel_AI_ExecuteToolOptions_1_Event"></a>

### Event


```csharp
public ToolExecutionStartEvent Event { get; }
```


The start event of the tool execution, as the integrations see it.

**Value:** [ToolExecutionStartEvent](/api/Vercel.AI/ToolExecutionStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L214)

<a id="Vercel_AI_ExecuteToolOptions_1_Execute_"></a>
<a id="Vercel_AI_ExecuteToolOptions_1_Execute"></a>

### Execute


```csharp
public Func<Task<T>> Execute { get; }
```


Runs the tool. Maps to `execute`.

**Value:** [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L223)

<a id="Vercel_AI_ExecuteToolOptions_1_ToolCallId_"></a>
<a id="Vercel_AI_ExecuteToolOptions_1_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call. Maps to `toolCallId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L220)
