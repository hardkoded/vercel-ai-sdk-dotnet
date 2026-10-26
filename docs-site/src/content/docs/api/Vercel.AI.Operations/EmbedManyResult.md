---
title: "EmbedManyResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedManyResult"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedManyResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbedManyResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L305)

## Constructors

<a id="Vercel_AI_Operations_EmbedManyResult__ctor_"></a>
<a id="Vercel_AI_Operations_EmbedManyResult__ctor_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_Double____Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ProviderResponse__"></a>

### `EmbedManyResult(IReadOnlyList<string>, IReadOnlyList<double[]>, OperationUsage, IReadOnlyList<OperationWarning>, JsonElement?, IReadOnlyList<ProviderResponse?>)`


```csharp
public EmbedManyResult(IReadOnlyList<string> values, IReadOnlyList<double[]> embeddings, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, IReadOnlyList<ProviderResponse?> responses)
```


Creates an embedMany result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `embeddings` | [IReadOnlyList&lt;double\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `responses` | [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L308)

## Properties

<a id="Vercel_AI_Operations_EmbedManyResult_Embeddings_"></a>
<a id="Vercel_AI_Operations_EmbedManyResult_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<double[]> Embeddings { get; }
```


Embeddings in value order.

**Value:** [IReadOnlyList&lt;double\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L322)

<a id="Vercel_AI_Operations_EmbedManyResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_EmbedManyResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Merged provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L331)

<a id="Vercel_AI_Operations_EmbedManyResult_Responses_"></a>
<a id="Vercel_AI_Operations_EmbedManyResult_Responses"></a>

### Responses


```csharp
public IReadOnlyList<ProviderResponse?> Responses { get; }
```


One response per model call.

**Value:** [IReadOnlyList&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L334)

<a id="Vercel_AI_Operations_EmbedManyResult_Usage_"></a>
<a id="Vercel_AI_Operations_EmbedManyResult_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Summed token usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L325)

<a id="Vercel_AI_Operations_EmbedManyResult_Values_"></a>
<a id="Vercel_AI_Operations_EmbedManyResult_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Values that were embedded.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L319)

<a id="Vercel_AI_Operations_EmbedManyResult_Warnings_"></a>
<a id="Vercel_AI_Operations_EmbedManyResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings from every chunk.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L328)
