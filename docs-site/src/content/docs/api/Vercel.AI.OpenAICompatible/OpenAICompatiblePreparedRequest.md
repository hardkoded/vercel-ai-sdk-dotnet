---
title: "OpenAICompatiblePreparedRequest"
description: "The request body and warnings PrepareRequest(LanguageModelCallOptions) builds."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatiblePreparedRequest"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatiblePreparedRequest"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatiblePreparedRequest
```


The request body and warnings [PrepareRequest(LanguageModelCallOptions)](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleLanguageModel/#Vercel_AI_OpenAICompatible_OpenAICompatibleLanguageModel_PrepareRequest_Vercel_AI_Provider_LanguageModelCallOptions_) builds.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L1212)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatiblePreparedRequest__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatiblePreparedRequest__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__"></a>

### `OpenAICompatiblePreparedRequest(JsonObject, IReadOnlyList<CallWarning>)`


```csharp
public OpenAICompatiblePreparedRequest(JsonObject body, IReadOnlyList<CallWarning> warnings)
```


Creates a prepared request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L1215)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatiblePreparedRequest_Body_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatiblePreparedRequest_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


The Chat Completions body, with the request body transform applied.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L1222)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatiblePreparedRequest_Warnings_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatiblePreparedRequest_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Warnings raised while building the body.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleLanguageModel.cs#L1225)
