---
title: "EmbeddingModelCallStartEvent"
description: "Event fired before an individual doEmbed call. Maps to EmbeddingModelCallStartEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingModelCallStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class EmbeddingModelCallStartEvent
```


Event fired before an individual `doEmbed` call. Maps to `EmbeddingModelCallStartEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EmbeddingModelCallEndEvent](/api/Vercel.AI.Operations/EmbeddingModelCallEndEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L268)

## Constructors

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent__ctor_System_String_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__"></a>

### `EmbeddingModelCallStartEvent(string, string, string, string, string, IReadOnlyList<string>, int?)`


```csharp
public EmbeddingModelCallStartEvent(string callId, string embedCallId, string operationId, string provider, string modelId, IReadOnlyList<string> values, int? dimensions = null)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `embedCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L271)

## Properties

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L312)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Operation call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L291)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_Dimensions_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; }
```


Requested output dimensions, when specified.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L309)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_EmbedCallId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_EmbedCallId"></a>

### EmbedCallId


```csharp
public string EmbedCallId { get; }
```


Id of this `doEmbed` invocation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L294)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L321)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L303)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.embed.doEmbed` or `ai.embedMany.doEmbed`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L297)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L300)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L315)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L318)

<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_Values_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallStartEvent_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Values in this call.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L306)
