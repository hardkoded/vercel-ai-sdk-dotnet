---
title: "AnthropicBatch"
description: "Validation and status mapping for the Anthropic Message Batches API."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicBatch"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicBatch"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicBatch
```


Validation and status mapping for the Anthropic Message Batches API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L37)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicBatch_Count_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_Count_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_"></a>

### Count(int, int, int, int, int)


```csharp
public static AnthropicBatchCounts Count(int processing, int succeeded, int errored, int canceled, int expired)
```


Builds SDK counts from the provider request_counts object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `processing` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `succeeded` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `errored` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `canceled` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `expired` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [AnthropicBatchCounts](/api/Vercel.AI.Anthropic/AnthropicBatchCounts/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L94)

<a id="Vercel_AI_Anthropic_AnthropicBatch_Count_System_Text_Json_JsonElement_"></a>

### Count(JsonElement)


```csharp
public static AnthropicBatchCounts Count(JsonElement batch)
```


Reads counts from a batch JSON object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `batch` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [AnthropicBatchCounts](/api/Vercel.AI.Anthropic/AnthropicBatchCounts/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L104)

<a id="Vercel_AI_Anthropic_AnthropicBatch_MapStatus_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_MapStatus_System_String_"></a>

### MapStatus(string?)


```csharp
public static string MapStatus(string? processingStatus)
```


Maps processing status. `ended` is completed. Other known states are pending.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `processingStatus` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L88)

<a id="Vercel_AI_Anthropic_AnthropicBatch_MissingResults_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_MissingResults_System_Text_Json_JsonElement_"></a>

### MissingResults(JsonElement)


```csharp
public static bool MissingResults(JsonElement batch)
```


True when a completed batch has no results URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `batch` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L120)

<a id="Vercel_AI_Anthropic_AnthropicBatch_RejectSpeed_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_RejectSpeed_System_String_System_Boolean_"></a>

### RejectSpeed(string?, bool)


```csharp
public static void RejectSpeed(string? speed, bool fallbackSpeed)
```


Rejects speed and fallback speed, which batches do not support.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `speed` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fallbackSpeed` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L74)

<a id="Vercel_AI_Anthropic_AnthropicBatch_ResultsUnavailable_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_ResultsUnavailable_System_Text_Json_JsonElement_"></a>

### ResultsUnavailable(JsonElement)


```csharp
public static bool ResultsUnavailable(JsonElement batch)
```


True when the batch is still pending, so results cannot be read.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `batch` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L128)

<a id="Vercel_AI_Anthropic_AnthropicBatch_ValidateRequestId_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_ValidateRequestId_System_String_"></a>

### ValidateRequestId(string)


```csharp
public static void ValidateRequestId(string id)
```


Throws when &lt;code class="paramref"&gt;id&lt;/code&gt; is not 1–64 letters, digits, underscores, or hyphens.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L42)

<a id="Vercel_AI_Anthropic_AnthropicBatch_ValidateRequestIds_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_ValidateRequestIds_System_Collections_Generic_IEnumerable_System_String__"></a>

### `ValidateRequestIds(IEnumerable<string>)`


```csharp
public static void ValidateRequestIds(IEnumerable<string> ids)
```


Validates ids and rejects duplicates before any HTTP call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `ids` | [IEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L51)

<a id="Vercel_AI_Anthropic_AnthropicBatch_ValidateRequestType_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatch_ValidateRequestType_System_String_"></a>

### ValidateRequestType(string?)


```csharp
public static void ValidateRequestType(string? type)
```


Rejects request types the batches API does not accept, such as image.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L65)
