---
title: "BatchModelUsage"
description: "Provider token usage before it is reshaped."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchModelUsage"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchModelUsage"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchModelUsage
```


Provider token usage before it is reshaped.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L133)

## Constructors

<a id="Vercel_AI_Operations_BatchModelUsage__ctor_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage__ctor_System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Text_Json_JsonElement__"></a>

### BatchModelUsage(int?, int?, int?, int?, int?, int?, int?, JsonElement?)


```csharp
public BatchModelUsage(int? inputTotal = null, int? noCache = null, int? cacheRead = null, int? cacheWrite = null, int? outputTotal = null, int? text = null, int? reasoning = null, JsonElement? raw = null)
```


Creates provider usage.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputTotal` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `noCache` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cacheRead` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cacheWrite` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `outputTotal` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `text` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `reasoning` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `raw` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L136)

## Properties

<a id="Vercel_AI_Operations_BatchModelUsage_CacheRead_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_CacheRead"></a>

### CacheRead


```csharp
public int? CacheRead { get; }
```


Cache-read tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L155)

<a id="Vercel_AI_Operations_BatchModelUsage_CacheWrite_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_CacheWrite"></a>

### CacheWrite


```csharp
public int? CacheWrite { get; }
```


Cache-write tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L158)

<a id="Vercel_AI_Operations_BatchModelUsage_InputTotal_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_InputTotal"></a>

### InputTotal


```csharp
public int? InputTotal { get; }
```


Input total.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L149)

<a id="Vercel_AI_Operations_BatchModelUsage_NoCache_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_NoCache"></a>

### NoCache


```csharp
public int? NoCache { get; }
```


Uncached input tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L152)

<a id="Vercel_AI_Operations_BatchModelUsage_OutputTotal_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_OutputTotal"></a>

### OutputTotal


```csharp
public int? OutputTotal { get; }
```


Output total.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L161)

<a id="Vercel_AI_Operations_BatchModelUsage_Raw_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; }
```


Raw usage.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L170)

<a id="Vercel_AI_Operations_BatchModelUsage_Reasoning_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_Reasoning"></a>

### Reasoning


```csharp
public int? Reasoning { get; }
```


Reasoning tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L167)

<a id="Vercel_AI_Operations_BatchModelUsage_Text_"></a>
<a id="Vercel_AI_Operations_BatchModelUsage_Text"></a>

### Text


```csharp
public int? Text { get; }
```


Text tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L164)
