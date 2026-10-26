---
title: "OpenAICompletionPrompt"
description: "A completion prompt and its default stop sequences."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompletionPrompt"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAICompletionPrompt"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAICompletionPrompt
```


A completion prompt and its default stop sequences.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L468)

## Properties

<a id="Vercel_AI_OpenAI_OpenAICompletionPrompt_Prompt_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionPrompt_Prompt"></a>

### Prompt


```csharp
public string Prompt { get; }
```


Prompt text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L477)

<a id="Vercel_AI_OpenAI_OpenAICompletionPrompt_Stop_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionPrompt_Stop"></a>

### Stop


```csharp
public IReadOnlyList<string> Stop { get; }
```


Stop sequences implied by the prompt.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L480)
