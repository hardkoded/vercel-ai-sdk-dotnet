---
title: "ParallelModelCatalog"
description: "Parallel model catalog. Maps to parallel-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParallelModelCatalog"
  hidden: true
---

<a id="Vercel_AI_Parallel_ParallelModelCatalog"></a>

Namespace: [Vercel.AI.Parallel](/api/Vercel.AI.Parallel/)


```csharp
public static class ParallelModelCatalog
```


Parallel model catalog. Maps to `parallel-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelModelCatalog.cs#L21)

## Properties

<a id="Vercel_AI_Parallel_ParallelModelCatalog_DecisionModelIds_"></a>
<a id="Vercel_AI_Parallel_ParallelModelCatalog_DecisionModelIds"></a>

### DecisionModelIds


```csharp
public static IReadOnlyList<string> DecisionModelIds { get; }
```


Known decision model ids. Maps to `parallelDecisionModelIds`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelModelCatalog.cs#L24)

<a id="Vercel_AI_Parallel_ParallelModelCatalog_Entries_"></a>
<a id="Vercel_AI_Parallel_ParallelModelCatalog_Entries"></a>

### Entries


```csharp
public static IReadOnlyDictionary<string, ModelEntry<object>> Entries { get; }
```


Entries of every known model id. Maps to `parallelModelCatalog`.

**Value:** [IReadOnlyDictionary&lt;string, ModelEntry&lt;object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelModelCatalog.cs#L31)

<a id="Vercel_AI_Parallel_ParallelModelCatalog_SupportedQuestionTypes_"></a>
<a id="Vercel_AI_Parallel_ParallelModelCatalog_SupportedQuestionTypes"></a>

### SupportedQuestionTypes


```csharp
public static IReadOnlyList<string> SupportedQuestionTypes { get; }
```


Question types every Parallel decision model supports. Maps to `parallelSupportedQuestionTypes`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelModelCatalog.cs#L27)

## Methods

<a id="Vercel_AI_Parallel_ParallelModelCatalog_GetParallelDecisionModelEntry_"></a>
<a id="Vercel_AI_Parallel_ParallelModelCatalog_GetParallelDecisionModelEntry_System_String_"></a>

### GetParallelDecisionModelEntry(string)


```csharp
public static ModelEntry<object> GetParallelDecisionModelEntry(string modelId)
```


Returns the entry of a decision model. An unknown model id has unknown option support. Maps to `getParallelDecisionModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `ModelEntry&lt;object&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelModelCatalog.cs#L39)
