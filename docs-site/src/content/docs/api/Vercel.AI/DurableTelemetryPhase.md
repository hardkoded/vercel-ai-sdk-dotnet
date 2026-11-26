---
title: "DurableTelemetryPhase"
description: "Whether a durable event is reported live from a step or replayed after the run completed. Maps to the phase of DurableTelemetryEventContext."
editUrl: false
prev: false
next: false
sidebar:
  label: "DurableTelemetryPhase"
  hidden: true
---

<a id="Vercel_AI_DurableTelemetryPhase"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public enum DurableTelemetryPhase
```


Whether a durable event is reported live from a step or replayed after the run completed. Maps to the `phase` of `DurableTelemetryEventContext`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L52)

## Fields

<a id="Vercel_AI_DurableTelemetryPhase_Live"></a>

### Live


```csharp
Live = 0
```


Reported live from a durable step. Maps to `live`.

**Value:** [DurableTelemetryPhase](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L55)

<a id="Vercel_AI_DurableTelemetryPhase_Replay"></a>

### Replay


```csharp
Replay = 1
```


Reported after the run completed. Maps to `replay`.

**Value:** [DurableTelemetryPhase](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L58)
