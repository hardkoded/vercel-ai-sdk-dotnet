---
title: "AssistantReasoningPart"
description: "Reasoning in an assistant message."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantReasoningPart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantReasoningPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantReasoningPart : AssistantContentPart
```


Reasoning in an assistant message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L784)

## Constructors

<a id="Vercel_AI_Provider_AssistantReasoningPart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantReasoningPart__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantReasoningPart(string, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantReasoningPart(string text, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a reasoning part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L787)

## Properties

<a id="Vercel_AI_Provider_AssistantReasoningPart_Text_"></a>
<a id="Vercel_AI_Provider_AssistantReasoningPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


Reasoning text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L794)
