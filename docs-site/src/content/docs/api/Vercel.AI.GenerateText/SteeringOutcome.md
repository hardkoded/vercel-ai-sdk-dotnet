---
title: "SteeringOutcome"
description: "The outcome of a steering message. Maps to SteeringOutcome."
editUrl: false
prev: false
next: false
sidebar:
  label: "SteeringOutcome"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SteeringOutcome"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public abstract class SteeringOutcome
```


The outcome of a steering message. Maps to `SteeringOutcome`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [DeliveredSteeringOutcome](/api/Vercel.AI.GenerateText/DeliveredSteeringOutcome/), [NotDeliveredSteeringOutcome](/api/Vercel.AI.GenerateText/NotDeliveredSteeringOutcome/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L37)

## Properties

<a id="Vercel_AI_GenerateText_SteeringOutcome_Id_"></a>
<a id="Vercel_AI_GenerateText_SteeringOutcome_Id"></a>

### Id


```csharp
public string Id { get; }
```


The id of the steering message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L50)

<a id="Vercel_AI_GenerateText_SteeringOutcome_Message_"></a>
<a id="Vercel_AI_GenerateText_SteeringOutcome_Message"></a>

### Message


```csharp
public UserModelMessage Message { get; }
```


The user message that was, or was not, added to the run.

**Value:** [UserModelMessage](/api/Vercel.AI.Provider/UserModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L53)

<a id="Vercel_AI_GenerateText_SteeringOutcome_Type_"></a>
<a id="Vercel_AI_GenerateText_SteeringOutcome_Type"></a>

### Type


```csharp
public string Type { get; }
```


`delivered` or `not-delivered`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L47)
