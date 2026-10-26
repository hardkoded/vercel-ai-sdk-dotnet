---
title: "UnprocessableEntityException"
description: "HTTP 422."
editUrl: false
prev: false
next: false
sidebar:
  label: "UnprocessableEntityException"
  hidden: true
---

<a id="Vercel_AI_Provider_UnprocessableEntityException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class UnprocessableEntityException : ApiException, ISerializable
```


HTTP 422.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [ApiException](/api/Vercel.AI.Provider/ApiException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L114)

## Constructors

<a id="Vercel_AI_Provider_UnprocessableEntityException__ctor_"></a>
<a id="Vercel_AI_Provider_UnprocessableEntityException__ctor_System_String_System_String_"></a>

### UnprocessableEntityException(string, string?)


```csharp
public UnprocessableEntityException(string message, string? responseBody)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L117)
