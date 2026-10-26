---
title: "GenerateTextOptions"
description: "Options for CancellationToken). Property names follow generateText."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateTextOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateTextOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public class GenerateTextOptions
```


Options for `CancellationToken)`. Property names follow `generateText`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [StreamTextOptions](/api/Vercel.AI/StreamTextOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L623)

## Properties

<a id="Vercel_AI_GenerateTextOptions_ActiveTools_"></a>
<a id="Vercel_AI_GenerateTextOptions_ActiveTools"></a>

### ActiveTools


```csharp
public IReadOnlyList<string>? ActiveTools { get; set; }
```


Tool names allowed on this call. Null allows every tool.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L653)

<a id="Vercel_AI_GenerateTextOptions_ApproveTool_"></a>
<a id="Vercel_AI_GenerateTextOptions_ApproveTool"></a>

### ApproveTool


```csharp
public Func<ToolApprovalRequest, CancellationToken, Task<bool>>? ApproveTool { get; set; }
```


Approves a tool call before it runs. A false result records an error tool result.

**Value:** [Func&lt;ToolApprovalRequest, CancellationToken, Task&lt;bool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L746)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalRepairToolCall_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalRepairToolCall"></a>

### ExperimentalRepairToolCall


```csharp
[Obsolete("Use RepairToolCall instead.")]
public ToolCallRepairFunction? ExperimentalRepairToolCall { get; set; }
```


Deprecated alias of [RepairToolCall](#Vercel_AI_GenerateTextOptions_RepairToolCall). [RepairToolCall](#Vercel_AI_GenerateTextOptions_RepairToolCall) wins when both are set.
Maps to `experimental_repairToolCall`.

**Value:** [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L758)

<a id="Vercel_AI_GenerateTextOptions_FrequencyPenalty_"></a>
<a id="Vercel_AI_GenerateTextOptions_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Frequency penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L686)

<a id="Vercel_AI_GenerateTextOptions_Headers_"></a>
<a id="Vercel_AI_GenerateTextOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra headers forwarded to the language model.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L710)

<a id="Vercel_AI_GenerateTextOptions_Instructions_"></a>
<a id="Vercel_AI_GenerateTextOptions_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


System prompt: a string, one [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/), or an array of them. Maps to `instructions`.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L641)

<a id="Vercel_AI_GenerateTextOptions_MaxOutputTokens_"></a>
<a id="Vercel_AI_GenerateTextOptions_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L671)

<a id="Vercel_AI_GenerateTextOptions_MaxRetries_"></a>
<a id="Vercel_AI_GenerateTextOptions_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; set; }
```


Retries for provider calls made by this request. Providers that own their HTTP stack use their own policy.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L701)

<a id="Vercel_AI_GenerateTextOptions_Messages_"></a>
<a id="Vercel_AI_GenerateTextOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; set; }
```


Full prompt. Maps to `messages`.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L647)

<a id="Vercel_AI_GenerateTextOptions_Model_"></a>
<a id="Vercel_AI_GenerateTextOptions_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Explicit language model. Wins over [ModelId](#Vercel_AI_GenerateTextOptions_ModelId).

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L635)

<a id="Vercel_AI_GenerateTextOptions_ModelId_"></a>
<a id="Vercel_AI_GenerateTextOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id such as `openai/gpt-4.1-mini`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L632)

<a id="Vercel_AI_GenerateTextOptions_OnError_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnError"></a>

### OnError


```csharp
public Func<Exception, CancellationToken, Task>? OnError { get; set; }
```


Called when generation fails. Maps to `onError`.

**Value:** [Func&lt;Exception, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L743)

<a id="Vercel_AI_GenerateTextOptions_OnFinish_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnFinish"></a>

### OnFinish


```csharp
public Func<GenerateTextResult, CancellationToken, Task>? OnFinish { get; set; }
```


Called when generation finishes. Maps to `onFinish`.

**Value:** [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L740)

<a id="Vercel_AI_GenerateTextOptions_OnLanguageModelCallEnd_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnLanguageModelCallEnd"></a>

### OnLanguageModelCallEnd


```csharp
public Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; set; }
```


Called after each provider model call returns, before tools run and before [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd).
Maps to `onLanguageModelCallEnd`.

**Value:** [Func&lt;LanguageModelCallEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L722)

<a id="Vercel_AI_GenerateTextOptions_OnStepEnd_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called after each step. Maps to `onStepEnd`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L737)

<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionEnd_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionEnd"></a>

### OnToolExecutionEnd


```csharp
public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; set; }
```


Called after a tool's `Execute` returns or throws, before [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd). An exception it throws is ignored.
Maps to `onToolExecutionEnd`.

**Value:** [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L734)

<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionStart_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionStart"></a>

### OnToolExecutionStart


```csharp
public Func<ToolExecutionStartEvent, CancellationToken, Task>? OnToolExecutionStart { get; set; }
```


Called before a tool's `Execute` runs. It is not called for a tool without `Execute`, a provider-executed call,
or a call that is invalid or not approved. An exception it throws is ignored. Maps to `onToolExecutionStart`.

**Value:** [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L728)

<a id="Vercel_AI_GenerateTextOptions_Output_"></a>
<a id="Vercel_AI_GenerateTextOptions_Output"></a>

### Output


```csharp
public OutputSpec? Output { get; set; }
```


Structured output.

**Value:** [OutputSpec](/api/Vercel.AI/OutputSpec/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L668)

<a id="Vercel_AI_GenerateTextOptions_PrepareStep_"></a>
<a id="Vercel_AI_GenerateTextOptions_PrepareStep"></a>

### PrepareStep


```csharp
public Func<PrepareStepContext, PrepareStepUpdate?>? PrepareStep { get; set; }
```


Called before each step. Maps to `prepareStep`.

**Value:** [Func&lt;PrepareStepContext, PrepareStepUpdate&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L716)

<a id="Vercel_AI_GenerateTextOptions_PresencePenalty_"></a>
<a id="Vercel_AI_GenerateTextOptions_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L683)

<a id="Vercel_AI_GenerateTextOptions_Prompt_"></a>
<a id="Vercel_AI_GenerateTextOptions_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; set; }
```


