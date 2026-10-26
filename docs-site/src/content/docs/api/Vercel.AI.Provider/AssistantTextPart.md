---
title: "AssistantTextPart"
description: "Text in an assistant message."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantTextPart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantTextPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantTextPart : AssistantContentPart
```


Text in an assistant message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L770)

## Constructors

<a id="Vercel_AI_Provider_AssistantTextPart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantTextPart__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantTextPart(string, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantTextPart(string text, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a text part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L773)

## Properties

<a id="Vercel_AI_Provider_AssistantTextPart_Text_"></a>
<a id="Vercel_AI_Provider_AssistantTextPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L780)
