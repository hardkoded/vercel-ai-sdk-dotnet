---
title: "AnthropicText"
description: "Generated text that also carries Anthropic provider metadata, such as citations."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicText"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicText"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicText : GeneratedText
```


Generated text that also carries Anthropic provider metadata, such as citations.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedText](/api/Vercel.AI.Provider/GeneratedText/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L11)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicText__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicText__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_Citation__"></a>

### `AnthropicText(string, JsonElement?, IReadOnlyList<Citation>?)`


```csharp
public AnthropicText(string text, JsonElement? providerMetadata, IReadOnlyList<Citation>? citations = null)
```


Creates text content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `citations` | [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L14)
