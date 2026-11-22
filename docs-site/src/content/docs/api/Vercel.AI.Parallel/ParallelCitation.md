---
title: "ParallelCitation"
description: "One piece of shared evidence of a Parallel decision."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParallelCitation"
  hidden: true
---

<a id="Vercel_AI_Parallel_ParallelCitation"></a>

Namespace: [Vercel.AI.Parallel](/api/Vercel.AI.Parallel/)


```csharp
public sealed class ParallelCitation
```


One piece of shared evidence of a Parallel decision.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L22)

## Properties

<a id="Vercel_AI_Parallel_ParallelCitation_Excerpts_"></a>
<a id="Vercel_AI_Parallel_ParallelCitation_Excerpts"></a>

### Excerpts


```csharp
public IReadOnlyList<string> Excerpts { get; init; }
```


Evidence excerpts.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L31)

<a id="Vercel_AI_Parallel_ParallelCitation_Title_"></a>
<a id="Vercel_AI_Parallel_ParallelCitation_Title"></a>

### Title


```csharp
public string? Title { get; init; }
```


Page title, when Parallel supplies one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L28)

<a id="Vercel_AI_Parallel_ParallelCitation_Url_"></a>
<a id="Vercel_AI_Parallel_ParallelCitation_Url"></a>

### Url


```csharp
public string Url { get; init; }
```


Source URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelDecisionModelOptions.cs#L25)
