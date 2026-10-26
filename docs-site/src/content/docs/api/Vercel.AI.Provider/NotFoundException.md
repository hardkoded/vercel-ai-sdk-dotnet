---
title: "NotFoundException"
description: "HTTP 404."
editUrl: false
prev: false
next: false
sidebar:
  label: "NotFoundException"
  hidden: true
---

<a id="Vercel_AI_Provider_NotFoundException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class NotFoundException : ApiException, ISerializable
```


HTTP 404.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [ApiException](/api/Vercel.AI.Provider/ApiException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L104)

## Constructors

<a id="Vercel_AI_Provider_NotFoundException__ctor_"></a>
<a id="Vercel_AI_Provider_NotFoundException__ctor_System_String_System_String_"></a>

### NotFoundException(string, string?)


```csharp
public NotFoundException(string message, string? responseBody)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L107)
