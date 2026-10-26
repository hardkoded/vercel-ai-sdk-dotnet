---
title: "LanguageModelUsage"
description: "Token counts for a model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelUsage"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelUsage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class LanguageModelUsage
```


Token counts for a model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L38)

## Constructors

<a id="Vercel_AI_Provider_LanguageModelUsage__ctor_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage__ctor_System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### LanguageModelUsage(int?, int?, int?, int?, int?, int?, JsonElement?, int?, int?)


```csharp
public LanguageModelUsage(int? inputTokens, int? outputTokens, int? totalTokens, int? cacheReadTokens = null, int? cacheWriteTokens = null, int? reasoningTokens = null, JsonElement? raw = null, int? noCacheInputTokens = null, int? textTokens = null)
```


Creates usage counts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `outputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `totalTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cacheReadTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cacheWriteTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `reasoningTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `raw` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `noCacheInputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `textTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L41)

## Properties

<a id="Vercel_AI_Provider_LanguageModelUsage_CacheReadTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_CacheReadTokens"></a>

### CacheReadTokens


```csharp
public int? CacheReadTokens { get; }
```


Input tokens read from cache, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L77)

<a id="Vercel_AI_Provider_LanguageModelUsage_CacheWriteTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_CacheWriteTokens"></a>

### CacheWriteTokens


```csharp
public int? CacheWriteTokens { get; }
```


Input tokens written to cache, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L80)

<a id="Vercel_AI_Provider_LanguageModelUsage_Empty_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_Empty"></a>

### Empty


```csharp
public static LanguageModelUsage Empty { get; }
```


Zero usage.

**Value:** [LanguageModelUsage](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L110)

<a id="Vercel_AI_Provider_LanguageModelUsage_InputTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_InputTokens"></a>

### InputTokens


```csharp
public int? InputTokens { get; }
```


Prompt tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L68)

<a id="Vercel_AI_Provider_LanguageModelUsage_NoCacheInputTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_NoCacheInputTokens"></a>

### NoCacheInputTokens


```csharp
public int? NoCacheInputTokens { get; }
```


Input tokens that were not served from cache, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L89)

<a id="Vercel_AI_Provider_LanguageModelUsage_Null_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_Null"></a>

### Null


```csharp
public static LanguageModelUsage Null { get; }
```


Usage with every count and [Raw](#Vercel_AI_Provider_LanguageModelUsage_Raw) unset. It is the start value for adding steps.

**Value:** [LanguageModelUsage](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L113)

<a id="Vercel_AI_Provider_LanguageModelUsage_OutputTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_OutputTokens"></a>

### OutputTokens


```csharp
public int? OutputTokens { get; }
```


Generated tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L71)

<a id="Vercel_AI_Provider_LanguageModelUsage_Raw_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; }
```


Provider usage object, unchanged.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L86)

<a id="Vercel_AI_Provider_LanguageModelUsage_ReasoningTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_ReasoningTokens"></a>

### ReasoningTokens


```csharp
public int? ReasoningTokens { get; }
```


Output tokens spent on reasoning, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L83)

<a id="Vercel_AI_Provider_LanguageModelUsage_TextTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_TextTokens"></a>

### TextTokens


```csharp
public int? TextTokens { get; }
```


Output tokens that were not reasoning tokens, when the provider reports them.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L92)

<a id="Vercel_AI_Provider_LanguageModelUsage_TotalTokens_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_TotalTokens"></a>

### TotalTokens


```csharp
public int? TotalTokens { get; }
```


Input plus output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L74)

## Methods

<a id="Vercel_AI_Provider_LanguageModelUsage_Add_"></a>
<a id="Vercel_AI_Provider_LanguageModelUsage_Add_Vercel_AI_Provider_LanguageModelUsage_Vercel_AI_Provider_LanguageModelUsage_"></a>

### Add(LanguageModelUsage, LanguageModelUsage)


```csharp
public static LanguageModelUsage Add(LanguageModelUsage left, LanguageModelUsage right)
```


Adds two usage values. [Raw](#Vercel_AI_Provider_LanguageModelUsage_Raw) survives only when one side is [Null](#Vercel_AI_Provider_LanguageModelUsage_Null).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `left` | [LanguageModelUsage](./) |  |
| `right` | [LanguageModelUsage](./) |  |

**Returns:** [LanguageModelUsage](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L95)
