---
title: "JsError"
description: "JavaScript Error analog whose ToString() omits the stack."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsError"
  hidden: true
---

<a id="Vercel_AI_Provider_JsError"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class JsError : Exception, ISerializable
```


JavaScript `Error` analog whose [ToString()](#Vercel_AI_Provider_JsError_ToString) omits the stack.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

**Derived:** [JsRangeError](/api/Vercel.AI.Provider/JsRangeError/), [JsTypeError](/api/Vercel.AI.Provider/JsTypeError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L55)

## Constructors

<a id="Vercel_AI_Provider_JsError__ctor_"></a>
<a id="Vercel_AI_Provider_JsError__ctor_System_String_"></a>

### JsError(string?)


```csharp
public JsError(string? message)
```


Creates an error with &lt;code class="paramref"&gt;message&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L58)

## Properties

<a id="Vercel_AI_Provider_JsError_ErrorName_"></a>
<a id="Vercel_AI_Provider_JsError_ErrorName"></a>

### ErrorName


```csharp
public virtual string ErrorName { get; }
```


JavaScript `error.name`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L64)

## Methods

<a id="Vercel_AI_Provider_JsError_ToString_"></a>
<a id="Vercel_AI_Provider_JsError_ToString"></a>

### ToString()


```csharp
public override string ToString()
```


Returns `Name` or `Name: message`, matching `Error.prototype.toString`.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ErrorMessage.cs#L70)
