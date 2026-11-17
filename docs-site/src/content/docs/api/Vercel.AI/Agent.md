---
title: "Agent"
description: "An agent that calls tools until StopWhen matches. Maps to ToolLoopAgent."
editUrl: false
prev: false
next: false
sidebar:
  label: "Agent"
  hidden: true
---

<a id="Vercel_AI_Agent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class Agent : IAgent
```


An agent that calls tools until [StopWhen](/api/Vercel.AI/StopWhen/) matches. Maps to `ToolLoopAgent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IAgent](/api/Vercel.AI/IAgent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L31)

## Constructors

<a id="Vercel_AI_Agent__ctor_"></a>
<a id="Vercel_AI_Agent__ctor_Vercel_AI_AgentOptions_Vercel_AI_IAiClient_"></a>

### Agent(AgentOptions, IAiClient?)


```csharp
public Agent(AgentOptions options, IAiClient? client = null)
```


Creates an agent. The default stop condition is 10 steps.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [AgentOptions](/api/Vercel.AI/AgentOptions/) |  |
| `client` | [IAiClient](/api/Vercel.AI/IAiClient/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L36)

## Properties

<a id="Vercel_AI_Agent_Options_"></a>
<a id="Vercel_AI_Agent_Options"></a>

### Options


```csharp
public AgentOptions Options { get; }
```


Agent options.

**Value:** [AgentOptions](/api/Vercel.AI/AgentOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L48)

<a id="Vercel_AI_Agent_Tools_"></a>
<a id="Vercel_AI_Agent_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; }
```


The tools of the agent. Maps to `tools`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L51)

## Methods

<a id="Vercel_AI_Agent_GenerateAsync_"></a>
<a id="Vercel_AI_Agent_GenerateAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Threading_CancellationToken_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_"></a>

### `GenerateAsync(IReadOnlyList<ModelMessage>, CancellationToken, TimeoutConfiguration?, ISteering?)`


```csharp
public Task<GenerateTextResult> GenerateAsync(IReadOnlyList<ModelMessage> messages, CancellationToken cancellationToken = default, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null)
```


Generates a reply to &lt;code class="paramref"&gt;messages&lt;/code&gt;. System messages in &lt;code class="paramref"&gt;messages&lt;/code&gt; are rejected unless
[AllowSystemInMessages](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_AllowSystemInMessages) is set. Maps to `messages` of `generate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L77)

<a id="Vercel_AI_Agent_GenerateAsync_System_String_System_Threading_CancellationToken_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_"></a>

### GenerateAsync(string, CancellationToken, TimeoutConfiguration?, ISteering?)


```csharp
public Task<GenerateTextResult> GenerateAsync(string prompt, CancellationToken cancellationToken = default, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null)
```


Generates a reply to &lt;code class="paramref"&gt;prompt&lt;/code&gt;.
&lt;code class="paramref"&gt;timeout&lt;/code&gt; replaces [Timeout](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_Timeout) and the timeout that [PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall) returns for this call. Maps to `timeout` of `generate`.
&lt;code class="paramref"&gt;experimentalSteering&lt;/code&gt; adds user messages to the run at the next step boundary and is not passed to [PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall). Maps to `experimental_steering`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L58)

<a id="Vercel_AI_Agent_Stream_"></a>
<a id="Vercel_AI_Agent_Stream_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Threading_CancellationToken_Vercel_AI_Util_AbortSignal_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_System_Func_Vercel_AI_StepResult_System_Threading_CancellationToken_System_Threading_Tasks_Task__"></a>

### `Stream(IReadOnlyList<ModelMessage>, CancellationToken, AbortSignal?, TimeoutConfiguration?, ISteering?, Func<StepResult, CancellationToken, Task>?)`


```csharp
public StreamTextResult Stream(IReadOnlyList<ModelMessage> messages, CancellationToken cancellationToken = default, AbortSignal? abortSignal = null, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null, Func<StepResult, CancellationToken, Task>? onStepEnd = null)
```


Streams a reply to &lt;code class="paramref"&gt;messages&lt;/code&gt;. System messages in &lt;code class="paramref"&gt;messages&lt;/code&gt; are rejected unless
[AllowSystemInMessages](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_AllowSystemInMessages) is set. &lt;code class="paramref"&gt;onStepEnd&lt;/code&gt; replaces the callback that
[PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall) returns for this call. Maps to `messages` of `stream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `abortSignal` | [AbortSignal](/api/Vercel.AI.Util/AbortSignal/) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |
| `onStepEnd` | [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L137)

<a id="Vercel_AI_Agent_Stream_System_String_System_Threading_CancellationToken_Vercel_AI_Util_AbortSignal_Vercel_AI_Prompt_TimeoutConfiguration_Vercel_AI_GenerateText_ISteering_"></a>

### Stream(string, CancellationToken, AbortSignal?, TimeoutConfiguration?, ISteering?)


```csharp
public StreamTextResult Stream(string prompt, CancellationToken cancellationToken = default, AbortSignal? abortSignal = null, TimeoutConfiguration? timeout = null, ISteering? experimentalSteering = null)
```


Streams a reply to &lt;code class="paramref"&gt;prompt&lt;/code&gt;. &lt;code class="paramref"&gt;abortSignal&lt;/code&gt; aborts the stream and carries the reason.
&lt;code class="paramref"&gt;timeout&lt;/code&gt; replaces [Timeout](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_Timeout) and the timeout that [PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall) returns for this call. Maps to `timeout` of `stream`.
&lt;code class="paramref"&gt;experimentalSteering&lt;/code&gt; adds user messages to the run at the next step boundary and is not passed to [PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall). Maps to `experimental_steering`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `abortSignal` | [AbortSignal](/api/Vercel.AI.Util/AbortSignal/) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |
| `experimentalSteering` | [ISteering](/api/Vercel.AI.GenerateText/ISteering/) |  |

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L115)
