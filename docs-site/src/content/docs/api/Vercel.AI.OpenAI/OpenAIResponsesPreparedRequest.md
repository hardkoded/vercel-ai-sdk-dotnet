---
title: "OpenAIResponsesPreparedRequest"
description: "Responses API request body and the warnings produced while building it."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesPreparedRequest"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesPreparedRequest"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesPreparedRequest
```


Responses API request body and the warnings produced while building it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L16)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesPreparedRequest_Body_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesPreparedRequest_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


JSON body sent to `/responses`.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L25)

<a id="Vercel_AI_OpenAI_OpenAIResponsesPreparedRequest_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesPreparedRequest_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing the request.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesLanguageModel.cs#L28)
