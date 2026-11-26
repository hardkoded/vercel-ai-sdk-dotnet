---
title: "Agent<TCallOptions>"
description: "An agent that takes options on each call. Maps to ToolLoopAgent with a CALLOPTIONS type."
editUrl: false
prev: false
next: false
sidebar:
  label: "Agent<TCallOptions>"
  hidden: true
---

<a id="Vercel_AI_Agent_1"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class Agent<TCallOptions> : IAgent<TCallOptions>
```


An agent that takes options on each call. Maps to `ToolLoopAgent` with a `CALL_OPTIONS` type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TCallOptions` | The type of the per-call options. Maps to `CALL_OPTIONS`. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** `IAgent&lt;TCallOptions&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1454)

## Constructors

<a id="Vercel_AI_Agent_1__ctor_"></a>
<a id="Vercel_AI_Agent_1__ctor_Vercel_AI_AgentOptions__0__Vercel_AI_IAiClient_"></a>

### `Agent(AgentOptions<TCallOptions>, IAiClient?)`


```csharp
public Agent(AgentOptions<TCallOptions> options, IAiClient? client = null)
```


Creates an agent. The default stop condition is 10 steps.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | `AgentOptions&lt;TCallOptions&gt;` |  |
| `client` | [IAiClient](/api/Vercel.AI/IAiClient/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1463)

## Properties

<a id="Vercel_AI_Agent_1_Options_"></a>
<a id="Vercel_AI_Agent_1_Options"></a>

### Options


```csharp
public AgentOptions<TCallOptions> Options { get; }
```


Agent options.

**Value:** `AgentOptions&lt;TCallOptions&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1475)

