---
title: "TextUIPart"
description: "A text part. Maps to TextUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_TextUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class TextUIPart : UIMessagePart
```


A text part. Maps to `TextUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L62)

## Constructors

<a id="Vercel_AI_Ui_TextUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_TextUIPart__ctor_System_String_"></a>

### TextUIPart(string)


```csharp
public TextUIPart(string text)
```


Creates a text part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L65)

## Properties

<a id="Vercel_AI_Ui_TextUIPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Ui_TextUIPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
```


The provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L78)

<a id="Vercel_AI_Ui_TextUIPart_State_"></a>
<a id="Vercel_AI_Ui_TextUIPart_State"></a>

### State


```csharp
public string? State { get; init; }
```


The state of the text part: `streaming` or `done`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L75)

<a id="Vercel_AI_Ui_TextUIPart_Text_"></a>
<a id="Vercel_AI_Ui_TextUIPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


The text content.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L72)
