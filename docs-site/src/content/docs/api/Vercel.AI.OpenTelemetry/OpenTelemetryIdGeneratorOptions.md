---
title: "OpenTelemetryIdGeneratorOptions"
description: "Options of OpenTelemetryIdGenerator. Maps to the constructor options of OpenTelemetryIdGenerator."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenTelemetryIdGeneratorOptions"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGeneratorOptions"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class OpenTelemetryIdGeneratorOptions
```


Options of [OpenTelemetryIdGenerator](/api/Vercel.AI.OpenTelemetry/OpenTelemetryIdGenerator/). Maps to the constructor options of `OpenTelemetryIdGenerator`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L35)

## Properties

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGeneratorOptions_Sampler_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGeneratorOptions_Sampler"></a>

### Sampler


```csharp
public DurableSampler? Sampler { get; init; }
```


The sampler that decides for a durable run. Null uses parent-based sampling with roots enabled. Maps to `sampler`.

**Value:** [DurableSampler](/api/Vercel.AI.OpenTelemetry/DurableSampler/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L38)
