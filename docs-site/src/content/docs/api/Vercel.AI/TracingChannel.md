---
title: "TracingChannel"
description: "The telemetry tracing channel. Maps to telemetry/tracing-channel.ts. Upstream publishes on a Node diagnosticschannel tracing channel. The port publishes on a..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TracingChannel"
  hidden: true
---

<a id="Vercel_AI_TracingChannel"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public static class TracingChannel
```


The telemetry tracing channel. Maps to `telemetry/tracing-channel.ts`. Upstream publishes on a Node
`diagnostics_channel` tracing channel. The port publishes on a [DiagnosticListener](https://learn.microsoft.com/dotnet/api/system.diagnostics.diagnosticlistener) with
the same name, one event per sub-channel of the tracing channel: `start`, `end`, `asyncStart`, `asyncEnd` and
`error`. Each event value is a [TelemetryTracingChannelMessage](/api/Vercel.AI/TelemetryTracingChannelMessage/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L13)

## Fields

<a id="Vercel_AI_TracingChannel_AiSdkTelemetryTracingChannel"></a>

### AiSdkTelemetryTracingChannel


```csharp
public const string AiSdkTelemetryTracingChannel = "ai:telemetry"
```


The name of the tracing channel. Maps to `AI_SDK_TELEMETRY_TRACING_CHANNEL`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L16)

## Properties

<a id="Vercel_AI_TracingChannel_Current_"></a>
<a id="Vercel_AI_TracingChannel_Current"></a>

### Current


```csharp
public static TelemetryTracingChannelMessage? Current { get; }
```


The span that is current in the running async flow, or null outside any span. A [DiagnosticListener](https://learn.microsoft.com/dotnet/api/system.diagnostics.diagnosticlistener)
has no store to bind, so this is the port's counterpart of the store that a Node subscriber binds with `bindStore` on the
`start` sub-channel. A `start` subscriber sees the parent span here, because the span becomes current after
`start` is published. Work started inside a span, including awaited and background work, sees that span here. A span whose
operation already finished is skipped, so a step that follows another step has the same parent.

**Value:** [TelemetryTracingChannelMessage](/api/Vercel.AI/TelemetryTracingChannelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L18)
