---
title: "ExecuteLanguageModelCallOptions<T>"
description: "The options of ExecuteLanguageModelCall: the model-call start event as context, the call id and the function that performs the model call. Maps to Partial<La..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExecuteLanguageModelCallOptions<T>"
  hidden: true
---

<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ExecuteLanguageModelCallOptions<T>
```


The options of [ExecuteLanguageModelCall](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_ExecuteLanguageModelCall): the model-call start event as context, the call id and the
function that performs the model call. Maps to `Partial<LanguageModelCallStartEvent> & { callId, execute }`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L178)

## Constructors

<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1__ctor_"></a>
<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1__ctor_Vercel_AI_LanguageModelCallStartEvent_System_Func_System_Threading_Tasks_Task__0___"></a>

### `ExecuteLanguageModelCallOptions(LanguageModelCallStartEvent, Func<Task<T>>)`


```csharp
public ExecuteLanguageModelCallOptions(LanguageModelCallStartEvent @event, Func<Task<T>> execute)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `event` | [LanguageModelCallStartEvent](/api/Vercel.AI/LanguageModelCallStartEvent/) |  |
| `execute` | [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L181)

## Properties

<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1_CallId_"></a>
<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Id of the model call. Maps to `callId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L194)

<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1_Event_"></a>
<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1_Event"></a>

### Event


```csharp
public LanguageModelCallStartEvent Event { get; }
```


The start event of the model call, as the integrations see it.

**Value:** [LanguageModelCallStartEvent](/api/Vercel.AI/LanguageModelCallStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L191)

<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1_Execute_"></a>
<a id="Vercel_AI_ExecuteLanguageModelCallOptions_1_Execute"></a>

### Execute


```csharp
public Func<Task<T>> Execute { get; }
```


Performs the model call. Maps to `execute`.

**Value:** [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/Telemetry.cs#L197)
