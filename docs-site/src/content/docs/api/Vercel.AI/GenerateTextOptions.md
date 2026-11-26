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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1461)

## Properties

<a id="Vercel_AI_GenerateTextOptions_ActiveTools_"></a>
<a id="Vercel_AI_GenerateTextOptions_ActiveTools"></a>

### ActiveTools


```csharp
public IReadOnlyList<string>? ActiveTools { get; set; }
```


Tool names allowed on this call. Null allows every tool.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1541)

<a id="Vercel_AI_GenerateTextOptions_AllowSystemInMessages_"></a>
<a id="Vercel_AI_GenerateTextOptions_AllowSystemInMessages"></a>

### AllowSystemInMessages


```csharp
public bool AllowSystemInMessages { get; set; }
```


Whether system messages are allowed in [Messages](#Vercel_AI_GenerateTextOptions_Messages). Defaults to false, which rejects them with an [InvalidPromptException](/api/Vercel.AI.Prompt/InvalidPromptException/).
Use [Instructions](#Vercel_AI_GenerateTextOptions_Instructions) for the system prompt instead. Maps to `allowSystemInMessages`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1535)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalDownload_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalDownload"></a>

### ExperimentalDownload


```csharp
public Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? ExperimentalDownload { get; set; }
```


Custom download function for URLs in the prompt and for generated file URLs returned by the model. Pass `DownloadFetch)`
to route them through a custom [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/). By default, prompt files are downloaded when the model does not support
their URL (see [SupportedUrls](/api/Vercel.AI.Provider/ILanguageModel/#Vercel_AI_Provider_ILanguageModel_SupportedUrls)), and generated file URLs are downloaded with [Download](/api/Vercel.AI.Util/Download/).
The function receives one [DownloadRequestItem](/api/Vercel.AI.Util/DownloadRequestItem/) per prompt URL, with [IsUrlSupportedByModel](/api/Vercel.AI.Util/DownloadRequestItem/#Vercel_AI_Util_DownloadRequestItem_IsUrlSupportedByModel) set,
and returns `null` for a URL the model can read itself. Generated file URLs are always downloaded, so returning `null`
for one throws a [DownloadException](/api/Vercel.AI.GenerateText/DownloadException/).
Maps to `experimental_download`.

**Value:** [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1508)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalInclude_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalInclude"></a>

### ExperimentalInclude


```csharp
[Obsolete("Use Include instead.")]
public GenerateTextInclude? ExperimentalInclude { get; set; }
```


Deprecated alias of [Include](#Vercel_AI_GenerateTextOptions_Include). Maps to `experimental_include`.

**Value:** [GenerateTextInclude](/api/Vercel.AI/GenerateTextInclude/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1478)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1734)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalSandbox_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalSandbox"></a>

### ExperimentalSandbox


```csharp
public ISandboxSession? ExperimentalSandbox { get; set; }
```


The sandbox environment that is passed through to tool execution. Maps to `experimental_sandbox`.

**Value:** [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1619)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalSteering_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalSteering"></a>

### ExperimentalSteering


```csharp
public ISteering? ExperimentalSteering { get; set; }
```


Experimental. A steering object created with [ExperimentalCreateSteering()](/api/Vercel.AI.GenerateText/Steering/#Vercel_AI_GenerateText_Steering_ExperimentalCreateSteering) that adds user messages to the generation
at the next step boundary. A steering object can only be used for one generation. Maps to `experimental_steering`.

**Value:** [ISteering](/api/Vercel.AI.GenerateText/ISteering/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1616)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalTelemetry_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalTelemetry"></a>

### ExperimentalTelemetry


```csharp
public TelemetryOptions? ExperimentalTelemetry { get; set; }
```


Deprecated alias of [Telemetry](#Vercel_AI_GenerateTextOptions_Telemetry). Maps to `experimental_telemetry`.

**Value:** [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1470)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecret_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecret"></a>

### ExperimentalToolApprovalSecret


```csharp
public string? ExperimentalToolApprovalSecret { get; set; }
```


Secret for HMAC-signing tool approval requests. When it is set, approval requests are signed when they are issued, and
the signature is verified when the approval is sent back, which prevents forged approvals.
Approval requests of automatically denied tool calls are not signed.
Maps to `experimental_toolApprovalSecret` given as a string. See also [ExperimentalToolApprovalSecretBytes](#Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecretBytes).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1714)

<a id="Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecretBytes_"></a>
<a id="Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecretBytes"></a>

### ExperimentalToolApprovalSecretBytes


```csharp
public byte[]? ExperimentalToolApprovalSecretBytes { get; set; }
```


The same secret as [ExperimentalToolApprovalSecret](#Vercel_AI_GenerateTextOptions_ExperimentalToolApprovalSecret), given as bytes. Maps to
`experimental_toolApprovalSecret` given as a `Uint8Array`. It is used when the string secret is not set.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1720)

<a id="Vercel_AI_GenerateTextOptions_FrequencyPenalty_"></a>
<a id="Vercel_AI_GenerateTextOptions_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Frequency penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1580)

<a id="Vercel_AI_GenerateTextOptions_Headers_"></a>
<a id="Vercel_AI_GenerateTextOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra headers forwarded to the language model.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1604)

<a id="Vercel_AI_GenerateTextOptions_Include_"></a>
<a id="Vercel_AI_GenerateTextOptions_Include"></a>

### Include


```csharp
public GenerateTextInclude? Include { get; set; }
```


What to retain in the step results. By default request bodies, request messages and response bodies are excluded. Maps to `include`.

**Value:** [GenerateTextInclude](/api/Vercel.AI/GenerateTextInclude/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1475)

<a id="Vercel_AI_GenerateTextOptions_Instructions_"></a>
<a id="Vercel_AI_GenerateTextOptions_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


System prompt: a string, one [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/), or an array of them. Maps to `instructions`.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1523)

<a id="Vercel_AI_GenerateTextOptions_MaxOutputTokens_"></a>
<a id="Vercel_AI_GenerateTextOptions_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1565)

<a id="Vercel_AI_GenerateTextOptions_MaxRetries_"></a>
<a id="Vercel_AI_GenerateTextOptions_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; set; }
```


Retries for provider calls made by this request. Providers that own their HTTP stack use their own policy.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1595)

<a id="Vercel_AI_GenerateTextOptions_Messages_"></a>
<a id="Vercel_AI_GenerateTextOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; set; }
```


Full prompt. Maps to `messages`.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1529)

<a id="Vercel_AI_GenerateTextOptions_Model_"></a>
<a id="Vercel_AI_GenerateTextOptions_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Explicit language model. Wins over [ModelId](#Vercel_AI_GenerateTextOptions_ModelId).

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1517)

<a id="Vercel_AI_GenerateTextOptions_ModelId_"></a>
<a id="Vercel_AI_GenerateTextOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id such as `openai/gpt-4.1-mini`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1514)

<a id="Vercel_AI_GenerateTextOptions_OnEnd_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnEnd"></a>

### OnEnd


```csharp
public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; set; }
```


Called when all steps are finished and the response is complete. Maps to `onEnd`.

**Value:** [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1680)

<a id="Vercel_AI_GenerateTextOptions_OnError_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnError"></a>

### OnError


```csharp
public Func<Exception, CancellationToken, Task>? OnError { get; set; }
```


Called when generation fails. Maps to `onError`.

**Value:** [Func&lt;Exception, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1687)

<a id="Vercel_AI_GenerateTextOptions_OnFinish_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnFinish"></a>

### OnFinish


```csharp
[Obsolete("Use OnEnd instead.")]
public Func<GenerateTextResult, CancellationToken, Task>? OnFinish { get; set; }
```


Deprecated alias for [OnEnd](#Vercel_AI_GenerateTextOptions_OnEnd); [OnEnd](#Vercel_AI_GenerateTextOptions_OnEnd) wins when both are set. Maps to `onFinish`.

**Value:** [Func&lt;GenerateTextResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1683)

<a id="Vercel_AI_GenerateTextOptions_OnLanguageModelCallEnd_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnLanguageModelCallEnd"></a>

### OnLanguageModelCallEnd


```csharp
public Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd { get; set; }
```


Called after each provider model call returns, before tools run and before [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd).
Maps to `onLanguageModelCallEnd`.

**Value:** [Func&lt;LanguageModelCallEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1650)

<a id="Vercel_AI_GenerateTextOptions_OnLanguageModelCallStart_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnLanguageModelCallStart"></a>

### OnLanguageModelCallStart


```csharp
public Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart { get; set; }
```


Called immediately before each provider model call begins. An exception it throws is ignored.
Maps to `onLanguageModelCallStart`.

**Value:** [Func&lt;LanguageModelCallStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1640)

<a id="Vercel_AI_GenerateTextOptions_OnStart_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnStart"></a>

### OnStart


```csharp
public Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart { get; set; }
```


Called when the generation begins, before any model call. An exception it throws is ignored. Maps to `onStart`.

**Value:** [Func&lt;GenerateTextStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1622)

<a id="Vercel_AI_GenerateTextOptions_OnStepEnd_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }
```


Called after each step. Maps to `onStepEnd`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1673)

<a id="Vercel_AI_GenerateTextOptions_OnStepError_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnStepError"></a>

### OnStepError


```csharp
public Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError { get; set; }
```


Called when a step that started fails before it produces a step result, that is when the provider call throws or rejects,
or, for `GenerateTextAsync`, when the call is cancelled or times out. [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd) is not called for such a step.
For `StreamTextAsync` it is also called when the model stream ends without output; a provider error part still ends the
step with [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd), and a cancel or a timeout ends it with [OnAbort](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_OnAbort).
An exception it throws is ignored. Maps to `onStepError`.

**Value:** [Func&lt;GenerateTextStepErrorEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1634)

<a id="Vercel_AI_GenerateTextOptions_OnStepFinish_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnStepFinish"></a>

### OnStepFinish


```csharp
[Obsolete("Use OnStepEnd instead.")]
public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }
```


Deprecated alias for [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd); [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd) wins when both are set. Maps to `onStepFinish`.

**Value:** [Func&lt;StepResult, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1676)

<a id="Vercel_AI_GenerateTextOptions_OnStepStart_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnStepStart"></a>

### OnStepStart


```csharp
public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }
```


Called when each step begins, before the provider is called. An exception it throws is ignored. Maps to `onStepStart`.

**Value:** [Func&lt;GenerateTextStepStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1625)

<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionEnd_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionEnd"></a>

### OnToolExecutionEnd


```csharp
public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; set; }
```


Called after a tool's `Execute` returns or throws, before [OnStepEnd](#Vercel_AI_GenerateTextOptions_OnStepEnd). An exception it throws is ignored.
Maps to `onToolExecutionEnd`.

**Value:** [Func&lt;ToolExecutionEndEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1670)

<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionStart_"></a>
<a id="Vercel_AI_GenerateTextOptions_OnToolExecutionStart"></a>

### OnToolExecutionStart


```csharp
public Func<ToolExecutionStartEvent, CancellationToken, Task>? OnToolExecutionStart { get; set; }
```


Called before a tool's `Execute` runs. It is not called for a tool without `Execute`, a provider-executed call,
or a call that is invalid or not approved. An exception it throws is ignored. Maps to `onToolExecutionStart`.

**Value:** [Func&lt;ToolExecutionStartEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1660)

<a id="Vercel_AI_GenerateTextOptions_Output_"></a>
<a id="Vercel_AI_GenerateTextOptions_Output"></a>

### Output


```csharp
public OutputSpec? Output { get; set; }
```


Structured output.

**Value:** [OutputSpec](/api/Vercel.AI/OutputSpec/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1562)

<a id="Vercel_AI_GenerateTextOptions_PrepareStep_"></a>
<a id="Vercel_AI_GenerateTextOptions_PrepareStep"></a>

### PrepareStep


```csharp
public Func<PrepareStepContext, PrepareStepUpdate?>? PrepareStep { get; set; }
```


Called before each step. Maps to `prepareStep`.

**Value:** [Func&lt;PrepareStepContext, PrepareStepUpdate&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1610)

<a id="Vercel_AI_GenerateTextOptions_PresencePenalty_"></a>
<a id="Vercel_AI_GenerateTextOptions_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1577)

<a id="Vercel_AI_GenerateTextOptions_Prompt_"></a>
<a id="Vercel_AI_GenerateTextOptions_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; set; }
```


A single user prompt.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1526)

<a id="Vercel_AI_GenerateTextOptions_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateTextOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider-specific options.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1607)

<a id="Vercel_AI_GenerateTextOptions_Reasoning_"></a>
<a id="Vercel_AI_GenerateTextOptions_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Reasoning effort: `none`, `minimal`, `low`, `medium`, `high`, `xhigh`, or `max`.
A provider option with the same setting wins when both are set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1592)

<a id="Vercel_AI_GenerateTextOptions_RefineToolInput_"></a>
<a id="Vercel_AI_GenerateTextOptions_RefineToolInput"></a>

### RefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; set; }
```


Refines the parsed input of a tool call, keyed by tool name, before the tool runs and before outputs and callbacks see it.
A tool without an entry keeps its parsed input. Maps to `experimental_refineToolInput`.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, CancellationToken, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1741)

<a id="Vercel_AI_GenerateTextOptions_RepairToolCall_"></a>
<a id="Vercel_AI_GenerateTextOptions_RepairToolCall"></a>

### RepairToolCall


```csharp
public ToolCallRepairFunction? RepairToolCall { get; set; }
```


Repairs a tool call whose tool is unknown or whose input is invalid, before the call is reported or executed.
Return the repaired call, or null to keep the error. Maps to `repairToolCall`.

**Value:** [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1728)

<a id="Vercel_AI_GenerateTextOptions_RuntimeContext_"></a>
<a id="Vercel_AI_GenerateTextOptions_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }
```


User-defined context that is passed to the approval functions. Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1706)

