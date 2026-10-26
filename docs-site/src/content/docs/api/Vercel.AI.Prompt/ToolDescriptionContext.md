---
title: "ToolDescriptionContext"
description: "Context passed to a function-valued tool description."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolDescriptionContext"
  hidden: true
---

<a id="Vercel_AI_Prompt_ToolDescriptionContext"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class ToolDescriptionContext
```


Context passed to a function-valued tool description.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L10)

## Constructors

<a id="Vercel_AI_Prompt_ToolDescriptionContext__ctor_"></a>
<a id="Vercel_AI_Prompt_ToolDescriptionContext__ctor_System_Object_System_String_"></a>

### ToolDescriptionContext(object?, string?)


```csharp
public ToolDescriptionContext(object? context, string? sandboxDescription)
```


Creates context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `context` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `sandboxDescription` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L13)

## Properties

<a id="Vercel_AI_Prompt_ToolDescriptionContext_Context_"></a>
<a id="Vercel_AI_Prompt_ToolDescriptionContext_Context"></a>

### Context


```csharp
public object? Context { get; }
```


Per-tool context value.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L20)

<a id="Vercel_AI_Prompt_ToolDescriptionContext_SandboxDescription_"></a>
<a id="Vercel_AI_Prompt_ToolDescriptionContext_SandboxDescription"></a>

### SandboxDescription


```csharp
public string? SandboxDescription { get; }
```


Sandbox description, when a sandbox is active.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L23)
