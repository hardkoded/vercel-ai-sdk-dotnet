---
title: "ExtractEndEvent"
description: "Event passed to OnEnd for extraction operations. Maps to ExtractEndEvent. Called when the operation completes successfully."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractEndEvent : ExtractStartEvent
```


Event passed to [OnEnd](/api/Vercel.AI.Operations/ExtractRequest/#Vercel_AI_Operations_ExtractRequest_OnEnd) for extraction operations. Maps to `ExtractEndEvent`.
Called when the operation completes successfully.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ExtractStartEvent](/api/Vercel.AI.Operations/ExtractStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L107)

## Constructors

<a id="Vercel_AI_Operations_ExtractEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractEndEvent__ctor_Vercel_AI_Operations_ExtractStartEvent_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionAnswer__Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `ExtractEndEvent(ExtractStartEvent, IReadOnlyDictionary<string, ExtractionAnswer>, OperationUsage, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse)`


```csharp
public ExtractEndEvent(ExtractStartEvent start, IReadOnlyDictionary<string, ExtractionAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `start` | [ExtractStartEvent](/api/Vercel.AI.Operations/ExtractStartEvent/) |  |
| `answers` | [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L110)

## Properties

<a id="Vercel_AI_Operations_ExtractEndEvent_Answers_"></a>
<a id="Vercel_AI_Operations_ExtractEndEvent_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }
```


Exactly one typed answer per field id. Telemetry receives an empty map when outputs are not recorded.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L145)

<a id="Vercel_AI_Operations_ExtractEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ExtractEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Optional provider-specific metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L154)

<a id="Vercel_AI_Operations_ExtractEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_ExtractEndEvent_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response metadata, including the resolved model id and timestamp.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L157)

<a id="Vercel_AI_Operations_ExtractEndEvent_Usage_"></a>
<a id="Vercel_AI_Operations_ExtractEndEvent_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Token usage for the extraction operation.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L148)

<a id="Vercel_AI_Operations_ExtractEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_ExtractEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from the extraction model.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L151)
