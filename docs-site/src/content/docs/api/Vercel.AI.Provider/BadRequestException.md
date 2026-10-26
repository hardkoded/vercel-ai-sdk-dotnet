---
title: "BadRequestException"
description: "HTTP 400."
editUrl: false
prev: false
next: false
sidebar:
  label: "BadRequestException"
  hidden: true
---

<a id="Vercel_AI_Provider_BadRequestException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class BadRequestException : ApiException, ISerializable
```


HTTP 400.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [ApiException](/api/Vercel.AI.Provider/ApiException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L74)

## Constructors

<a id="Vercel_AI_Provider_BadRequestException__ctor_"></a>
<a id="Vercel_AI_Provider_BadRequestException__ctor_System_String_System_String_"></a>

### BadRequestException(string, string?)


```csharp
public BadRequestException(string message, string? responseBody)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L77)
