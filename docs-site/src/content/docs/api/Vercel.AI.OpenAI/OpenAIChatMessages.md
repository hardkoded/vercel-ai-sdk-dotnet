---
title: "OpenAIChatMessages"
description: "Converts prompts into OpenAI Chat Completions messages."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatMessages"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatMessages"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public static class OpenAIChatMessages
```


Converts prompts into OpenAI Chat Completions messages.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L107)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIChatMessages_Convert_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatMessages_Convert_System_Collections_Generic_IReadOnlyList_Vercel_AI_OpenAI_OpenAIChatPromptMessage__System_String_"></a>

### `Convert(IReadOnlyList<OpenAIChatPromptMessage>, string)`


```csharp
public static OpenAIChatMessageResult Convert(IReadOnlyList<OpenAIChatPromptMessage> prompt, string systemMessageMode = "system")
```


Converts prepared messages. &lt;code class="paramref"&gt;systemMessageMode&lt;/code&gt; is `system`, `developer`, or `remove`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [IReadOnlyList&lt;OpenAIChatPromptMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `systemMessageMode` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIChatMessageResult](/api/Vercel.AI.OpenAI/OpenAIChatMessageResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L148)

<a id="Vercel_AI_OpenAI_OpenAIChatMessages_Convert_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_String_"></a>

### `Convert(IReadOnlyList<ModelMessage>, string)`


```csharp
public static OpenAIChatMessageResult Convert(IReadOnlyList<ModelMessage> prompt, string systemMessageMode = "system")
```


Converts SDK messages. &lt;code class="paramref"&gt;systemMessageMode&lt;/code&gt; is `system`, `developer`, or `remove`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `systemMessageMode` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIChatMessageResult](/api/Vercel.AI.OpenAI/OpenAIChatMessageResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L110)
