---
title: "EmbedStartEvent"
description: "Start event for embed and embedMany."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbedStartEvent : IOperationStartEvent
```


Start event for `embed` and `embedMany`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IOperationStartEvent](/api/Vercel.AI/IOperationStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L119)

## Constructors

<a id="Vercel_AI_Operations_EmbedStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_Vercel_AI_EmbedEventValue_System_String_System_Collections_Generic_IReadOnlyList_System_String___System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Int32__"></a>

### `EmbedStartEvent(string, string, IReadOnlyDictionary<string, object?>, string, string, EmbedEventValue<string, IReadOnlyList<string>>, int, IReadOnlyDictionary<string, string>, JsonElement?, int?)`


```csharp
public EmbedStartEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, EmbedEventValue<string, IReadOnlyList<string>> value, int maxRetries, IReadOnlyDictionary<string, string> headers, JsonElement? providerOptions, int? dimensions = null)
```


Creates a start event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | `EmbedEventValue&lt;string, IReadOnlyList&lt;string&gt;&gt;` |  |
| `maxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L122)

## Properties

<a id="Vercel_AI_Operations_EmbedStartEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L178)

<a id="Vercel_AI_Operations_EmbedStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Correlates start and end.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L148)

<a id="Vercel_AI_Operations_EmbedStartEvent_Dimensions_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; }
```


Requested output dimensions, when specified.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L166)

<a id="Vercel_AI_Operations_EmbedStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L187)

<a id="Vercel_AI_Operations_EmbedStartEvent_Headers_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers sent to the model.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L172)

<a id="Vercel_AI_Operations_EmbedStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; }
```


Resolved retry limit.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L169)

<a id="Vercel_AI_Operations_EmbedStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L160)

<a id="Vercel_AI_Operations_EmbedStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.embed` or `ai.embedMany`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L151)

<a id="Vercel_AI_Operations_EmbedStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L157)

<a id="Vercel_AI_Operations_EmbedStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L175)

<a id="Vercel_AI_Operations_EmbedStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L181)

<a id="Vercel_AI_Operations_EmbedStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L184)

<a id="Vercel_AI_Operations_EmbedStartEvent_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; }
```


Caller runtime context.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L154)

<a id="Vercel_AI_Operations_EmbedStartEvent_Value_"></a>
<a id="Vercel_AI_Operations_EmbedStartEvent_Value"></a>

### Value


```csharp
public EmbedEventValue<string, IReadOnlyList<string>> Value { get; }
```


A string for embed, or the value list for embedMany.

**Value:** `EmbedEventValue&lt;string, IReadOnlyList&lt;string&gt;&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L163)
