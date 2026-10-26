---
title: "CohereRerankResult"
description: "Cohere rerank result. Provider metadata stays unset."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereRerankResult"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereRerankResult"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereRerankResult
```


Cohere rerank result. Provider metadata stays unset.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L366)

## Constructors

<a id="Vercel_AI_Cohere_CohereRerankResult__ctor_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_RerankItem__System_Collections_Generic_IReadOnlyList_Vercel_AI_Cohere_CohereRerankWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Cohere_CohereRerankResponse_"></a>

### `CohereRerankResult(IReadOnlyList<RerankItem>, IReadOnlyList<CohereRerankWarning>, JsonElement?, CohereRerankResponse)`


```csharp
public CohereRerankResult(IReadOnlyList<RerankItem> ranking, IReadOnlyList<CohereRerankWarning> warnings, JsonElement? providerMetadata, CohereRerankResponse response)
```


Creates a rerank result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `ranking` | [IReadOnlyList&lt;RerankItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;CohereRerankWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [CohereRerankResponse](/api/Vercel.AI.Cohere/CohereRerankResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L369)

## Properties

<a id="Vercel_AI_Cohere_CohereRerankResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Always null. The raw payload is [Body](/api/Vercel.AI.Cohere/CohereRerankResponse/#Vercel_AI_Cohere_CohereRerankResponse_Body).

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L384)

<a id="Vercel_AI_Cohere_CohereRerankResult_Ranking_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResult_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RerankItem> Ranking { get; }
```


Documents in provider order.

**Value:** [IReadOnlyList&lt;RerankItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L378)

<a id="Vercel_AI_Cohere_CohereRerankResult_Response_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResult_Response"></a>

### Response


```csharp
public CohereRerankResponse Response { get; }
```


Response id, headers, and body.

**Value:** [CohereRerankResponse](/api/Vercel.AI.Cohere/CohereRerankResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L387)

<a id="Vercel_AI_Cohere_CohereRerankResult_Warnings_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CohereRerankWarning> Warnings { get; }
```


Warnings. Object documents produce one compatibility warning.

**Value:** [IReadOnlyList&lt;CohereRerankWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L381)
