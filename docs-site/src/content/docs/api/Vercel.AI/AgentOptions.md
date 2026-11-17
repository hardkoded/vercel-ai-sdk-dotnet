---
title: "AgentOptions"
description: "Agent settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "AgentOptions"
  hidden: true
---

<a id="Vercel_AI_AgentOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class AgentOptions
```


Agent settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L242)

## Properties

<a id="Vercel_AI_AgentOptions_AllowSystemInMessages_"></a>
<a id="Vercel_AI_AgentOptions_AllowSystemInMessages"></a>

### AllowSystemInMessages


```csharp
public bool AllowSystemInMessages { get; set; }
```


Whether system messages are allowed in the messages passed to `ISteering)`
and `Task%7d)`.
Defaults to false, which rejects them with an [InvalidPromptException](/api/Vercel.AI.Prompt/InvalidPromptException/). Use [Instructions](#Vercel_AI_AgentOptions_Instructions) for the system prompt instead.
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `allowSystemInMessages`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L259)

<a id="Vercel_AI_AgentOptions_Instructions_"></a>
<a id="Vercel_AI_AgentOptions_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


System instructions: a string, one [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/), or an array of them.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L251)

<a id="Vercel_AI_AgentOptions_MaxOutputTokens_"></a>
<a id="Vercel_AI_AgentOptions_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L271)

<a id="Vercel_AI_AgentOptions_Model_"></a>
<a id="Vercel_AI_AgentOptions_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Explicit model.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L245)

<a id="Vercel_AI_AgentOptions_ModelId_"></a>
<a id="Vercel_AI_AgentOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L248)

<a id="Vercel_AI_AgentOptions_OnAbort_"></a>
<a id="Vercel_AI_AgentOptions_OnAbort"></a>

### OnAbort


```csharp
public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; set; }
```


Called after `ISteering)` is aborted. See [OnAbort](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_OnAbort).

**Value:** [Func&lt;StreamAbortContext, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L302)

<a id="Vercel_AI_AgentOptions_PrepareCall_"></a>
<a id="Vercel_AI_AgentOptions_PrepareCall"></a>

### PrepareCall


```csharp
public Func<GenerateTextOptions, GenerateTextOptions?>? PrepareCall { get; set; }
```


Called before each `ISteering)` and `ISteering)` call with the options the agent would use,
including the default [StopWhen](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_StopWhen). Return replacement options, or null to keep them.
A returned options object that sets [StopWhen](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_StopWhen) replaces the default limit. Maps to `prepareCall`.

**Value:** [Func&lt;GenerateTextOptions, GenerateTextOptions&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L299)

<a id="Vercel_AI_AgentOptions_RefineToolInput_"></a>
<a id="Vercel_AI_AgentOptions_RefineToolInput"></a>

### RefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; set; }
```


Refines the parsed input of tool calls. See [RefineToolInput](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RefineToolInput). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, CancellationToken, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L283)

<a id="Vercel_AI_AgentOptions_RepairToolCall_"></a>
<a id="Vercel_AI_AgentOptions_RepairToolCall"></a>

### RepairToolCall


```csharp
public ToolCallRepairFunction? RepairToolCall { get; set; }
```


Repairs a tool call that failed to parse. See [RepairToolCall](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RepairToolCall). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L280)

<a id="Vercel_AI_AgentOptions_StopWhen_"></a>
<a id="Vercel_AI_AgentOptions_StopWhen"></a>

### StopWhen


```csharp
public StopCondition? StopWhen { get; set; }
```


Stop condition. Defaults to 10 steps.

**Value:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L265)

<a id="Vercel_AI_AgentOptions_Temperature_"></a>
<a id="Vercel_AI_AgentOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L268)

<a id="Vercel_AI_AgentOptions_Timeout_"></a>
<a id="Vercel_AI_AgentOptions_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


Timeout of each `ISteering)` and `ISteering)` call. See [Timeout](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_Timeout).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `timeout` of `ToolLoopAgentSettings`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L292)

<a id="Vercel_AI_AgentOptions_ToolCallConcurrency_"></a>
<a id="Vercel_AI_AgentOptions_ToolCallConcurrency"></a>

### ToolCallConcurrency


```csharp
public int? ToolCallConcurrency { get; set; }
```


Maximum number of tool calls that may execute concurrently within a step. See [ToolCallConcurrency](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolCallConcurrency).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L277)

<a id="Vercel_AI_AgentOptions_Tools_"></a>
<a id="Vercel_AI_AgentOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; set; }
```


Tools.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L262)
