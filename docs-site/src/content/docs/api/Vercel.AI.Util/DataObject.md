---
title: "DataObject"
description: "Object with an explicit constructor token and own properties. Used where JavaScript distinguishes prototypes and inherited names."
editUrl: false
prev: false
next: false
sidebar:
  label: "DataObject"
  hidden: true
---

<a id="Vercel_AI_Util_DataObject"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DataObject
```


Object with an explicit constructor token and own properties.
Used where JavaScript distinguishes prototypes and inherited names.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DataObject.cs#L11)

## Constructors

<a id="Vercel_AI_Util_DataObject__ctor_"></a>
<a id="Vercel_AI_Util_DataObject__ctor"></a>

### DataObject()


```csharp
public DataObject()
```


Creates an object whose constructor token is [object](https://learn.microsoft.com/dotnet/api/system.object).

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DataObject.cs#L14)

## Properties

<a id="Vercel_AI_Util_DataObject_Constructor_"></a>
<a id="Vercel_AI_Util_DataObject_Constructor"></a>

### Constructor


```csharp
public object Constructor { get; set; }
```


Identity compared by `Object)` before properties.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DataObject.cs#L20)

<a id="Vercel_AI_Util_DataObject_Properties_"></a>
<a id="Vercel_AI_Util_DataObject_Properties"></a>

### Properties


```csharp
public Dictionary<string, object> Properties { get; }
```


Own properties. Inherited names are not stored here.

**Value:** [Dictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DataObject.cs#L23)
