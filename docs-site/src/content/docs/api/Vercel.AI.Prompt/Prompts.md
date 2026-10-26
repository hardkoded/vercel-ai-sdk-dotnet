---
title: "Prompts"
description: "Prompt normalization. Maps to standardizePrompt."
editUrl: false
prev: false
next: false
sidebar:
  label: "Prompts"
  hidden: true
---

<a id="Vercel_AI_Prompt_Prompts"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class Prompts
```


Prompt normalization. Maps to `standardizePrompt`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L164)

## Methods

<a id="Vercel_AI_Prompt_Prompts_StandardizePrompt_"></a>
<a id="Vercel_AI_Prompt_Prompts_StandardizePrompt_Vercel_AI_Prompt_StandardizePromptInput_"></a>

### StandardizePrompt(StandardizePromptInput)


```csharp
public static StandardizedPrompt StandardizePrompt(StandardizePromptInput input)
```


Validates and normalizes a prompt into instructions plus messages.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `input` | [StandardizePromptInput](/api/Vercel.AI.Prompt/StandardizePromptInput/) |  |

**Returns:** [StandardizedPrompt](/api/Vercel.AI.Prompt/StandardizedPrompt/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L167)
