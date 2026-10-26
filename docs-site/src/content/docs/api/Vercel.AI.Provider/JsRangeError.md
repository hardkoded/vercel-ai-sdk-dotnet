---
title: "JsRangeError"
description: "JavaScript RangeError analog."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsRangeError"
  hidden: true
---

<a id="Vercel_AI_Provider_JsRangeError"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class JsRangeError : JsError, ISerializable
```


JavaScript `RangeError` analog.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [JsError](/api/Vercel.AI.Provider/JsError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L98)

## Constructors

<a id="Vercel_AI_Provider_JsRangeError__ctor_"></a>
<a id="Vercel_AI_Provider_JsRangeError__ctor_System_String_"></a>

### JsRangeError(string)


```csharp
public JsRangeError(string message)
```


Creates a range error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L101)

## Properties

<a id="Vercel_AI_Provider_JsRangeError_ErrorName_"></a>
<a id="Vercel_AI_Provider_JsRangeError_ErrorName"></a>

### ErrorName


```csharp
public override string ErrorName { get; }
```


JavaScript `error.name`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L107)
