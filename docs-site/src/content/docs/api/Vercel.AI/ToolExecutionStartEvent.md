---
title: "ToolExecutionStartEvent"
description: "Passed to OnToolExecutionStart before a tool runs."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecutionStartEvent"
  hidden: true
---

<a id="Vercel_AI_ToolExecutionStartEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public class ToolExecutionStartEvent
```


Passed to [OnToolExecutionStart](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnToolExecutionStart) before a tool runs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ToolExecutionEndEvent](/api/Vercel.AI/ToolExecutionEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L818)

## Constructors

<a id="Vercel_AI_ToolExecutionStartEvent__ctor_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent__ctor_Vercel_AI_Provider_GeneratedToolCall_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `ToolExecutionStartEvent(GeneratedToolCall, IReadOnlyList<ModelMessage>)`


```csharp
public ToolExecutionStartEvent(GeneratedToolCall toolCall, IReadOnlyList<ModelMessage> messages)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L821)

## Properties

<a id="Vercel_AI_ToolExecutionStartEvent_Messages_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


The messages the model received in the step that made the call.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L831)

<a id="Vercel_AI_ToolExecutionStartEvent_ToolCall_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The call about to run.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L828)
