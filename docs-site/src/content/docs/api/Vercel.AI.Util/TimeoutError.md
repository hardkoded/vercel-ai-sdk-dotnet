---
title: "TimeoutError"
description: "DOM TimeoutError analog used as an abort reason."
editUrl: false
prev: false
next: false
sidebar:
  label: "TimeoutError"
  hidden: true
---

<a id="Vercel_AI_Util_TimeoutError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class TimeoutError : Exception, ISerializable
```


DOM `TimeoutError` analog used as an abort reason.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L8)

## Constructors

<a id="Vercel_AI_Util_TimeoutError__ctor_"></a>
<a id="Vercel_AI_Util_TimeoutError__ctor_System_String_"></a>

### TimeoutError(string)


```csharp
public TimeoutError(string message)
```


Creates a timeout error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L11)

## Properties

<a id="Vercel_AI_Util_TimeoutError_ErrorName_"></a>
<a id="Vercel_AI_Util_TimeoutError_ErrorName"></a>

### ErrorName


```csharp
public string ErrorName { get; }
```


Always `TimeoutError`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L17)
