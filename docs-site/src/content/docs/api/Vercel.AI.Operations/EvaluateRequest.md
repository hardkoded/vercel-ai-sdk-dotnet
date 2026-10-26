---
title: "EvaluateRequest"
description: "Deprecated alias of DecideRequest."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluateRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluateRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecideRequest instead.")]
public sealed class EvaluateRequest : OperationRequest
```


Deprecated alias of [DecideRequest](/api/Vercel.AI.Operations/DecideRequest/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L192)

## Properties

<a id="Vercel_AI_Operations_EvaluateRequest_Model_"></a>
<a id="Vercel_AI_Operations_EvaluateRequest_Model"></a>

### Model


```csharp
public IEvaluationCaller? Model { get; set; }
```


Decision model. A model that implements only [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/) still runs.

**Value:** [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L196)

<a id="Vercel_AI_Operations_EvaluateRequest_OnEnd_"></a>
<a id="Vercel_AI_Operations_EvaluateRequest_OnEnd"></a>

### OnEnd


```csharp
public Func<EvaluateEvent, Task>? OnEnd { get; set; }
```


Called when the decision completes.

**Value:** [Func&lt;EvaluateEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L208)

<a id="Vercel_AI_Operations_EvaluateRequest_OnStart_"></a>
<a id="Vercel_AI_Operations_EvaluateRequest_OnStart"></a>

### OnStart


```csharp
public Func<EvaluateEvent, Task>? OnStart { get; set; }
```


Called when the decision begins.

**Value:** [Func&lt;EvaluateEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L205)

<a id="Vercel_AI_Operations_EvaluateRequest_Questions_"></a>
<a id="Vercel_AI_Operations_EvaluateRequest_Questions"></a>

### Questions


```csharp
public IReadOnlyDictionary<string, EvaluationQuestion>? Questions { get; set; }
```


Nonempty question map.

**Value:** [IReadOnlyDictionary&lt;string, EvaluationQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L202)

<a id="Vercel_AI_Operations_EvaluateRequest_State_"></a>
<a id="Vercel_AI_Operations_EvaluateRequest_State"></a>

### State


```csharp
public DecisionState? State { get; set; }
```


Shared state. See [State](/api/Vercel.AI.Operations/DecideRequest/#Vercel_AI_Operations_DecideRequest_State).

**Value:** [DecisionState](/api/Vercel.AI.Operations/DecisionState/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L199)

<a id="Vercel_AI_Operations_EvaluateRequest_Telemetry_"></a>
<a id="Vercel_AI_Operations_EvaluateRequest_Telemetry"></a>

### Telemetry


```csharp
public EvaluateTelemetry? Telemetry { get; set; }
```


Telemetry.

**Value:** [EvaluateTelemetry](/api/Vercel.AI.Operations/EvaluateTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L211)
