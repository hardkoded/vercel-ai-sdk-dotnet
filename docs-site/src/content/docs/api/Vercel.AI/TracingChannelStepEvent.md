---
title: "TracingChannelStepEvent"
description: "The event of a step span. Upstream publishes the object literal { callId, stepNumber }, which needs a type here."
editUrl: false
prev: false
next: false
sidebar:
  label: "TracingChannelStepEvent"
  hidden: true
---

<a id="Vercel_AI_TracingChannelStepEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TracingChannelStepEvent
```


The event of a `step` span. Upstream publishes the object literal `{ callId, stepNumber }`, which needs a type here.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L91)

## Constructors

<a id="Vercel_AI_TracingChannelStepEvent__ctor_"></a>
<a id="Vercel_AI_TracingChannelStepEvent__ctor_System_String_System_Int32_"></a>

### TracingChannelStepEvent(string, int)


```csharp
public TracingChannelStepEvent(string callId, int stepNumber)
```


Creates the event of a step span.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `stepNumber` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L94)

## Properties

<a id="Vercel_AI_TracingChannelStepEvent_AiVersion_"></a>
<a id="Vercel_AI_TracingChannelStepEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L116)

<a id="Vercel_AI_TracingChannelStepEvent_CallId_"></a>
<a id="Vercel_AI_TracingChannelStepEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


The id of the call that the step belongs to.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L101)

<a id="Vercel_AI_TracingChannelStepEvent_FunctionId_"></a>
<a id="Vercel_AI_TracingChannelStepEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


The function id of the telemetry settings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L113)

<a id="Vercel_AI_TracingChannelStepEvent_RecordInputs_"></a>
<a id="Vercel_AI_TracingChannelStepEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Whether inputs are recorded. Null when the call does not say.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L107)

<a id="Vercel_AI_TracingChannelStepEvent_RecordOutputs_"></a>
<a id="Vercel_AI_TracingChannelStepEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Whether outputs are recorded. Null when the call does not say.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L110)

<a id="Vercel_AI_TracingChannelStepEvent_StepNumber_"></a>
<a id="Vercel_AI_TracingChannelStepEvent_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


The number of the step, starting at zero.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TracingChannel.cs#L104)
