---
title: "OpenAICompatibleContentPart"
description: "One text or reasoning fragment from a Chat Completions content field."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleContentPart"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleContentPart"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatibleContentPart
```


One text or reasoning fragment from a Chat Completions content field.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L650)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleContentPart__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleContentPart__ctor_System_Boolean_System_String_"></a>

### OpenAICompatibleContentPart(bool, string)


```csharp
public OpenAICompatibleContentPart(bool reasoning, string text)
```


Creates a fragment.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L653)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleContentPart_Reasoning_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleContentPart_Reasoning"></a>

### Reasoning


```csharp
public bool Reasoning { get; }
```


True when the fragment is reasoning.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L660)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleContentPart_Text_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleContentPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


Fragment text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L663)
