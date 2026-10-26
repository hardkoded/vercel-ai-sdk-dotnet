---
title: "PreparedToolChoice"
description: "A tool choice sent to a language model. Maps to the result of prepareToolChoice."
editUrl: false
prev: false
next: false
sidebar:
  label: "PreparedToolChoice"
  hidden: true
---

<a id="Vercel_AI_Prompt_PreparedToolChoice"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class PreparedToolChoice
```


A tool choice sent to a language model. Maps to the result of `prepareToolChoice`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareToolChoice.cs#L10)

## Constructors

<a id="Vercel_AI_Prompt_PreparedToolChoice__ctor_"></a>
<a id="Vercel_AI_Prompt_PreparedToolChoice__ctor_System_String_System_String_"></a>

### PreparedToolChoice(string, string?)


```csharp
public PreparedToolChoice(string type, string? toolName = null)
```


Creates a tool choice.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareToolChoice.cs#L13)

## Properties

<a id="Vercel_AI_Prompt_PreparedToolChoice_ToolName_"></a>
<a id="Vercel_AI_Prompt_PreparedToolChoice_ToolName"></a>

### ToolName


```csharp
public string? ToolName { get; }
```


Tool name when [Type](#Vercel_AI_Prompt_PreparedToolChoice_Type) is `tool`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareToolChoice.cs#L23)

<a id="Vercel_AI_Prompt_PreparedToolChoice_Type_"></a>
<a id="Vercel_AI_Prompt_PreparedToolChoice_Type"></a>

### Type


```csharp
public string Type { get; }
```


Choice type: auto, none, required, or tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareToolChoice.cs#L20)
