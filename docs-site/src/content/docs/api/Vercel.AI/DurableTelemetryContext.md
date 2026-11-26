---
title: "DurableTelemetryContext"
description: "Serializable tracing identity shared by the processes of a durable run. Maps to DurableTelemetryContext in telemetry/telemetry.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "DurableTelemetryContext"
  hidden: true
---

<a id="Vercel_AI_DurableTelemetryContext"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class DurableTelemetryContext
```


Serializable tracing identity shared by the processes of a durable run. Maps to `DurableTelemetryContext` in `telemetry/telemetry.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L8)

## Properties

<a id="Vercel_AI_DurableTelemetryContext_ParentSpanContext_"></a>
<a id="Vercel_AI_DurableTelemetryContext_ParentSpanContext"></a>

### ParentSpanContext


```csharp
public DurableParentSpanContext? ParentSpanContext { get; init; }
```


The span the run was started under, when there was one. Maps to `parentSpanContext`.

**Value:** [DurableParentSpanContext](/api/Vercel.AI/DurableParentSpanContext/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L32)

<a id="Vercel_AI_DurableTelemetryContext_RootAttributes_"></a>
<a id="Vercel_AI_DurableTelemetryContext_RootAttributes"></a>

### RootAttributes


```csharp
public IReadOnlyDictionary<string, object?>? RootAttributes { get; init; }
```


The integration's root-span attributes, reused when the run is reported. Maps to `rootAttributes`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L20)

<a id="Vercel_AI_DurableTelemetryContext_RootSpanId_"></a>
<a id="Vercel_AI_DurableTelemetryContext_RootSpanId"></a>

### RootSpanId


```csharp
public string RootSpanId { get; init; }
```


The span id of the run's root span, as 16 lowercase hex characters. Maps to `rootSpanId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L14)

<a id="Vercel_AI_DurableTelemetryContext_RootSpanName_"></a>
<a id="Vercel_AI_DurableTelemetryContext_RootSpanName"></a>

### RootSpanName


```csharp
public string? RootSpanName { get; init; }
```


The integration's root-span name, reused when the run is reported. Maps to `rootSpanName`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L17)

<a id="Vercel_AI_DurableTelemetryContext_SamplingDecision_"></a>
<a id="Vercel_AI_DurableTelemetryContext_SamplingDecision"></a>

### SamplingDecision


```csharp
public int SamplingDecision { get; init; }
```


The sampling decision: 0 not recorded, 1 recorded, 2 recorded and sampled. Maps to `samplingDecision`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L29)

<a id="Vercel_AI_DurableTelemetryContext_TraceFlags_"></a>
<a id="Vercel_AI_DurableTelemetryContext_TraceFlags"></a>

### TraceFlags


```csharp
public int TraceFlags { get; init; }
```


The W3C trace flags. Maps to `traceFlags`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L23)

<a id="Vercel_AI_DurableTelemetryContext_TraceId_"></a>
<a id="Vercel_AI_DurableTelemetryContext_TraceId"></a>

### TraceId


```csharp
public string TraceId { get; init; }
```


The trace id, as 32 lowercase hex characters. Maps to `traceId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L11)

<a id="Vercel_AI_DurableTelemetryContext_TraceState_"></a>
<a id="Vercel_AI_DurableTelemetryContext_TraceState"></a>

### TraceState


```csharp
public string? TraceState { get; init; }
```


The serialized W3C trace state. Maps to `traceState`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L26)
