---
title: "OpenAIChatMessageResult"
description: "Chat Completions messages and the warnings produced while converting them."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatMessageResult"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatMessageResult"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIChatMessageResult
```


Chat Completions messages and the warnings produced while converting them.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L91)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIChatMessageResult_Messages_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatMessageResult_Messages"></a>

### Messages


```csharp
public JsonArray Messages { get; }
```


OpenAI `messages` array.

**Value:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L100)

<a id="Vercel_AI_OpenAI_OpenAIChatMessageResult_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatMessageResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while converting the prompt.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L103)
