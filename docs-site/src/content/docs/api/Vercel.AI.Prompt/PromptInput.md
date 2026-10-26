---
title: "PromptInput"
description: "The prompt of a call: a user string or a list of messages. Maps to prompt: string | ModelMessage[]."
editUrl: false
prev: false
next: false
sidebar:
  label: "PromptInput"
  hidden: true
---

<a id="Vercel_AI_Prompt_PromptInput"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class PromptInput
```


The prompt of a call: a user string or a list of messages. Maps to `prompt: string | ModelMessage[]`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L75)

## Properties

<a id="Vercel_AI_Prompt_PromptInput_Messages_"></a>
<a id="Vercel_AI_Prompt_PromptInput_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; }
```


The messages, when the prompt is a message list.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L87)

<a id="Vercel_AI_Prompt_PromptInput_Text_"></a>
<a id="Vercel_AI_Prompt_PromptInput_Text"></a>

### Text


```csharp
public string? Text { get; }
```


The user text, when the prompt is a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L84)

## Methods

<a id="Vercel_AI_Prompt_PromptInput_From_"></a>
<a id="Vercel_AI_Prompt_PromptInput_From_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `From(IReadOnlyList<ModelMessage>)`


```csharp
public static PromptInput From(IReadOnlyList<ModelMessage> messages)
```


Creates a prompt from a message list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [PromptInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L96)

<a id="Vercel_AI_Prompt_PromptInput_From_System_String_"></a>

### From(string)


```csharp
public static PromptInput From(string text)
```


Creates a prompt from a user string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [PromptInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L90)

## Operators

<a id="Vercel_AI_Prompt_PromptInput_op_Implicit_"></a>
<a id="Vercel_AI_Prompt_PromptInput_op_Implicit_System_String__Vercel_AI_Prompt_PromptInput"></a>

### implicit operator PromptInput(string)


```csharp
public static implicit operator PromptInput(string text)
```


Converts a string to a prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [PromptInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L102)

<a id="Vercel_AI_Prompt_PromptInput_op_Implicit_Vercel_AI_Provider_ModelMessage____Vercel_AI_Prompt_PromptInput"></a>

### implicit operator PromptInput(ModelMessage[])


```csharp
public static implicit operator PromptInput(ModelMessage[] messages)
```


Converts an array of messages to a prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | `ModelMessage\[\]` |  |

**Returns:** [PromptInput](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L108)
