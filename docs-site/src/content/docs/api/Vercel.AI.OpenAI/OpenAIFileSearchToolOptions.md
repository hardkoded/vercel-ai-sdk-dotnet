---
title: "OpenAIFileSearchToolOptions"
description: "Options of FileSearch(OpenAIFileSearchToolOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIFileSearchToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIFileSearchToolOptions
```


Options of [FileSearch(OpenAIFileSearchToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_FileSearch_Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L112)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_Filters_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_Filters"></a>

### Filters


```csharp
public OpenAIFileSearchFilter? Filters { get; set; }
```


A filter applied to the search.

**Value:** [OpenAIFileSearchFilter](/api/Vercel.AI.OpenAI/OpenAIFileSearchFilter/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L127)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_MaxNumResults_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_MaxNumResults"></a>

### MaxNumResults


```csharp
public int? MaxNumResults { get; set; }
```


Maximum number of results to return.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L118)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_Ranker_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_Ranker"></a>

### Ranker


```csharp
public string? Ranker { get; set; }
```


Ranker used for the search.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L121)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_ScoreThreshold_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_ScoreThreshold"></a>

### ScoreThreshold


```csharp
public double? ScoreThreshold { get; set; }
```


Score threshold between 0 and 1.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L124)

<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_VectorStoreIds_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_VectorStoreIds"></a>

### VectorStoreIds


```csharp
public IReadOnlyList<string> VectorStoreIds { get; set; }
```


Ids of the vector stores to search.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L115)
