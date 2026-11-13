---
title: "GenerateObjectRepairContext"
description: "Input passed to RepairText."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectRepairContext"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectRepairContext"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectRepairContext
```


Input passed to `RepairText`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L138)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectRepairContext_Error_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectRepairContext_Error"></a>

### Error


```csharp
public Exception Error { get; }
```


The parse or validation error.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L150)

<a id="Vercel_AI_GenerateObject_GenerateObjectRepairContext_Text_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectRepairContext_Text"></a>

### Text


```csharp
public string Text { get; }
```


The text that failed parsing or validation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L147)
