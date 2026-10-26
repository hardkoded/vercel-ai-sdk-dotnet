---
title: "ToolCallRepairFunction"
description: "Attempts to repair a tool call that failed to parse. Returns the repaired call, or null to keep the error. Maps to ToolCallRepairFunction."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallRepairFunction"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolCallRepairFunction"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public delegate Task<GeneratedToolCall?> ToolCallRepairFunction(ToolCallRepairContext context)
```


Attempts to repair a tool call that failed to parse. Returns the repaired call, or null to keep the error.
Maps to `ToolCallRepairFunction`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `context` | [ToolCallRepairContext](/api/Vercel.AI.GenerateText/ToolCallRepairContext/) |  |

**Returns:** [Task&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L124)
