---
title: "DurableSampler"
description: "A sampler for Sampler. Maps to the Sampler type of durable-span-ids.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "DurableSampler"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_DurableSampler"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public delegate DurableSamplingResult DurableSampler(ActivityContext parentContext, string traceId, string spanName, ActivityKind spanKind, IReadOnlyDictionary<string, object?> attributes)
```


A sampler for [Sampler](/api/Vercel.AI.OpenTelemetry/OpenTelemetryIdGeneratorOptions/#Vercel_AI_OpenTelemetry_OpenTelemetryIdGeneratorOptions_Sampler). Maps to the `Sampler` type of `durable-span-ids.ts`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parentContext` | [ActivityContext](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitycontext) |  |
| `traceId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `spanName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `spanKind` | [ActivityKind](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitykind) |  |
| `attributes` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [DurableSamplingResult](/api/Vercel.AI.OpenTelemetry/DurableSamplingResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L26)
