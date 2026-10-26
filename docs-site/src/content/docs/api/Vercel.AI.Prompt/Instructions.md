---
title: "Instructions"
description: "Instructions for a call: a string, one system message, or a list of system messages. Maps to Instructions. A single system message is kept as a one-item Mess..."
editUrl: false
prev: false
next: false
sidebar:
  label: "Instructions"
  hidden: true
---

<a id="Vercel_AI_Prompt_Instructions"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class Instructions
```


Instructions for a call: a string, one system message, or a list of system messages.
Maps to `Instructions`. A single system message is kept as a one-item [Messages](#Vercel_AI_Prompt_Instructions_Messages) list.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L13)

## Properties

<a id="Vercel_AI_Prompt_Instructions_Messages_"></a>
<a id="Vercel_AI_Prompt_Instructions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<SystemModelMessage>? Messages { get; }
```


The system messages, when the instructions are one or more system messages.

**Value:** [IReadOnlyList&lt;SystemModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L25)

<a id="Vercel_AI_Prompt_Instructions_Text_"></a>
<a id="Vercel_AI_Prompt_Instructions_Text"></a>

### Text


```csharp
public string? Text { get; }
```


The instruction text, when the instructions are a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L22)

## Methods

<a id="Vercel_AI_Prompt_Instructions_From_"></a>
<a id="Vercel_AI_Prompt_Instructions_From_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_SystemModelMessage__"></a>

### `From(IReadOnlyList<SystemModelMessage>)`


```csharp
public static Instructions From(IReadOnlyList<SystemModelMessage> messages)
```


Creates instructions from a list of system messages.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;SystemModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [Instructions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L40)

<a id="Vercel_AI_Prompt_Instructions_From_System_String_"></a>

### From(string)


```csharp
public static Instructions From(string text)
```


Creates instructions from a string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Instructions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L28)

<a id="Vercel_AI_Prompt_Instructions_From_Vercel_AI_Provider_SystemModelMessage_"></a>

### From(SystemModelMessage)


```csharp
public static Instructions From(SystemModelMessage message)
```


Creates instructions from one system message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/) |  |

**Returns:** [Instructions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L34)

## Operators

<a id="Vercel_AI_Prompt_Instructions_op_Implicit_"></a>
<a id="Vercel_AI_Prompt_Instructions_op_Implicit_System_String__Vercel_AI_Prompt_Instructions"></a>

### implicit operator Instructions(string)


```csharp
public static implicit operator Instructions(string text)
```


Converts a string to instructions.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Instructions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L56)

<a id="Vercel_AI_Prompt_Instructions_op_Implicit_Vercel_AI_Provider_SystemModelMessage__Vercel_AI_Prompt_Instructions"></a>

### implicit operator Instructions(SystemModelMessage)


```csharp
public static implicit operator Instructions(SystemModelMessage message)
```


Converts a system message to instructions.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [SystemModelMessage](/api/Vercel.AI.Provider/SystemModelMessage/) |  |

**Returns:** [Instructions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L62)

<a id="Vercel_AI_Prompt_Instructions_op_Implicit_Vercel_AI_Provider_SystemModelMessage____Vercel_AI_Prompt_Instructions"></a>

### implicit operator Instructions(SystemModelMessage[])


```csharp
public static implicit operator Instructions(SystemModelMessage[] messages)
```


Converts an array of system messages to instructions.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | `SystemModelMessage\[\]` |  |

**Returns:** [Instructions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L68)
