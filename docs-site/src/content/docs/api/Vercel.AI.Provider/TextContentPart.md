---
title: "TextContentPart"
description: "Text in a user message."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_TextContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class TextContentPart : UserContentPart
```


Text in a user message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L654)

## Constructors

<a id="Vercel_AI_Provider_TextContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_TextContentPart__ctor_System_String_"></a>

### TextContentPart(string)


```csharp
public TextContentPart(string text)
```


Creates a text part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L657)

## Properties

<a id="Vercel_AI_Provider_TextContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_TextContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part, such as the Amazon Bedrock `guardContent` or `cachePoint`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L667)

<a id="Vercel_AI_Provider_TextContentPart_Text_"></a>
<a id="Vercel_AI_Provider_TextContentPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L664)
