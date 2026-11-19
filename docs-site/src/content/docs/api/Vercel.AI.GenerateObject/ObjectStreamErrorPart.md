---
title: "ObjectStreamErrorPart"
description: "An error. Maps to { type: 'error' }."
editUrl: false
prev: false
next: false
sidebar:
  label: "ObjectStreamErrorPart"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_ObjectStreamErrorPart"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class ObjectStreamErrorPart : ObjectStreamPart
```


An error. Maps to `{ type: 'error' }`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ObjectStreamPart](/api/Vercel.AI.GenerateObject/ObjectStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L183)

## Properties

<a id="Vercel_AI_GenerateObject_ObjectStreamErrorPart_Error_"></a>
<a id="Vercel_AI_GenerateObject_ObjectStreamErrorPart_Error"></a>

### Error


```csharp
public Exception Error { get; }
```


The error.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L192)
