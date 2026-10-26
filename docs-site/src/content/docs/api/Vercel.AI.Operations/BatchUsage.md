---
title: "BatchUsage"
description: "Usage returned to batch callers."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchUsage"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchUsage"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchUsage
```


Usage returned to batch callers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L174)

## Constructors

<a id="Vercel_AI_Operations_BatchUsage__ctor_"></a>
<a id="Vercel_AI_Operations_BatchUsage__ctor_System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Text_Json_JsonElement__"></a>

### BatchUsage(int?, int?, int?, int?, int?, int?, int?, int?, JsonElement?)


```csharp
public BatchUsage(int? inputTokens, int? noCacheTokens, int? cacheReadTokens, int? cacheWriteTokens, int? outputTokens, int? textTokens, int? reasoningTokens, int? totalTokens, JsonElement? raw)
```


Creates caller usage.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `noCacheTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cacheReadTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cacheWriteTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `outputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `textTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `reasoningTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `totalTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `raw` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L177)

## Properties

<a id="Vercel_AI_Operations_BatchUsage_CacheReadTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_CacheReadTokens"></a>

### CacheReadTokens


```csharp
public int? CacheReadTokens { get; }
```


Cache-read tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L197)

<a id="Vercel_AI_Operations_BatchUsage_CacheWriteTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_CacheWriteTokens"></a>

### CacheWriteTokens


```csharp
public int? CacheWriteTokens { get; }
```


Cache-write tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L200)

<a id="Vercel_AI_Operations_BatchUsage_InputTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_InputTokens"></a>

### InputTokens


```csharp
public int? InputTokens { get; }
```


Input tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L191)

<a id="Vercel_AI_Operations_BatchUsage_NoCacheTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_NoCacheTokens"></a>

### NoCacheTokens


```csharp
public int? NoCacheTokens { get; }
```


Uncached input tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L194)

<a id="Vercel_AI_Operations_BatchUsage_OutputTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_OutputTokens"></a>

### OutputTokens


```csharp
public int? OutputTokens { get; }
```


Output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L203)

<a id="Vercel_AI_Operations_BatchUsage_Raw_"></a>
<a id="Vercel_AI_Operations_BatchUsage_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; }
```


Raw usage.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L215)

<a id="Vercel_AI_Operations_BatchUsage_ReasoningTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_ReasoningTokens"></a>

### ReasoningTokens


```csharp
public int? ReasoningTokens { get; }
```


Reasoning tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L209)

<a id="Vercel_AI_Operations_BatchUsage_TextTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_TextTokens"></a>

### TextTokens


```csharp
public int? TextTokens { get; }
```


Text tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L206)

<a id="Vercel_AI_Operations_BatchUsage_TotalTokens_"></a>
<a id="Vercel_AI_Operations_BatchUsage_TotalTokens"></a>

### TotalTokens


```csharp
public int? TotalTokens { get; }
```


Input plus output when both totals are known.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L212)

## Methods

<a id="Vercel_AI_Operations_BatchUsage_FromModel_"></a>
<a id="Vercel_AI_Operations_BatchUsage_FromModel_Vercel_AI_Operations_BatchModelUsage_"></a>

### FromModel(BatchModelUsage)


```csharp
public static BatchUsage FromModel(BatchModelUsage usage)
```


Maps provider usage the way `asLanguageModelUsage` does.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [BatchModelUsage](/api/Vercel.AI.Operations/BatchModelUsage/) |  |

**Returns:** [BatchUsage](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L218)
