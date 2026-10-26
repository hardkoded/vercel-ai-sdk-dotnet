---
title: "TimeoutConfiguration"
description: "Timeout configuration for GenerateTextAsync and StreamTextAsync. Maps to TimeoutConfiguration. Upstream takes a number (the total timeout in milliseconds) or..."
editUrl: false
prev: false
next: false
sidebar:
  label: "TimeoutConfiguration"
  hidden: true
---

<a id="Vercel_AI_Prompt_TimeoutConfiguration"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class TimeoutConfiguration
```


Timeout configuration for `GenerateTextAsync` and `StreamTextAsync`. Maps to `TimeoutConfiguration`.
Upstream takes a number (the total timeout in milliseconds) or an object. A [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) converts to the total timeout, as the number form does upstream.
An expired timeout aborts the call with a [TimeoutError](/api/Vercel.AI.Util/TimeoutError/) reason.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L12)

## Properties

<a id="Vercel_AI_Prompt_TimeoutConfiguration_ChunkMs_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_ChunkMs"></a>

### ChunkMs


```csharp
public int? ChunkMs { get; set; }
```


Timeout between meaningful model output chunks, in milliseconds. Streaming only.
It starts with the first output chunk, so it does not limit the wait for that chunk. It ends when the model response ends.
`GenerateTextAsync` does not support it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L54)

<a id="Vercel_AI_Prompt_TimeoutConfiguration_FirstChunkMs_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_FirstChunkMs"></a>

### FirstChunkMs


```csharp
public int? FirstChunkMs { get; set; }
```


Timeout until the first content chunk of each step, in milliseconds. Streaming only.
It starts when the provider returns the response stream of the step, so it does not limit the wait for the response itself. A chunk without output, such as response metadata or an empty delta, does not end it.
`GenerateTextAsync` does not support it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L43)

<a id="Vercel_AI_Prompt_TimeoutConfiguration_StepMs_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_StepMs"></a>

### StepMs


```csharp
public int? StepMs { get; set; }
```


Timeout of each complete step, in milliseconds. A step covers model generation, retries, and local tool execution.
The budget starts again for each step.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L32)

<a id="Vercel_AI_Prompt_TimeoutConfiguration_ToolMs_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_ToolMs"></a>

### ToolMs


```csharp
public int? ToolMs { get; set; }
```


Default timeout of each tool execution, in milliseconds. A tool that runs longer is cancelled and its call ends with a tool error, so the model can respond or retry.
[Tools](#Vercel_AI_Prompt_TimeoutConfiguration_Tools) overrides it for one tool.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L64)

<a id="Vercel_AI_Prompt_TimeoutConfiguration_Tools_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_Tools"></a>

### Tools


```csharp
public IReadOnlyDictionary<string, int>? Tools { get; set; }
```


Per-tool timeouts in milliseconds. A key is the tool name followed by `Ms`, such as `weatherMs` for the tool `weather`, as in upstream.
An entry takes precedence over [ToolMs](#Vercel_AI_Prompt_TimeoutConfiguration_ToolMs). Upstream types the keys from the tool set. C# cannot, so a key that matches no tool has no effect.

**Value:** [IReadOnlyDictionary&lt;string, int&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L74)

<a id="Vercel_AI_Prompt_TimeoutConfiguration_TotalMs_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_TotalMs"></a>

### TotalMs


```csharp
public int? TotalMs { get; set; }
```


Total timeout for the entire call, including all steps, in milliseconds.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L22)

## Methods

<a id="Vercel_AI_Prompt_TimeoutConfiguration_GetToolTimeoutMs_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_GetToolTimeoutMs_Vercel_AI_Prompt_TimeoutConfiguration_System_String_"></a>

### GetToolTimeoutMs(TimeoutConfiguration?, string)


```csharp
public static int? GetToolTimeoutMs(TimeoutConfiguration? timeout, string toolName)
```


The timeout of the tool &lt;code class="paramref"&gt;toolName&lt;/code&gt; in milliseconds: its entry in [Tools](#Vercel_AI_Prompt_TimeoutConfiguration_Tools), else [ToolMs](#Vercel_AI_Prompt_TimeoutConfiguration_ToolMs), else `null`.
A [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) converts to the total timeout only, so it has no tool timeout. Maps to `getToolTimeoutMs`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timeout` | [TimeoutConfiguration](./) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L99)

## Operators

<a id="Vercel_AI_Prompt_TimeoutConfiguration_op_Implicit_"></a>
<a id="Vercel_AI_Prompt_TimeoutConfiguration_op_Implicit_System_TimeSpan__Vercel_AI_Prompt_TimeoutConfiguration"></a>

### implicit operator TimeoutConfiguration(TimeSpan)


```csharp
public static implicit operator TimeoutConfiguration(TimeSpan timeout)
```


A configuration whose total timeout is &lt;code class="paramref"&gt;timeout&lt;/code&gt;, rounded up to a whole millisecond.
[InfiniteTimeSpan](https://learn.microsoft.com/dotnet/api/system.threading.timeout.infinitetimespan) and a value longer than [MaxValue](https://learn.microsoft.com/dotnet/api/system.int32.maxvalue) milliseconds mean no timeout.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timeout` | [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

**Returns:** [TimeoutConfiguration](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/TimeoutConfiguration.cs#L118)
