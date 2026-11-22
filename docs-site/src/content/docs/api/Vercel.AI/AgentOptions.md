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
public class AgentOptions
```


Agent settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AgentOptions&lt;TCallOptions&gt;](/api/Vercel.AI/AgentOptions-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L400)

## Properties

<a id="Vercel_AI_AgentOptions_AllowSystemInMessages_"></a>
<a id="Vercel_AI_AgentOptions_AllowSystemInMessages"></a>

### AllowSystemInMessages


```csharp
public bool AllowSystemInMessages { get; set; }
```


Whether system messages are allowed in the messages passed to `ISandboxSession)`
and `ISandboxSession)`.
Defaults to false, which rejects them with an [InvalidPromptException](/api/Vercel.AI.Prompt/InvalidPromptException/). Use [Instructions](#Vercel_AI_AgentOptions_Instructions) for the system prompt instead.
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `allowSystemInMessages`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L417)

<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecret_"></a>
<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecret"></a>

### ExperimentalToolApprovalSecret


```csharp
public string? ExperimentalToolApprovalSecret { get; set; }
```


Secret for HMAC-signing tool approval requests. Approval requests of automatically denied tool calls are not signed. See [ExperimentalToolApprovalSecret](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecret). Maps to `experimental_toolApprovalSecret`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L456)

<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecretBytes_"></a>
<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecretBytes"></a>

### ExperimentalToolApprovalSecretBytes


```csharp
public byte[]? ExperimentalToolApprovalSecretBytes { get; set; }
```


The same secret as [ExperimentalToolApprovalSecret](#Vercel_AI_AgentOptions_ExperimentalToolApprovalSecret), given as bytes. See [ExperimentalToolApprovalSecretBytes](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecretBytes).

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L459)

<a id="Vercel_AI_AgentOptions_Headers_"></a>
<a id="Vercel_AI_AgentOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Additional HTTP headers sent with every request. The agent appends `ai-sdk-agent/tool-loop` to the `user-agent` header.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L471)

<a id="Vercel_AI_AgentOptions_Instructions_"></a>
<a id="Vercel_AI_AgentOptions_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


System instructions: a string, one [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/), or an array of them.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L409)

<a id="Vercel_AI_AgentOptions_MaxOutputTokens_"></a>
<a id="Vercel_AI_AgentOptions_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L468)

<a id="Vercel_AI_AgentOptions_Model_"></a>
<a id="Vercel_AI_AgentOptions_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Explicit model.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L403)

<a id="Vercel_AI_AgentOptions_ModelId_"></a>
<a id="Vercel_AI_AgentOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L406)

<a id="Vercel_AI_AgentOptions_OnAbort_"></a>
<a id="Vercel_AI_AgentOptions_OnAbort"></a>

### OnAbort


```csharp
public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; set; }
```


Called after `ISandboxSession)` is aborted. See [OnAbort](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_OnAbort).

**Value:** [Func&lt;StreamAbortContext, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L505)

<a id="Vercel_AI_AgentOptions_OnEnd_"></a>
<a id="Vercel_AI_AgentOptions_OnEnd"></a>

### OnEnd


```csharp
public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; set; }
```


Called when a generation completes. It runs together with the callback of the call, and a failure of one does not stop the other.
Maps to `onFinish` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L441)

<a id="Vercel_AI_AgentOptions_OnStart_"></a>
<a id="Vercel_AI_AgentOptions_OnStart"></a>

### OnStart


```csharp
public Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart { get; set; }
```


Called when a generation begins. It runs together with the callback of the call, and a failure of one does not stop the other.
Maps to `onStart` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L435)

<a id="Vercel_AI_AgentOptions_OnStepEnd_"></a>
<a id="Vercel_AI_AgentOptions_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called when each generation step ends. It runs together with the callback of the call, and a failure of one does not stop the other.
Maps to `onStepEnd` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L426)

<a id="Vercel_AI_AgentOptions_PrepareCall_"></a>
<a id="Vercel_AI_AgentOptions_PrepareCall"></a>

### PrepareCall


```csharp
public Func<GenerateTextOptions, GenerateTextOptions?>? PrepareCall { get; set; }
```


Called before each `ISandboxSession)` and `ISandboxSession)` call with the options the agent would use,
including the default [StopWhen](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_StopWhen). Return replacement options, or null to keep them.
A returned options object that sets [StopWhen](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_StopWhen) replaces the default limit. Maps to `prepareCall`.

**Value:** [Func&lt;GenerateTextOptions, GenerateTextOptions&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L502)

<a id="Vercel_AI_AgentOptions_RefineToolInput_"></a>
<a id="Vercel_AI_AgentOptions_RefineToolInput"></a>

### RefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; set; }
```


