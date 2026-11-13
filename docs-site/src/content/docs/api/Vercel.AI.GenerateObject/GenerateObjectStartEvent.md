---
title: "GenerateObjectStartEvent"
description: "Event sent before object generation begins."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectStartEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectStartEvent : GenerateObjectEvent
```


Event sent before object generation begins.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateObjectEvent](/api/Vercel.AI.GenerateObject/GenerateObjectEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L174)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Messages_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; }
```


The prompt messages, when supplied.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L187)

<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Prompt_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStartEvent_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; }
```


The simple prompt, when supplied.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L184)
