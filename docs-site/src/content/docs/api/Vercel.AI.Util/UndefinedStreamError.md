---
title: "UndefinedStreamError"
description: "Rejection whose reason is JavaScript undefined."
editUrl: false
prev: false
next: false
sidebar:
  label: "UndefinedStreamError"
  hidden: true
---

<a id="Vercel_AI_Util_UndefinedStreamError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class UndefinedStreamError : Exception, ISerializable
```


Rejection whose reason is JavaScript `undefined`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L45)

## Constructors

<a id="Vercel_AI_Util_UndefinedStreamError__ctor_"></a>
<a id="Vercel_AI_Util_UndefinedStreamError__ctor"></a>

### UndefinedStreamError()


```csharp
public UndefinedStreamError()
```


Creates the error.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L48)

## Properties

<a id="Vercel_AI_Util_UndefinedStreamError_Reason_"></a>
<a id="Vercel_AI_Util_UndefinedStreamError_Reason"></a>

### Reason


```csharp
public object? Reason { get; }
```


Always `null`, matching a missing rejection reason.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L54)
