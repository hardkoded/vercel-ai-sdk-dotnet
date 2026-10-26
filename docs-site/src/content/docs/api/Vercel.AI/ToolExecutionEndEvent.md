---
title: "ToolExecutionEndEvent"
description: "Passed to OnToolExecutionEnd after a tool ran."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecutionEndEvent"
  hidden: true
---

<a id="Vercel_AI_ToolExecutionEndEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolExecutionEndEvent : ToolExecutionStartEvent
```


Passed to [OnToolExecutionEnd](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnToolExecutionEnd) after a tool ran.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ToolExecutionStartEvent](/api/Vercel.AI/ToolExecutionStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L835)

## Constructors

<a id="Vercel_AI_ToolExecutionEndEvent__ctor_"></a>
<a id="Vercel_AI_ToolExecutionEndEvent__ctor_Vercel_AI_Provider_GeneratedToolCall_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__Vercel_AI_ExecutedTool_System_TimeSpan_"></a>

### `ToolExecutionEndEvent(GeneratedToolCall, IReadOnlyList<ModelMessage>, ExecutedTool, TimeSpan)`


```csharp
public ToolExecutionEndEvent(GeneratedToolCall toolCall, IReadOnlyList<ModelMessage> messages, ExecutedTool toolOutput, TimeSpan toolExecutionTime)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolOutput` | [ExecutedTool](/api/Vercel.AI/ExecutedTool/) |  |
| `toolExecutionTime` | [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L838)

## Properties

<a id="Vercel_AI_ToolExecutionEndEvent_ToolExecutionTime_"></a>
<a id="Vercel_AI_ToolExecutionEndEvent_ToolExecutionTime"></a>

### ToolExecutionTime


```csharp
public TimeSpan ToolExecutionTime { get; }
```


Time the tool took. Maps to `toolExecutionMs`.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L849)

<a id="Vercel_AI_ToolExecutionEndEvent_ToolOutput_"></a>
<a id="Vercel_AI_ToolExecutionEndEvent_ToolOutput"></a>

### ToolOutput


```csharp
public ExecutedTool ToolOutput { get; }
```


The result of the call. [IsError](/api/Vercel.AI/ExecutedTool/#Vercel_AI_ExecutedTool_IsError) is set when the tool threw.

**Value:** [ExecutedTool](/api/Vercel.AI/ExecutedTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L846)
