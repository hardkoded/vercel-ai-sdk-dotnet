---
title: "NotDeliveredSteeringOutcome"
description: "The message was not added to the run."
editUrl: false
prev: false
next: false
sidebar:
  label: "NotDeliveredSteeringOutcome"
  hidden: true
---

<a id="Vercel_AI_GenerateText_NotDeliveredSteeringOutcome"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class NotDeliveredSteeringOutcome : SteeringOutcome
```


The message was not added to the run.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [SteeringOutcome](/api/Vercel.AI.GenerateText/SteeringOutcome/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L71)

## Constructors

<a id="Vercel_AI_GenerateText_NotDeliveredSteeringOutcome__ctor_"></a>
<a id="Vercel_AI_GenerateText_NotDeliveredSteeringOutcome__ctor_System_String_Vercel_AI_Provider_UserModelMessage_Vercel_AI_GenerateText_SteeringNotDeliveredReason_"></a>

### NotDeliveredSteeringOutcome(string, UserModelMessage, SteeringNotDeliveredReason)


```csharp
public NotDeliveredSteeringOutcome(string id, UserModelMessage message, SteeringNotDeliveredReason reason)
```


Creates the outcome.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [UserModelMessage](/api/Vercel.AI.Provider/UserModelMessage/) |  |
| `reason` | [SteeringNotDeliveredReason](/api/Vercel.AI.GenerateText/SteeringNotDeliveredReason/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L74)

## Properties

<a id="Vercel_AI_GenerateText_NotDeliveredSteeringOutcome_Reason_"></a>
<a id="Vercel_AI_GenerateText_NotDeliveredSteeringOutcome_Reason"></a>

### Reason


```csharp
public SteeringNotDeliveredReason Reason { get; }
```


Why the message was not delivered.

**Value:** [SteeringNotDeliveredReason](/api/Vercel.AI.GenerateText/SteeringNotDeliveredReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L85)
