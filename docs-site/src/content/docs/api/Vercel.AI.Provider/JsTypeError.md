---
title: "JsTypeError"
description: "JavaScript TypeError analog."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsTypeError"
  hidden: true
---

<a id="Vercel_AI_Provider_JsTypeError"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class JsTypeError : JsError, ISerializable
```


JavaScript `TypeError` analog.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [JsError](/api/Vercel.AI.Provider/JsError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L82)

## Constructors

<a id="Vercel_AI_Provider_JsTypeError__ctor_"></a>
<a id="Vercel_AI_Provider_JsTypeError__ctor_System_String_"></a>

### JsTypeError(string)


```csharp
public JsTypeError(string message)
```


Creates a type error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L85)

## Properties

<a id="Vercel_AI_Provider_JsTypeError_ErrorName_"></a>
<a id="Vercel_AI_Provider_JsTypeError_ErrorName"></a>

### ErrorName


```csharp
public override string ErrorName { get; }
```


JavaScript `error.name`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L91)
