---
title: "DecideTelemetry"
description: "Telemetry callbacks. Generic onStart and onEnd are not invoked."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecideTelemetry"
  hidden: true
---

<a id="Vercel_AI_Operations_DecideTelemetry"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecideTelemetry
```


Telemetry callbacks. Generic `onStart` and `onEnd` are not invoked.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluateTelemetry](/api/Vercel.AI.Operations/EvaluateTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L363)

## Properties

<a id="Vercel_AI_Operations_DecideTelemetry_FunctionId_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; set; }
```


Copied onto telemetry events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L414)

<a id="Vercel_AI_Operations_DecideTelemetry_IncludeRuntimeContext_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_IncludeRuntimeContext"></a>

### IncludeRuntimeContext


```csharp
public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }
```


Keys copied into telemetry runtime context. Null yields an empty context.

**Value:** [IReadOnlyDictionary&lt;string, bool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L405)

<a id="Vercel_AI_Operations_DecideTelemetry_IsEnabled_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_IsEnabled"></a>

### IsEnabled


```csharp
public bool IsEnabled { get; set; }
```


When false, telemetry callbacks are skipped.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L417)

<a id="Vercel_AI_Operations_DecideTelemetry_OnDecideEnd_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnDecideEnd"></a>

### OnDecideEnd


```csharp
public Func<DecideEvent, Task>? OnDecideEnd { get; set; }
```


`experimental_onDecideEnd`.

**Value:** [Func&lt;DecideEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L369)

<a id="Vercel_AI_Operations_DecideTelemetry_OnDecideStart_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnDecideStart"></a>

### OnDecideStart


```csharp
public Func<DecideEvent, Task>? OnDecideStart { get; set; }
```


`experimental_onDecideStart`.

**Value:** [Func&lt;DecideEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L366)

<a id="Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallEnd_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallEnd"></a>

### OnDecisionModelCallEnd


```csharp
public Func<DecisionModelEvent, Task>? OnDecisionModelCallEnd { get; set; }
```


`experimental_onDecisionModelCallEnd`.

**Value:** [Func&lt;DecisionModelEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L375)

<a id="Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallStart_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallStart"></a>

### OnDecisionModelCallStart


```csharp
public Func<DecisionModelEvent, Task>? OnDecisionModelCallStart { get; set; }
```


`experimental_onDecisionModelCallStart`.

**Value:** [Func&lt;DecisionModelEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L372)

<a id="Vercel_AI_Operations_DecideTelemetry_OnError_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnError"></a>

### OnError


```csharp
public Func<string, Exception, Task>? OnError { get; set; }
```


`onError`.

**Value:** [Func&lt;string, Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L402)

<a id="Vercel_AI_Operations_DecideTelemetry_OnEvaluateEnd_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnEvaluateEnd"></a>

### OnEvaluateEnd


```csharp
[Obsolete("Use OnDecideEnd instead.")]
public Func<EvaluateEvent, Task>? OnEvaluateEnd { get; set; }
```


Deprecated alias of [OnDecideEnd](#Vercel_AI_Operations_DecideTelemetry_OnDecideEnd). It runs only when [OnDecideEnd](#Vercel_AI_Operations_DecideTelemetry_OnDecideEnd) is not set.

**Value:** [Func&lt;EvaluateEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L382)

<a id="Vercel_AI_Operations_DecideTelemetry_OnEvaluateStart_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnEvaluateStart"></a>

### OnEvaluateStart


```csharp
[Obsolete("Use OnDecideStart instead.")]
public Func<EvaluateEvent, Task>? OnEvaluateStart { get; set; }
```


Deprecated alias of [OnDecideStart](#Vercel_AI_Operations_DecideTelemetry_OnDecideStart). It runs only when [OnDecideStart](#Vercel_AI_Operations_DecideTelemetry_OnDecideStart) is not set.

**Value:** [Func&lt;EvaluateEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L378)

<a id="Vercel_AI_Operations_DecideTelemetry_OnModelEnd_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnModelEnd"></a>

### OnModelEnd


```csharp
[Obsolete("Use OnDecisionModelCallEnd instead.")]
public Func<EvaluateModelEvent, Task>? OnModelEnd { get; set; }
```


Deprecated alias of [OnDecisionModelCallEnd](#Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallEnd). It runs only when [OnDecisionModelCallEnd](#Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallEnd) is not set.

**Value:** [Func&lt;EvaluateModelEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L390)

<a id="Vercel_AI_Operations_DecideTelemetry_OnModelStart_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_OnModelStart"></a>

### OnModelStart


```csharp
[Obsolete("Use OnDecisionModelCallStart instead.")]
public Func<EvaluateModelEvent, Task>? OnModelStart { get; set; }
```


Deprecated alias of [OnDecisionModelCallStart](#Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallStart). It runs only when [OnDecisionModelCallStart](#Vercel_AI_Operations_DecideTelemetry_OnDecisionModelCallStart) is not set.

**Value:** [Func&lt;EvaluateModelEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L386)

<a id="Vercel_AI_Operations_DecideTelemetry_RecordInputs_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; set; }
```


Copied onto telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L408)

<a id="Vercel_AI_Operations_DecideTelemetry_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_DecideTelemetry_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; set; }
```


Copied onto telemetry events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L411)
