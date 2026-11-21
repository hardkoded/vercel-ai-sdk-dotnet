---
title: "ObjectStreamPart"
description: "A part of the full stream of a streamObject call. Maps to ObjectStreamPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "ObjectStreamPart"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_ObjectStreamPart"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public abstract class ObjectStreamPart
```


A part of the full stream of a `streamObject` call. Maps to `ObjectStreamPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ObjectStreamErrorPart](/api/Vercel.AI.GenerateObject/ObjectStreamErrorPart/), [ObjectStreamFinishPart](/api/Vercel.AI.GenerateObject/ObjectStreamFinishPart/), [ObjectStreamObjectPart](/api/Vercel.AI.GenerateObject/ObjectStreamObjectPart/), [ObjectStreamTextDeltaPart](/api/Vercel.AI.GenerateObject/ObjectStreamTextDeltaPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L175)

## Properties

<a id="Vercel_AI_GenerateObject_ObjectStreamPart_Type_"></a>
<a id="Vercel_AI_GenerateObject_ObjectStreamPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type: `object`, `text-delta`, `error`, or `finish`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L183)
