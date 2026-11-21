---
title: "ParallelDecisionProviderMetadata"
description: "Provider metadata of a Parallel decision, under providerMetadata.parallel. Maps to ParallelDecisionProviderMetadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParallelDecisionProviderMetadata"
  hidden: true
---

<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata"></a>

Namespace: [Vercel.AI.Parallel](/api/Vercel.AI.Parallel/)


```csharp
public sealed class ParallelDecisionProviderMetadata
```


Provider metadata of a Parallel decision, under `providerMetadata.parallel`. Maps to `ParallelDecisionProviderMetadata`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L35)

## Properties

<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Citations_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Citations"></a>

### Citations


```csharp
public IReadOnlyList<ParallelCitation> Citations { get; init; }
```


Shared evidence for the whole request, without a per-answer mapping.

**Value:** [IReadOnlyList&lt;ParallelCitation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L38)

<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Confidence_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Confidence"></a>

### Confidence


```csharp
public IReadOnlyDictionary<string, double> Confidence { get; init; }
```


Confidence per question id. Boolean and refused questions have none.

**Value:** [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L41)

<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_DecisionId_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_DecisionId"></a>

### DecisionId


```csharp
public string DecisionId { get; init; }
```


Decision id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L45)

<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Legend_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Legend"></a>

### Legend


```csharp
public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Legend { get; init; }
```


Score legend per question id.

**Value:** [IReadOnlyDictionary&lt;string, IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L48)

<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Mode_"></a>
<a id="Vercel_AI_Parallel_ParallelDecisionProviderMetadata_Mode"></a>

### Mode


```csharp
public string Mode { get; init; }
```


Mode that answered.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L52)
