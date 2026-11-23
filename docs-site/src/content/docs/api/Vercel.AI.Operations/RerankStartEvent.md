---
title: "RerankStartEvent"
description: "Start event for rerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankStartEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankStartEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankStartEvent : IOperationStartEvent
```


Start event for rerank.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IOperationStartEvent](/api/Vercel.AI/IOperationStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L239)

## Constructors

<a id="Vercel_AI_Operations_RerankStartEvent__ctor_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_System_Collections_Generic_IReadOnlyList_System_Object__System_String_System_Nullable_System_Int32__System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__"></a>

### `RerankStartEvent(string, string, IReadOnlyDictionary<string, object?>, string, string, IReadOnlyList<object?>, string, int?, int, IReadOnlyDictionary<string, string>?, JsonElement?)`


```csharp
public RerankStartEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, IReadOnlyList<object?> documents, string query, int? topN, int maxRetries, IReadOnlyDictionary<string, string>? headers, JsonElement? providerOptions)
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
| `documents` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `topN` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L242)

## Properties

<a id="Vercel_AI_Operations_RerankStartEvent_AiVersion_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L303)

<a id="Vercel_AI_Operations_RerankStartEvent_CallId_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L270)

<a id="Vercel_AI_Operations_RerankStartEvent_Documents_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_Documents"></a>

### Documents


```csharp
public IReadOnlyList<object?> Documents { get; }
```


Submitted documents.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L285)

<a id="Vercel_AI_Operations_RerankStartEvent_FunctionId_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId). Set only on events sent to telemetry integrations.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L312)

<a id="Vercel_AI_Operations_RerankStartEvent_Headers_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Request headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L297)

<a id="Vercel_AI_Operations_RerankStartEvent_MaxRetries_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; }
```


Retry limit.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L294)

<a id="Vercel_AI_Operations_RerankStartEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L282)

<a id="Vercel_AI_Operations_RerankStartEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.rerank`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L273)

<a id="Vercel_AI_Operations_RerankStartEvent_Provider_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L279)

<a id="Vercel_AI_Operations_RerankStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L300)

<a id="Vercel_AI_Operations_RerankStartEvent_Query_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_Query"></a>

### Query


```csharp
public string Query { get; }
```


Query.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L288)

<a id="Vercel_AI_Operations_RerankStartEvent_RecordInputs_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L306)

<a id="Vercel_AI_Operations_RerankStartEvent_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs). Set only on events sent to telemetry integrations.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L309)

<a id="Vercel_AI_Operations_RerankStartEvent_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; }
```


Runtime context.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L276)

<a id="Vercel_AI_Operations_RerankStartEvent_TopN_"></a>
<a id="Vercel_AI_Operations_RerankStartEvent_TopN"></a>

### TopN


```csharp
public int? TopN { get; }
```


Maximum results.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L291)
