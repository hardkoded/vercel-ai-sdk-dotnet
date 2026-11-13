---
title: "GenerateObjectStepStartEvent"
description: "Event sent immediately before the generate-object model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectStepStartEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectStepStartEvent : GenerateObjectEvent
```


Event sent immediately before the generate-object model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateObjectEvent](/api/Vercel.AI.GenerateObject/GenerateObjectEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L191)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_StepNumber_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


The only step number made by generateObject.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L199)
