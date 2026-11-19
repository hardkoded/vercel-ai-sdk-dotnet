---
title: "QueueOptions"
description: "Options of `QueueAsync`. Queued messages wait for the task, so they cannot interrupt."
editUrl: false
prev: false
next: false
sidebar:
  label: "QueueOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_QueueOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class QueueOptions
```


Options of `QueueAsync`. Queued messages wait for the task, so they cannot interrupt.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L103)

## Properties

<a id="Vercel_AI_GenerateText_QueueOptions_Id_"></a>
<a id="Vercel_AI_GenerateText_QueueOptions_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


The id of the message. Sending a message again with the same id returns the outcome of the first message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/Steering.cs#L106)