<a id="Vercel_AI_GenerateTextOptions_Seed_"></a>
<a id="Vercel_AI_GenerateTextOptions_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1586)

<a id="Vercel_AI_GenerateTextOptions_StopSequences_"></a>
<a id="Vercel_AI_GenerateTextOptions_StopSequences"></a>

### StopSequences


```csharp
public IReadOnlyList<string>? StopSequences { get; set; }
```


Stop sequences.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1583)

<a id="Vercel_AI_GenerateTextOptions_StopWhen_"></a>
<a id="Vercel_AI_GenerateTextOptions_StopWhen"></a>

### StopWhen


```csharp
public StopCondition? StopWhen { get; set; }
```


When to stop the tool loop. Defaults to one step.

**Value:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1559)

<a id="Vercel_AI_GenerateTextOptions_System_"></a>
<a id="Vercel_AI_GenerateTextOptions_System"></a>

### System


```csharp
public string? System { get; set; }
```


System prompt. [Instructions](#Vercel_AI_GenerateTextOptions_Instructions) wins when both are set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1520)

<a id="Vercel_AI_GenerateTextOptions_Telemetry_"></a>
<a id="Vercel_AI_GenerateTextOptions_Telemetry"></a>

### Telemetry


```csharp
public TelemetryOptions? Telemetry { get; set; }
```


Telemetry configuration. Maps to `telemetry`.

**Value:** [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1467)

<a id="Vercel_AI_GenerateTextOptions_Temperature_"></a>
<a id="Vercel_AI_GenerateTextOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1568)

<a id="Vercel_AI_GenerateTextOptions_Timeout_"></a>
<a id="Vercel_AI_GenerateTextOptions_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; set; }
```


Aborts the call with a [TimeoutError](/api/Vercel.AI.Util/TimeoutError/) when a timeout expires. A [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) sets the total timeout.
[StepMs](/api/Vercel.AI.Prompt/TimeoutConfiguration/#Vercel_AI_Prompt_TimeoutConfiguration_StepMs), [FirstChunkMs](/api/Vercel.AI.Prompt/TimeoutConfiguration/#Vercel_AI_Prompt_TimeoutConfiguration_FirstChunkMs), and [ChunkMs](/api/Vercel.AI.Prompt/TimeoutConfiguration/#Vercel_AI_Prompt_TimeoutConfiguration_ChunkMs) set the others. Maps to `timeout`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1601)

<a id="Vercel_AI_GenerateTextOptions_ToolApproval_"></a>
<a id="Vercel_AI_GenerateTextOptions_ToolApproval"></a>

### ToolApproval


```csharp
public ToolApprovalConfiguration? ToolApproval { get; set; }
```


Configures whether tools require approval before they run: a generic function for all tool calls, or a status
(or a function) for each tool. It takes precedence over [NeedsApproval](/api/Vercel.AI/Tool/#Vercel_AI_Tool_NeedsApproval). Maps to `toolApproval`.

**Value:** [ToolApprovalConfiguration](/api/Vercel.AI.GenerateText/ToolApprovalConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1697)

<a id="Vercel_AI_GenerateTextOptions_ToolCallConcurrency_"></a>
<a id="Vercel_AI_GenerateTextOptions_ToolCallConcurrency"></a>

### ToolCallConcurrency


```csharp
public int? ToolCallConcurrency { get; set; }
```


Maximum number of tool calls that may execute concurrently within a step. Set to 1 to execute them one at a time in model order.
Null runs every tool call at once. Values below 1 throw [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/). Maps to `toolCallConcurrency`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1553)

<a id="Vercel_AI_GenerateTextOptions_ToolChoice_"></a>
<a id="Vercel_AI_GenerateTextOptions_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice? ToolChoice { get; set; }
```


Tool selection mode.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1556)

