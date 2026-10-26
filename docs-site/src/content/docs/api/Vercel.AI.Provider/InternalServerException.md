---
title: "InternalServerException"
description: "HTTP 500-599."
editUrl: false
prev: false
next: false
sidebar:
  label: "InternalServerException"
  hidden: true
---

<a id="Vercel_AI_Provider_InternalServerException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class InternalServerException : ApiException, ISerializable
```


HTTP 500-599.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [ApiException](/api/Vercel.AI.Provider/ApiException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L134)

## Constructors

<a id="Vercel_AI_Provider_InternalServerException__ctor_"></a>
<a id="Vercel_AI_Provider_InternalServerException__ctor_System_String_System_Int32_System_String_"></a>

### InternalServerException(string, int, string?)


```csharp
public InternalServerException(string message, int statusCode, string? responseBody)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L137)