<a id="Vercel_AI_Agent_1_Tools_"></a>
<a id="Vercel_AI_Agent_1_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; }
```


The tools of the agent. Maps to `tools`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1478)

## Methods

<a id="Vercel_AI_Agent_1_GenerateAsync_"></a>
<a id="Vercel_AI_Agent_1_GenerateAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage___0_System_Threading_CancellationToken_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_Vercel_AI_ProviderUtils_ISandboxSession_System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionEndEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__"></a>

### `GenerateAsync(IReadOnlyList<ModelMessage>, TCallOptions, CancellationToken, TimeoutConfiguration?, ISteering?, ISandboxSession?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<ToolExecutionStartEvent, CancellationToken, Task>?, Func<ToolExecutionEndEvent, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<StepResult, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<StepResult, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?)`


```csharp
public Task<GenerateTextResult> GenerateAsync(IReadOnlyList<ModelMessage> messages, TCallOptions options, CancellationToken cancellationToken = default, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, ISandboxSession? experimentalSandbox = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null, Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null, Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? onStart = null, Func<StepResult, CancellationToken, Task>? onStepEnd = null, Func<GenerateTextResult, CancellationToken, Task>? onEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? experimentalOnStart = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? experimentalOnStepStart = null, Func<StepResult, CancellationToken, Task>? onStepFinish = null, Func<GenerateTextResult, CancellationToken, Task>? onFinish = null)
```


Generates a reply to &lt;code class="paramref"&gt;messages&lt;/code&gt; with &lt;code class="paramref"&gt;options&lt;/code&gt;. Maps to `generate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `options` | `TCallOptions` |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |
| `experimentalSandbox` | [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/) |  |
| `onStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionStart` | [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionEnd` | [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStepEnd` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onEnd` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStepFinish` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onFinish` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1526)

<a id="Vercel_AI_Agent_1_GenerateAsync_System_String__0_System_Threading_CancellationToken_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_Vercel_AI_ProviderUtils_ISandboxSession_System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionEndEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__"></a>

### `GenerateAsync(string, TCallOptions, CancellationToken, TimeoutConfiguration?, ISteering?, ISandboxSession?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<ToolExecutionStartEvent, CancellationToken, Task>?, Func<ToolExecutionEndEvent, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<StepResult, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<StepResult, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?)`


```csharp
public Task<GenerateTextResult> GenerateAsync(string prompt, TCallOptions options, CancellationToken cancellationToken = default, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, ISandboxSession? experimentalSandbox = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null, Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null, Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? onStart = null, Func<StepResult, CancellationToken, Task>? onStepEnd = null, Func<GenerateTextResult, CancellationToken, Task>? onEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? experimentalOnStart = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? experimentalOnStepStart = null, Func<StepResult, CancellationToken, Task>? onStepFinish = null, Func<GenerateTextResult, CancellationToken, Task>? onFinish = null)
```


Generates a reply to &lt;code class="paramref"&gt;prompt&lt;/code&gt; with &lt;code class="paramref"&gt;options&lt;/code&gt;, which are validated against
`CallOptionsSchema` first. Maps to `generate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | `TCallOptions` |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |
| `experimentalSandbox` | [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/) |  |
| `onStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionStart` | [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionEnd` | [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStepEnd` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onEnd` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStepFinish` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onFinish` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1484)

<a id="Vercel_AI_Agent_1_Stream_"></a>
<a id="Vercel_AI_Agent_1_Stream_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage___0_System_Threading_CancellationToken_Vercel_AI_Util_AbortSignal_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__Vercel_AI_ProviderUtils_ISandboxSession_System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionEndEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__"></a>

### `Stream(IReadOnlyList<ModelMessage>, TCallOptions, CancellationToken, AbortSignal?, TimeoutConfiguration?, ISteering?, Func<StepResult, CancellationToken, Task>?, ISandboxSession?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<ToolExecutionStartEvent, CancellationToken, Task>?, Func<ToolExecutionEndEvent, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<StepResult, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?)`


```csharp
public StreamTextResult Stream(IReadOnlyList<ModelMessage> messages, TCallOptions options, CancellationToken cancellationToken = default, AbortSignal? abortSignal = null, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, Func<StepResult, CancellationToken, Task>? onStepEnd = null, ISandboxSession? experimentalSandbox = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null, Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null, Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? onStart = null, Func<GenerateTextResult, CancellationToken, Task>? onEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? experimentalOnStart = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? experimentalOnStepStart = null, Func<StepResult, CancellationToken, Task>? onStepFinish = null, Func<GenerateTextResult, CancellationToken, Task>? onFinish = null)
```


Streams a reply to &lt;code class="paramref"&gt;messages&lt;/code&gt; with &lt;code class="paramref"&gt;options&lt;/code&gt;. Maps to `stream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `options` | `TCallOptions` |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `abortSignal` | [AbortSignal](/api/Vercel.AI.Util/AbortSignal/) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |
| `onStepEnd` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalSandbox` | [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/) |  |
| `onStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionStart` | [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionEnd` | [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onEnd` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStepFinish` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onFinish` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1616)

<a id="Vercel_AI_Agent_1_Stream_System_String__0_System_Threading_CancellationToken_Vercel_AI_Util_AbortSignal_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_Vercel_AI_ProviderUtils_ISandboxSession_System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_ToolExecutionEndEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextStepStartEvent_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__System_Func_Vercel_AI_GenerateTextResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__"></a>

### `Stream(string, TCallOptions, CancellationToken, AbortSignal?, TimeoutConfiguration?, ISteering?, ISandboxSession?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<ToolExecutionStartEvent, CancellationToken, Task>?, Func<ToolExecutionEndEvent, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<StepResult, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?, Func<GenerateTextStartEvent, CancellationToken, Task>?, Func<GenerateTextStepStartEvent, CancellationToken, Task>?, Func<StepResult, CancellationToken, Task>?, Func<GenerateTextResult, CancellationToken, Task>?)`


```csharp
public StreamTextResult Stream(string prompt, TCallOptions options, CancellationToken cancellationToken = default, AbortSignal? abortSignal = null, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, ISandboxSession? experimentalSandbox = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null, Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null, Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? onStart = null, Func<StepResult, CancellationToken, Task>? onStepEnd = null, Func<GenerateTextResult, CancellationToken, Task>? onEnd = null, Func<GenerateTextStartEvent, CancellationToken, Task>? experimentalOnStart = null, Func<GenerateTextStepStartEvent, CancellationToken, Task>? experimentalOnStepStart = null, Func<StepResult, CancellationToken, Task>? onStepFinish = null, Func<GenerateTextResult, CancellationToken, Task>? onFinish = null)
```


Streams a reply to &lt;code class="paramref"&gt;prompt&lt;/code&gt; with &lt;code class="paramref"&gt;options&lt;/code&gt;. Maps to `stream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | `TCallOptions` |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `abortSignal` | [AbortSignal](/api/Vercel.AI.Util/AbortSignal/) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |
| `experimentalSandbox` | [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/) |  |
| `onStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionStart` | [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onToolExecutionEnd` | [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStepEnd` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onEnd` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStart` | [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `experimentalOnStepStart` | [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onStepFinish` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `onFinish` | [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1572)
