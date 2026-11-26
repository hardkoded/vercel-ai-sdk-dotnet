---
title: "SteeringMessagePart"
description: "Experimental. A steering message that was added to the run before the step with the given number. Streamed before the output of that step. Maps to the steeri..."
editUrl: false
prev: false
next: false
sidebar:
  label: "SteeringMessagePart"
  hidden: true
---

<a id="Vercel_AI_SteeringMessagePart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class SteeringMessagePart : TextStreamPart
```


Experimental. A steering message that was added to the run before the step with the given number. Streamed before the output of that step.
Maps to the `steering-message` stream part. `StreamText` emits it when it delivers a message sent with [ExperimentalSteering](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ExperimentalSteering).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2942)

## Constructors

<a id="Vercel_AI_SteeringMessagePart__ctor_"></a>
<a id="Vercel_AI_SteeringMessagePart__ctor_System_String_Vercel_AI_Provider_UserModelMessage_System_Int32_"></a>

### SteeringMessagePart(string, UserModelMessage, int)


```csharp
public SteeringMessagePart(string id, UserModelMessage message, int stepNumber)
```


Creates a steering-message part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [UserModelMessage](/api/Vercel.AI.Provider/UserModelMessage/) |  |
| `stepNumber` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2945)

## Properties

<a id="Vercel_AI_SteeringMessagePart_Id_"></a>
<a id="Vercel_AI_SteeringMessagePart_Id"></a>

### Id


```csharp
public string Id { get; }
```


The id of the steering message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2954)

<a id="Vercel_AI_SteeringMessagePart_Message_"></a>
<a id="Vercel_AI_SteeringMessagePart_Message"></a>

### Message


```csharp
public UserModelMessage Message { get; }
```


The user message that was added to the run.

**Value:** [UserModelMessage](/api/Vercel.AI.Provider/UserModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2957)

<a id="Vercel_AI_SteeringMessagePart_StepNumber_"></a>
<a id="Vercel_AI_SteeringMessagePart_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


The number of the step whose model call received the message.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2960)
