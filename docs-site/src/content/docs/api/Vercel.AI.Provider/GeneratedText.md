---
title: "GeneratedText"
description: "Generated text."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedText"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedText"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class GeneratedText : GeneratedContent
```


Generated text.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

**Derived:** [AnthropicText](/api/Vercel.AI.Anthropic/AnthropicText/), [GoogleText](/api/Vercel.AI.Google/GoogleText/), [HuggingFaceText](/api/Vercel.AI.HuggingFace/HuggingFaceText/), [OpenAIText](/api/Vercel.AI.OpenAI/OpenAIText/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L182)

## Constructors

<a id="Vercel_AI_Provider_GeneratedText__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedText__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_Citation__System_Nullable_System_Text_Json_JsonElement__"></a>

### `GeneratedText(string, IReadOnlyList<Citation>?, JsonElement?)`


```csharp
public GeneratedText(string text, IReadOnlyList<Citation>? citations = null, JsonElement? providerMetadata = null)
```


Creates text content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `citations` | [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L185)

## Properties

<a id="Vercel_AI_Provider_GeneratedText_Citations_"></a>
<a id="Vercel_AI_Provider_GeneratedText_Citations"></a>

### Citations


```csharp
public IReadOnlyList<Citation>? Citations { get; }
```


References supporting this text, or null when the provider sent none. They are separate from the retrieved
[GeneratedSource](/api/Vercel.AI.Provider/GeneratedSource/) parts.

**Value:** [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L200)

<a id="Vercel_AI_Provider_GeneratedText_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedText_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this text, keyed by provider name, such as the message `itemId`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L203)

<a id="Vercel_AI_Provider_GeneratedText_Text_"></a>
<a id="Vercel_AI_Provider_GeneratedText_Text"></a>

### Text


```csharp
public string Text { get; }
```


Generated text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L194)