A single user prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L644)

<a id="Vercel_AI_GenerateTextOptions_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateTextOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider-specific options.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L713)

<a id="Vercel_AI_GenerateTextOptions_Reasoning_"></a>
<a id="Vercel_AI_GenerateTextOptions_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort: `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, or `max`.
A provider option with the same setting wins when both are set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L698)

<a id="Vercel_AI_GenerateTextOptions_RefineToolInput_"></a>
<a id="Vercel_AI_GenerateTextOptions_RefineToolInput"></a>

### RefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; set; }
```


Refines the parsed input of a tool call, keyed by tool name, before the tool runs and before outputs and callbacks see it.
A tool without an entry keeps its parsed input. Maps to `experimental_refineToolInput`.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, CancellationToken, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L765)

<a id="Vercel_AI_GenerateTextOptions_RepairToolCall_"></a>
<a id="Vercel_AI_GenerateTextOptions_RepairToolCall"></a>

### RepairToolCall


```csharp
public ToolCallRepairFunction? RepairToolCall { get; set; }
```


Repairs a tool call whose tool is unknown or whose input is invalid, before the call is reported or executed.
Return the repaired call, or null to keep the error. Maps to `repairToolCall`.

**Value:** [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L752)

<a id="Vercel_AI_GenerateTextOptions_Seed_"></a>
<a id="Vercel_AI_GenerateTextOptions_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L692)

<a id="Vercel_AI_GenerateTextOptions_StopSequences_"></a>
<a id="Vercel_AI_GenerateTextOptions_StopSequences"></a>

### StopSequences


```csharp
public IReadOnlyList<string>? StopSequences { get; set; }
```


Stop sequences.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L689)

<a id="Vercel_AI_GenerateTextOptions_StopWhen_"></a>
<a id="Vercel_AI_GenerateTextOptions_StopWhen"></a>

### StopWhen


```csharp
public StopCondition? StopWhen { get; set; }
```


When to stop the tool loop. Defaults to one step.

**Value:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L665)

<a id="Vercel_AI_GenerateTextOptions_System_"></a>
<a id="Vercel_AI_GenerateTextOptions_System"></a>

### System


```csharp
public string? System { get; set; }
```


System prompt. [Instructions](#Vercel_AI_GenerateTextOptions_Instructions) wins when both are set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L638)

<a id="Vercel_AI_GenerateTextOptions_Temperature_"></a>
<a id="Vercel_AI_GenerateTextOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L674)

<a id="Vercel_AI_GenerateTextOptions_Timeout_"></a>
<a id="Vercel_AI_GenerateTextOptions_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


Aborts the call with a [TimeoutError](/api/Vercel.AI.Util/TimeoutError/) when a timeout expires. A [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) sets the total timeout.
[StepMs](/api/Vercel.AI.Prompt/TimeoutConfiguration/#Vercel_AI_Prompt_TimeoutConfiguration_StepMs), [FirstChunkMs](/api/Vercel.AI.Prompt/TimeoutConfiguration/#Vercel_AI_Prompt_TimeoutConfiguration_FirstChunkMs), and [ChunkMs](/api/Vercel.AI.Prompt/TimeoutConfiguration/#Vercel_AI_Prompt_TimeoutConfiguration_ChunkMs) set the others. Maps to `timeout`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L707)

<a id="Vercel_AI_GenerateTextOptions_ToolCallConcurrency_"></a>
<a id="Vercel_AI_GenerateTextOptions_ToolCallConcurrency"></a>

### ToolCallConcurrency


```csharp
public int? ToolCallConcurrency { get; set; }
```


Maximum number of tool calls that may execute concurrently within a step. Set to 1 to execute them one at a time in model order.
Null runs every tool call at once. Values below 1 throw [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/). Maps to `toolCallConcurrency`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L659)

<a id="Vercel_AI_GenerateTextOptions_ToolChoice_"></a>
<a id="Vercel_AI_GenerateTextOptions_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice? ToolChoice { get; set; }
```


Tool selection mode.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L662)

<a id="Vercel_AI_GenerateTextOptions_Tools_"></a>
<a id="Vercel_AI_GenerateTextOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; set; }
```


Tools available to the model.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L650)

<a id="Vercel_AI_GenerateTextOptions_TopK_"></a>
<a id="Vercel_AI_GenerateTextOptions_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


Top-k.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L680)

<a id="Vercel_AI_GenerateTextOptions_TopP_"></a>
<a id="Vercel_AI_GenerateTextOptions_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Top-p.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L677)
