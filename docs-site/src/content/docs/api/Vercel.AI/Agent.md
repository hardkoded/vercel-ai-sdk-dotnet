---
title: "Agent"
description: "An agent that calls tools until StopWhen matches. Maps to Agent."
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
public sealed class Agent
```


An agent that calls tools until [StopWhen](/api/Vercel.AI/StopWhen/) matches. Maps to `Agent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L14)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L19)

## Properties

<a id="Vercel_AI_Agent_Options_"></a>
<a id="Vercel_AI_Agent_Options"></a>

### Options


```csharp
public AgentOptions Options { get; }
```


Agent options.

**Value:** [AgentOptions](/api/Vercel.AI/AgentOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L31)

## Methods

<a id="Vercel_AI_Agent_GenerateAsync_"></a>
<a id="Vercel_AI_Agent_GenerateAsync_System_String_System_Threading_CancellationToken_Vercel_AI_Prompt_TimeoutConfiguration_"></a>

### GenerateAsync(string, CancellationToken, TimeoutConfiguration?)


```csharp
public Task<GenerateTextResult> GenerateAsync(string prompt, CancellationToken cancellationToken = default, TimeoutConfiguration? timeout = null)
```


Generates a reply to &lt;code class="paramref"&gt;prompt&lt;/code&gt;.
&lt;code class="paramref"&gt;timeout&lt;/code&gt; replaces [Timeout](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_Timeout) and the timeout that [PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall) returns for this call. Maps to `timeout` of `generate`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |

**Returns:** [Task&lt;GenerateTextResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L37)

<a id="Vercel_AI_Agent_Stream_"></a>
<a id="Vercel_AI_Agent_Stream_System_String_System_Threading_CancellationToken_Vercel_AI_Util_AbortSignal_Vercel_AI_Prompt_TimeoutConfiguration_"></a>

### Stream(string, CancellationToken, AbortSignal?, TimeoutConfiguration?)


```csharp
public StreamTextResult Stream(string prompt, CancellationToken cancellationToken = default, AbortSignal? abortSignal = null, TimeoutConfiguration? timeout = null)
```


Streams a reply to &lt;code class="paramref"&gt;prompt&lt;/code&gt;. &lt;code class="paramref"&gt;abortSignal&lt;/code&gt; aborts the stream and carries the reason.
&lt;code class="paramref"&gt;timeout&lt;/code&gt; replaces [Timeout](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_Timeout) and the timeout that [PrepareCall](/api/Vercel.AI/AgentOptions/#Vercel_AI_AgentOptions_PrepareCall) returns for this call. Maps to `timeout` of `stream`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `abortSignal` | [AbortSignal](/api/Vercel.AI.Util/AbortSignal/) |  |
| `timeout` | [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) |  |

**Returns:** [StreamTextResult](/api/Vercel.AI/StreamTextResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L53)
