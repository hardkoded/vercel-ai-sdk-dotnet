---
title: "ToolExecutionStartEvent"
description: "Passed to OnToolExecutionStart before a tool runs."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecutionStartEvent"
  hidden: true
---

<a id="Vercel_AI_ToolExecutionStartEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public class ToolExecutionStartEvent
```


Passed to [OnToolExecutionStart](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnToolExecutionStart) before a tool runs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ToolExecutionEndEvent](/api/Vercel.AI/ToolExecutionEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2075)

## Constructors

<a id="Vercel_AI_ToolExecutionStartEvent__ctor_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent__ctor_Vercel_AI_Provider_GeneratedToolCall_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `ToolExecutionStartEvent(GeneratedToolCall, IReadOnlyList<ModelMessage>)`


```csharp
public ToolExecutionStartEvent(GeneratedToolCall toolCall, IReadOnlyList<ModelMessage> messages)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2078)

## Properties

<a id="Vercel_AI_ToolExecutionStartEvent_AiVersion_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2106)

<a id="Vercel_AI_ToolExecutionStartEvent_CallId_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_CallId"></a>

### CallId


```csharp
public string? CallId { get; init; }
```


Unique identifier of the generation this tool call belongs to. Maps to `callId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2085)

<a id="Vercel_AI_ToolExecutionStartEvent_FunctionId_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations. Maps to `functionId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2103)

<a id="Vercel_AI_ToolExecutionStartEvent_Messages_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


The messages the model received in the step that made the call.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2094)

<a id="Vercel_AI_ToolExecutionStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations. Maps to `recordInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2097)

<a id="Vercel_AI_ToolExecutionStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations. Maps to `recordOutputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2100)

<a id="Vercel_AI_ToolExecutionStartEvent_ToolCall_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The call about to run.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2088)

<a id="Vercel_AI_ToolExecutionStartEvent_ToolContext_"></a>
<a id="Vercel_AI_ToolExecutionStartEvent_ToolContext"></a>

### ToolContext


```csharp
public JsonElement? ToolContext { get; init; }
```


The validated context of the tool. Maps to `toolContext`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2091)
