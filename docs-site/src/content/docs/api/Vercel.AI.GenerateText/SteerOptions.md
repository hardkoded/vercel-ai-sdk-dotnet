---
title: "SteerOptions"
description: "Options of SteerAsync."
editUrl: false
prev: false
next: false
sidebar:
  label: "SteerOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SteerOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class SteerOptions
```


Options of `SteerAsync`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L89)

## Properties

<a id="Vercel_AI_GenerateText_SteerOptions_Id_"></a>
<a id="Vercel_AI_GenerateText_SteerOptions_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


The id of the message. Sending a message again with the same id returns the outcome of the first message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L92)

<a id="Vercel_AI_GenerateText_SteerOptions_Interrupt_"></a>
<a id="Vercel_AI_GenerateText_SteerOptions_Interrupt"></a>

### Interrupt


```csharp
public bool Interrupt { get; set; }
```


Cancels the current model call when it has not produced any output yet, and makes it again in the same step with the message.
Otherwise the message is added at the next step boundary. Running tools are never interrupted. A cancelled model call can still be billed
by the provider, and it is not included in the usage. The step is not counted again, so stop conditions do not limit how often a step is redone.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L99)
