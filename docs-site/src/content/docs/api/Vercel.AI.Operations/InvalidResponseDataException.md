---
title: "InvalidResponseDataException"
description: "The provider response did not match the expected shape."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidResponseDataException"
  hidden: true
---

<a id="Vercel_AI_Operations_InvalidResponseDataException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class InvalidResponseDataException : AiSdkException, ISerializable
```


The provider response did not match the expected shape.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L153)

## Constructors

<a id="Vercel_AI_Operations_InvalidResponseDataException__ctor_"></a>
<a id="Vercel_AI_Operations_InvalidResponseDataException__ctor_System_Object_System_String_"></a>

### InvalidResponseDataException(object?, string)


```csharp
public InvalidResponseDataException(object? data, string message)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L156)

## Properties

<a id="Vercel_AI_Operations_InvalidResponseDataException_Data_"></a>
<a id="Vercel_AI_Operations_InvalidResponseDataException_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Response payload that failed validation.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L163)
