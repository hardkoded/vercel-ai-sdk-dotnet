---
title: "AssemblyAICustomSpelling"
description: "One custom spelling rule. Maps to an entry of customSpelling."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAICustomSpelling"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAICustomSpelling"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAICustomSpelling
```


One custom spelling rule. Maps to an entry of `customSpelling`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L12)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAICustomSpelling_From_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAICustomSpelling_From"></a>

### From


```csharp
public IReadOnlyList<string> From { get; init; }
```


The spellings to replace. Maps to `from`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L15)

<a id="Vercel_AI_AssemblyAI_AssemblyAICustomSpelling_To_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAICustomSpelling_To"></a>

### To


```csharp
public string To { get; init; }
```


The replacement. Maps to `to`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L18)
