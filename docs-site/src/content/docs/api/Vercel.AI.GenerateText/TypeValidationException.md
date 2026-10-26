---
title: "TypeValidationException"
description: "Tool context failed schema validation. Maps to TypeValidationError."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeValidationException"
  hidden: true
---

<a id="Vercel_AI_GenerateText_TypeValidationException"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class TypeValidationException : AiSdkException, ISerializable
```


Tool context failed schema validation. Maps to `TypeValidationError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L10)

## Constructors

<a id="Vercel_AI_GenerateText_TypeValidationException__ctor_"></a>
<a id="Vercel_AI_GenerateText_TypeValidationException__ctor_System_Object_System_String_System_String_"></a>

### TypeValidationException(object?, string, string)


```csharp
public TypeValidationException(object? value, string field, string entityName)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `field` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `entityName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L13)

## Properties

<a id="Vercel_AI_GenerateText_TypeValidationException_Context_"></a>
<a id="Vercel_AI_GenerateText_TypeValidationException_Context"></a>

### Context


```csharp
public ToolValidationContext Context { get; }
```


Field and entity that failed.

**Value:** [ToolValidationContext](/api/Vercel.AI.GenerateText/ToolValidationContext/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L24)

<a id="Vercel_AI_GenerateText_TypeValidationException_Value_"></a>
<a id="Vercel_AI_GenerateText_TypeValidationException_Value"></a>

### Value


```csharp
public object? Value { get; }
```


Value that failed validation.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolContext.cs#L21)
