---
title: "DurableRequestedIds"
description: "The ids a span of a durable run must start with. Maps to the ids argument of OpenTelemetryIdGenerator.withIds."
editUrl: false
prev: false
next: false
sidebar:
  label: "DurableRequestedIds"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class DurableRequestedIds
```


The ids a span of a durable run must start with. Maps to the `ids` argument of `OpenTelemetryIdGenerator.withIds`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L42)

## Properties

<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_SamplingDecision_"></a>
<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_SamplingDecision"></a>

### SamplingDecision


```csharp
public int? SamplingDecision { get; init; }
```


Maps to `samplingDecision`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L54)

<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_SpanId_"></a>
<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_SpanId"></a>

### SpanId


```csharp
public string? SpanId { get; init; }
```


Maps to `spanId`. .NET gives every [Activity](https://learn.microsoft.com/dotnet/api/system.diagnostics.activity) a random span id, so the port cannot honor it. See
[OpenTelemetryIdGenerator](/api/Vercel.AI.OpenTelemetry/OpenTelemetryIdGenerator/).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L51)

<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_TraceId_"></a>
<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_TraceId"></a>

### TraceId


```csharp
public string? TraceId { get; init; }
```


Maps to `traceId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L45)

<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_TraceState_"></a>
<a id="Vercel_AI_OpenTelemetry_DurableRequestedIds_TraceState"></a>

### TraceState


```csharp
public string? TraceState { get; init; }
```


Maps to `traceState`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L57)
