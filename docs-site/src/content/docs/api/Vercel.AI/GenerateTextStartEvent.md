---
title: "GenerateTextStartEvent"
description: "Passed to OnStart when the generation begins, before any model call. Maps to GenerateTextStartEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateTextStartEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateTextStartEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class GenerateTextStartEvent : IOperationStartEvent
```


Passed to [OnStart](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnStart) when the generation begins, before any model call. Maps to `GenerateTextStartEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IOperationStartEvent](/api/Vercel.AI/IOperationStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1719)

## Properties

<a id="Vercel_AI_GenerateTextStartEvent_ActiveTools_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_ActiveTools"></a>

### ActiveTools


```csharp
public IReadOnlyList<string>? ActiveTools { get; init; }
```


Limits which tools are available for the model to call. Maps to `activeTools`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1746)

<a id="Vercel_AI_GenerateTextStartEvent_CallId_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; init; }
```


Unique identifier for this generation call, used to correlate events. Maps to `callId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1722)

<a id="Vercel_AI_GenerateTextStartEvent_FrequencyPenalty_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; init; }
```


Frequency penalty. Maps to `frequencyPenalty`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1767)

<a id="Vercel_AI_GenerateTextStartEvent_Headers_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; init; }
```


Additional HTTP headers sent with the request. Maps to `headers`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1785)

<a id="Vercel_AI_GenerateTextStartEvent_Instructions_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; init; }
```


Instructions of the call. Maps to `instructions`.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1734)

<a id="Vercel_AI_GenerateTextStartEvent_MaxOutputTokens_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; init; }
```


Maximum output tokens. Maps to `maxOutputTokens`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1752)

<a id="Vercel_AI_GenerateTextStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; init; }
```


Maximum number of retries for failed requests. Maps to `maxRetries`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1779)

<a id="Vercel_AI_GenerateTextStartEvent_Messages_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; init; }
```


Messages of the call, without the instructions. Maps to `messages`.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1737)

<a id="Vercel_AI_GenerateTextStartEvent_ModelId_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; init; }
```


The model identifier. Maps to `modelId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1731)

<a id="Vercel_AI_GenerateTextStartEvent_OperationId_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; init; }
```


Identifies the operation type, `ai.generateText`. Maps to `operationId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1725)

<a id="Vercel_AI_GenerateTextStartEvent_Output_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Output"></a>

### Output


```csharp
public OutputSpec? Output { get; init; }
```


The output specification for structured outputs, if configured. Maps to `output`.

**Value:** [OutputSpec](/api/Vercel.AI/OutputSpec/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1791)

<a id="Vercel_AI_GenerateTextStartEvent_PresencePenalty_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; init; }
```


Presence penalty. Maps to `presencePenalty`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1764)

<a id="Vercel_AI_GenerateTextStartEvent_Provider_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; init; }
```


The provider identifier. Maps to `provider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1728)

<a id="Vercel_AI_GenerateTextStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Additional provider-specific options. Maps to `providerOptions`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1788)

<a id="Vercel_AI_GenerateTextStartEvent_Reasoning_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; init; }
```


Reasoning effort. Maps to `reasoning`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1776)

<a id="Vercel_AI_GenerateTextStartEvent_RuntimeContext_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; init; }
```


User-defined runtime context. Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1801)

<a id="Vercel_AI_GenerateTextStartEvent_Seed_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Seed"></a>

### Seed


```csharp
public int? Seed { get; init; }
```


Seed. Maps to `seed`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1773)

<a id="Vercel_AI_GenerateTextStartEvent_StopSequences_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_StopSequences"></a>

### StopSequences


```csharp
public IReadOnlyList<string>? StopSequences { get; init; }
```


Stop sequences. Maps to `stopSequences`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1770)

<a id="Vercel_AI_GenerateTextStartEvent_Temperature_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; init; }
```


Temperature. Maps to `temperature`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1755)

<a id="Vercel_AI_GenerateTextStartEvent_Timeout_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Timeout"></a>

### Timeout


```csharp
public TimeoutConfiguration? Timeout { get; init; }
```


Timeout configuration for the generation. Maps to `timeout`.

**Value:** [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1782)

<a id="Vercel_AI_GenerateTextStartEvent_ToolChoice_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice? ToolChoice { get; init; }
```


The tool choice strategy for this generation. Maps to `toolChoice`.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1743)

<a id="Vercel_AI_GenerateTextStartEvent_ToolOrder_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_ToolOrder"></a>

### ToolOrder


```csharp
public IReadOnlyList<string>? ToolOrder { get; init; }
```


Controls the order in which tools are sent to the provider. Maps to `toolOrder`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1749)

<a id="Vercel_AI_GenerateTextStartEvent_Tools_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; init; }
```


The tools available for this generation. Maps to `tools`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1740)

<a id="Vercel_AI_GenerateTextStartEvent_ToolsContext_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; init; }
```


Tool context, keyed by tool name. Maps to `toolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1794)

<a id="Vercel_AI_GenerateTextStartEvent_TopK_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_TopK"></a>

### TopK


```csharp
public int? TopK { get; init; }
```


Top-k. Maps to `topK`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1761)

<a id="Vercel_AI_GenerateTextStartEvent_TopP_"></a>
<a id="Vercel_AI_GenerateTextStartEvent_TopP"></a>

### TopP


```csharp
public double? TopP { get; init; }
```


Top-p. Maps to `topP`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1758)
