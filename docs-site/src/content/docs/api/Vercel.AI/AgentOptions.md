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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L674)

## Properties

<a id="Vercel_AI_AgentOptions_AllowSystemInMessages_"></a>
<a id="Vercel_AI_AgentOptions_AllowSystemInMessages"></a>

### AllowSystemInMessages


```csharp
public bool AllowSystemInMessages { get; set; }
```


Whether system messages are allowed in the messages passed to `GenerateAsync`
and `Stream`.
Defaults to false, which rejects them with an [InvalidPromptException](/api/Vercel.AI.Prompt/InvalidPromptException/). Use [Instructions](#Vercel_AI_AgentOptions_Instructions) for the system prompt instead.
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `allowSystemInMessages`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L691)

<a id="Vercel_AI_AgentOptions_ExperimentalOnStart_"></a>
<a id="Vercel_AI_AgentOptions_ExperimentalOnStart"></a>

### ExperimentalOnStart


```csharp
[Obsolete("Use OnStart instead.")]
public Func<GenerateTextStartEvent, CancellationToken, Task>? ExperimentalOnStart { get; set; }
```


Deprecated alias for [OnStart](#Vercel_AI_AgentOptions_OnStart); [OnStart](#Vercel_AI_AgentOptions_OnStart) wins when both are set. Maps to `experimental_onStart` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L743)

<a id="Vercel_AI_AgentOptions_ExperimentalOnStepStart_"></a>
<a id="Vercel_AI_AgentOptions_ExperimentalOnStepStart"></a>

### ExperimentalOnStepStart


```csharp
[Obsolete("Use OnStepStart instead.")]
public Func<GenerateTextStepStartEvent, CancellationToken, Task>? ExperimentalOnStepStart { get; set; }
```


Deprecated alias for [OnStepStart](#Vercel_AI_AgentOptions_OnStepStart); [OnStepStart](#Vercel_AI_AgentOptions_OnStepStart) wins when both are set. Maps to `experimental_onStepStart` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L735)

<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecret_"></a>
<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecret"></a>

### ExperimentalToolApprovalSecret


```csharp
public string? ExperimentalToolApprovalSecret { get; set; }
```


Secret for HMAC-signing tool approval requests. Approval requests of automatically denied tool calls are not signed. See [ExperimentalToolApprovalSecret](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecret). Maps to `experimental_toolApprovalSecret`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L799)

<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecretBytes_"></a>
<a id="Vercel_AI_AgentOptions_ExperimentalToolApprovalSecretBytes"></a>

### ExperimentalToolApprovalSecretBytes


```csharp
public byte[]? ExperimentalToolApprovalSecretBytes { get; set; }
```


The same secret as [ExperimentalToolApprovalSecret](#Vercel_AI_AgentOptions_ExperimentalToolApprovalSecret), given as bytes. See [ExperimentalToolApprovalSecretBytes](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecretBytes).

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L802)

<a id="Vercel_AI_AgentOptions_Headers_"></a>
<a id="Vercel_AI_AgentOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Additional HTTP headers sent with every request. The agent appends `ai-sdk-agent/tool-loop` to the `user-agent` header.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L814)

<a id="Vercel_AI_AgentOptions_Instructions_"></a>
<a id="Vercel_AI_AgentOptions_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


System instructions: a string, one [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/), or an array of them.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L683)

<a id="Vercel_AI_AgentOptions_MaxOutputTokens_"></a>
<a id="Vercel_AI_AgentOptions_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L811)

