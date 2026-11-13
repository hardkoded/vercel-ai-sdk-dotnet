---
title: "GenericToolApprovalFunction"
description: "Determines the approval status of any tool call. Returning <a href=\"https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null\">null</a> has..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenericToolApprovalFunction"
  hidden: true
---

<a id="Vercel_AI_GenerateText_GenericToolApprovalFunction"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public delegate Task<ToolApprovalStatus?> GenericToolApprovalFunction(GenericToolApprovalOptions options)
```


Determines the approval status of any tool call. Returning &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; has the effect of `not-applicable`.
Maps to `GenericToolApprovalFunction`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GenericToolApprovalOptions](/api/Vercel.AI.GenerateText/GenericToolApprovalOptions/) |  |

**Returns:** [Task&lt;ToolApprovalStatus&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L125)
