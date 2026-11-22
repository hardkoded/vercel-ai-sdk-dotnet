---
title: "ExtractionModelCallEndEvent"
description: "Event fired after the extraction model response has been validated. Maps to ExtractionModelCallEndEvent. Contains the result of the logical model call, inclu..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionModelCallEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionModelCallEndEvent : ExtractionModelCallStartEvent
```


Event fired after the extraction model response has been validated. Maps to `ExtractionModelCallEndEvent`.
Contains the result of the logical model call, including any retries.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ExtractionModelCallStartEvent](/api/Vercel.AI.Operations/ExtractionModelCallStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L228)

## Constructors

<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent__ctor_Vercel_AI_Operations_ExtractionModelCallStartEvent_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionAnswer__Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `ExtractionModelCallEndEvent(ExtractionModelCallStartEvent, IReadOnlyDictionary<string, ExtractionAnswer>, OperationUsage?, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse?)`


```csharp
public ExtractionModelCallEndEvent(ExtractionModelCallStartEvent start, IReadOnlyDictionary<string, ExtractionAnswer> answers, OperationUsage? usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse? response)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `start` | [ExtractionModelCallStartEvent](/api/Vercel.AI.Operations/ExtractionModelCallStartEvent/) |  |
| `answers` | [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L231)

## Properties

<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Answers_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }
```


Exactly one answer per field id. Telemetry receives an empty map when outputs are not recorded.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L260)

<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Optional provider-specific metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L269)

<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Optional raw response metadata from the provider.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L272)

<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Usage_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Token usage reported by the extraction model, before total tokens are derived.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L263)

<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_ExtractionModelCallEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from the extraction model.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractEvents.cs#L266)
