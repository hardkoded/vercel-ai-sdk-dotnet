---
title: "GenerateTextStepErrorEvent"
description: "Passed to OnStepError when a step that started fails before it produces a step result. Carries the step start event of the failed step and the error. Maps to..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateTextStepErrorEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateTextStepErrorEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class GenerateTextStepErrorEvent : GenerateTextStepStartEvent
```


Passed to [OnStepError](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnStepError) when a step that started fails before it produces a step result. Carries the step
start event of the failed step and the error. Maps to `GenerateTextStepErrorEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateTextStepStartEvent](/api/Vercel.AI/GenerateTextStepStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1921)

## Constructors

<a id="Vercel_AI_GenerateTextStepErrorEvent__ctor_"></a>
<a id="Vercel_AI_GenerateTextStepErrorEvent__ctor_Vercel_AI_GenerateTextStepStartEvent_System_Exception_"></a>

### GenerateTextStepErrorEvent(GenerateTextStepStartEvent, Exception)


```csharp
public GenerateTextStepErrorEvent(GenerateTextStepStartEvent step, Exception error)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `step` | [GenerateTextStepStartEvent](/api/Vercel.AI/GenerateTextStepStartEvent/) |  |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1924)

## Properties

<a id="Vercel_AI_GenerateTextStepErrorEvent_Error_"></a>
<a id="Vercel_AI_GenerateTextStepErrorEvent_Error"></a>

### Error


```csharp
public Exception Error { get; }
```


The error that ended the step. Maps to `error`.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1931)
