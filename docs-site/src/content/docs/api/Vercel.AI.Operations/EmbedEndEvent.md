---
title: "EmbedEndEvent"
description: "End event for embed and embedMany."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbedEndEvent
```


End event for `embed` and `embedMany`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L152)

## Constructors

<a id="Vercel_AI_Operations_EmbedEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_Vercel_AI_EmbedEventValue_System_String_System_Collections_Generic_IReadOnlyList_System_String___Vercel_AI_EmbedEventValue_System_Double___System_Collections_Generic_IReadOnlyList_System_Double_____Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_EmbedEventValue_Vercel_AI_Operations_ProviderResponse_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse___"></a>

### `EmbedEndEvent(string, string, IReadOnlyDictionary<string, object?>, string, string, EmbedEventValue<string, IReadOnlyList<string>>, EmbedEventValue<double[], IReadOnlyList<double[]>>, OperationUsage, IReadOnlyList<OperationWarning>, JsonElement?, EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>>)`


```csharp
public EmbedEndEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, EmbedEventValue<string, IReadOnlyList<string>> value, EmbedEventValue<double[], IReadOnlyList<double[]>> embedding, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>> response)
```


Creates an end event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | `EmbedEventValue&lt;string, IReadOnlyList&lt;string&gt;&gt;` |  |
| `embedding` | `EmbedEventValue&lt;double\[\], IReadOnlyList&lt;double\[\]&gt;&gt;` |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | `EmbedEventValue&lt;ProviderResponse, IReadOnlyList&lt;ProviderResponse&gt;&gt;` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L155)

## Properties

<a id="Vercel_AI_Operations_EmbedEndEvent_CallId_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Correlates start and end.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L171)

<a id="Vercel_AI_Operations_EmbedEndEvent_Embedding_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_Embedding"></a>

### Embedding


```csharp
public EmbedEventValue<double[], IReadOnlyList<double[]>> Embedding { get; }
```


One vector, or the vector list for embedMany.

**Value:** `EmbedEventValue&lt;double\[\], IReadOnlyList&lt;double\[\]&gt;&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L189)

<a id="Vercel_AI_Operations_EmbedEndEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L183)

<a id="Vercel_AI_Operations_EmbedEndEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.embed` or `ai.embedMany`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L174)

<a id="Vercel_AI_Operations_EmbedEndEvent_Provider_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L180)

<a id="Vercel_AI_Operations_EmbedEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Merged provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L198)

<a id="Vercel_AI_Operations_EmbedEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_Response"></a>

### Response


```csharp
public EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>> Response { get; }
```


One response for embed, or the response list for embedMany.

**Value:** `EmbedEventValue&lt;ProviderResponse, IReadOnlyList&lt;ProviderResponse&gt;&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L201)

<a id="Vercel_AI_Operations_EmbedEndEvent_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; }
```


Caller runtime context.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L177)

<a id="Vercel_AI_Operations_EmbedEndEvent_Usage_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Token usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L192)

<a id="Vercel_AI_Operations_EmbedEndEvent_Value_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_Value"></a>

### Value


```csharp
public EmbedEventValue<string, IReadOnlyList<string>> Value { get; }
```


Embedded value or values.

**Value:** `EmbedEventValue&lt;string, IReadOnlyList&lt;string&gt;&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L186)

<a id="Vercel_AI_Operations_EmbedEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_EmbedEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Aggregated warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L195)