<a id="Vercel_AI_AgentOptions_MaxRetries_"></a>
<a id="Vercel_AI_AgentOptions_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; set; }
```


Maximum number of automatic retries of a failed model call. Defaults to 2. [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `maxRetries`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L720)

<a id="Vercel_AI_AgentOptions_Model_"></a>
<a id="Vercel_AI_AgentOptions_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Explicit model.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L677)

<a id="Vercel_AI_AgentOptions_ModelId_"></a>
<a id="Vercel_AI_AgentOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L680)

<a id="Vercel_AI_AgentOptions_OnAbort_"></a>
<a id="Vercel_AI_AgentOptions_OnAbort"></a>

### OnAbort


```csharp
public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; set; }
```


Called after `Stream` is aborted. See [OnAbort](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_OnAbort).

**Value:** [Func&lt;StreamAbortContext, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L848)

<a id="Vercel_AI_AgentOptions_OnEnd_"></a>
<a id="Vercel_AI_AgentOptions_OnEnd"></a>

### OnEnd


```csharp
public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; set; }
```


Called when a generation completes. It runs together with the callback of the call, and a failure of one does not stop the other.
Maps to `onFinish` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L784)

<a id="Vercel_AI_AgentOptions_OnError_"></a>
<a id="Vercel_AI_AgentOptions_OnError"></a>

### OnError


```csharp
public Func<Exception, CancellationToken, Task>? OnError { get; set; }
```


Called when an error occurs. For `Stream` it receives the errors of the stream.
For `GenerateAsync` it receives the error before it is thrown.
Aborts and timeouts are not reported. It can only be set here and not per call. Maps to `onError` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;Exception, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L717)

<a id="Vercel_AI_AgentOptions_OnFinish_"></a>
<a id="Vercel_AI_AgentOptions_OnFinish"></a>

### OnFinish


```csharp
[Obsolete("Use OnEnd instead.")]
public Func<GenerateTextResult, CancellationToken, Task>? OnFinish { get; set; }
```


Deprecated alias for [OnEnd](#Vercel_AI_AgentOptions_OnEnd); [OnEnd](#Vercel_AI_AgentOptions_OnEnd) wins when both are set. Maps to `onFinish` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L747)

<a id="Vercel_AI_AgentOptions_OnLanguageModelCallEnd_"></a>
<a id="Vercel_AI_AgentOptions_OnLanguageModelCallEnd"></a>

### OnLanguageModelCallEnd


```csharp
public Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; set; }
```


Called after each provider model call. It runs together with the callback of the call. Maps to `onLanguageModelCallEnd` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;LanguageModelCallEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L774)

<a id="Vercel_AI_AgentOptions_OnLanguageModelCallStart_"></a>
<a id="Vercel_AI_AgentOptions_OnLanguageModelCallStart"></a>

### OnLanguageModelCallStart


```csharp
public Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; set; }
```


Called before each provider model call. It runs together with the callback of the call. Maps to `onLanguageModelCallStart` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;LanguageModelCallStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L767)

<a id="Vercel_AI_AgentOptions_OnStart_"></a>
<a id="Vercel_AI_AgentOptions_OnStart"></a>

### OnStart


```csharp
public Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart { get; set; }
```


Called when a generation begins. It runs together with the callback of the call, and a failure of one does not stop the other.
Maps to `onStart` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L729)

<a id="Vercel_AI_AgentOptions_OnStepEnd_"></a>
<a id="Vercel_AI_AgentOptions_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called when each generation step ends. It runs together with the callback of the call, and a failure of one does not stop the other.
Maps to `onStepEnd` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L704)

<a id="Vercel_AI_AgentOptions_OnStepError_"></a>
<a id="Vercel_AI_AgentOptions_OnStepError"></a>

### OnStepError


```csharp
public Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError { get; set; }
```


Called when a step that started fails before it produces a step result. [OnStepEnd](#Vercel_AI_AgentOptions_OnStepEnd) is not called for such a step.
Unlike [OnStepEnd](#Vercel_AI_AgentOptions_OnStepEnd), it can only be set here and not per call. Maps to `onStepError` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextStepErrorEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L710)

<a id="Vercel_AI_AgentOptions_OnStepFinish_"></a>
<a id="Vercel_AI_AgentOptions_OnStepFinish"></a>

### OnStepFinish


```csharp
[Obsolete("Use OnStepEnd instead.")]
public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }
```


Deprecated alias for [OnStepEnd](#Vercel_AI_AgentOptions_OnStepEnd); [OnStepEnd](#Vercel_AI_AgentOptions_OnStepEnd) wins when both are set. Maps to `onStepFinish` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L697)

<a id="Vercel_AI_AgentOptions_OnStepStart_"></a>
<a id="Vercel_AI_AgentOptions_OnStepStart"></a>

### OnStepStart


```csharp
public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }
```


Called when each step begins. It runs together with the callback of the call. Maps to `onStepStart` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L732)

<a id="Vercel_AI_AgentOptions_OnToolExecutionEnd_"></a>
<a id="Vercel_AI_AgentOptions_OnToolExecutionEnd"></a>

### OnToolExecutionEnd


```csharp
public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; set; }
```


Called after a tool ran. It runs together with the callback of the call. Maps to `onToolExecutionEnd` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L758)

<a id="Vercel_AI_AgentOptions_OnToolExecutionStart_"></a>
<a id="Vercel_AI_AgentOptions_OnToolExecutionStart"></a>

### OnToolExecutionStart


```csharp
public Func<ToolExecutionStartEvent, CancellationToken, Task>? OnToolExecutionStart { get; set; }
```


Called before a tool runs. It runs together with the callback of the call. Maps to `onToolExecutionStart` of the `ToolLoopAgent` settings.

**Value:** [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L751)

<a id="Vercel_AI_AgentOptions_PrepareCall_"></a>
<a id="Vercel_AI_AgentOptions_PrepareCall"></a>

### PrepareCall


```csharp
public Func<GenerateTextOptions, GenerateTextOptions?>? PrepareCall { get; set; }
```


Called before each `GenerateAsync` and `Stream` call with the options the agent would use,
including the default [StopWhen](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_StopWhen). Return replacement options, or null to keep them.
A returned options object that sets [StopWhen](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_StopWhen) replaces the default limit. Maps to `prepareCall`.

**Value:** [Func&lt;GenerateTextOptions, GenerateTextOptions&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L845)

<a id="Vercel_AI_AgentOptions_RefineToolInput_"></a>
<a id="Vercel_AI_AgentOptions_RefineToolInput"></a>

### RefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; set; }
```


