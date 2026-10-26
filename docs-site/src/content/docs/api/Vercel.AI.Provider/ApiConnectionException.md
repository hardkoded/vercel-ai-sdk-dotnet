---
title: "ApiConnectionException"
description: "The connection to the provider failed."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiConnectionException"
  hidden: true
---

<a id="Vercel_AI_Provider_ApiConnectionException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class ApiConnectionException : AiSdkException, ISerializable
```


The connection to the provider failed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

**Derived:** [ApiTimeoutException](/api/Vercel.AI.Provider/ApiTimeoutException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L144)

## Constructors

<a id="Vercel_AI_Provider_ApiConnectionException__ctor_"></a>
<a id="Vercel_AI_Provider_ApiConnectionException__ctor_System_String_System_Exception_"></a>

### ApiConnectionException(string, Exception?)


```csharp
public ApiConnectionException(string message, Exception? innerException = null)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `innerException` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L147)
