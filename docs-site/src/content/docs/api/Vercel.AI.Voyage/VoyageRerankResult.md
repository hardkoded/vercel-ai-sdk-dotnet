---
title: "VoyageRerankResult"
description: "Voyage rerank result."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageRerankResult"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageRerankResult"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageRerankResult
```


Voyage rerank result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L108)

## Constructors

<a id="Vercel_AI_Voyage_VoyageRerankResult__ctor_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_RerankItem__System_Collections_Generic_IReadOnlyList_Vercel_AI_Voyage_VoyageWarning__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_"></a>

### `VoyageRerankResult(IReadOnlyList<RerankItem>, IReadOnlyList<VoyageWarning>, IReadOnlyDictionary<string, string>, JsonElement)`


```csharp
public VoyageRerankResult(IReadOnlyList<RerankItem> ranking, IReadOnlyList<VoyageWarning> warnings, IReadOnlyDictionary<string, string> headers, JsonElement body)
```


Creates a rerank result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `ranking` | [IReadOnlyList&lt;RerankItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;VoyageWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L111)

## Properties

<a id="Vercel_AI_Voyage_VoyageRerankResult_Body_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankResult_Body"></a>

### Body


```csharp
public JsonElement Body { get; }
```


Raw response body.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L129)

<a id="Vercel_AI_Voyage_VoyageRerankResult_Headers_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankResult_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L126)

<a id="Vercel_AI_Voyage_VoyageRerankResult_Ranking_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankResult_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RerankItem> Ranking { get; }
```


Documents in provider order.

**Value:** [IReadOnlyList&lt;RerankItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L120)

<a id="Vercel_AI_Voyage_VoyageRerankResult_Warnings_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<VoyageWarning> Warnings { get; }
```


Warnings. Object documents produce one compatibility warning.

**Value:** [IReadOnlyList&lt;VoyageWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L123)
