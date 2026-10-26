---
title: "StepPerformance"
description: "Performance metrics for one step. Maps to StepResultPerformance."
editUrl: false
prev: false
next: false
sidebar:
  label: "StepPerformance"
  hidden: true
---

<a id="Vercel_AI_StepPerformance"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StepPerformance
```


Performance metrics for one step. Maps to `StepResultPerformance`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1391)

## Properties

<a id="Vercel_AI_StepPerformance_EffectiveOutputTokensPerSecond_"></a>
<a id="Vercel_AI_StepPerformance_EffectiveOutputTokensPerSecond"></a>

### EffectiveOutputTokensPerSecond


```csharp
public double EffectiveOutputTokensPerSecond { get; init; }
```


Effective output tokens per second over the full model response.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1397)

<a id="Vercel_AI_StepPerformance_EffectiveTotalTokensPerSecond_"></a>
<a id="Vercel_AI_StepPerformance_EffectiveTotalTokensPerSecond"></a>

### EffectiveTotalTokensPerSecond


```csharp
public double EffectiveTotalTokensPerSecond { get; init; }
```


Effective input and output tokens per second over the full model response.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1406)

<a id="Vercel_AI_StepPerformance_Empty_"></a>
<a id="Vercel_AI_StepPerformance_Empty"></a>

### Empty


```csharp
public static StepPerformance Empty { get; }
```


A step with every metric at zero.

**Value:** [StepPerformance](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1394)

<a id="Vercel_AI_StepPerformance_InputTokensPerSecond_"></a>
<a id="Vercel_AI_StepPerformance_InputTokensPerSecond"></a>

### InputTokensPerSecond


```csharp
public double? InputTokensPerSecond { get; init; }
```


Input tokens per second before the first output chunk. Only set for streaming steps.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1403)

<a id="Vercel_AI_StepPerformance_OutputTokensPerSecond_"></a>
<a id="Vercel_AI_StepPerformance_OutputTokensPerSecond"></a>

### OutputTokensPerSecond


```csharp
public double? OutputTokensPerSecond { get; init; }
```


Output tokens per second after the first output chunk. Only set for streaming steps.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1400)

<a id="Vercel_AI_StepPerformance_ResponseTimeMs_"></a>
<a id="Vercel_AI_StepPerformance_ResponseTimeMs"></a>

### ResponseTimeMs


```csharp
public double ResponseTimeMs { get; init; }
```


Time spent waiting for the model response in milliseconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1412)

<a id="Vercel_AI_StepPerformance_StepTimeMs_"></a>
<a id="Vercel_AI_StepPerformance_StepTimeMs"></a>

### StepTimeMs


```csharp
public double StepTimeMs { get; init; }
```


Total time spent on the step in milliseconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1409)

<a id="Vercel_AI_StepPerformance_TimeBetweenOutputChunksMs_"></a>
<a id="Vercel_AI_StepPerformance_TimeBetweenOutputChunksMs"></a>

### TimeBetweenOutputChunksMs


```csharp
public OutputChunkTimingStats? TimeBetweenOutputChunksMs { get; init; }
```


Gaps between output chunks. Only set for streaming steps with at least two output chunks.

**Value:** [OutputChunkTimingStats](/api/Vercel.AI/OutputChunkTimingStats/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1421)

<a id="Vercel_AI_StepPerformance_TimeToFirstOutputMs_"></a>
<a id="Vercel_AI_StepPerformance_TimeToFirstOutputMs"></a>

### TimeToFirstOutputMs


```csharp
public double? TimeToFirstOutputMs { get; init; }
```


Time until the first generated output chunk in milliseconds. Only set for streaming steps.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1418)

<a id="Vercel_AI_StepPerformance_ToolExecutionMs_"></a>
<a id="Vercel_AI_StepPerformance_ToolExecutionMs"></a>

### ToolExecutionMs


```csharp
public IReadOnlyDictionary<string, double> ToolExecutionMs { get; init; }
```


Time spent executing each client-side tool call in milliseconds, keyed by tool call id.

**Value:** [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1415)
