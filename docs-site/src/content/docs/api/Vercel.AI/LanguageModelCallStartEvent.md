---
title: "LanguageModelCallStartEvent"
description: "Passed to OnLanguageModelCallStart before the model is called."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelCallStartEvent"
  hidden: true
---

<a id="Vercel_AI_LanguageModelCallStartEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class LanguageModelCallStartEvent
```


Passed to [OnLanguageModelCallStart](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnLanguageModelCallStart) before the model is called.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2032)

## Properties

<a id="Vercel_AI_LanguageModelCallStartEvent_AiVersion_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2065)

<a id="Vercel_AI_LanguageModelCallStartEvent_CallId_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; init; }
```


Id of the model call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2035)

<a id="Vercel_AI_LanguageModelCallStartEvent_CallOptions_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_CallOptions"></a>

### CallOptions


```csharp
public LanguageModelCallOptions CallOptions { get; init; }
```


Call settings.

**Value:** [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2053)

<a id="Vercel_AI_LanguageModelCallStartEvent_DurableContext_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_DurableContext"></a>

### DurableContext


```csharp
public DurableTelemetryEventContext? DurableContext { get; init; }
```


Identifies live model events and the run events reported after completion of a durable run. Maps to `durableContext`.

**Value:** [DurableTelemetryEventContext](/api/Vercel.AI/DurableTelemetryEventContext/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2071)

<a id="Vercel_AI_LanguageModelCallStartEvent_FunctionId_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


The [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId) of the call. Set only on events sent to telemetry integrations. Maps to `functionId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2062)

<a id="Vercel_AI_LanguageModelCallStartEvent_Instructions_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; init; }
```


Instructions of the call.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2044)

<a id="Vercel_AI_LanguageModelCallStartEvent_Messages_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; init; }
```


Messages of the call, without the instructions.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2047)

<a id="Vercel_AI_LanguageModelCallStartEvent_ModelId_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; init; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2041)

<a id="Vercel_AI_LanguageModelCallStartEvent_Provider_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; init; }
```


Provider id of the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2038)

<a id="Vercel_AI_LanguageModelCallStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Whether input recording was enabled for the call, as [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs) set it. Set only on events sent to telemetry integrations. Maps to `recordInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2056)

<a id="Vercel_AI_LanguageModelCallStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Whether output recording was enabled for the call, as [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs) set it. Set only on events sent to telemetry integrations. Maps to `recordOutputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2059)

<a id="Vercel_AI_LanguageModelCallStartEvent_Timestamp_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Timestamp"></a>

### Timestamp


```csharp
public long? Timestamp { get; init; }
```


Time at which the event occurred, in milliseconds since the Unix epoch. Set when events are reported after they happened, for example when a durable workflow reports a run that spanned several processes. Integrations use it instead of the current time when present. Maps to `timestamp`.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2068)

<a id="Vercel_AI_LanguageModelCallStartEvent_Tools_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Tools"></a>

### Tools


```csharp
public IReadOnlyList<LanguageModelTool>? Tools { get; init; }
```


Tools offered to the model.

**Value:** [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2050)
