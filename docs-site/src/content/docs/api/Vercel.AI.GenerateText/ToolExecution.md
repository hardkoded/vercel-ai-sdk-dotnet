---
title: "ToolExecution"
description: "Finish-reason checks for the tool loop. Maps to isToolExecutionAllowedFinishReason."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecution"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolExecution"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolExecution
```


Finish-reason checks for the tool loop. Maps to `isToolExecutionAllowedFinishReason`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/IsToolExecutionAllowedFinishReason.cs#L10)

## Methods

<a id="Vercel_AI_GenerateText_ToolExecution_IsToolExecutionAllowedFinishReason_"></a>
<a id="Vercel_AI_GenerateText_ToolExecution_IsToolExecutionAllowedFinishReason_Vercel_AI_Provider_FinishReason_"></a>

### IsToolExecutionAllowedFinishReason(FinishReason)


```csharp
public static bool IsToolExecutionAllowedFinishReason(FinishReason finishReason)
```


Returns true when client tools may run for &lt;code class="paramref"&gt;finishReason&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/IsToolExecutionAllowedFinishReason.cs#L13)
