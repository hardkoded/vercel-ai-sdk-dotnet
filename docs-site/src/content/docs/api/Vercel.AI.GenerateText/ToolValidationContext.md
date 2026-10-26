---
title: "ToolValidationContext"
description: "Where tool-context validation failed."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolValidationContext"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolValidationContext"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolValidationContext
```


Where tool-context validation failed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L28)

## Constructors

<a id="Vercel_AI_GenerateText_ToolValidationContext__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolValidationContext__ctor_System_String_System_String_"></a>

### ToolValidationContext(string, string)


```csharp
public ToolValidationContext(string field, string entityName)
```


Creates context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `field` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `entityName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L31)

## Properties

<a id="Vercel_AI_GenerateText_ToolValidationContext_EntityName_"></a>
<a id="Vercel_AI_GenerateText_ToolValidationContext_EntityName"></a>

### EntityName


```csharp
public string EntityName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L41)

<a id="Vercel_AI_GenerateText_ToolValidationContext_Field_"></a>
<a id="Vercel_AI_GenerateText_ToolValidationContext_Field"></a>

### Field


```csharp
public string Field { get; }
```


Field name. Tool context uses `tool context`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L38)
