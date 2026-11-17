---
title: "DeliveredSteeringOutcome"
description: "The message was added to the run."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeliveredSteeringOutcome"
  hidden: true
---

<a id="Vercel_AI_GenerateText_DeliveredSteeringOutcome"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class DeliveredSteeringOutcome : SteeringOutcome
```


The message was added to the run.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [SteeringOutcome](/api/Vercel.AI.GenerateText/SteeringOutcome/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L57)

## Constructors

<a id="Vercel_AI_GenerateText_DeliveredSteeringOutcome__ctor_"></a>
<a id="Vercel_AI_GenerateText_DeliveredSteeringOutcome__ctor_System_String_Vercel_AI_Provider_UserModelMessage_System_Int32_"></a>

### DeliveredSteeringOutcome(string, UserModelMessage, int)


```csharp
public DeliveredSteeringOutcome(string id, UserModelMessage message, int stepNumber)
```


Creates the outcome.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [UserModelMessage](/api/Vercel.AI.Provider/UserModelMessage/) |  |
| `stepNumber` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L60)

## Properties

<a id="Vercel_AI_GenerateText_DeliveredSteeringOutcome_StepNumber_"></a>
<a id="Vercel_AI_GenerateText_DeliveredSteeringOutcome_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


The number of the step whose model call received the message.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L67)
