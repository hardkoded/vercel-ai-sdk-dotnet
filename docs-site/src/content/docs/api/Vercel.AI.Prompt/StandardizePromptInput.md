---
title: "StandardizePromptInput"
description: "Input for StandardizePrompt(StandardizePromptInput)."
editUrl: false
prev: false
next: false
sidebar:
  label: "StandardizePromptInput"
  hidden: true
---

<a id="Vercel_AI_Prompt_StandardizePromptInput"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class StandardizePromptInput
```


Input for [StandardizePrompt(StandardizePromptInput)](/api/Vercel.AI.Prompt/Prompts/#Vercel_AI_Prompt_Prompts_StandardizePrompt_Vercel_AI_Prompt_StandardizePromptInput_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L145)

## Properties

<a id="Vercel_AI_Prompt_StandardizePromptInput_AllowSystemInMessages_"></a>
<a id="Vercel_AI_Prompt_StandardizePromptInput_AllowSystemInMessages"></a>

### AllowSystemInMessages


```csharp
public bool AllowSystemInMessages { get; set; }
```


When true, system messages may appear in [Prompt](#Vercel_AI_Prompt_StandardizePromptInput_Prompt) or [Messages](#Vercel_AI_Prompt_StandardizePromptInput_Messages).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L148)

<a id="Vercel_AI_Prompt_StandardizePromptInput_Instructions_"></a>
<a id="Vercel_AI_Prompt_StandardizePromptInput_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


Instructions. They win over [System](#Vercel_AI_Prompt_StandardizePromptInput_System).

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L154)

<a id="Vercel_AI_Prompt_StandardizePromptInput_Messages_"></a>
<a id="Vercel_AI_Prompt_StandardizePromptInput_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; set; }
```


Message list. Cannot be combined with [Prompt](#Vercel_AI_Prompt_StandardizePromptInput_Prompt).

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L160)

<a id="Vercel_AI_Prompt_StandardizePromptInput_Prompt_"></a>
<a id="Vercel_AI_Prompt_StandardizePromptInput_Prompt"></a>

### Prompt


```csharp
public PromptInput? Prompt { get; set; }
```


A user string or a message list. Cannot be combined with [Messages](#Vercel_AI_Prompt_StandardizePromptInput_Messages).

**Value:** [PromptInput](/api/Vercel.AI.Prompt/PromptInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L157)

<a id="Vercel_AI_Prompt_StandardizePromptInput_System_"></a>
<a id="Vercel_AI_Prompt_StandardizePromptInput_System"></a>

### System


```csharp
public string? System { get; set; }
```


Legacy system string. Used when [Instructions](#Vercel_AI_Prompt_StandardizePromptInput_Instructions) is null.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L151)
