---
title: "EmbedResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedResult"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbedResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L272)

## Constructors

<a id="Vercel_AI_Operations_EmbedResult__ctor_"></a>
<a id="Vercel_AI_Operations_EmbedResult__ctor_System_String_System_Double___Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `EmbedResult(string, double[], OperationUsage, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse?)`


```csharp
public EmbedResult(string value, double[] embedding, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse? response)
```


Creates an embed result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `embedding` | [double\[\]](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L275)

## Properties

<a id="Vercel_AI_Operations_EmbedResult_Embedding_"></a>
<a id="Vercel_AI_Operations_EmbedResult_Embedding"></a>

### Embedding


```csharp
public double[] Embedding { get; }
```


Embedding vector.

**Value:** [double\[\]](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L289)

<a id="Vercel_AI_Operations_EmbedResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_EmbedResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L298)

<a id="Vercel_AI_Operations_EmbedResult_Response_"></a>
<a id="Vercel_AI_Operations_EmbedResult_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Provider response.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L301)

<a id="Vercel_AI_Operations_EmbedResult_Usage_"></a>
<a id="Vercel_AI_Operations_EmbedResult_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Token usage.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L292)

<a id="Vercel_AI_Operations_EmbedResult_Value_"></a>
<a id="Vercel_AI_Operations_EmbedResult_Value"></a>

### Value


```csharp
public string Value { get; }
```


Value that was embedded.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L286)

<a id="Vercel_AI_Operations_EmbedResult_Warnings_"></a>
<a id="Vercel_AI_Operations_EmbedResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L295)
