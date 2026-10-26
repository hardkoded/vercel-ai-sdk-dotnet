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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L769)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L772)

## Properties

<a id="Vercel_AI_LanguageModelCallEndEvent_Content_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Content"></a>

### Content


```csharp
public IReadOnlyList<GeneratedContent> Content { get; }
```


Content the model generated, including its tool calls.

**Value:** [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L805)

<a id="Vercel_AI_LanguageModelCallEndEvent_FinishReason_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L799)

<a id="Vercel_AI_LanguageModelCallEndEvent_ModelId_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id the provider reported, or the requested one when it reported none.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L796)

<a id="Vercel_AI_LanguageModelCallEndEvent_Provider_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id of the model that was called.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L793)

<a id="Vercel_AI_LanguageModelCallEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the response, when the provider sent any.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L811)

<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseId_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; }
```


Provider response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L808)

<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseTime_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_ResponseTime"></a>

### ResponseTime


```csharp
public TimeSpan ResponseTime { get; }
```


Time the model call took. Maps to `performance.responseTimeMs`.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L814)

<a id="Vercel_AI_LanguageModelCallEndEvent_Usage_"></a>
<a id="Vercel_AI_LanguageModelCallEndEvent_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Usage of this model call.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L802)
