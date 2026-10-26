---
title: "OpenAIText"
description: "Generated text that carries the Responses output item id and its annotations."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIText"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIText"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIText : GeneratedText
```


Generated text that carries the Responses output item id and its annotations.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedText](/api/Vercel.AI.Provider/GeneratedText/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesOutput.cs#L12)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIText__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIText__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_Citation__System_Text_Json_JsonElement_"></a>

### `OpenAIText(string, IReadOnlyList<Citation>?, JsonElement)`


```csharp
public OpenAIText(string text, IReadOnlyList<Citation>? citations, JsonElement providerMetadata)
```


Creates text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `citations` | [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesOutput.cs#L15)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIText_ProviderMetadata_"></a>
<a id="Vercel_AI_OpenAI_OpenAIText_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Provider metadata: `openai.itemId`, `openai.phase` when the message has one, and, when the text is annotated, `openai.annotations`.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesOutput.cs#L22)