Refines the parsed input of tool calls. See [RefineToolInput](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RefineToolInput). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, CancellationToken, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L829)

<a id="Vercel_AI_AgentOptions_RepairToolCall_"></a>
<a id="Vercel_AI_AgentOptions_RepairToolCall"></a>

### RepairToolCall


```csharp
public ToolCallRepairFunction? RepairToolCall { get; set; }
```


Repairs a tool call that failed to parse. See [RepairToolCall](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RepairToolCall). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L826)

<a id="Vercel_AI_AgentOptions_RuntimeContext_"></a>
<a id="Vercel_AI_AgentOptions_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }
```


User-defined context that is passed to the approval functions. See [RuntimeContext](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_RuntimeContext). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L796)

<a id="Vercel_AI_AgentOptions_StopWhen_"></a>
<a id="Vercel_AI_AgentOptions_StopWhen"></a>

### StopWhen


```csharp
public StopCondition? StopWhen { get; set; }
```


Stop condition. Defaults to 10 steps.

**Value:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L805)

<a id="Vercel_AI_AgentOptions_Telemetry_"></a>
<a id="Vercel_AI_AgentOptions_Telemetry"></a>

### Telemetry


```csharp
public TelemetryOptions? Telemetry { get; set; }
```


Telemetry configuration of every call. See [Telemetry](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_Telemetry). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `telemetry`.

**Value:** [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L723)

<a id="Vercel_AI_AgentOptions_Temperature_"></a>
<a id="Vercel_AI_AgentOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L808)

<a id="Vercel_AI_AgentOptions_Timeout_"></a>
<a id="Vercel_AI_AgentOptions_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


Timeout of each `GenerateAsync` and `Stream` call. See [Timeout](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_Timeout).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `timeout` of `ToolLoopAgentSettings`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L838)

<a id="Vercel_AI_AgentOptions_ToolApproval_"></a>
<a id="Vercel_AI_AgentOptions_ToolApproval"></a>

### ToolApproval


```csharp
public ToolApprovalConfiguration? ToolApproval { get; set; }
```


Configures whether tools require approval before they run. See [ToolApproval](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolApproval).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `toolApproval`.

**Value:** [ToolApprovalConfiguration](/api/Vercel.AI.GenerateText/ToolApprovalConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L790)

<a id="Vercel_AI_AgentOptions_ToolCallConcurrency_"></a>
<a id="Vercel_AI_AgentOptions_ToolCallConcurrency"></a>

### ToolCallConcurrency


```csharp
public int? ToolCallConcurrency { get; set; }
```


Maximum number of tool calls that may execute concurrently within a step. See [ToolCallConcurrency](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolCallConcurrency).
[PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L823)

<a id="Vercel_AI_AgentOptions_ToolOrder_"></a>
<a id="Vercel_AI_AgentOptions_ToolOrder"></a>

### ToolOrder


```csharp
public IReadOnlyList<string>? ToolOrder { get; set; }
```


Controls the order in which tools are sent to the provider. See [ToolOrder](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolOrder). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L817)

<a id="Vercel_AI_AgentOptions_Tools_"></a>
<a id="Vercel_AI_AgentOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; set; }
```


Tools.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L694)

<a id="Vercel_AI_AgentOptions_ToolsContext_"></a>
<a id="Vercel_AI_AgentOptions_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; set; }
```


The context of each tool, keyed by tool name. See [ToolsContext](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolsContext). [PrepareCall](#Vercel_AI_AgentOptions_PrepareCall) can replace it. Maps to `toolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Agent.cs#L793)
