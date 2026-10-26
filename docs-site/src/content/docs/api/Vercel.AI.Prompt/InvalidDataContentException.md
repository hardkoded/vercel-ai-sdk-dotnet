---
title: "InvalidDataContentException"
description: "File content could not be converted. Maps to InvalidDataContentError."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidDataContentException"
  hidden: true
---

<a id="Vercel_AI_Prompt_InvalidDataContentException"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class InvalidDataContentException : AiSdkException, ISerializable
```


File content could not be converted. Maps to `InvalidDataContentError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L143)

## Constructors

<a id="Vercel_AI_Prompt_InvalidDataContentException__ctor_"></a>
<a id="Vercel_AI_Prompt_InvalidDataContentException__ctor_System_String_"></a>

### InvalidDataContentException(string)


```csharp
public InvalidDataContentException(string message)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L146)
