---
title: "RateLimitException"
description: "HTTP 429."
editUrl: false
prev: false
next: false
sidebar:
  label: "RateLimitException"
  hidden: true
---

<a id="Vercel_AI_Provider_RateLimitException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RateLimitException : ApiException, ISerializable
```


HTTP 429.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [ApiException](/api/Vercel.AI.Provider/ApiException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L124)

## Constructors

<a id="Vercel_AI_Provider_RateLimitException__ctor_"></a>
<a id="Vercel_AI_Provider_RateLimitException__ctor_System_String_System_String_"></a>

### RateLimitException(string, string?)


```csharp
public RateLimitException(string message, string? responseBody)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L127)
