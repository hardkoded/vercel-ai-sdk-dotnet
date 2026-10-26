---
title: "ReasoningUIPart"
description: "A reasoning part. Maps to ReasoningUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_ReasoningUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class ReasoningUIPart : UIMessagePart
```


A reasoning part. Maps to `ReasoningUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L99)

## Constructors

<a id="Vercel_AI_Ui_ReasoningUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_ReasoningUIPart__ctor_System_String_"></a>

### ReasoningUIPart(string)


```csharp
public ReasoningUIPart(string text)
```


Creates a reasoning part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L102)

## Properties

<a id="Vercel_AI_Ui_ReasoningUIPart_Id_"></a>
<a id="Vercel_AI_Ui_ReasoningUIPart_Id"></a>

### Id


```csharp
public string? Id { get; init; }
```


The reasoning part ID.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L109)

<a id="Vercel_AI_Ui_ReasoningUIPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Ui_ReasoningUIPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
```


The provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L118)

<a id="Vercel_AI_Ui_ReasoningUIPart_State_"></a>
<a id="Vercel_AI_Ui_ReasoningUIPart_State"></a>

### State


```csharp
public string? State { get; init; }
```


The state of the reasoning part: `streaming` or `done`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L115)

<a id="Vercel_AI_Ui_ReasoningUIPart_Text_"></a>
<a id="Vercel_AI_Ui_ReasoningUIPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


The reasoning text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L112)
