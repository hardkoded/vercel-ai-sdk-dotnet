---
title: "OpenAIChatPreparedRequest"
description: "Chat Completions request body and the warnings produced while building it."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatPreparedRequest"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatPreparedRequest"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIChatPreparedRequest
```


Chat Completions request body and the warnings produced while building it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L14)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIChatPreparedRequest_Body_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPreparedRequest_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


JSON body sent to `/chat/completions`.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L23)

<a id="Vercel_AI_OpenAI_OpenAIChatPreparedRequest_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPreparedRequest_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing the request.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatLanguageModel.cs#L26)
