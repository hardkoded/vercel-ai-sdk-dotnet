---
title: "GenerateObjectEvent"
description: "Base information shared by generate-object callback events."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectEvent"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public abstract class GenerateObjectEvent
```


Base information shared by generate-object callback events.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [GenerateObjectEndEvent&lt;T&gt;](/api/Vercel.AI.GenerateObject/GenerateObjectEndEvent-1/), [GenerateObjectStartEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStartEvent/), [GenerateObjectStepEndEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStepEndEvent/), [GenerateObjectStepStartEvent](/api/Vercel.AI.GenerateObject/GenerateObjectStepStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L154)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectEvent_CallId_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Identifier shared by every callback from this call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L164)

<a id="Vercel_AI_GenerateObject_GenerateObjectEvent_ModelId_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Requested model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L170)

<a id="Vercel_AI_GenerateObject_GenerateObjectEvent_Provider_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L167)
