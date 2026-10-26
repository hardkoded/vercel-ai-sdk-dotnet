---
title: "OutputChunkTimingStats"
description: "Timing statistics for the gaps between generated output chunks, in milliseconds."
editUrl: false
prev: false
next: false
sidebar:
  label: "OutputChunkTimingStats"
  hidden: true
---

<a id="Vercel_AI_OutputChunkTimingStats"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class OutputChunkTimingStats
```


Timing statistics for the gaps between generated output chunks, in milliseconds.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1358)

## Constructors

<a id="Vercel_AI_OutputChunkTimingStats__ctor_"></a>
<a id="Vercel_AI_OutputChunkTimingStats__ctor_System_Double_System_Double_System_Double_System_Double_System_Double_System_Double_"></a>

### OutputChunkTimingStats(double, double, double, double, double, double)


```csharp
public OutputChunkTimingStats(double min, double max, double avg, double median, double p10, double p90)
```


Creates the statistics.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `min` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `max` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `avg` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `median` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `p10` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `p90` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1361)

## Properties

<a id="Vercel_AI_OutputChunkTimingStats_Avg_"></a>
<a id="Vercel_AI_OutputChunkTimingStats_Avg"></a>

### Avg


```csharp
public double Avg { get; }
```


Average gap.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1378)

<a id="Vercel_AI_OutputChunkTimingStats_Max_"></a>
<a id="Vercel_AI_OutputChunkTimingStats_Max"></a>

### Max


```csharp
public double Max { get; }
```


Largest gap.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1375)

<a id="Vercel_AI_OutputChunkTimingStats_Median_"></a>
<a id="Vercel_AI_OutputChunkTimingStats_Median"></a>

### Median


```csharp
public double Median { get; }
```


Median gap.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1381)

<a id="Vercel_AI_OutputChunkTimingStats_Min_"></a>
<a id="Vercel_AI_OutputChunkTimingStats_Min"></a>

### Min


```csharp
public double Min { get; }
```


Smallest gap.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1372)

<a id="Vercel_AI_OutputChunkTimingStats_P10_"></a>
<a id="Vercel_AI_OutputChunkTimingStats_P10"></a>

### P10


```csharp
public double P10 { get; }
```


10th percentile gap.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1384)

<a id="Vercel_AI_OutputChunkTimingStats_P90_"></a>
<a id="Vercel_AI_OutputChunkTimingStats_P90"></a>

### P90


```csharp
public double P90 { get; }
```


90th percentile gap.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1387)
