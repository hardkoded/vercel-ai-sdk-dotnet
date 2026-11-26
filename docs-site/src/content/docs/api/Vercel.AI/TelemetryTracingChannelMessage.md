---
title: "TelemetryTracingChannelMessage"
description: "The value a tracing channel subscriber receives. Maps to TelemetryTracingChannelMessage. Result and Error are set before the asyncEnd and error events, like..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetryTracingChannelMessage"
  hidden: true
---

<a id="Vercel_AI_TelemetryTracingChannelMessage"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TelemetryTracingChannelMessage
```


The value a tracing channel subscriber receives. Maps to `TelemetryTracingChannelMessage`. [Result](#Vercel_AI_TelemetryTracingChannelMessage_Result) and
[Error](#Vercel_AI_TelemetryTracingChannelMessage_Error) are set before the `asyncEnd` and `error` events, like the context object of a Node tracing channel.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L94)

## Constructors

<a id="Vercel_AI_TelemetryTracingChannelMessage__ctor_"></a>
<a id="Vercel_AI_TelemetryTracingChannelMessage__ctor_System_String_System_Object_"></a>

### TelemetryTracingChannelMessage(string, object?)


```csharp
public TelemetryTracingChannelMessage(string type, object? @event)
```


Creates a message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `event` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L97)

## Properties

<a id="Vercel_AI_TelemetryTracingChannelMessage_Error_"></a>
<a id="Vercel_AI_TelemetryTracingChannelMessage_Error"></a>

### Error


```csharp
public Exception? Error { get; set; }
```


The error of the operation. Set on `error`.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L120)

<a id="Vercel_AI_TelemetryTracingChannelMessage_Event_"></a>
<a id="Vercel_AI_TelemetryTracingChannelMessage_Event"></a>

### Event


```csharp
public object? Event { get; }
```


The telemetry event of the span, already filtered and redacted.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L114)

<a id="Vercel_AI_TelemetryTracingChannelMessage_Result_"></a>
<a id="Vercel_AI_TelemetryTracingChannelMessage_Result"></a>

### Result


```csharp
public object? Result { get; set; }
```


The result of the operation. Set on `asyncEnd`, and only when inputs and outputs are recorded.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L117)

<a id="Vercel_AI_TelemetryTracingChannelMessage_Type_"></a>
<a id="Vercel_AI_TelemetryTracingChannelMessage_Type"></a>

### Type


```csharp
public string Type { get; }
```


The kind of span. One of the [TelemetryTracingEventType](/api/Vercel.AI/TelemetryTracingEventType/) values.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L111)
