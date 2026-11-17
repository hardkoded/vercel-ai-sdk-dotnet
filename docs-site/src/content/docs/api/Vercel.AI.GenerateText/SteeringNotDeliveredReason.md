---
title: "SteeringNotDeliveredReason"
description: "Why a steering message was not delivered. Maps to SteeringNotDeliveredReason."
editUrl: false
prev: false
next: false
sidebar:
  label: "SteeringNotDeliveredReason"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SteeringNotDeliveredReason"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public enum SteeringNotDeliveredReason
```


Why a steering message was not delivered. Maps to `SteeringNotDeliveredReason`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L15)

## Fields

<a id="Vercel_AI_GenerateText_SteeringNotDeliveredReason_Aborted"></a>

### Aborted


```csharp
Aborted = 4
```


The run was aborted (`aborted`).

**Value:** [SteeringNotDeliveredReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L30)

<a id="Vercel_AI_GenerateText_SteeringNotDeliveredReason_Error"></a>

### Error


```csharp
Error = 5
```


The run failed (`error`).

**Value:** [SteeringNotDeliveredReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L33)

<a id="Vercel_AI_GenerateText_SteeringNotDeliveredReason_Finished"></a>

### Finished


```csharp
Finished = 0
```


The run ended before the message could be delivered (`finished`).

**Value:** [SteeringNotDeliveredReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L18)

<a id="Vercel_AI_GenerateText_SteeringNotDeliveredReason_RemovedByPrepareStep"></a>

### RemovedByPrepareStep


```csharp
RemovedByPrepareStep = 1
```


`prepareStep` returned messages without it (`removed-by-prepare-step`).

**Value:** [SteeringNotDeliveredReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L21)

<a id="Vercel_AI_GenerateText_SteeringNotDeliveredReason_StopCondition"></a>

### StopCondition


```csharp
StopCondition = 2
```


A stop condition ended the run (`stop-condition`).

**Value:** [SteeringNotDeliveredReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L24)

<a id="Vercel_AI_GenerateText_SteeringNotDeliveredReason_WaitingForToolResults"></a>

### WaitingForToolResults


```csharp
WaitingForToolResults = 3
```


The run ended to wait for tool results, e.g. a tool approval or a tool without an `execute` function (`waiting-for-tool-results`).

**Value:** [SteeringNotDeliveredReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L27)
