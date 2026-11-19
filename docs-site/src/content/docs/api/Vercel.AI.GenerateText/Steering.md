---
title: "Steering"
description: "Creates steering objects and starts their runs. Maps to steering.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "Steering"
  hidden: true
---

<a id="Vercel_AI_GenerateText_Steering"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class Steering
```


Creates steering objects and starts their runs. Maps to `steering.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L167)

## Methods

<a id="Vercel_AI_GenerateText_Steering_ExperimentalCreateSteering_"></a>
<a id="Vercel_AI_GenerateText_Steering_ExperimentalCreateSteering"></a>

### ExperimentalCreateSteering()


```csharp
public static ISteering ExperimentalCreateSteering()
```


Creates a steering object that sends user messages to a running generation. Maps to `experimental_createSteering`.

**Returns:** [ISteering](/api/Vercel.AI.GenerateText/ISteering/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L172)
