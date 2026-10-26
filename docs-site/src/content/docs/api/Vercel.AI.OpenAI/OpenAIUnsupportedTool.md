---
title: "OpenAIUnsupportedTool"
description: "A non-function tool passed to Chat Completions preparation."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIUnsupportedTool"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIUnsupportedTool"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIUnsupportedTool
```


A non-function tool passed to Chat Completions preparation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L11)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIUnsupportedTool__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIUnsupportedTool__ctor_System_String_System_String_"></a>

### OpenAIUnsupportedTool(string, string)


```csharp
public OpenAIUnsupportedTool(string type, string name)
```


Creates an unsupported tool description.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L14)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIUnsupportedTool_Name_"></a>
<a id="Vercel_AI_OpenAI_OpenAIUnsupportedTool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L24)

<a id="Vercel_AI_OpenAI_OpenAIUnsupportedTool_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAIUnsupportedTool_Type"></a>

### Type


```csharp
public string Type { get; }
```


Tool type recorded in the warning.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L21)
