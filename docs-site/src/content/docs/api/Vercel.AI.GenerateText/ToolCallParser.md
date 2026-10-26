---
title: "ToolCallParser"
description: "Parses the input of a tool call and validates it against the tool's input schema. Maps to parseToolCall."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallParser"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolCallParser"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolCallParser
```


Parses the input of a tool call and validates it against the tool's input schema. Maps to `parseToolCall`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L158)

## Methods

<a id="Vercel_AI_GenerateText_ToolCallParser_ParseToolCallAsync_"></a>
<a id="Vercel_AI_GenerateText_ToolCallParser_ParseToolCallAsync_Vercel_AI_GenerateText_ParseToolCallOptions_System_Threading_CancellationToken_"></a>

### ParseToolCallAsync(ParseToolCallOptions, CancellationToken)


```csharp
public static Task<ParsedToolCall> ParseToolCallAsync(ParseToolCallOptions options, CancellationToken cancellationToken = default)
```


Parses the call in &lt;code class="paramref"&gt;options&lt;/code&gt;. Empty input is validated as an empty object. A call that cannot be parsed
comes back with [Invalid](/api/Vercel.AI.GenerateText/ParsedToolCall/#Vercel_AI_GenerateText_ParsedToolCall_Invalid) set and the failure in [Error](/api/Vercel.AI.GenerateText/ParsedToolCall/#Vercel_AI_GenerateText_ParsedToolCall_Error): a
[NoSuchToolError](/api/Vercel.AI.Util/NoSuchToolError/), an [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/), or a [ToolCallRepairError](/api/Vercel.AI.Util/ToolCallRepairError/) when
[RepairToolCall](/api/Vercel.AI.GenerateText/ParseToolCallOptions/#Vercel_AI_GenerateText_ParseToolCallOptions_RepairToolCall) failed. A failing repair never hides a cancellation:
when &lt;code class="paramref"&gt;cancellationToken&lt;/code&gt; is cancelled the task is cancelled.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ParseToolCallOptions](/api/Vercel.AI.GenerateText/ParseToolCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ParsedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ParseToolCall.cs#L169)