<a id="Vercel_AI_GenerateTextOptions_ToolOrder_"></a>
<a id="Vercel_AI_GenerateTextOptions_ToolOrder"></a>

### ToolOrder


```csharp
public IReadOnlyList<string>? ToolOrder { get; set; }
```


Controls the order in which tools are sent to the provider. The list can be partial. Tools not listed are sent after the listed tools,
sorted alphabetically. This can improve provider-side caching by keeping tool definitions in a stable order. Maps to `toolOrder`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1547)

<a id="Vercel_AI_GenerateTextOptions_Tools_"></a>
<a id="Vercel_AI_GenerateTextOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; set; }
```


Tools available to the model.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1538)

<a id="Vercel_AI_GenerateTextOptions_ToolsContext_"></a>
<a id="Vercel_AI_GenerateTextOptions_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; set; }
```


The context of each tool, keyed by tool name. It is validated with the [ContextSchema](/api/Vercel.AI/Tool/#Vercel_AI_Tool_ContextSchema) of the tool and then
passed to the approval functions. Maps to `toolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1703)

<a id="Vercel_AI_GenerateTextOptions_TopK_"></a>
<a id="Vercel_AI_GenerateTextOptions_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


Top-k.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1574)

<a id="Vercel_AI_GenerateTextOptions_TopP_"></a>
<a id="Vercel_AI_GenerateTextOptions_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Top-p.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1571)
