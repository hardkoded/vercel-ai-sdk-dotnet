---
title: "ObjectStreamObjectPart"
description: "A new partial object. Maps to { type: 'object' }."
editUrl: false
prev: false
next: false
sidebar:
  label: "ObjectStreamObjectPart"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_ObjectStreamObjectPart"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class ObjectStreamObjectPart : ObjectStreamPart
```


A new partial object. Maps to `{ type: 'object' }`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ObjectStreamPart](/api/Vercel.AI.GenerateObject/ObjectStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L187)

## Properties

<a id="Vercel_AI_GenerateObject_ObjectStreamObjectPart_Object_"></a>
<a id="Vercel_AI_GenerateObject_ObjectStreamObjectPart_Object"></a>

### Object


```csharp
public JsonElement Object { get; }
```


The partial object. It is not validated.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L196)
