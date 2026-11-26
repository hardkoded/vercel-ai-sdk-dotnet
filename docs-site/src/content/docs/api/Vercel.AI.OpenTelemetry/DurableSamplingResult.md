---
title: "DurableSamplingResult"
description: "The result of a DurableSampler. Maps to the return type of the Sampler of durable-span-ids.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "DurableSamplingResult"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_DurableSamplingResult"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class DurableSamplingResult
```


The result of a [DurableSampler](/api/Vercel.AI.OpenTelemetry/DurableSampler/). Maps to the return type of the `Sampler` of `durable-span-ids.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L13)

## Properties

<a id="Vercel_AI_OpenTelemetry_DurableSamplingResult_Attributes_"></a>
<a id="Vercel_AI_OpenTelemetry_DurableSamplingResult_Attributes"></a>

### Attributes


```csharp
public IReadOnlyDictionary<string, object?>? Attributes { get; init; }
```


Attributes the sampler adds to the span. Maps to `attributes`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L22)

<a id="Vercel_AI_OpenTelemetry_DurableSamplingResult_Decision_"></a>
<a id="Vercel_AI_OpenTelemetry_DurableSamplingResult_Decision"></a>

### Decision


```csharp
public int Decision { get; init; }
```


0 not recorded, 1 recorded, 2 recorded and sampled. Maps to `decision`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L16)

<a id="Vercel_AI_OpenTelemetry_DurableSamplingResult_TraceState_"></a>
<a id="Vercel_AI_OpenTelemetry_DurableSamplingResult_TraceState"></a>

### TraceState


```csharp
public string? TraceState { get; init; }
```


The serialized trace state the sampler wants on the span. Maps to `traceState`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L19)
