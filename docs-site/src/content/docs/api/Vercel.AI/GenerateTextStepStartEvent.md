---
title: "GenerateTextStepStartEvent"
description: "Passed to OnStepStart when a step begins, before the provider is called. Maps to GenerateTextStepStartEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateTextStepStartEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateTextStepStartEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public class GenerateTextStepStartEvent
```


Passed to [OnStepStart](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnStepStart) when a step begins, before the provider is called. Maps to `GenerateTextStepStartEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [GenerateTextStepErrorEvent](/api/Vercel.AI/GenerateTextStepErrorEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1856)

## Constructors

<a id="Vercel_AI_GenerateTextStepStartEvent__ctor_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent__ctor"></a>

### GenerateTextStepStartEvent()


```csharp
public GenerateTextStepStartEvent()
```


Creates an empty event.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1859)

<a id="Vercel_AI_GenerateTextStepStartEvent__ctor_Vercel_AI_GenerateTextStepStartEvent_"></a>

### GenerateTextStepStartEvent(GenerateTextStepStartEvent)


```csharp
protected GenerateTextStepStartEvent(GenerateTextStepStartEvent source)
```


Creates an event with the values of &lt;code class="paramref"&gt;source&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | [GenerateTextStepStartEvent](./) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1862)

## Properties

<a id="Vercel_AI_GenerateTextStepStartEvent_ActiveTools_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_ActiveTools"></a>

### ActiveTools


```csharp
public IReadOnlyList<string>? ActiveTools { get; init; }
```


Limits which tools are available for this step. Maps to `activeTools`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1906)

<a id="Vercel_AI_GenerateTextStepStartEvent_CallId_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; init; }
```


Unique identifier for this generation call, used to correlate events. Maps to `callId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1882)

<a id="Vercel_AI_GenerateTextStepStartEvent_Instructions_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; init; }
```


Instructions of the step. Maps to `instructions`.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1894)

<a id="Vercel_AI_GenerateTextStepStartEvent_Messages_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; init; }
```


Messages of the step, without the instructions. Maps to `messages`.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1897)

<a id="Vercel_AI_GenerateTextStepStartEvent_ModelId_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; init; }
```


The model identifier. Maps to `modelId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1888)

<a id="Vercel_AI_GenerateTextStepStartEvent_Output_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_Output"></a>

### Output


```csharp
public OutputSpec? Output { get; init; }
```


The output specification for structured outputs, if configured. Maps to `output`.

**Value:** [OutputSpec](/api/Vercel.AI/OutputSpec/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1924)

<a id="Vercel_AI_GenerateTextStepStartEvent_Provider_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; init; }
```


The provider identifier. Maps to `provider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1885)

<a id="Vercel_AI_GenerateTextStepStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Additional provider-specific options for this step. Maps to `providerOptions`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1921)

<a id="Vercel_AI_GenerateTextStepStartEvent_RuntimeContext_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; init; }
```


User-defined runtime context. Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1927)

<a id="Vercel_AI_GenerateTextStepStartEvent_StepNumber_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; init; }
```


Zero-based index of the current step. Maps to `stepNumber`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1891)

<a id="Vercel_AI_GenerateTextStepStartEvent_Steps_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_Steps"></a>

### Steps


```csharp
public IReadOnlyList<StepResult> Steps { get; init; }
```


Results of the previous steps, empty for the first step. Maps to `steps`.

**Value:** [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1912)

<a id="Vercel_AI_GenerateTextStepStartEvent_ToolChoice_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice? ToolChoice { get; init; }
```


The tool choice configuration for this step. Maps to `toolChoice`.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1903)

<a id="Vercel_AI_GenerateTextStepStartEvent_ToolOrder_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_ToolOrder"></a>

### ToolOrder


```csharp
public IReadOnlyList<string>? ToolOrder { get; init; }
```


Controls the order in which tools are sent to the provider for this step. Maps to `toolOrder`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1909)

<a id="Vercel_AI_GenerateTextStepStartEvent_Tools_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; init; }
```


The tools available for this generation. Maps to `tools`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1900)

<a id="Vercel_AI_GenerateTextStepStartEvent_ToolsContext_"></a>
<a id="Vercel_AI_GenerateTextStepStartEvent_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; init; }
```


Tool context, keyed by tool name. Maps to `toolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1934)
