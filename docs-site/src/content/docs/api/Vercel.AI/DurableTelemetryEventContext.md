---
title: "DurableTelemetryEventContext"
description: "Identifies live model events and the run events reported after completion. Maps to DurableTelemetryEventContext."
editUrl: false
prev: false
next: false
sidebar:
  label: "DurableTelemetryEventContext"
  hidden: true
---

<a id="Vercel_AI_DurableTelemetryEventContext"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class DurableTelemetryEventContext
```


Identifies live model events and the run events reported after completion. Maps to `DurableTelemetryEventContext`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L62)

## Properties

<a id="Vercel_AI_DurableTelemetryEventContext_Headers_"></a>
<a id="Vercel_AI_DurableTelemetryEventContext_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; init; }
```


Maps to `headers`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L77)

<a id="Vercel_AI_DurableTelemetryEventContext_Phase_"></a>
<a id="Vercel_AI_DurableTelemetryEventContext_Phase"></a>

### Phase


```csharp
public DurableTelemetryPhase Phase { get; init; }
```


Maps to `phase`.

**Value:** [DurableTelemetryPhase](/api/Vercel.AI/DurableTelemetryPhase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L71)

<a id="Vercel_AI_DurableTelemetryEventContext_Run_"></a>
<a id="Vercel_AI_DurableTelemetryEventContext_Run"></a>

### Run


```csharp
public DurableTelemetryContext Run { get; init; }
```


The tracing identity of the run. Maps to `run`.

**Value:** [DurableTelemetryContext](/api/Vercel.AI/DurableTelemetryContext/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L65)

<a id="Vercel_AI_DurableTelemetryEventContext_RuntimeContext_"></a>
<a id="Vercel_AI_DurableTelemetryEventContext_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; init; }
```


Context that was already filtered before crossing the step boundary. Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L74)

<a id="Vercel_AI_DurableTelemetryEventContext_StepNumber_"></a>
<a id="Vercel_AI_DurableTelemetryEventContext_StepNumber"></a>

### StepNumber


```csharp
public int? StepNumber { get; init; }
```


The step the event belongs to. Maps to `stepNumber`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/DurableTelemetryContext.cs#L68)
