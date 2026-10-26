---
title: "ToolModelContentPart"
description: "One content part inside a content tool output."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolModelContentPart"
  hidden: true
---

<a id="Vercel_AI_Prompt_ToolModelContentPart"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class ToolModelContentPart
```


One content part inside a content tool output.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L109)

## Constructors

<a id="Vercel_AI_Prompt_ToolModelContentPart__ctor_"></a>
<a id="Vercel_AI_Prompt_ToolModelContentPart__ctor_System_String_System_String_"></a>

### ToolModelContentPart(string, string)


```csharp
public ToolModelContentPart(string type, string text)
```


Creates a text part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L112)

## Properties

<a id="Vercel_AI_Prompt_ToolModelContentPart_Text_"></a>
<a id="Vercel_AI_Prompt_ToolModelContentPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


Part text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L122)

<a id="Vercel_AI_Prompt_ToolModelContentPart_Type_"></a>
<a id="Vercel_AI_Prompt_ToolModelContentPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L119)
