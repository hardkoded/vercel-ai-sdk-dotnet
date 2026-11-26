---
title: "LanguageModelCallEndEvent"
description: "Passed to OnLanguageModelCallEnd after a provider model call returns."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelCallEndEvent"
  hidden: true
---

<a id="Vercel_AI_LanguageModelCallEndEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class LanguageModelCallEndEvent
```


Passed to [OnLanguageModelCallEnd](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnLanguageModelCallEnd) after a provider model call returns.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2075)

## Constructors

<a id="Vercel_AI_LanguageModelCallEndEvent__ctor_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent__ctor_System_String_System_String_Vercel_AI_Provider_FinishReason_Vercel_AI_Provider_LanguageModelUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedContent__System_String_System_Nullable_System_Text_Json_JsonElement__System_TimeSpan_"></a>

### `LanguageModelCallEndEvent(string, string, FinishReason, LanguageModelUsage, IReadOnlyList<GeneratedContent>, string?, JsonElement?, TimeSpan)`


```csharp
public LanguageModelCallEndEvent(string provider, string modelId, FinishReason finishReason, LanguageModelUsage usage, IReadOnlyList<GeneratedContent> content, string? responseId, JsonElement? providerMetadata, TimeSpan responseTime)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `content` | [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responseId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `responseTime` | [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2078)

## Properties

<a id="Vercel_AI_LanguageModelCallEndEvent_AiVersion_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to `aiVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2142)

<a id="Vercel_AI_LanguageModelCallEndEvent_CallId_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_CallId"></a>

### CallId


```csharp
public string? CallId { get; init; }
```


Id of the model call, when the call has one. Maps to `callId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2124)

<a id="Vercel_AI_LanguageModelCallEndEvent_Content_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Content"></a>

### Content


```csharp
public IReadOnlyList<GeneratedContent> Content { get; }
```


Content the model generated, including its tool calls.

**Value:** [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2112)

<a id="Vercel_AI_LanguageModelCallEndEvent_FinishReason_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2106)

<a id="Vercel_AI_LanguageModelCallEndEvent_FunctionId_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


The [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId) of the call. Set only on events sent to telemetry integrations. Maps to `functionId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2139)

<a id="Vercel_AI_LanguageModelCallEndEvent_ModelId_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id the provider reported, or the requested one when it reported none.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2103)

<a id="Vercel_AI_LanguageModelCallEndEvent_Provider_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id of the model that was called.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2100)

<a id="Vercel_AI_LanguageModelCallEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the response, when the provider sent any.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2118)

<a id="Vercel_AI_LanguageModelCallEndEvent_RecordInputs_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Whether input recording was enabled for the call, as [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs) set it. Set only on events sent to telemetry integrations. Maps to `recordInputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2133)

<a id="Vercel_AI_LanguageModelCallEndEvent_RecordOutputs_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Whether output recording was enabled for the call, as [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs) set it. Set only on events sent to telemetry integrations. Maps to `recordOutputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2136)

<a id="Vercel_AI_LanguageModelCallEndEvent_Request_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Request"></a>

### Request


```csharp
public LanguageModelStreamRequest? Request { get; init; }
```


Request of the model call. Only [Url](/api/Vercel.AI.Provider/LanguageModelStreamRequest/#Vercel_AI_Provider_LanguageModelStreamRequest_Url) is set, reduced to its origin. Maps to `request`.

**Value:** [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2127)

<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseId_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; }
```


Provider response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2115)

<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseTime_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseTime"></a>

### ResponseTime


```csharp
public TimeSpan ResponseTime { get; }
```


Time the model call took. Maps to `performance.responseTimeMs`.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2121)

<a id="Vercel_AI_LanguageModelCallEndEvent_Timestamp_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Timestamp"></a>

### Timestamp


```csharp
public long? Timestamp { get; init; }
```


Time at which the event occurred, in milliseconds since the Unix epoch. Set when events are reported after they happened, for example when a durable workflow reports a run that spanned several processes. Integrations use it instead of the current time when present. Maps to `timestamp`.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2145)

<a id="Vercel_AI_LanguageModelCallEndEvent_Usage_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Usage of this model call.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2109)
