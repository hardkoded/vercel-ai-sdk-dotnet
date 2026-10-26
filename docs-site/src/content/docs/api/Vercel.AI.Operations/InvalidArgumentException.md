---
title: "InvalidArgumentException"
description: "Invalid caller argument. Maps to InvalidArgumentError."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidArgumentException"
  hidden: true
---

<a id="Vercel_AI_Operations_InvalidArgumentException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class InvalidArgumentException : AiSdkException, ISerializable
```


Invalid caller argument. Maps to `InvalidArgumentError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L135)

## Constructors

<a id="Vercel_AI_Operations_InvalidArgumentException__ctor_"></a>
<a id="Vercel_AI_Operations_InvalidArgumentException__ctor_System_String_System_Object_System_String_"></a>

### InvalidArgumentException(string, object?, string)


```csharp
public InvalidArgumentException(string parameter, object? value, string message)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parameter` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L138)

## Properties

<a id="Vercel_AI_Operations_InvalidArgumentException_Parameter_"></a>
<a id="Vercel_AI_Operations_InvalidArgumentException_Parameter"></a>

### Parameter


```csharp
public string Parameter { get; }
```


Parameter name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L146)

<a id="Vercel_AI_Operations_InvalidArgumentException_Value_"></a>
<a id="Vercel_AI_Operations_InvalidArgumentException_Value"></a>

### Value


```csharp
public object? Value { get; }
```


Rejected value.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L149)
