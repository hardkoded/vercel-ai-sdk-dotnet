---
title: "AnthropicBashTool"
description: "Runs a bash provider tool through a caller-supplied sandbox."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicBashTool"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicBashTool"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicBashTool
```


Runs a bash provider tool through a caller-supplied sandbox.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L507)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicBashTool_ExecuteAsync_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBashTool_ExecuteAsync_System_String_System_Func_System_String_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Anthropic_AnthropicBashResult___System_Threading_CancellationToken_"></a>

### `ExecuteAsync(string, Func<string, CancellationToken, Task<AnthropicBashResult>>, CancellationToken)`


```csharp
public static Task<AnthropicBashResult> ExecuteAsync(string command, Func<string, CancellationToken, Task<AnthropicBashResult>> sandbox, CancellationToken cancellationToken)
```


Executes &lt;code class="paramref"&gt;command&lt;/code&gt; and forwards &lt;code class="paramref"&gt;cancellationToken&lt;/code&gt; to the sandbox.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `command` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sandbox` | [Func&lt;string, CancellationToken, Task&lt;AnthropicBashResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AnthropicBashResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L512)
