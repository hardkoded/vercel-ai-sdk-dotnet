---
title: "DurableParentSpanContext"
description: "The span a durable run was started under. Maps to the parentSpanContext of DurableTelemetryContext."
editUrl: false
prev: false
next: false
sidebar:
  label: "DurableParentSpanContext"
  hidden: true
---

<a id="Vercel_AI_DurableParentSpanContext"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class DurableParentSpanContext
```


The span a durable run was started under. Maps to the `parentSpanContext` of `DurableTelemetryContext`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L36)

## Properties

<a id="Vercel_AI_DurableParentSpanContext_SpanId_"></a>
<a id="Vercel_AI_DurableParentSpanContext_SpanId"></a>

### SpanId


```csharp
public string SpanId { get; init; }
```


Maps to `spanId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L42)

<a id="Vercel_AI_DurableParentSpanContext_TraceFlags_"></a>
<a id="Vercel_AI_DurableParentSpanContext_TraceFlags"></a>

### TraceFlags


```csharp
public int TraceFlags { get; init; }
```


Maps to `traceFlags`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L45)

<a id="Vercel_AI_DurableParentSpanContext_TraceId_"></a>
<a id="Vercel_AI_DurableParentSpanContext_TraceId"></a>

### TraceId


```csharp
public string TraceId { get; init; }
```


Maps to `traceId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L39)

<a id="Vercel_AI_DurableParentSpanContext_TraceState_"></a>
<a id="Vercel_AI_DurableParentSpanContext_TraceState"></a>

### TraceState


```csharp
public string? TraceState { get; init; }
```


Maps to `traceState`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L48)
