---
title: "SingleToolApprovalFunction"
description: "Determines the approval status of a call to one tool, from the input of the call. Returning <a href=\"https://learn.microsoft.com/dotnet/csharp/language-refer..."
editUrl: false
prev: false
next: false
sidebar:
  label: "SingleToolApprovalFunction"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SingleToolApprovalFunction"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public delegate Task<ToolApprovalStatus?> SingleToolApprovalFunction(JsonElement input, SingleToolApprovalOptions options)
```


Determines the approval status of a call to one tool, from the input of the call. Returning &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; has the
effect of `not-applicable`. Maps to `SingleToolApprovalFunction`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `options` | [SingleToolApprovalOptions](/api/Vercel.AI.GenerateText/SingleToolApprovalOptions/) |  |

**Returns:** [Task&lt;ToolApprovalStatus&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L95)
