---
title: "OpenAICompletionPreparedRequest"
description: "Completion request body and warnings."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompletionPreparedRequest"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAICompletionPreparedRequest"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAICompletionPreparedRequest
```


Completion request body and warnings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L15)

## Properties

<a id="Vercel_AI_OpenAI_OpenAICompletionPreparedRequest_Body_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionPreparedRequest_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


JSON body sent to `/completions`.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L24)

<a id="Vercel_AI_OpenAI_OpenAICompletionPreparedRequest_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAICompletionPreparedRequest_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing the request.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Completion/OpenAICompletionLanguageModel.cs#L27)
