---
title: "RerankModelResponse"
description: "Model response for doRerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankModelResponse"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankModelResponse"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankModelResponse
```


Model response for `doRerank`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L44)

## Constructors

<a id="Vercel_AI_Operations_RerankModelResponse__ctor_"></a>
<a id="Vercel_AI_Operations_RerankModelResponse__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_RerankModelRank__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `RerankModelResponse(IReadOnlyList<RerankModelRank>, IReadOnlyList<OperationWarning>?, JsonElement?, ProviderResponse?)`


```csharp
public RerankModelResponse(IReadOnlyList<RerankModelRank> ranking, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
```


Creates a model response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `ranking` | [IReadOnlyList&lt;RerankModelRank&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L47)

## Properties

<a id="Vercel_AI_Operations_RerankModelResponse_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_RerankModelResponse_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L62)

<a id="Vercel_AI_Operations_RerankModelResponse_Ranking_"></a>
<a id="Vercel_AI_Operations_RerankModelResponse_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RerankModelRank> Ranking { get; }
```


Ranked indexes.

**Value:** [IReadOnlyList&lt;RerankModelRank&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L56)

<a id="Vercel_AI_Operations_RerankModelResponse_Response_"></a>
<a id="Vercel_AI_Operations_RerankModelResponse_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Raw response.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L65)

<a id="Vercel_AI_Operations_RerankModelResponse_Warnings_"></a>
<a id="Vercel_AI_Operations_RerankModelResponse_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L59)
