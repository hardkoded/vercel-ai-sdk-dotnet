---
title: "EmbeddingModelResponse"
description: "One embedding returned by a model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingModelResponse"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbeddingModelResponse"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbeddingModelResponse
```


One embedding returned by a model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L11)

## Constructors

<a id="Vercel_AI_Operations_EmbeddingModelResponse__ctor_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelResponse__ctor_System_Collections_Generic_IReadOnlyList_System_Double____System_Nullable_System_Double__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_System_Boolean_"></a>

### `EmbeddingModelResponse(IReadOnlyList<double[]>, double?, IReadOnlyList<OperationWarning>?, JsonElement?, ProviderResponse?, bool)`


```csharp
public EmbeddingModelResponse(IReadOnlyList<double[]> embeddings, double? tokens = null, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null, bool usageOmitted = false)
```


Creates a model response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `embeddings` | [IReadOnlyList&lt;double\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `tokens` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |
| `usageOmitted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L14)

## Properties

<a id="Vercel_AI_Operations_EmbeddingModelResponse_Embeddings_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelResponse_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<double[]> Embeddings { get; }
```


Vectors in request order.

**Value:** [IReadOnlyList&lt;double\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L30)

<a id="Vercel_AI_Operations_EmbeddingModelResponse_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelResponse_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L39)

<a id="Vercel_AI_Operations_EmbeddingModelResponse_Response_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelResponse_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Raw response.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L42)

<a id="Vercel_AI_Operations_EmbeddingModelResponse_Tokens_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelResponse_Tokens"></a>

### Tokens


```csharp
public double Tokens { get; }
```


Token count. [NaN](https://learn.microsoft.com/dotnet/api/system.double.nan) when the provider omitted usage.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L33)

<a id="Vercel_AI_Operations_EmbeddingModelResponse_Warnings_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelResponse_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Provider warnings. Missing warnings become an empty list.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L36)
