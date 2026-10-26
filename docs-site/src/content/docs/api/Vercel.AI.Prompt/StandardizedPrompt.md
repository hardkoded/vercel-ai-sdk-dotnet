---
title: "StandardizedPrompt"
description: "A standardized prompt. Maps to the result of standardizePrompt."
editUrl: false
prev: false
next: false
sidebar:
  label: "StandardizedPrompt"
  hidden: true
---

<a id="Vercel_AI_Prompt_StandardizedPrompt"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class StandardizedPrompt
```


A standardized prompt. Maps to the result of `standardizePrompt`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L115)

## Constructors

<a id="Vercel_AI_Prompt_StandardizedPrompt__ctor_"></a>
<a id="Vercel_AI_Prompt_StandardizedPrompt__ctor_Vercel_AI_Prompt_Instructions_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `StandardizedPrompt(Instructions?, IReadOnlyList<ModelMessage>)`


```csharp
public StandardizedPrompt(Instructions? instructions, IReadOnlyList<ModelMessage> messages)
```


Creates a standardized prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `instructions` | [Instructions](/api/Vercel.AI.Prompt/Instructions/) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L118)

## Properties

<a id="Vercel_AI_Prompt_StandardizedPrompt_Instructions_"></a>
<a id="Vercel_AI_Prompt_StandardizedPrompt_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; }
```


Instructions, or null when there are none.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L125)

<a id="Vercel_AI_Prompt_StandardizedPrompt_Messages_"></a>
<a id="Vercel_AI_Prompt_StandardizedPrompt_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


User and assistant messages, plus system messages when they are allowed.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L128)
