---
title: "ApiTimeoutException"
description: "The provider call timed out."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiTimeoutException"
  hidden: true
---

<a id="Vercel_AI_Provider_ApiTimeoutException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ApiTimeoutException : ApiConnectionException, ISerializable
```


The provider call timed out.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [ApiConnectionException](/api/Vercel.AI.Provider/ApiConnectionException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L154)

## Constructors

<a id="Vercel_AI_Provider_ApiTimeoutException__ctor_"></a>
<a id="Vercel_AI_Provider_ApiTimeoutException__ctor_System_String_System_Exception_"></a>

### ApiTimeoutException(string, Exception?)


```csharp
public ApiTimeoutException(string message, Exception? innerException = null)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `innerException` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L157)
