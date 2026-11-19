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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L946)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L955)

## Properties

<a id="Vercel_AI_Agent_1_Options_"></a>
<a id="Vercel_AI_Agent_1_Options"></a>

### Options


```csharp
public AgentOptions<TCallOptions> Options { get; }
```


Agent options.

**Value:** `AgentOptions&lt;TCallOptions&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L967)

<a id="Vercel_AI_Agent_1_Tools_"></a>
<a id="Vercel_AI_Agent_1_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; }
```


The tools of the agent. Maps to `tools`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L970)

## Methods

<a id="Vercel_AI_Agent_1_GenerateAsync_"></a>
<a id="Vercel_AI_Agent_1_GenerateAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage___0_System_Threading_CancellationToken_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_Vercel_AI_ProviderUtils_ISandboxSession_"></a>

### `GenerateAsync(IReadOnlyList<ModelMessage>, TCallOptions, CancellationToken, TimeoutConfiguration?, ISteering?, ISandboxSession?)`


```csharp
public Task<GenerateTextResult> GenerateAsync(IReadOnlyList<ModelMessage> messages, TCallOptions options, CancellationToken cancellationToken = default, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, ISandboxSession? experimentalSandbox = null)
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

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L996)

<a id="Vercel_AI_Agent_1_GenerateAsync_System_String__0_System_Threading_CancellationToken_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_Vercel_AI_ProviderUtils_ISandboxSession_"></a>

### GenerateAsync(string, TCallOptions, CancellationToken, TimeoutConfiguration?, ISteering?, ISandboxSession?)


```csharp
public Task<GenerateTextResult> GenerateAsync(string prompt, TCallOptions options, CancellationToken cancellationToken = default, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, ISandboxSession? experimentalSandbox = null)
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

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L976)

<a id="Vercel_AI_Agent_1_Stream_"></a>
<a id="Vercel_AI_Agent_1_Stream_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage___0_System_Threading_CancellationToken_Vercel_AI_Util_AbortSignal_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__Vercel_AI_ProviderUtils_ISandboxSession_"></a>

### `Stream(IReadOnlyList<ModelMessage>, TCallOptions, CancellationToken, AbortSignal?, TimeoutConfiguration?, ISteering?, Func<StepResult, CancellationToken, Task>?, ISandboxSession?)`


```csharp
public StreamTextResult Stream(IReadOnlyList<ModelMessage> messages, TCallOptions options, CancellationToken cancellationToken = default, AbortSignal? abortSignal = null, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, Func<StepResult, CancellationToken, Task>? onStepEnd = null, ISandboxSession? experimentalSandbox = null)
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

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1043)

<a id="Vercel_AI_Agent_1_Stream_System_String__0_System_Threading_CancellationToken_Vercel_AI_Util_AbortSignal_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_Vercel_AI_ProviderUtils_ISandboxSession_"></a>

### Stream(string, TCallOptions, CancellationToken, AbortSignal?, TimeoutConfiguration?, ISteering?, ISandboxSession?)


```csharp
public StreamTextResult Stream(string prompt, TCallOptions options, CancellationToken cancellationToken = default, AbortSignal? abortSignal = null, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, ISandboxSession? experimentalSandbox = null)
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

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L1020)
