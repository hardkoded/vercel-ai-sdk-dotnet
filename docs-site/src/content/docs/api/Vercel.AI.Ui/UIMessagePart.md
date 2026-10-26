---
title: "UIMessagePart"
description: "A part of a UIMessage. Maps to UIMessagePart."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessagePart"
  hidden: true
---

<a id="Vercel_AI_Ui_UIMessagePart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public abstract class UIMessagePart
```


A part of a [UIMessage](/api/Vercel.AI.Ui/UIMessage/). Maps to `UIMessagePart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [CustomContentUIPart](/api/Vercel.AI.Ui/CustomContentUIPart/), [DataUIPart](/api/Vercel.AI.Ui/DataUIPart/), [FileUIPart](/api/Vercel.AI.Ui/FileUIPart/), [ReasoningFileUIPart](/api/Vercel.AI.Ui/ReasoningFileUIPart/), [ReasoningUIPart](/api/Vercel.AI.Ui/ReasoningUIPart/), [SourceDocumentUIPart](/api/Vercel.AI.Ui/SourceDocumentUIPart/), [SourceUrlUIPart](/api/Vercel.AI.Ui/SourceUrlUIPart/), [StepStartUIPart](/api/Vercel.AI.Ui/StepStartUIPart/), [TextUIPart](/api/Vercel.AI.Ui/TextUIPart/), [ToolUIPart](/api/Vercel.AI.Ui/ToolUIPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L49)

## Constructors

<a id="Vercel_AI_Ui_UIMessagePart__ctor_"></a>
<a id="Vercel_AI_Ui_UIMessagePart__ctor_System_String_"></a>

### UIMessagePart(string)


```csharp
protected UIMessagePart(string type)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L52)

## Properties

<a id="Vercel_AI_Ui_UIMessagePart_Type_"></a>
<a id="Vercel_AI_Ui_UIMessagePart_Type"></a>

### Type


```csharp
public string Type { get; }
```


The part type, such as `text`, `tool-get-weather`, or `data-chart`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L58)