Refines the parsed input of tool calls. See [RefineToolInput](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RefineToolInput). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, CancellationToken, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L486)

<a id="Vercel_AI_AgentOptions_RepairToolCall_"></a>
<a id="Vercel_AI_AgentOptions_RepairToolCall"></a>

### RepairToolCall


```csharp
public ToolCallRepairFunction? RepairToolCall { get; set; }
```


Repairs a tool call that failed to parse. See [RepairToolCall](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RepairToolCall). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L483)

<a id="Vercel_AI_AgentOptions_RuntimeContext_"></a>
<a id="Vercel_AI_AgentOptions_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }
```


User-defined context that is passed to the approval functions. See [RuntimeContext](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RuntimeContext). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L453)

<a id="Vercel_AI_AgentOptions_StopWhen_"></a>
<a id="Vercel_AI_AgentOptions_StopWhen"></a>

### StopWhen


```csharp
public StopCondition? StopWhen { get; set; }
```


Stop condition. Defaults to 10 steps.

**Value:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L462)

<a id="Vercel_AI_AgentOptions_Telemetry_"></a>
<a id="Vercel_AI_AgentOptions_Telemetry"></a>

### Telemetry


```csharp
public TelemetryOptions? Telemetry { get; set; }
```


Telemetry configuration of every call. See [Telemetry](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_Telemetry). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `telemetry`.

**Value:** [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L429)

<a id="Vercel_AI_AgentOptions_Temperature_"></a>
<a id="Vercel_AI_AgentOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L465)

<a id="Vercel_AI_AgentOptions_Timeout_"></a>
<a id="Vercel_AI_AgentOptions_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


Timeout of each `ISandboxSession)` and `ISandboxSession)` call. See [Timeout](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_Timeout).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `timeout` of `ToolLoopAgentSettings`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L495)

<a id="Vercel_AI_AgentOptions_ToolApproval_"></a>
<a id="Vercel_AI_AgentOptions_ToolApproval"></a>

### ToolApproval


```csharp
public ToolApprovalConfiguration? ToolApproval { get; set; }
```


Configures whether tools require approval before they run. See [ToolApproval](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolApproval).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `toolApproval`.

**Value:** [ToolApprovalConfiguration](/api/Vercel.AI.GenerateText/ToolApprovalConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L447)

<a id="Vercel_AI_AgentOptions_ToolCallConcurrency_"></a>
<a id="Vercel_AI_AgentOptions_ToolCallConcurrency"></a>

### ToolCallConcurrency


```csharp
public int? ToolCallConcurrency { get; set; }
```


Maximum number of tool calls that may execute concurrently within a step. See [ToolCallConcurrency](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolCallConcurrency).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L480)

<a id="Vercel_AI_AgentOptions_ToolOrder_"></a>
<a id="Vercel_AI_AgentOptions_ToolOrder"></a>

### ToolOrder


```csharp
public IReadOnlyList<string>? ToolOrder { get; set; }
```


Controls the order in which tools are sent to the provider. See [ToolOrder](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolOrder). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L474)

<a id="Vercel_AI_AgentOptions_Tools_"></a>
<a id="Vercel_AI_AgentOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; set; }
```


Tools.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L420)

<a id="Vercel_AI_AgentOptions_ToolsContext_"></a>
<a id="Vercel_AI_AgentOptions_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; set; }
```


The context of each tool, keyed by tool name. See [ToolsContext](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolsContext). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `toolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L450)
